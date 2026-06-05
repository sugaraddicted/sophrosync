# ZAP Scan Notes

**Scan date:** _fill in after running_
**Stack version:** Sophrosync v0.1 (diploma build)
**Tester:** Mariia Prylutska
**ZAP version:** zaproxy/zaproxy:stable (latest)
**Gateway URL:** http://localhost:5000

---

## How to Run

```powershell
# From docs/security/
.\run-security-scans.ps1
```

See `run-security-scans.ps1` for full prerequisites.

---

## Baseline Scan Results

_Fill in after running: `docs/security/zap-reports/zap-baseline-report.html`_

| Alert | Risk | Status | Notes |
|-------|------|--------|-------|
| X-Content-Type-Options header missing | Low | ✅ Fixed | Added in Gateway middleware |
| X-Frame-Options header missing | Medium | ✅ Fixed | Added in Gateway middleware |
| Missing Anti-CSRF Tokens | Medium | N/A — Suppressed | JWT-based API; CSRF not applicable |
| _other alerts_ | | | |

---

## Full Authenticated Scan Results

_Fill in after running: `docs/security/zap-reports/zap-full-report.html`_

| Alert | Risk | Status | Notes |
|-------|------|--------|-------|
| | | | |

---

## Suppressed False Positives

The following findings are documented as intentionally accepted or N/A for a JWT-based .NET API:

| Finding | Reason Suppressed |
|---------|------------------|
| Missing Anti-CSRF Tokens | All state-changing operations require a Bearer JWT. Stateless API, no cookie-based session. OWASP ASVS 4.2.2 exemption applies. |
| Application Error Disclosure | ASP.NET dev exception handler is enabled only in Development environment; Production uses a silent handler. |
| X-Content-Type-Options | Fixed — added to Gateway response headers. |
| X-Frame-Options | Fixed — added to Gateway response headers. |

---

## Rate Limiter Note

Gateway `FixedWindowRateLimiter` config (Gateway/Program.cs):
- `PermitLimit = 100` per IP per minute
- `RejectionStatusCode = 429`

For the JMeter TG2 test (100 threads, 5 loops): if the window resets between runs, not all requests will hit 429. For reproducible results, temporarily lower `PermitLimit` to `10` in development, run the test, then restore. Document production recommendation as `100/min`.
