# Roadmap (gates)

> v0.2 · 2026-11-20 · v0.2: FA+EN at launch, OTP enabled, one staff person · A gate closes only when its exit criteria are met. **Current gate: G1** (G0 closed 2026-11-21).

## MVP = G0 → G3
A guest can browse and learn exercises; the specialist manages the library from Admin; members can register with mobile + password. Free for everyone (D-03).

### G0 — Decisions & skeleton
- [x] Answer Q-01, Q-02, Q-04 (D-12, D-13, D-14)
- [x] Answer Q-11 (OTP at registration + reset)
- [x] Accept ADR-0003 (auth, D-16: Api tokens + Web BFF)
- [x] Accept ADR-0001 (Api + Web split), backend-guide + frontend-guide (D-18)
- [x] Answer Q-14 (D-15: Calm Clinic)
- [x] Answer Q-16 (D-17: light only at launch)
- [x] Solution `MoveWell.sln`: Domain, Application, Infrastructure, Api, Web + `shared/` + tests
- [x] `lib/CookieBytes.Core` copied as physical source (ProjectReference only) from the yildiz copy
- [x] `Directory.Build.props` (net10.0, nullable, shared versions), `global.json`
- [x] Serilog + correlation id, global error handling (ProblemDetails), health check (`/health/live`, `/health/ready`) — Api
- [x] SQL Server (LocalDB on dev), first migration `InitialIdentityProfilesOtp`, role seeding (+ first Admin when configured)
- [x] Web shell: RTL/LTR, Vazirmatn local, Calm Clinic tokens, `fa-IR` default + `en-US` under `/en` (D-12), shared JSON localization
- [x] Identity: Admin + Content roles (staff), Member role; mobile + password; 1:1 `UserProfile`; JWT access + rotating refresh tokens (Api)
- [x] SMS OTP via `ISmsSender` (Dev adapter logs code): registration verification + password reset (Api)
- [x] `ArchitectureTests` (layer dependency checks) — 3 green
- [x] Smoke tests for Api auth (14, WebApplicationFactory + LocalDB)
- [x] BFF account pages (register, verify, sign in/out, forgot, reset, my account) + admin sign-in/dashboard shell; Web smoke tests (14)

**Exit:** `dotnet build` green; Api + Web start locally; staff can sign in to `/Admin`; a member can register/sign in with mobile + password; smoke tests green. Met 2026-11-21 (96 tests green; also checked in the browser).

### G1 — Exercise library (Admin)
- [ ] Answer Q-05, Q-06, Q-09, Q-13, (Q-07)
- [ ] Taxonomy: `BodyRegion`, `PosturalIssue`, `Equipment`, `ExerciseType`, `Level`
- [ ] `Exercise` aggregate: FA + EN content, steps, common mistakes, educational cautions, media, taxonomy links, `AccessLevel` (always Free in MVP), draft/publish
- [ ] Media: image upload + video via the ADR-0002 choice
- [ ] Admin CRUD + publish flow for the specialist

**Exit:** the specialist enters 10 real exercises with media and publishes them from Admin.

### G2 — Public site
- [ ] Answer Q-03, Q-08, Q-10
- [ ] Home, library browse with filters (region / issue / equipment / level), search
- [ ] Exercise detail page (video, steps, mistakes, cautions, disclaimer, related exercises)
- [ ] Postural-issue landing pages (educational)
- [ ] SEO: CookieSeo (meta, OG, sitemap, robots), JSON-LD; CookieMenus; CookieConsent
- [ ] Mobile-first layout; Lighthouse ≥ 90 mobile on detail page
- [ ] Member area per Q-03

**Exit:** a guest on a phone finds an exercise by issue and plays its video; pages pass the SEO smoke pack.

### G3 — Launch
- [ ] Buy host + domain (D-07); SMS provider account (Q-12), IIS deploy, SQL Server prod, HTTPS, backups + one tested restore
- [ ] Production config secrets outside repo; error logging monitored
- [ ] Content: ≥ N exercises live (N decided by the user)
- [ ] User sign-off

**Exit:** site live on the real domain; backup restored once successfully.

## After MVP (order to be decided)
- **Subscription** (D-03): plans, payment gateway (CookiePayment module candidate), premium flag on exercises/videos, protected video.
- **Mobile app** (D-08): token auth on Api, same endpoints.
- Collections/routines, articles, favorites/history (if not in MVP per Q-03/Q-07/Q-08).
