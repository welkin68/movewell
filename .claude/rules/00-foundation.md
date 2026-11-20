# Foundation (always)

- .NET 10, Clean Architecture + DDD + CQRS + domain events. `lib/CookieBytes.Core` is physical source, ProjectReference only — never NuGet.
- Dependency direction: **Web → Api (HTTP)** · **Api → Infrastructure → Application → Domain** (ADR-0001). Web never references Application/Infrastructure/Domain.
- Api is the only door to data and must stay mobile-ready (D-08): versioned `/api/v1`, DTO contracts in `shared/MoveWell.Common`.
- **Education only (D-02):** no diagnosis, no assessment, no personalised rule ("user has X → exercise Y"). Cautions are generic educational text.
- **Free at launch (D-03):** `AccessLevel` exists on content, checked in Api queries; no payment/checkout code before its gate.
- Auth: mobile + password, SMS OTP through `ISmsSender` (D-09, D-13, ADR-0003). Person data lives in `UserProfile` (1:1), never on `ApplicationUser`.
- Dates stored UTC; Jalali only in presentation. Money (later) as `long` rials.
- Static assets local under `wwwroot/` — no CDN (Iran hosting).
- FA + EN at launch (D-12): every user-facing string and every content field exists in both languages.
- Out of MVP: assessment, programs, chat, payments. Don't build ahead of the roadmap.
- After a gate ships: update `docs/foundation/FOUNDATION.md` and tick `docs/roadmap/ROADMAP.md`.
