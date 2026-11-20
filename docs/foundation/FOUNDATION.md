# Foundation — what exists

> Last updated: 2026-11-20 · Gate G0 in progress: backend auth slice done (Api); Web shell + BFF auth pages next.

## Docs & rules
| Item | Status |
|------|--------|
| Product decisions | `docs/product/vision.md` (D-01…D-18) |
| Open questions | `docs/product/open-questions.md` |
| Roadmap | `docs/roadmap/ROADMAP.md` (G0–G3) |
| Architecture | `docs/architecture/architecture.md` + ADR-0001 (Accepted), ADR-0002 (Open), ADR-0003 (Accepted) |
| Implementation guides (Accepted, D-18) | `docs/architecture/backend-guide.md`, `docs/architecture/frontend-guide.md` |
| Rules | `CLAUDE.md` + `.claude/rules/` |
| Repository | https://github.com/welkin68/movewell (branch `main`, first push 2026-11-20); `.gitattributes` CRLF |

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
| Area | What |
|------|------|
| Ports | `IMoveWellUnitOfWork`, `IIdentityService`, `ITokenService`, `ISmsSender`, `IOtpCodeGenerator`, `IOtpHasher` |
| Services | `OtpService` (send with resend 120 s + 5/hour per mobile limits; verify against latest challenge, attempt saved on failure) |
| Features/Auth | `StartRegistration`, `CompleteRegistration` (checks first, then OTP, one transaction), `SignIn`, `RefreshSession`, `SignOut`, `StartPasswordReset` (neutral for unknown numbers), `CompletePasswordReset` (revokes all sessions), `ChangePassword` |
| Errors | `AuthErrors` (auth.*, otp.*, sms.*, password.*), `ValidationCodes` (validator messages are codes) |
| Options | `OtpOptions` (length 6, lifetime 120 s, 5 attempts, resend 120 s, 5/hour) |

### Infrastructure (`src/MoveWell.Infrastructure`)
| Area | What |
|------|------|
| Persistence | `MoveWellDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` (audit, soft-delete filters, ignore events), design-time factory (LocalDB or `MOVEWELL_DESIGN_CONNECTION`), `MoveWellUnitOfWork`, configurations Users/Otp, `ValueObjectConverters` (Mobile, Slug) |
| Migrations | `InitialIdentityProfilesOtp` — AspNet* tables, `UserProfiles`, `OtpChallenges`; `AddRefreshTokens` — `RefreshTokens` (unique TokenHash, FamilyId, UserId+RevokedAt) |
| Repositories | `UserProfileRepository`, `OtpChallengeRepository` |
| Identity | `ApplicationUser`, `ApplicationRole`, Identity rules (password ≥ 8 + digit, lockout 5 / 15 min, confirmed phone required), `HttpCurrentUserService` (`sub` claim), `IdentityService`, `TokenService` (JWT HS256 15 min; refresh 30 days hashed, rotation, family revoke on reuse), `RefreshToken` table |
| Security / SMS | `OtpCodeGenerator` (CSPRNG), `OtpHasher` (HMAC-SHA256 with `Otp:HashSecret`), `NullSmsSender` (default), `DevSmsSender` (logs text; Development/Testing only), `SmsTexts` fa/en |
| Auth DI | `AddMoveWellJwtAuthentication` (bearer validation, policies AdminOnly / ContentEditor / Member); dev/test get random secrets per start, production requires `Jwt:SigningKey` + `Otp:HashSecret` |
| Seeding | `IdentitySeeder`: roles Admin/Content/Member; first Admin from `Admin:Seed:*` (user-secrets / env vars) with profile |
| Host | `DatabaseInitializer` (migrate when `Database:MigrateOnStartup`, then seed) |
| Logging | `SerilogSetup` (console + daily file 14 days, correlation id) |
| DI | `AddMoveWellInfrastructure` (Core + FluentResult validation mode, MediatR over Application, EF SQL Server with retry, Identity, health check `database` tagged `ready`) |

### Api (`src/MoveWell.Api`, http://localhost:5100)
Middleware: forwarded headers (loopback only) → correlation id → exception handler (ProblemDetails) → status pages → security headers → Serilog request log → authentication → authorization → rate limiter.
Endpoints: `/health/live`, `/health/ready`; `/api/v1/auth/` `register/start`, `register/complete`, `signin`, `refresh`, `signout`, `password/reset/start`, `password/reset/complete`, `password/change` (Member).
Errors: `ResultHttpExtensions` → ProblemDetails `errors: [{code, message}]`, message from `Localization/Errors/errors.{fa|en}.json` by `Accept-Language`; 400/401/403/404/409/429 (+ Retry-After). Rate limits per IP: auth 10/min, OTP send 20/hour (`RateLimits:*`). User-secrets id `movewell-api-dev`. Dev DB: LocalDB `MoveWell_Dev` (auto-migrate in Development only).

### Shared
`MoveWell.Common`: `Identity/Roles`, `Identity/Policies`, `Contracts/Auth` (requests/responses), `Contracts/Common/ApiError`. Localization/Globalization projects still empty.

### Web (`src/MoveWell.Web`)
Still the `mvc` template, with template Bootstrap/jQuery removed (frontend-guide §7) — shell comes after the auth slice.

### Tests
| Project | Count |
|---------|-------|
| Domain.Tests | 65 green (MobileNumber, Slug, LocalizedText, Culture, OtpChallenge, UserProfile) |
| ArchitectureTests | 3 green |
| SmokeTests | 14 green — full auth flows over HTTP on a throw-away LocalDB database (register, normalization, conflict fa, wrong code, weak password, resend 429, wrong password en, refresh rotation + reuse, sign-out, reset, change password) |
| Application.Tests | empty |

## Build notes
- Security pin `System.Security.Cryptography.Pkcs 10.0.15` in `Directory.Build.props`; remaining NU1902/NU1903 only from `lib/CookieBytes.Core` (fix upstream).
- Verified 2026-11-20: Api starts, applies migration on LocalDB, seeds 3 roles, `/health/live` and `/health/ready` → 200, correlation id echoed.

## Not yet
JWT + refresh tokens; auth commands/endpoints; SMS adapters; Web shell/theme/localization; smoke tests. Host/domain not bought (D-07).
