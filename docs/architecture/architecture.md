# Architecture

> v0.1 · 2026-11-20 · Same family as YildizFurnish (`B:\project\yildiz`), right-sized for MoveWell.

## Stack
| Layer | Choice |
|-------|--------|
| Runtime | .NET 10 / ASP.NET Core 10 |
| Style | Clean Architecture + DDD + CQRS + Domain Events |
| Core | `lib/CookieBytes.Core` physical source, ProjectReference only (never NuGet) |
| Mediation / validation / results | MediatR · FluentValidation · FluentResults |
| ORM / DB | EF Core 10 · SQL Server (LocalDB on dev, SQL Server on Iran VPS) |
| Logging | Serilog (console + rolling file), correlation id |
| Auth | ASP.NET Core Identity — mobile number + password, SMS OTP via `ISmsSender` (ADR-0003) |
| UI | Razor SSR (MVC) + `/Admin` area, RTL, Vazirmatn self-hosted, no CDN |
| Culture | `fa-IR` default (RTL, `/`) + `en-US` (LTR, `/en`) at launch (D-12) |

## Solution shape
```text
MoveWell/
├── src/
│   ├── MoveWell.Domain/          Aggregates, VOs, events, repository ports
│   ├── MoveWell.Application/     Features/{Feature}/Commands|Queries, DTOs, event handlers
│   ├── MoveWell.Infrastructure/  EF, repositories, Identity, media storage, seeding
│   ├── MoveWell.Api/             REST /api/v1 — the only door to data (Web today, mobile later)
│   └── MoveWell.Web/             Razor public site + /Admin; talks to Api over HTTP
├── shared/
│   ├── MoveWell.Common/          constants, roles/policies, API contracts (DTO shapes)
│   ├── MoveWell.Shared.Localization/
│   └── MoveWell.Shared.Globalization/
├── lib/
│   ├── CookieBytes.Core/
│   ├── CookieSeo/  CookieMenus/  CookieConsent/   (G2)
├── tests/
│   ├── MoveWell.Domain.Tests/  MoveWell.Application.Tests/
│   ├── MoveWell.ArchitectureTests/  MoveWell.SmokeTests/
└── docs/
```
Dependency direction: **Web → Api (HTTP)** · **Api → Infrastructure → Application → Domain**. Domain references only CookieBytes.Core.

## Domain map (MVP)
| Aggregate | Purpose |
|-----------|---------|
| `Exercise` | Title, slug, summary, steps (ordered), common mistakes, educational cautions, media refs, taxonomy refs, `AccessLevel` (Free/Premium — always Free in MVP), `PublishState` |
| `BodyRegion` · `PosturalIssue` · `Equipment` | Taxonomy managed in Admin (slug, title, description, order) |
| `ExerciseType` · `Level` | `Enumeration` types (fixed lists) |
| `MediaAsset` | Uploaded image/video metadata; storage behind `IMediaStorage` port (ADR-0002) |
| `UserProfile` | 1:1 with Identity user (display name, etc.) — person data never on `ApplicationUser` |

Later (not MVP): `Collection`, `Article`, `Plan`, `Subscription`, `Payment`.

## Laws
- No medical logic anywhere (D-02): no rule like "user has X → show/hide exercise Y".
- Premium is a domain property (`AccessLevel`), enforced in Api queries — never only hidden in UI.
- Web holds no business logic and never touches DbContext/Domain.
- Handlers use `IMoveWellUnitOfWork`, never DbContext; `SaveAndDispatchEventsAsync()` once per command.

## Detailed guides
- `backend-guide.md` — every layer from CookieBytes.Core to IIS deploy: classes, methods, tables, endpoints.
- `frontend-guide.md` — UI direction, design tokens, components, pages, admin, frontend tech.

## Decisions
See `decisions/`.
