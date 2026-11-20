# MoveWell — Product vision & decisions

> v0.1 · 2026-11-20 · Every line marked **[Decided]** came from the user. Anything not here is unknown → see `open-questions.md`.

## What MoveWell is
A website that teaches **corrective exercises** (حرکات اصلاحی) and **general exercises**: what each movement is, which body region/issue it relates to, and how to perform it correctly.

## Decided

| # | Decision | Date |
|---|----------|------|
| D-01 | **[Decided]** MVP is about **knowing and learning movements** (exercise library + education). | 2026-11-20 |
| D-02 | **[Decided]** MoveWell gives **no therapy and no medical advice**. No diagnosis, no assessment, no prescribed programs in MVP. | 2026-11-20 |
| D-03 | **[Decided]** Business model: **monthly subscription** later; the site is **free at launch** until it gains traction. | 2026-11-20 |
| D-04 | **[Decided]** Content is authored by **one specialist/coach**. No marketplace of coaches. | 2026-11-20 |
| D-05 | **[Decided]** No coach↔member communication (no chat, no video review, no visits). | 2026-11-20 |
| D-06 | **[Decided]** Exercise videos are **not ready yet**. | 2026-11-20 |
| D-07 | **[Decided]** Hosting: **Windows Server + IIS + SQL Server in Iran**. Nothing bought (host/domain) until MVP is complete. | 2026-11-20 |
| D-08 | **[Decided]** A **mobile app is planned** → the REST Api must be usable by a mobile client, not only by the Web. | 2026-11-20 |
| D-09 | **[Decided]** Sign-in: **mobile number + password**, **no SMS OTP**. | 2026-11-20 |
| D-10 | **[Decided]** Product name: **MoveWell**. No visual identity / brand design exists yet. | 2026-11-20 |
| D-11 | **[Decided]** No deadline for now. | 2026-11-20 |
| D-12 | **[Decided]** Languages: **Persian + English at launch**. `fa-IR` default (RTL, root `/`), `en-US` under `/en/...`. | 2026-11-20 |
| D-13 | **[Decided]** **SMS OTP is enabled** (replaces the "no OTP" part of D-09). Sign-in itself stays mobile + password. OTP is used to **verify the mobile at registration** and for **password reset** (Q-11). | 2026-11-20 |
| D-14 | **[Decided]** The specialist and the coach are **one person** → no author/reviewer split; the same staff user drafts and publishes. | 2026-11-20 |
| D-15 | **[Decided]** UI design direction: **A "Calm Clinic"** — light and airy, teal primary `#0F766E`, coral accent `#F97362`, rounded cards, line illustrations (frontend-guide §2). | 2026-11-20 |
| D-16 | **[Decided]** Auth transport: the **Api issues JWT access + rotating refresh tokens** for Web and the future mobile app; **Web is a BFF** that keeps the tokens in its own encrypted HttpOnly cookie (ADR-0003, backend-guide §4.4/§6.1). | 2026-11-20 |
| D-17 | **[Decided]** **Light theme only** at launch; dark mode later (tokens make it a small add). | 2026-11-20 |
| D-18 | **[Decided]** ADR-0001 (Api + Web split), `backend-guide.md` and `frontend-guide.md` accepted as the build contract; G0 coding started. | 2026-11-20 |

## Consequences we derive (not new decisions)
- Because of D-02, every exercise page shows an **educational disclaimer** and exercises carry **cautions** as plain educational text — never personalised warnings.
- Because of D-03, the domain must be able to mark content as free/premium later **without a rewrite**; the paywall itself is not built in MVP.
- Because of D-08, auth must be able to grow from cookies (Web) to tokens (mobile) — see ADR-0003.

## Out of MVP (user decision)
Assessment, personalised programs, coach panel for clients, chat, payments/subscription checkout.
