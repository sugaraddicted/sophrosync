#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Runs all Sophrosync security scans: ZAP baseline, ZAP full (authenticated), JMeter load test.

.DESCRIPTION
    Prerequisites:
      - Full stack running: docker compose up postgres keycloak -d  (then start all services)
      - Gateway accessible at http://localhost:5000
      - Docker Desktop running (for ZAP)
      - JMeter installed (choco install jmeter -y) and on PATH

    Usage:
      .\run-security-scans.ps1                   # runs all scans
      .\run-security-scans.ps1 -SkipZap          # JMeter only
      .\run-security-scans.ps1 -SkipJmeter       # ZAP only
      .\run-security-scans.ps1 -JwtToken "<jwt>" # pass JWT directly (skips Keycloak prompt)
#>

param(
    [switch]$SkipZap,
    [switch]$SkipJmeter,
    [string]$JwtToken = "",
    [string]$KeycloakUser = "therapist@demo.com",
    [string]$KeycloakPassword = ""
)

$ErrorActionPreference = "Stop"
$RepoRoot = Split-Path -Parent $PSScriptRoot
$OutDir   = $PSScriptRoot

# ─── Obtain JWT ──────────────────────────────────────────────────────────────

function Get-JwtToken {
    param([string]$User, [string]$Password)
    if (-not $Password) {
        $Password = Read-Host "Keycloak password for $User"
    }
    $body = "grant_type=password&client_id=sophrosync-spa&username=$User&password=$Password"
    $resp = Invoke-RestMethod `
        -Uri "http://localhost:8080/realms/sophrosync/protocol/openid-connect/token" `
        -Method POST -Body $body `
        -ContentType "application/x-www-form-urlencoded"
    return $resp.access_token
}

# ─── ZAP Baseline (unauthenticated) ─────────────────────────────────────────

function Invoke-ZapBaseline {
    Write-Host "`n[ZAP] Running baseline scan (unauthenticated)..." -ForegroundColor Cyan
    New-Item -ItemType Directory -Force -Path "$OutDir\zap-reports" | Out-Null

    # Use host.docker.internal on Windows Docker Desktop (--network host not supported)
    # Use 2.15.0 tag — :stable (2.17) has empty Python script stubs
    docker run --rm --user 0 `
        -v "${OutDir}/zap-reports:/zap/wrk:rw" `
        ghcr.io/zaproxy/zaproxy:2.15.0 `
        zap-baseline.py `
            -t http://host.docker.internal:5000 `
            -r zap-baseline-report.html `
            -J zap-baseline-alerts.json `
            -I

    Write-Host "[ZAP] Baseline report: $OutDir\zap-reports\zap-baseline-report.html" -ForegroundColor Green
}

# ─── ZAP Full (authenticated) ────────────────────────────────────────────────

function Invoke-ZapFull {
    param([string]$Jwt)
    Write-Host "`n[ZAP] Running full authenticated scan..." -ForegroundColor Cyan

    # Patch JWT into context file
    $contextContent = Get-Content "$OutDir\zap-context.yaml" -Raw
    $contextContent  = $contextContent -replace '<JWT_TOKEN>', $Jwt
    $contextContent | Out-File "$OutDir\zap-context-patched.yaml" -Encoding utf8

    docker run --rm --user 0 `
        -v "${OutDir}:/zap/wrk:rw" `
        ghcr.io/zaproxy/zaproxy:2.15.0 `
        zap-full-scan.py `
            -t http://host.docker.internal:5000/api `
            -r zap-reports/zap-full-report.html `
            -J zap-reports/zap-full-alerts.json `
            -z "-config replacer.full_list(0).description=Authorization -config replacer.full_list(0).enabled=true -config replacer.full_list(0).matchtype=REQ_HEADER -config replacer.full_list(0).matchstr=Authorization -config replacer.full_list(0).replacement=`"Bearer $Jwt`"" `
            -I

    Remove-Item "$OutDir\zap-context-patched.yaml" -ErrorAction SilentlyContinue
    Write-Host "[ZAP] Full report: $OutDir\zap-reports\zap-full-report.html" -ForegroundColor Green
}

# ─── JMeter Load Test ────────────────────────────────────────────────────────

function Invoke-JmeterTests {
    param([string]$Jwt)
    Write-Host "`n[JMeter] Running load tests..." -ForegroundColor Cyan

    # JMeter cannot handle paths with non-ASCII characters — use a temp dir
    $TmpDir = "C:\sophrosync-jmeter"
    New-Item -ItemType Directory -Force -Path "$TmpDir\report" | Out-Null

    # Patch JWT into JMX
    $jmxContent = Get-Content "$OutDir\sophrosync-load-test.jmx" -Raw
    $jmxContent  = $jmxContent -replace '__JWT_TOKEN__', $Jwt
    $runJmx = "$TmpDir\run.jmx"
    [System.IO.File]::WriteAllText($runJmx, $jmxContent, [System.Text.Encoding]::UTF8)

    # Locate jmeter.bat (prefer ~/tools/jmeter, fall back to PATH)
    $jmeterBin = if (Test-Path "$env:USERPROFILE\tools\jmeter\bin\jmeter.bat") {
        "$env:USERPROFILE\tools\jmeter\bin\jmeter.bat"
    } else { "jmeter" }

    & $jmeterBin -n `
        -t "$runJmx" `
        -l "$TmpDir\results.jtl" `
        -e -o "$TmpDir\report" `
        -j "$TmpDir\jmeter.log"

    # Copy results back to docs/security
    New-Item -ItemType Directory -Force -Path "$OutDir\jmeter-report" | Out-Null
    Copy-Item "$TmpDir\results.jtl" "$OutDir\jmeter-results.jtl" -Force
    Copy-Item "$TmpDir\report\*" "$OutDir\jmeter-report\" -Recurse -Force

    Write-Host "[JMeter] HTML report: $OutDir\jmeter-report\index.html" -ForegroundColor Green
    Write-Host "[JMeter] Results JTL: $OutDir\jmeter-results.jtl" -ForegroundColor Green
}

# ─── Main ────────────────────────────────────────────────────────────────────

Write-Host "=== Sophrosync Security Scans ===" -ForegroundColor Yellow

if (-not $SkipZap -or -not $SkipJmeter) {
    if (-not $JwtToken) {
        $JwtToken = Get-JwtToken -User $KeycloakUser -Password $KeycloakPassword
    }
    Write-Host "[Auth] JWT obtained ($(($JwtToken.Length)) chars)" -ForegroundColor Green
}

if (-not $SkipZap) {
    Invoke-ZapBaseline
    Invoke-ZapFull -Jwt $JwtToken
}

if (-not $SkipJmeter) {
    Invoke-JmeterTests -Jwt $JwtToken
}

Write-Host "`n=== All scans complete ===" -ForegroundColor Yellow
Write-Host "Artifacts saved to: $OutDir" -ForegroundColor Green
Write-Host "Next: copy reports to docs/security/ and fill zap-scan-notes.md"
