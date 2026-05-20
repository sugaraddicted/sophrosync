# Sophrosync — Notes Feature: Product Requirements Document

**Version:** 1.0
**Date:** 2026-03-19
**Scope:** NotesService — MVP definition, UX requirements, compliance red lines, and competitive positioning
**Target practice type:** Solo and small-group private practice therapists (cash-pay primary; insurance-adjacent secondary)

---

## Table of Contents

1. [Workflow Overview — The Therapist's Day](#1-workflow-overview)
2. [MVP vs. Nice-to-Have Feature Matrix](#2-mvp-vs-nice-to-have)
3. [Note Types Prioritized for Private Practice](#3-note-types)
4. [Sign / Lock / Amendment Workflow](#4-sign-lock-amendment)
5. [Notes-to-Client and Notes-to-Appointment Relationships in the UI](#5-ui-relationships)
6. [Progress Note Form Fields](#6-progress-note-form-fields)
7. [HIPAA / GDPR Compliance Requirements That Affect UX](#7-compliance-ux)
8. [Competitive Analysis and Differentiation Strategy](#8-competitive-differentiation)
9. [User Stories](#9-user-stories)
10. [Alignment with Sophrosync Architecture](#10-architecture-alignment)

---

## 1. Workflow Overview

### The Core Loop: What a Therapist Does Every Single Day

Understanding where notes live in a therapist's actual day is the foundation for every UX decision. The loop is tight:

```
Morning: Review caseload → check which sessions have incomplete notes
  │
  ▼
Session (50 min): Therapist is present with client — not writing
  │
  ▼
Between sessions (10 min): Open note for the session that just ended,
  draft quickly while memory is fresh, save as Draft
  │
  ▼
End of day (15–30 min): Review all Draft notes, finalize, Sign/Lock
  │
  ▼
Billing (if applicable): Locked note triggers or accompanies invoice
  │
  ▼
Supervision (weekly): Supervisor reviews notes, adds co-signature
```

**The friction points that matter most:**

- Getting into the note form from the calendar/session view in under two taps
- Not losing a draft when a browser tab closes or a session expires
- Knowing at a glance which notes are overdue (past the 24–72 hour window)
- The sign action feeling deliberate and final, not buried

### What "Overdue" Means in Practice

Most state licensing boards and HIPAA-adjacent best practices require progress notes to be completed within 24–72 hours of the session. Payers (for insurance-billing practices) often require notes within 7 days. A therapist who falls behind by a week is at professional and legal risk. Sophrosync must make overdue notes visible, not hidden.

---

## 2. MVP vs. Nice-to-Have Feature Matrix

### Tier 1 — Launch Blockers (MVP must-haves)

Without these, a therapist cannot replace their current tool with Sophrosync.

| Feature | Why It Blocks Launch |
|---------|----------------------|
| Create a progress note linked to an appointment and a client | Core unit of clinical work |
| SOAP and DAP note templates | The two most-used formats in private practice |
| Draft autosave (minimum: explicit "Save Draft" button) | Therapists will not risk losing clinical text |
| Sign note action with timestamp and author capture | Licensing board compliance — unsigned notes are a liability |
| Lock note action that prevents edits | Standard of care and legal protection |
| View all notes for a client in chronological order | Required for continuity of care and clinical review |
| Note linked to appointment (one-to-one) | Notes without an appointment context lose their audit meaning |
| Overdue note indicator | Therapist risk management; required for sustainable clinical practice |
| Role-based access: therapist can only access own clients' notes | HIPAA minimum necessary standard |
| Note content encrypted at rest | HIPAA Security Rule — already designed as AES-256-GCM in architecture spec |
| Audit trail: who created, signed, and locked each note | HIPAA Audit Controls (§164.312(b)) |

### Tier 2 — Retention Drivers (must be built within 90 days of launch)

Without these, therapists will switch to a competitor once they hit these gaps.

| Feature | Why Therapists Churn Without It |
|---------|--------------------------------|
| BIRP template | Mandated in many community mental health and insurance settings |
| Free-form / narrative note template | Many therapists with hybrid or specialized practices need an unstructured option |
| Addendum workflow (append to locked note) | Therapists will discover they need corrections; locking with no amendment path creates fear |
| Treatment plan (linked to client) | Inseparable from ongoing session notes; most state boards require a current plan |
| Note completion dashboard / caseload view | Without visibility into pending notes, therapists fall behind |
| Supervisor co-signature | Required for pre-licensed therapists — a significant portion of private practice |
| Print / PDF export of a single note or full client chart | Required for records requests, ROI, and court-ordered disclosures |

### Tier 3 — Differentiators (build to win, not to retain)

| Feature | Competitive Value |
|---------|------------------|
| AI-assisted note drafting (template population from brief inputs) | SimplePractice's biggest weakness; strong retention driver |
| Voice-to-text note input | Reduces after-session admin time significantly for high-volume therapists |
| PHQ-9 / GAD-7 outcome measure tracking linked to session notes | Evidence-based practice signal; appeals to outcome-focused therapists |
| Note templates customizable per therapist | Jane App does this well; respects clinical style diversity |
| Smart note warnings (e.g., flagging notes > 72 hours old as high-urgency) | Proactive risk management |
| Group therapy note with per-member sub-note | Niche but high-value for practices with group programs |

### Tier 4 — Defer Beyond 12 Months

| Feature | Why to Defer |
|---------|--------------|
| Insurance billing integration from note (CMS-1500 auto-fill) | BillingService is explicitly Phase 2; do not couple now |
| Telehealth session auto-transcription | High infrastructure cost; not table-stakes for private practice |
| Client-visible session summaries | Requires careful clinical boundary design; defer until client portal is mature |
| Outcome measure library (beyond PHQ-9/GAD-7) | Narrow segment need; build after proving core |

---

## 3. Note Types Prioritized for Private Practice

### Why Template Choice Matters

Therapists self-identify with a note format. A therapist trained in SOAP who is forced to write in a format they do not recognize will churn. The template is a clinical identity signal, not just a UX preference.

### The Four Templates to Build

#### SOAP (Subjective / Objective / Assessment / Plan)
Most widely recognized across all clinical backgrounds. Borrowed from medicine.

| Section | What Therapists Write |
|---------|----------------------|
| Subjective | Client's self-reported experience; mood, presenting concerns this session |
| Objective | Therapist's clinical observations; appearance, affect, behavior, cognitive functioning |
| Assessment | Clinical interpretation; progress toward goals, diagnostic impressions |
| Plan | Next steps; interventions planned, homework, next appointment |

**When therapists use it:** General outpatient therapy, especially therapists trained in medical or hospital settings.

#### DAP (Data / Assessment / Plan)
Simpler than SOAP; the two most common sections from SOAP (Subjective + Objective) are combined into "Data."

| Section | What Therapists Write |
|---------|----------------------|
| Data | Combined subjective and objective observations |
| Assessment | Clinical interpretation and progress |
| Plan | Next steps and interventions |

**When therapists use it:** Solo practitioners who want speed; therapists trained in outpatient community mental health. Often preferred in private practice because it is faster to complete.

#### BIRP (Behavior / Intervention / Response / Plan)
Behavior-focused; required by many Medicaid-adjacent and EAP payers.

| Section | What Therapists Write |
|---------|----------------------|
| Behavior | Specific behaviors observed and reported |
| Intervention | Techniques and interventions used this session |
| Response | How the client responded to the intervention |
| Plan | What happens next |

**When therapists use it:** Practices with EAP contracts, insurance billing, or substance use treatment focus.

#### Free Form / Narrative
An open text area with no section structure. Critical for:
- Therapists with niche modalities (EMDR, somatic, psychoanalytic) who cannot force their notes into structured formats
- Intake session summaries that are more narrative
- Consultation notes and supervision notes

**Important:** Free form should still require a minimum set of metadata fields (date, client, session duration, therapist signature) even if the clinical content is unstructured.

### Note Types That Are NOT Progress Notes

These are distinct note-adjacent record types that share the notes infrastructure but serve different purposes:

| Type | Purpose | MVP Priority |
|------|---------|-------------|
| Treatment Plan | Goals, interventions, and review schedule for a client's care | Tier 2 |
| Intake/Biopsychosocial Assessment | Initial comprehensive evaluation | Tier 3 |
| Consultation Note | Documentation of peer or supervisor consultation | Tier 3 |
| Discharge Summary | Closing documentation when a client ends treatment | Tier 3 |
| Risk Assessment Note | Standalone documentation of a safety assessment | Tier 2 (if RiskLevel is High/Crisis in ClientService) |

---

## 4. Sign / Lock / Amendment Workflow

### The Status State Machine

The architecture spec already defines three statuses: `Draft | Signed | Locked`. This is correct. Here is what each state means clinically and what the UX must communicate:

```
[Draft] ──── Sign action ──── [Signed] ──── Lock action ──── [Locked]
                                 │                               │
                         Can still be edited                 Cannot be edited
                         (minor corrections)                 Addendum only
```

### State Descriptions and UX Requirements

#### Draft
- **Clinical meaning:** Work in progress. Not a legal record yet.
- **Who can edit:** The author only. No co-author editing in MVP.
- **UX signal:** Clear "Draft" badge in the note list and on the note header. The note is visually distinct from completed notes.
- **Autosave:** Drafts must autosave. At minimum, autosave on a 30-second timer while the user is typing, plus an explicit "Save" button. The user must never see a blank form after a page reload on an in-progress draft.
- **Risk of getting this wrong:** If drafts are not saved reliably, therapists will stop trusting the product after losing clinical text once.

#### Signed
- **Clinical meaning:** The therapist attests this note is an accurate clinical record. This is the point of professional accountability.
- **Who can act:** Only the note author (therapist), or a supervisor co-signing a supervisee's note.
- **UX requirements:**
  - The Sign action must require a deliberate confirmation — not a single accidental click. Use a modal: "You are signing this note for [Client Name] on [Date]. This attests to its accuracy as a clinical record. Signed as: [Therapist Full Name]. Sign note?"
  - Capture and display: signed by [Full Name], at [exact timestamp with timezone].
  - After signing: the note is still editable in most licensing contexts (minor corrections before locking), but this must be clearly communicated with a warning: "This note has been signed. Any edits will be logged."
  - Do NOT lock automatically on sign. These are two distinct clinical actions.

#### Locked
- **Clinical meaning:** The note is finalized and immutable. It is now a permanent part of the legal medical record. This is the state that billing, audits, and records requests reference.
- **Who can act:** The note author, or an admin/supervisor.
- **UX requirements:**
  - The Lock action must be even more deliberate than Sign. Use a two-step confirmation: "Locking this note makes it permanently uneditable. Only an addendum can be added. Lock note?"
  - After locking: the edit form is replaced entirely with a read-only view. There is no edit button. The locked state is visually unmistakable — a lock icon in the header, a distinct background or border.
  - Timestamp and locker identity must be displayed prominently on the note.
  - The locked state is what triggers the "note complete" status for the ReportingService note completion rate metric.

### Amendment / Addendum Workflow

**What therapists actually need:** They discover an error in a locked note — a wrong date, a wrong medication dose, a missing statement. They cannot edit the locked note, but they need to correct the record.

**The correct clinical solution is an addendum, not a correction.** An addendum is a new, timestamped entry that is appended to the original note, stating what is being clarified or corrected. The original note remains unchanged. This is standard healthcare documentation practice.

**UX requirements for addendum:**
- Only available on Locked notes.
- The "Add Addendum" button is prominent and clearly labeled — therapists must be able to find it without hunting.
- The addendum form is a simple text area with a label: "Addendum — added by [Name] on [Date/Time]."
- The addendum is itself signed (author + timestamp captured) but does not go through a separate lock workflow — it is append-only by definition.
- In the read-only note view, addenda are displayed below the original note content, in chronological order, each with a clear visual separator and timestamp.
- Multiple addenda are allowed on a single note.

**What to NOT build:** A "request correction" workflow where a supervisor edits a therapist's note. This is ethically and legally problematic. Each therapist is responsible for their own documentation. Supervisors can add addenda and co-sign; they do not edit the original author's clinical text.

---

## 5. UI Relationships — Notes to Clients and Appointments

### The Navigation Principle

A therapist should be able to reach a note from three different entry points, and all three should land on the same note form:

```
Entry Point 1: Calendar / Schedule View
  └── Click on a completed/past appointment
      └── "View / Add Note" button on appointment detail
          └── Opens note form for that appointment

Entry Point 2: Client Profile
  └── Notes tab on client profile page
      └── List of all notes for this client (chronological, newest first)
          └── Click a note → opens note detail / edit form

Entry Point 3: Notes Inbox / Dashboard Widget
  └── "Incomplete Notes" widget on the main dashboard
      └── List of sessions from the past 7 days with no locked note
          └── Click an item → opens note form for that appointment
```

### The Appointment-to-Note Relationship

The architecture spec correctly models this as one-to-one: one `SessionNote` per `AppointmentId`. This is the right call for private practice outpatient therapy.

**UX implications of the one-to-one constraint:**
- When a therapist opens an appointment that already has a note, they should see the existing note — not a button to create a new one. Never allow duplicate notes for the same appointment.
- The appointment card in the calendar should display the note status as a visual indicator: no note (red dot / warning), draft (yellow), signed (blue), locked (green). This gives the therapist a caseload health view at a glance.
- If an appointment is cancelled or marked as no-show, the note form should still be accessible but the template should include a "No Show / Cancellation" note type with minimal required fields (therapist observation, cancellation policy documentation).

### The Client Profile Notes Tab

This is where therapists review clinical history. Layout requirements:

- Chronological list, newest first, with the ability to sort oldest-first
- Each row shows: date, appointment type, note template type, status badge, author name, and a quick-preview of the first line of content
- Filter by status (Draft / Signed / Locked) and by date range
- Clicking a row expands to full note view in-page or opens a side panel — not a new page load. Full-page navigation breaks the review flow.
- The treatment plan (once built) is pinned to the top of this tab, not buried in the chronological list

### Notes and the Dashboard

The main dashboard (currently scaffolded with a calendar) should include a "Documentation Status" widget:

| Metric | Purpose |
|--------|---------|
| Notes overdue (> 72 hours, no lock) | Clinical risk indicator |
| Notes in Draft (completed today but not signed) | Daily reminder |
| Notes completed this week | Positive reinforcement, progress tracking |

This widget is the therapist's daily conscience. It should be visible without scrolling on the dashboard.

---

## 6. Progress Note Form Fields

### Universal Header Fields (all note types)

These fields appear on every note regardless of template and are mostly auto-populated from the appointment context:

| Field | Source | Required | Notes |
|-------|--------|----------|-------|
| Client name | Auto-filled from appointment | Yes | Display only; not editable |
| Date of service | Auto-filled from appointment start time | Yes | Editable if note is created outside of appointment context |
| Session start time | Auto-filled from appointment | Yes | |
| Session end time | Auto-filled from appointment | Yes | |
| Session duration (minutes) | Calculated | Yes | Auto-calculated; displayed but editable to handle actual vs. scheduled duration |
| Session format | Auto-filled from appointment (InPerson / Telehealth / Phone) | Yes | |
| Note template type | Selected by therapist | Yes | SOAP / DAP / BIRP / Free Form |
| Therapist name | Auto-filled from authenticated user | Yes | Display only |
| Diagnosis codes at time of service | Pulled from ClientService diagnoses | No for MVP | Read-only display of active diagnoses; links notes to clinical context |
| CPT code | Manual entry | No for MVP; Tier 2 | Required when BillingService is active |

### SOAP Template Fields

| Field | Guidance Hint | Max Length |
|-------|---------------|------------|
| Subjective | "Client's self-reported experience, mood, presenting issues this session" | 2000 chars |
| Objective | "Your clinical observations: appearance, affect, behavior, speech, thought process, insight, judgment" | 2000 chars |
| Assessment | "Clinical interpretation, progress toward treatment goals, diagnostic impressions" | 2000 chars |
| Plan | "Interventions for next session, homework, referrals, next appointment" | 1000 chars |

### DAP Template Fields

| Field | Guidance Hint | Max Length |
|-------|---------------|------------|
| Data | "Subjective and objective observations combined — what the client reported and what you observed" | 3000 chars |
| Assessment | "Your clinical interpretation and progress assessment" | 2000 chars |
| Plan | "Next steps, homework, planned interventions, follow-up" | 1000 chars |

### BIRP Template Fields

| Field | Guidance Hint | Max Length |
|-------|---------------|------------|
| Behavior | "Specific client behaviors, symptoms, and presenting concerns observed or reported this session" | 2000 chars |
| Intervention | "Techniques and interventions you used — be specific (e.g., CBT cognitive restructuring, EMDR processing)" | 2000 chars |
| Response | "How the client responded to the interventions — affect, engagement, insight demonstrated" | 2000 chars |
| Plan | "What will happen next — homework, planned techniques, next appointment" | 1000 chars |

### Free Form Template Fields

| Field | Guidance Hint | Max Length |
|-------|---------------|------------|
| Session Notes | Open text area — no required structure | 5000 chars |

### Footer Fields (all note types, below clinical content)

| Field | Purpose | Required |
|-------|---------|----------|
| Safety / Risk notation | Was a safety assessment conducted? (Yes/No toggle + notes field) | Tier 2 (important but allow blank for MVP) |
| Medications discussed | Free text — medications mentioned during session | Optional |
| Referrals made | Free text — any referrals initiated this session | Optional |
| Internal note (never shared) | Private therapist memo — not part of the clinical record; clearly labeled | Optional — but must be clearly distinguished from clinical content |

### Field Design Principles

- **Every text area must show character count remaining.** Therapists who export notes for insurance or records requests need notes that are substantive but not padded. Visible limits promote better clinical writing habits.
- **Guidance hints (placeholders) should disappear on focus, not persist as watermarks.** A therapist who glances at a filled-in field should not have to mentally filter out placeholder text.
- **Tab order must be logical** — SOAP fields should tab in S → O → A → P order, not randomly by DOM position.
- **No required-field blocking on autosave.** A therapist mid-session may close the tab. The draft must save with whatever content exists, even if the note is incomplete. Required fields are enforced only on the Sign action.

---

## 7. HIPAA / GDPR Compliance Requirements That Affect UX

These are not backend-only concerns. Each item below has a direct UX implication that must be designed into the interface, not retrofitted.

### Red Line 1 — Note Content Is PHI, Treat It as Such Everywhere

**Requirement:** Clinical note content must never appear in URLs, browser history entries, notification previews, or any surface that could be cached or screenshot without the therapist's intent.

**UX implications:**
- Note content must never be in query strings. Navigation to a note must use opaque IDs only (e.g., `/notes/{noteId}`) — not `/notes?client=Smith&date=2026-03-19`.
- Email notifications about notes (e.g., "You have 3 overdue notes") must never include client names or clinical content in the notification body. The notification is a nudge, not a data container.
- Browser session timeout must log the therapist out and clear any in-memory note content. The timeout warning must give the therapist an opportunity to save their draft before expiry.
- Print-to-PDF note exports must open in a new browser tab, not download silently to a Downloads folder that a spouse or child might see on a shared computer.

### Red Line 2 — The Audit Trail Is User-Visible

**Requirement (HIPAA §164.312(b) Audit Controls):** Every access and mutation of a note must be logged. Beyond backend compliance, therapists must be able to see who has accessed their notes.

**UX implications:**
- Each locked note must display an "Activity Log" section: created by X on [date], signed by X on [date], locked by X on [date], viewed by [list of users who accessed it].
- In group practices, this protects therapists from unauthorized access by other clinicians. A therapist who sees an unexpected viewer in their note's activity log has grounds for an incident report.
- The audit log view must be read-only and cannot be deleted or modified by any UI action — this is enforced by the AuditService's append-only design.

### Red Line 3 — Sign and Lock Actions Require Re-Authentication Confirmation on Shared Devices

**Requirement:** In practices where multiple therapists share a workstation, a sign or lock action must confirm identity. Even on individual workstations, a deliberate confirmation prevents accidental signing.

**UX implications:**
- Minimum: a modal confirmation that displays the therapist's full name and asks them to confirm. This is the MVP approach.
- Better (Tier 2): require the therapist to re-enter their password or confirm a Keycloak session challenge before signing a note. This creates a stronger attestation record.

### Red Line 4 — Minimum Necessary Standard Enforced at the UI Level

**Requirement (HIPAA §164.502(b) Minimum Necessary):** A therapist should only be able to see notes for clients on their own caseload. An admin can see all notes. A supervisor can see supervisee notes.

**UX implications:**
- The note search / filter interface must not allow a therapist to query across all clients in the practice. The client selector must be scoped to their own caseload.
- If a client transfers between therapists, the transferring therapist retains read access to historical notes but cannot create new notes. The UI must reflect this distinction clearly.
- The "shared note" concept (where two therapists both document the same session) is not supported in MVP. Do not build a multi-author note form.

### Red Line 5 — GDPR Right to Erasure and Data Portability

**Requirement (GDPR Art. 17 and Art. 20):** If a client requests data deletion or data export, notes are included. The UX must support this workflow for admins.

**UX implications:**
- The admin interface must include a "Client Data Export" function that packages all notes (and associated audit entries) for a given client into a downloadable archive (PDF or structured JSON).
- A "soft delete" flag on notes must be supported. A soft-deleted note is not visible in the standard UI but is retained for the legally required period before hard deletion.
- The deletion workflow must show the admin a count of records to be deleted (including notes) before confirming, and must require a secondary confirmation for notes specifically ("This will mark X session notes for deletion.").

### Red Line 6 — Note Locking Is Irreversible Through the UI

**Requirement:** No UI path must exist for unlocking a note. Period. The only mechanism for correcting a locked note is an addendum.

**UX implications:**
- There is no "Unlock" button in any view, including admin views. The admin interface may surface an "Admin Override" for extraordinary circumstances (with full audit trail), but this must be a deliberate configuration choice, not a standard feature.
- This must be communicated to therapists proactively — a tooltip or help text on the Lock button: "Locking is permanent. You can add an addendum after locking, but the original note cannot be edited."

---

## 8. Competitive Analysis and Differentiation Strategy

### SimplePractice

**Strengths:**
- Polished consumer-grade UI; therapists feel comfortable immediately
- Strong client portal with self-scheduling and secure messaging
- Robust telehealth integration
- Good mobile experience

**Weaknesses that matter for notes specifically:**
- Note templates are limited and not customizable — SOAP and a basic free-form only in the standard tier
- The sign/lock workflow is a single action ("Sign & Lock") — no clinical distinction between signing (attestation) and locking (immutability). This is a clinical UX error.
- No AI-assisted note drafting as of early 2026
- Group note functionality is limited
- The addendum workflow is present but underpromoted — many therapists do not know it exists

**Sophrosync response:** Separate Sign and Lock into distinct actions with clear clinical rationale. Build customizable note templates before SimplePractice does. Target the AI-assisted drafting gap as the primary differentiator.

### TherapyNotes

**Strengths:**
- Excellent insurance billing integration — the go-to for multi-payer practices
- Strong treatment plan workflow with goal-tracking
- Good supervisor co-signature support
- Robust note history and audit trail

**Weaknesses that matter:**
- UI is dated and dense — therapists describe it as "feeling like accounting software"
- Mobile experience is poor — nearly unusable for note completion on a phone
- No voice-to-text or AI assistance
- Onboarding friction is high; takes weeks to configure for a new practice

**Sophrosync response:** Win on UX quality and onboarding speed. TherapyNotes' billing strength is something Sophrosync should not compete with in Phase 1 (BillingService is deferred). Compete on clinical documentation experience and time-to-value.

### TheraNest

**Strengths:**
- Competitive pricing for group practices
- Good group therapy session note support
- Decent client portal

**Weaknesses that matter:**
- Note template variety is poor — mostly free-form
- No AI features
- Customer support reputation is inconsistent
- The note completion and reminder workflow is weak — therapists fall behind without good visibility

**Sophrosync response:** Win on template quality (SOAP/DAP/BIRP properly implemented), the overdue note dashboard, and the sign/lock workflow clarity. These are all gaps where TheraNest underperforms.

### Jane App

**Strengths:**
- Best-in-class customizable note templates — therapists can build their own
- Excellent multi-disciplinary support (physiotherapy, psychotherapy, etc.)
- Clean UI, good mobile experience
- Strong appointment and documentation workflow integration

**Weaknesses that matter:**
- Primarily Canadian market focus; HIPAA compliance is present but not the primary design lens
- Insurance billing for US practices is cumbersome
- AI features are still early

**Sophrosync response:** Jane App sets the bar on template customization. Sophrosync should match Jane's template flexibility in Tier 3 and then extend it with AI-assisted drafting. For US-focused practices, lean into HIPAA-first positioning, which Jane App cannot credibly claim at the same level.

### Sophrosync Differentiation Summary

| Dimension | Competitor Gap | Sophrosync Position |
|-----------|---------------|---------------------|
| Sign vs. Lock distinction | SimplePractice conflates them | Two distinct actions with clinical rationale explained in UI |
| AI-assisted note drafting | None of the above offer it well | Build as Tier 3 differentiator; market it prominently |
| Overdue note visibility | TheraNest and SimplePractice are passive | Proactive dashboard widget with urgency tiers |
| Template quality and coverage | TheraNest (weak), SimplePractice (limited) | SOAP + DAP + BIRP + Free Form at launch; customization in Phase 2 |
| Onboarding speed | TherapyNotes is slow | Pre-built templates, smart defaults — under 30 minutes to first signed note |
| Audit trail visibility | All competitors hide it in admin panels | Surface the audit log on the note itself for therapist transparency |

---

## 9. User Stories

### Epic: Create and Manage Progress Notes

**US-N01 — Create note from appointment (Tier 1)**
As a therapist, after completing a session, I want to open a progress note for that appointment directly from the calendar view so that I can document the session while it is fresh.

Acceptance criteria:
- Clicking a past or current appointment in the calendar shows a "Add Note" (if no note exists) or "View Note" (if note exists) button
- Clicking "Add Note" opens the note form pre-filled with the appointment's client, date, time, and session format
- If a note already exists for this appointment, no second note can be created — the existing note is opened instead

**US-N02 — Select note template (Tier 1)**
As a therapist, I want to choose between SOAP, DAP, BIRP, and Free Form templates when creating a note so that I can document in the format that matches my clinical training and the session type.

Acceptance criteria:
- Template selection appears at the top of the note form before the content fields
- Switching templates prompts a warning if any content has already been entered: "Changing the template will clear the current content. Are you sure?"
- The therapist's last-used template is remembered and pre-selected on the next new note (localStorage or user preference)

**US-N03 — Autosave draft (Tier 1)**
As a therapist, I want my in-progress note to be saved automatically so that I do not lose clinical documentation if my browser closes or my session expires.

Acceptance criteria:
- The note autosaves every 30 seconds while the user is typing
- A "Last saved [timestamp]" indicator is visible on the note form
- If the user navigates away with unsaved changes, they receive a browser-standard "Leave page? Changes you made may not be saved" warning
- The note is retrievable in its last-saved state if the user returns to the appointment

**US-N04 — Sign a note (Tier 1)**
As a therapist, I want to sign my completed note so that I create an official record attesting to its accuracy and my authorship.

Acceptance criteria:
- The Sign button is visible on Draft notes only
- Clicking Sign opens a confirmation modal: "Sign note for [Client Name] on [Date]? This attests this is an accurate clinical record. Signed as: [Full Name]."
- Confirmation captures and persists: SignedAt timestamp, SignedByUserId, and SignedByFullName
- Signed notes display: "Signed by [Full Name] on [Date/Time timezone]"
- Validation is enforced on Sign: all required template sections must be non-empty
- The note transitions to Signed status; an audit event is written

**US-N05 — Lock a note (Tier 1)**
As a therapist, I want to lock my signed note so that it becomes a permanent, uneditable part of the clinical record.

Acceptance criteria:
- The Lock button is visible on Signed notes only
- Clicking Lock opens a two-step confirmation: "Locking is permanent. Only an addendum can be added after locking. Lock this note?"
- After locking, the edit form is replaced with a read-only view
- The note header displays a lock icon and "Locked by [Full Name] on [Date/Time]"
- The note transitions to Locked status; an audit event is written
- The note is included in the ReportingService NotesSigned count

**US-N06 — View all notes for a client (Tier 1)**
As a therapist, I want to see all notes for a specific client in one place so that I can review clinical history before a session.

Acceptance criteria:
- Client profile has a "Notes" tab
- Notes are listed chronologically, newest first, with date, note type, status badge, and first-line preview
- Clicking a note opens the full note in a side panel without navigating away from the client profile
- Notes can be filtered by status (All / Draft / Signed / Locked)

**US-N07 — View overdue notes dashboard widget (Tier 1)**
As a therapist, I want to see a list of sessions for which I have not yet locked a note so that I can prioritize documentation and stay compliant.

Acceptance criteria:
- The dashboard displays an "Incomplete Notes" widget
- The widget shows sessions from the past 14 days where no Locked note exists
- Sessions older than 72 hours are marked with a visual urgency indicator
- Clicking an item in the widget opens the note form for that appointment

**US-N08 — Add an addendum to a locked note (Tier 2)**
As a therapist, I want to add an addendum to a locked note so that I can correct or clarify the record without altering the original documentation.

Acceptance criteria:
- "Add Addendum" button is visible only on Locked notes
- Clicking it opens a text area within the note view (not a separate page)
- The addendum is saved with: author full name, timestamp, and content
- The addendum appears below the original note content with a clear visual separator
- The addendum is itself signed (author + timestamp) but does not go through the Lock workflow
- An audit event is written for the addendum creation

**US-N09 — Supervisor co-signature (Tier 2)**
As a supervisor, I want to co-sign a supervisee's note so that my oversight of their clinical work is documented.

Acceptance criteria:
- Supervisors (role: supervisor) can see notes for therapists they supervise
- A "Co-sign" action is available on Signed and Locked notes
- Co-signature captures: co-signer's full name, timestamp, and role
- Co-signature is displayed on the note: "Co-signed by [Name] ([Role]) on [Date/Time]"
- An audit event is written for the co-signature

**US-N10 — Export note as PDF (Tier 2)**
As a therapist, I want to export a locked note as a PDF so that I can respond to records requests and provide documentation for other providers.

Acceptance criteria:
- A "Export PDF" button is available on Locked notes
- The PDF includes: clinic name, therapist name, client name and date of birth, date of service, session format, note template type, all clinical content sections, signature line with timestamp, and any addenda
- The PDF opens in a new browser tab (not a silent download)
- The export action is logged in the audit trail

**US-N11 — Create a treatment plan (Tier 2)**
As a therapist, I want to create a treatment plan for a client so that the clinical goals guiding our work together are documented and accessible alongside session notes.

Acceptance criteria:
- Treatment plan is accessible from the client profile (separate from the Notes tab, or as a pinned item at the top)
- A plan includes: presenting problem, at least one treatment goal, planned interventions, plan start date, and next review date
- Goals have a status (In Progress / Achieved / Discontinued) and an optional progress percentage
- The plan has its own status: Draft / Active / Completed / Discontinued
- A client can have only one Active treatment plan at a time

---

## 10. Alignment with Sophrosync Architecture

### Domain Model Assessment

The existing `SessionNote` aggregate in the architecture spec is well-designed. Key observations:

| Aspect | Current Spec | PRD Requirement | Gap |
|--------|-------------|-----------------|-----|
| Note templates | `NoteTemplate` enum: SOAP, DAP, BIRP, Free | All four required for MVP | None — already correct |
| Status state machine | Draft, Signed, Locked | Required by PRD | None — already correct |
| Content encryption | AES-256-GCM via NoteContent value object | Red Line 1 compliance | None — already correct |
| Addenda | `NoteAddendum` owned entity with encryption | Tier 2 addendum workflow | Already modeled — build the command and UI |
| Sign metadata | `SignedAt`, `SignedByUserId` | Required | Present but `SignedByFullName` should be denormalized at signing time — do not rely on Keycloak lookup at render time |
| Lock metadata | `LockedAt` — present on SessionNote | Required | Note: `LockedByUserId` should also be captured, mirroring the SignedBy pattern. Check if it is present in the spec. |
| Author identity | `AuthorId` (Keycloak sub) | Required | Denormalize `AuthorFullName` at creation time for the same reason as above |
| Appointment link | `AppointmentId` | One-to-one constraint required | Present — enforce unique constraint at DB level |
| Risk notation | Not modeled | Tier 2 footer field | Future: add `RiskNotationJson` as a nullable encrypted column |
| CPT code | Not modeled | Tier 4 (BillingService dependency) | Defer — do not add to NotesService until BillingService is active |

### Commands Already in Spec vs. PRD Requirements

| Command | In Spec | PRD Tier | Notes |
|---------|---------|----------|-------|
| `CreateNoteCommand` | Yes | Tier 1 | Build now |
| `UpdateNoteDraftCommand` | Yes | Tier 1 | Build now; ensure idempotent autosave behavior |
| `SignNoteCommand` | Yes | Tier 1 | Build now; enforce required fields validation |
| `LockNoteCommand` | Yes | Tier 1 | Build now; check for Signed status precondition |
| `AddAddendumCommand` | Yes | Tier 2 | Build in first 90 days |
| `CreateTreatmentPlanCommand` | Yes | Tier 2 | Build in first 90 days |
| `UpdateGoalProgressCommand` | Yes | Tier 2 | Build in first 90 days |
| `UnlockNoteCommand` | Not in spec | Red Line 6 | Do NOT add this command |
| `DeleteNoteCommand` | Not in spec | GDPR only | Add soft-delete command for GDPR Art. 17 compliance; no UI path other than admin override |

### Inter-Service Contracts the Notes UI Depends On

| Dependency | What Notes Needs | Where It Comes From |
|------------|------------------|---------------------|
| Appointment context pre-fill | ClientId, TherapistId, StartsAt, EndsAt, SessionFormat | ScheduleService `GET /api/appointments/{id}` |
| Active diagnoses display | ICD codes and descriptions | ClientService `GET /api/clients/{id}` |
| Notification on overdue note | Trigger to NotificationService | NotesService calls `POST /internal/notifications/send` |
| Note completion rate | NotesSigned, NotesOverdue | `GET /internal/notes/summary` — already stubbed |
| Audit logging | Every sign, lock, addendum, view | NotesService calls AuditService `POST /internal/audit` |

### The ReportingService Notes Summary Endpoint

The existing stub at `GET /internal/notes/summary` returns:

```json
{
  "TotalAppointments": 0,
  "NotesCreated": 0,
  "NotesSigned": 0,
  "NotesOverdue": 0,
  "CompletionRate": 0.0,
  "PeriodStart": "...",
  "PeriodEnd": "..."
}
```

This shape is appropriate for the ReportingService `GetNoteCompletionRateQuery`. Once the NotesService domain is implemented, this endpoint should query against actual data. The `NotesOverdue` field requires a definition: a note is overdue when an appointment's `Status = Completed` and no `SessionNote` with `Status = Locked` exists for that `AppointmentId` and the appointment `EndsAt` is more than 72 hours ago.

---

## Appendix A — Glossary

| Term | Definition |
|------|-----------|
| Progress Note | A clinical note documenting a single therapy session |
| SOAP | Subjective / Objective / Assessment / Plan — note template format |
| DAP | Data / Assessment / Plan — simplified note template |
| BIRP | Behavior / Intervention / Response / Plan — note template |
| Draft | A note that has been started but not yet attested to by the therapist |
| Signed | A note that has been attested to by the author; the author takes professional responsibility |
| Locked | A note that is permanently immutable; the legal medical record |
| Addendum | A timestamped correction or clarification appended to a locked note |
| PHI | Protected Health Information — any data that identifies a patient and relates to their health |
| Overdue note | A session completed more than 72 hours ago with no locked note |
| Treatment Plan | A structured document describing the clinical goals and interventions for a client's care |
| Co-signature | A supervisor's attestation that they have reviewed and approve a supervisee's note |
| ROI | Release of Information — a clinical/legal process for sharing records with third parties |

---

*PRD Owner: Product Consultant*
*Next review: After NotesService domain layer implementation*
