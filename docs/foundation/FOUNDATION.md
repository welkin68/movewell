# Foundation — what exists

> Last updated: 2026-11-21 · **G0 closed** (exit criteria met). Current gate: G1 — exercise library (Admin), waiting on Q-05, Q-06, Q-09, Q-13.

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
| Project | What |
|---------|------|
| `MoveWell.Common` | `Identity/Roles`, `Identity/Policies`, `Cultures/Cultures` (fa/en, `/en` prefix), `Contracts/Auth`, `Contracts/Common/ApiError` |
| `MoveWell.Shared.Globalization` | `CulturePrefixMiddleware` (`/en` → PathBase, so routes exist once and links keep the language), `PathBaseRequestCultureProvider`, `CulturePath.For` (language switch/hreflang), `UseMoveWellCultures` |
| `MoveWell.Shared.Localization` | JSON `IStringLocalizer` (`Resources/{area}/{page}/{name}.{fa\|en}.json`), `AddMoveWellJsonLocalization` |

### Web (`src/MoveWell.Web`, http://localhost:5200) — BFF
| Area | What |
|------|------|
| Host | Serilog (`logs/movewell-web-.log`), correlation id (sent to Api), security headers + strict CSP (`'self'` only), status pages `/error/{code}`, Persian output not entity-encoded |
| Api access | One named HttpClient `MoveWellApi` (`Api:BaseUrl`) + `ApiContextHandler` (Accept-Language, X-Correlation-Id, X-Forwarded-For); `ApiClientBase` → `ApiResult` with the Api's localized `{code, message}` errors; `AuthApiClient` |
| Sessions | Cookies `.movewell.member` and `.movewell.staff`; Api tokens stored inside the encrypted cookie; `SessionRefresher` renews the access token 60 s before expiry or signs out; Data Protection keys persisted (`DataProtection:KeysPath`, default `App_Data/keys`) |
| Public pages | Home (hero, how it works, library "coming soon"), `/account/register` → `/account/verify` (+ resend with countdown), `/account/signin`, `/account/signout`, `/account/forgot` → `/account/reset`, `/account` (my account). Pending mobile kept in TempData (never in URL). All under `/en` too |
| Admin | `/admin/account/signin` (Admin/Content only; member accounts refused and their token revoked), `/admin` dashboard shell (sidebar with G1 items marked "soon"), area route |
| UI system | Calm Clinic tokens (`css/shared/tokens.css`), base (logical properties, layers), components (header, bottom nav, footer, buttons, cards, forms, OTP field, alerts, toast), page CSS, admin shell CSS; Vazirmatn local; SVG sprite + placeholder mark + hero illustration; `js/shared/ui.js` (password toggle, OTP digits, resend countdown, toast, double-submit guard) |
| Texts | `Resources/public/{shared,home,account,error}`, `Resources/admin/shared` — fa + en |
| Dev run | `.claude/launch.json` (api :5100, web :5200) |

### Tests
| Project | Count |
|---------|-------|
| Domain.Tests | 65 green (MobileNumber, Slug, LocalizedText, Culture, OtpChallenge, UserProfile) |
| ArchitectureTests | 3 green |
| SmokeTests | 28 green on a throw-away LocalDB database. Api auth (14): register, normalization, conflict fa, wrong code, weak password, resend 429, wrong password en, refresh rotation + reuse, sign-out, reset, change password. Web (14, Web → in-memory Api): fa RTL / en LTR home, `/en` links, no external assets + CSP on 5 pages, localized 404, registration through HTML forms, password mismatch, Api error in page language, admin staff sign-in (seeded test admin), member refused by admin, sign-out |
| Application.Tests | empty |

## Build notes
- Security pin `System.Security.Cryptography.Pkcs 10.0.15` in `Directory.Build.props`; remaining NU1902/NU1903 only from `lib/CookieBytes.Core` (fix upstream).
- Verified 2026-11-20: Api starts, applies migration on LocalDB, seeds 3 roles, `/health/live` and `/health/ready` → 200, correlation id echoed.

## Not yet
Exercise library (G1); real SMS provider (Q-12); profile editing; first real Admin account (set `Admin:Seed:*` with user-secrets on the Api); brand identity (placeholder mark in use, D-10). Host/domain not bought (D-07).
