# Foundation — what exists

> Last updated: 2026-11-20 · Gate G0 in progress: skeleton + domain building blocks + persistence + Api host done; auth slice next.

## Docs & rules
| Item | Status |
|------|--------|
| Product decisions | `docs/product/vision.md` (D-01…D-18) |
| Open questions | `docs/product/open-questions.md` |
| Roadmap | `docs/roadmap/ROADMAP.md` (G0–G3) |
| Architecture | `docs/architecture/architecture.md` + ADR-0001 (Accepted), ADR-0002 (Open), ADR-0003 (Accepted) |
| Implementation guides (Accepted, D-18) | `docs/architecture/backend-guide.md`, `docs/architecture/frontend-guide.md` |
| Rules | `CLAUDE.md` + `.claude/rules/` |

## Solution (`MoveWell.sln`, .NET 10, `global.json` ≥ 10.0.400)

### Domain (`src/MoveWell.Domain`)
| Area | Types |
|------|-------|
| Errors | `DomainError` (stable `Code` = localization key + `Kind`), `ErrorKind` (Validation/NotFound/Conflict/Forbidden), `DomainErrors` catalogue (text, slug, mobile, otp, profile) |
| Enumerations | `Culture` (fa/en, `FromCode`), `PublishState`, `AccessLevel` |
| Value objects | `LocalizedText` (FA+EN, `IsComplete`, `For(culture)` with FA fallback), `Slug` (`Create`, `FromText`), `MobileNumber` (Persian/Arabic digits, +98/0098/98 prefixes → `09xxxxxxxxx`, `Masked`) |
| Aggregates | `UserProfile` (1:1 user, display name 2–60, culture, terms accepted at), `OtpChallenge` (+ `OtpPurpose`; hashed code, TTL, attempts, `Verify` → `Consume`) |
| Repository ports | `IUserProfileRepository`, `IOtpChallengeRepository` |

### Application (`src/MoveWell.Application`)
`Abstractions/Persistence/IMoveWellUnitOfWork` (UserProfiles, OtpChallenges). No features yet.

### Infrastructure (`src/MoveWell.Infrastructure`)
| Area | What |
|------|------|
| Persistence | `MoveWellDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` (audit, soft-delete filters, ignore events), design-time factory (LocalDB or `MOVEWELL_DESIGN_CONNECTION`), `MoveWellUnitOfWork`, configurations Users/Otp, `ValueObjectConverters` (Mobile, Slug) |
| Migrations | `20261120133856_InitialIdentityProfilesOtp` — AspNet* tables, `UserProfiles` (FK cascade to user, unique UserId), `OtpChallenges` (index Mobile+PurposeId+CreatedAt) |
| Repositories | `UserProfileRepository`, `OtpChallengeRepository` |
| Identity | `ApplicationUser`, `ApplicationRole`, Identity rules (password ≥ 8 + digit, lockout 5 / 15 min, confirmed phone required), `HttpCurrentUserService` (`sub` claim) |
| Seeding | `IdentitySeeder`: roles Admin/Content/Member; first Admin from `Admin:Seed:*` (user-secrets / env vars) with profile |
| Host | `DatabaseInitializer` (migrate when `Database:MigrateOnStartup`, then seed) |
| Logging | `SerilogSetup` (console + daily file 14 days, correlation id) |
| DI | `AddMoveWellInfrastructure` (Core + FluentResult validation mode, MediatR over Application, EF SQL Server with retry, Identity, health check `database` tagged `ready`) |

### Api (`src/MoveWell.Api`, http://localhost:5100)
Middleware order: correlation id → exception handler (ProblemDetails) → security headers → Serilog request log. Endpoints: `/health/live`, `/health/ready`. User-secrets id `movewell-api-dev`. Dev DB: LocalDB `MoveWell_Dev` (auto-migrate in Development only).

### Shared
`MoveWell.Common/Identity`: `Roles`, `Policies`. Localization/Globalization projects still empty.

### Web (`src/MoveWell.Web`)
Still the `mvc` template — shell comes after the auth slice.

### Tests
| Project | Count |
|---------|-------|
| Domain.Tests | 65 green (MobileNumber, Slug, LocalizedText, Culture, OtpChallenge, UserProfile) |
| ArchitectureTests | 3 green |
| Application.Tests, SmokeTests | empty |

## Build notes
- Security pin `System.Security.Cryptography.Pkcs 10.0.15` in `Directory.Build.props`; remaining NU1902/NU1903 only from `lib/CookieBytes.Core` (fix upstream).
- Verified 2026-11-20: Api starts, applies migration on LocalDB, seeds 3 roles, `/health/live` and `/health/ready` → 200, correlation id echoed.

## Not yet
Git; JWT + refresh tokens; auth commands/endpoints; SMS adapters; Web shell/theme/localization; smoke tests. Host/domain not bought (D-07).
