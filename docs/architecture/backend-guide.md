# Backend implementation guide — from the core outward

> v0.1 · 2026-11-20 · Status: **Accepted (D-18)** — build contract for G0–G3.
> Read with `architecture.md`, ADR-0001…0003 and `.claude/rules/`.

This document walks through **every layer, from the innermost to the outermost**, and lists every class, method and endpoint we plan to build for the MVP. Names here are the names that will appear in code.

---

## 0. The big picture in two journeys

Before going layer by layer, follow two real requests end to end. Every later section is one stop on these journeys.

### Journey A — a guest opens an exercise page (read)

```text
Browser (phone)  GET https://movewell.ir/exercises/chin-tuck
  │
  ▼
IIS site "MoveWell.Web"  (ASP.NET Core Module → Kestrel)
  │  middleware: forwarded headers → culture from URL (fa) → static files → routing
  ▼
Web: ExercisesController.Detail("chin-tuck")
  │  calls IExercisesApiClient.GetPublishedBySlugAsync("chin-tuck", "fa")
  ▼  HTTP GET http://localhost:5100/api/v1/exercises/chin-tuck   (Accept-Language: fa-IR)
IIS site "MoveWell.Api"
  │  middleware: correlation id → security headers → request log → auth → rate limit
  ▼
Api endpoint  MapGet("/api/v1/exercises/{slug}")  → mediator.Send(new GetPublishedExerciseBySlugQuery(slug))
  ▼
Application: pipeline LoggingBehavior → ValidationBehavior → GetPublishedExerciseBySlugQueryHandler
  │  calls IExerciseReadRepository.GetPublishedBySlugAsync(slug)  (projection, AsNoTracking)
  ▼
Infrastructure: EF Core → SQL Server  SELECT … FROM Exercises WHERE Slug=@s AND PublishState=2
  ▲
  └─ result flows back: entity row → ExerciseDetailDto → JSON → Web view model → Razor view → HTML
```

### Journey B — the specialist publishes an exercise (write)

```text
Admin form POST /admin/exercises/{id}/publish  (Web, staff cookie)
  → Web AdminExercisesController.Publish(id) → IAdminExercisesApiClient.PublishAsync(id) with Bearer token
  → Api POST /api/v1/admin/exercises/{id}/publish  [policy ContentEditor]
  → PublishExerciseCommand → validator (id not empty) → handler:
        exercise = await uow.Exercises.FindByIdAsync(id)
        result   = exercise.Publish(clock.UtcNow)      ← domain checks the rules (has FA+EN title, has video…)
        if failed → return Result.Fail (400 ProblemDetails with messages)
        await uow.SaveAndDispatchEventsAsync()         ← one SQL transaction, then ExercisePublishedEvent
  → ExercisePublishedEventHandler → invalidate sitemap/home caches
  → Api 200 { exercise dto } → Web redirects back with a success toast
```

**The rule that makes all of this work:** each layer only talks to the layer directly inside it, and business rules live in exactly one place — the Domain.

---

## 1. Layer 0 — CookieBytes.Core (already in `lib/`)

We do not write this; we use it. What each piece gives us:

| Core type | Where | What it does for us |
|-----------|-------|---------------------|
| `Entity` / `Entity<TId>` | Domain/SeedWork | `Id` (Guid generated in ctor), `CreatedAt/By`, `UpdatedAt/By`, identity-based equality. **Never redeclare these.** |
| `AggregateRoot` | Domain/SeedWork | Entity + `DomainEvents` list, `AddDomainEvent()`, hooks `OnValidate/OnBeforeCreate/OnAfterCreate`. |
| `RowVersionedEntity` | Domain/SeedWork | `RowVersion` for optimistic concurrency (two admin tabs editing the same exercise). |
| `SoftDeletableEntity` | Domain/SeedWork | `IsDeleted/DeletedAt/DeletedBy` + global query filter applied by `BaseDbContext`. |
| `ValueObject` | Domain/SeedWork | Equality by components (`GetEqualityComponents()`). |
| `Enumeration` | Domain/SeedWork | "Smart enum": `Id` + `Name`, `FromValue<T>()`, `GetAll<T>()`. Used instead of C# `enum`. |
| `IRepository<T>` | Domain/Interfaces | `AddAsync, UpdateAsync, RemoveAsync, FindByIdAsync, FindAsync(predicate), AnyAsync, CountAsync…` |
| `IUnitOfWork` | Domain/Interfaces | `SaveAsync`, **`SaveAndDispatchEventsAsync`**, transactions. |
| `BaseDbContext` | Infrastructure | On save: fills audit fields from `ICurrentUserService` + `IDateTimeProvider`; soft-delete filters; ignores `DomainEvents`. *(We need Identity tables, so our context inherits `IdentityDbContext` and copies these conventions — §4.3.)* |
| `UnitOfWork<TDbContext>` | Infrastructure | Collects aggregates with events → `SaveChanges` → dispatches events **after** a successful save. |
| `Repository<T>` | Infrastructure | Generic EF implementation of `IRepository<T>`. |
| `EntityConfigurationBase<T,TId>` | Infrastructure | Maps Id + audit columns. Our configs inherit it and call `base.Configure(builder)` first. |
| `EnumerationConverter` | Infrastructure | Stores an `Enumeration` as its int `Id` column. |
| `LoggingBehavior`, `ValidationBehaviorFluentResult` | Application/Results | MediatR pipeline: log every request; run FluentValidation and return `Result.Fail` instead of throwing. |
| `PagedQuery`, `PagedResult<T>`, `SortDirection` | Application/Common | Paging contracts for list queries. |
| `Specification<T>` + `SpecificationEvaluator` | Application/Infrastructure | Reusable query filters (we use them for admin lists). |
| `AddCookieBytesCore()`, `AddCookieBytesCoreResults()` | DI | One call registers MediatR, behaviours, dispatcher, default services. |

**Validation mode:** we register Core with `ValidationMode = FluentResult` → invalid input becomes `Result.Fail(...)`, not an exception. One consistent error path from handler to HTTP 400.

---

## 2. Layer 1 — Domain (`src/MoveWell.Domain`)

Pure C#. No EF, no HTTP, no MediatR handlers. This is where **business rules** live.

### 2.1 Folder map

```text
MoveWell.Domain/
├── AssemblyReference.cs
├── Shared/
│   ├── ValueObjects/      LocalizedText.cs, Slug.cs, MobileNumber.cs, MediaRef.cs
│   ├── Enumerations/      PublishState.cs, AccessLevel.cs, MediaKind.cs, Culture.cs
│   └── Errors/            DomainErrors.cs   (error codes + messages, FA/EN keys)
├── Aggregates/
│   ├── Exercises/         Exercise.cs, Entities/ExerciseStep.cs, Entities/ExerciseTip.cs,
│   │                      Enumerations/ExerciseType.cs, Enumerations/Level.cs, TipKind.cs,
│   │                      Events/ExerciseCreatedEvent.cs, ExercisePublishedEvent.cs, ExerciseUnpublishedEvent.cs
│   ├── BodyRegions/       BodyRegion.cs
│   ├── PosturalIssues/    PosturalIssue.cs
│   ├── Equipments/        Equipment.cs
│   ├── MediaAssets/       MediaAsset.cs, Events/MediaAssetUploadedEvent.cs
│   ├── UserProfiles/      UserProfile.cs
│   └── OtpChallenges/     OtpChallenge.cs, Enumerations/OtpPurpose.cs
└── Repositories/
    ├── Exercises/         IExerciseRepository.cs
    ├── Taxonomy/          IBodyRegionRepository.cs, IPosturalIssueRepository.cs, IEquipmentRepository.cs
    ├── Media/             IMediaAssetRepository.cs
    ├── Users/             IUserProfileRepository.cs
    └── Otp/               IOtpChallengeRepository.cs
```

### 2.2 Shared value objects (the small bricks)

**`LocalizedText`** — the key to D-12 (FA + EN at launch). Every user-facing text field is one of these.
```csharp
public sealed class LocalizedText : ValueObject
{
    public string Fa { get; private set; } = string.Empty;
    public string En { get; private set; } = string.Empty;

    private LocalizedText() { }                       // EF
    private LocalizedText(string fa, string en) { Fa = fa; En = en; }

    public static Result<LocalizedText> Create(string? fa, string? en, int maxLength, bool required)
    {
        fa = fa?.Trim() ?? ""; en = en?.Trim() ?? "";
        if (required && fa.Length == 0) return Result.Fail(DomainErrors.Text.FaRequired);
        if (fa.Length > maxLength || en.Length > maxLength) return Result.Fail(DomainErrors.Text.TooLong(maxLength));
        return Result.Ok(new LocalizedText(fa, en));
    }

    public string For(Culture culture) => culture == Culture.En && En.Length > 0 ? En : Fa;   // fallback to FA
    public bool IsComplete => Fa.Length > 0 && En.Length > 0;                                   // used by Publish()
    protected override IEnumerable<object?> GetEqualityComponents() { yield return Fa; yield return En; }
}
```
Why a value object and not two columns everywhere by hand: one place for trimming, length, fallback and "is it complete" — the publish rule can simply ask `Title.IsComplete`.

**`Slug`** — URL part (`chin-tuck`). Rules: lowercase latin, digits, hyphen, 3–120 chars; `Slug.FromText(string)` builds one from the English title. English slugs for both languages (`/exercises/chin-tuck` and `/en/exercises/chin-tuck`) — simpler routing and stable URLs.

**`MobileNumber`** — `Create("۰۹۱۲ ۳۴۵ ۶۷۸۹")` → normalizes Persian/Arabic digits to latin, strips spaces/`+98`/`0098`, validates `^09\d{9}$`, stores `09123456789`. Used as the Identity user name and for SMS.

**`MediaRef`** — `MediaAssetId` + `Kind`; how an exercise points to an uploaded file without owning the file.

### 2.3 Enumerations

| Enumeration | Values |
|-------------|--------|
| `PublishState` | Draft(1), Published(2), Archived(3) |
| `AccessLevel` | Free(1), Premium(2) — MVP always Free (D-03) |
| `ExerciseType` | Stretch, Strengthening, Mobility, Stability/Balance, Breathing… (final list Q-06) |
| `Level` | Beginner, Intermediate, Advanced |
| `TipKind` | CommonMistake, Caution, Cue |
| `MediaKind` | Image, Video, Thumbnail |
| `OtpPurpose` | Registration, PasswordReset |
| `Culture` | Fa, En |

### 2.4 The `Exercise` aggregate (the heart of the product)

**State**
| Property | Type | Notes |
|----------|------|-------|
| `Title` | `LocalizedText` | required FA, max 150 |
| `Slug` | `Slug` | unique |
| `Summary` | `LocalizedText` | one-paragraph intro, max 500 |
| `Description` | `LocalizedText` | longer educational text, max 4000 |
| `Type` | `ExerciseType` | |
| `Level` | `Level` | |
| `Access` | `AccessLevel` | Free in MVP |
| `State` | `PublishState` | Draft → Published ↔ Draft, → Archived |
| `PublishedAt` | `DateTime?` | UTC |
| `Steps` | `IReadOnlyList<ExerciseStep>` | ordered `Order`, `Text: LocalizedText` |
| `Tips` | `IReadOnlyList<ExerciseTip>` | `Kind` + `Text` (mistakes, cautions, cues) |
| `VideoId` / `ThumbnailId` | `Guid?` | → `MediaAsset` |
| `ImageIds` | list | gallery |
| `BodyRegionIds`, `PosturalIssueIds`, `EquipmentIds` | `IReadOnlyList<Guid>` | taxonomy links (many-to-many) |
| `RelatedExerciseIds` | list | "see also" |
| `SeoTitle`, `SeoDescription` | `LocalizedText` | optional overrides |
| `RowVersion` | via base | concurrency |

Fields like sets/reps/duration/muscles wait for Q-09.

**Methods** (each returns `Result`, never throws for business rules):
| Method | Rules it enforces |
|--------|-------------------|
| `static Create(title, slug, type, level)` | title FA required; slug valid. Starts as Draft, Free. Raises `ExerciseCreatedEvent`. |
| `UpdateContent(title, summary, description)` | not Archived. |
| `ChangeSlug(slug)` | not allowed after first publish (protects SEO links) unless forced by Admin. |
| `Classify(type, level)` | |
| `SetSteps(IEnumerable<LocalizedText>)` | 1–20 steps; order is assigned 1..n by the aggregate. |
| `SetTips(IEnumerable<(TipKind, LocalizedText)>)` | max 20. |
| `SetTaxonomy(regionIds, issueIds, equipmentIds)` | at least one body region; no duplicates. |
| `AttachVideo(mediaId)`, `AttachThumbnail(mediaId)`, `SetImages(ids)` | |
| `SetRelated(ids)` | cannot relate to itself; max 8. |
| `SetSeo(seoTitle, seoDescription)` | lengths 60 / 160. |
| `Publish(DateTime utcNow)` | Title, Summary and every Step **complete in FA and EN** (D-12); ≥1 step; has thumbnail; has video *(rule can be relaxed while videos are not ready — D-06)*. Sets `PublishedAt` the first time. Raises `ExercisePublishedEvent`. |
| `Unpublish()` | back to Draft. Raises `ExerciseUnpublishedEvent`. |
| `Archive()` / `Restore()` | Archived is hidden everywhere except the Admin archive. |
| `ChangeAccess(AccessLevel)` | exists for the future subscription; no UI in MVP. |

Why "publish checks completeness" lives here: if the rule is in the admin form only, the mobile app or a future import could publish half-empty pages. The domain is the one door every caller passes.

### 2.5 Taxonomy aggregates

`BodyRegion`, `PosturalIssue`, `Equipment` — each: `Name: LocalizedText`, `Slug`, `Description: LocalizedText`, `SortOrder`, `IsActive`, optional `IconMediaId`. `PosturalIssue` also has `BodyRegionIds` (kyphosis → upper back). Methods: `Create`, `Rename`, `Describe`, `Reorder`, `Activate/Deactivate`. Deleting a used item is refused (deactivate instead).

### 2.6 `MediaAsset`

`FileName`, `StoredPath`, `ContentType`, `SizeBytes`, `Kind`, `Width/Height` (images), `DurationSeconds` (video), `AltText: LocalizedText`. Methods: `Create(...)`, `SetAltText`. The **file bytes are not domain** — Infrastructure stores them; the domain keeps metadata.

### 2.7 `UserProfile` (1:1 with the Identity user)

`UserId` (Identity Guid), `DisplayName`, `PreferredCulture`, `AcceptedTermsAt`. Methods: `Create(userId, displayName, culture, utcNow)`, `Rename`, `ChangeCulture`. Identity owns password/phone/lockout; the profile owns "person" data (user-profile law from yildiz).

### 2.8 `OtpChallenge`

`Mobile`, `Purpose`, `CodeHash` (never the plain code), `ExpiresAt` (now + 2 min), `Attempts` (max 5), `ConsumedAt`. Methods: `Issue(mobile, purpose, codeHash, now)`, `Verify(codeHash, now) → Result` (fails when expired, consumed or too many attempts; increments attempts), `Consume(now)`. Kept in our domain (not only Identity tokens) so we fully control TTL, attempts and audit.

### 2.9 Repository ports

```csharp
public interface IExerciseRepository : IRepository<Exercise>
{
    Task<Exercise?> GetBySlugAsync(Slug slug, CancellationToken ct);
    Task<bool> SlugExistsAsync(Slug slug, Guid? exceptId, CancellationToken ct);
}
```
Write repositories return aggregates. **Reads for pages use separate read services** (§3.5) so we never load a full aggregate just to show a list.

---

## 3. Layer 2 — Application (`src/MoveWell.Application`)

Use cases. Orchestrates domain + ports. No EF, no HTTP.

### 3.1 Folder map

```text
MoveWell.Application/
├── Abstractions/
│   ├── Persistence/  IMoveWellUnitOfWork.cs, IExerciseReadService.cs, ITaxonomyReadService.cs
│   ├── Identity/     IIdentityService.cs, ITokenService.cs
│   ├── Messaging/    ISmsSender.cs, IOtpCodeGenerator.cs, IOtpHasher.cs
│   ├── Media/        IMediaStorage.cs, IImageProcessor.cs
│   └── Caching/      ICacheInvalidator.cs
├── Features/
│   ├── Exercises/ Commands/… Queries/…
│   ├── Taxonomy/  Commands/… Queries/…
│   ├── Media/     Commands/… Queries/…
│   ├── Auth/      Commands/…
│   └── Profiles/  Commands/… Queries/…
├── DTOs/{Exercises|Taxonomy|Media|Auth|Profiles}/
├── EventHandlers/{Exercises|Media}/
├── Options/       OtpOptions.cs, MediaOptions.cs
└── DependencyInjection/ ApplicationServiceCollectionExtensions.cs  (AddMoveWellApplication)
```

### 3.2 `IMoveWellUnitOfWork`
```csharp
public interface IMoveWellUnitOfWork : IUnitOfWork
{
    IExerciseRepository Exercises { get; }
    IBodyRegionRepository BodyRegions { get; }
    IPosturalIssueRepository PosturalIssues { get; }
    IEquipmentRepository Equipments { get; }
    IMediaAssetRepository MediaAssets { get; }
    IUserProfileRepository UserProfiles { get; }
    IOtpChallengeRepository OtpChallenges { get; }
}
```

### 3.3 Full list of use cases (MVP)

**Exercises — admin commands**
`CreateExercise`, `UpdateExerciseContent`, `ChangeExerciseSlug`, `ClassifyExercise`, `SetExerciseSteps`, `SetExerciseTips`, `SetExerciseTaxonomy`, `SetExerciseMedia`, `SetRelatedExercises`, `SetExerciseSeo`, `PublishExercise`, `UnpublishExercise`, `ArchiveExercise`, `RestoreExercise`.
**Exercises — admin queries**
`ListExercisesForAdmin` (paged; filters: state, text, region, type), `GetExerciseForEdit(id)`.
**Exercises — public queries**
`ListPublishedExercises` (paged; filters region/issue/equipment/type/level; text search), `GetPublishedExerciseBySlug`, `GetRelatedExercises`, `ListExercisesForSitemap`.

**Taxonomy**
Commands: `CreateBodyRegion`, `UpdateBodyRegion`, `ReorderBodyRegions`, `SetBodyRegionActive` (same four for `PosturalIssue` and `Equipment`).
Queries: `ListTaxonomy` (all three, active only, for filters/menus), `GetPosturalIssueBySlug` (landing page), `ListTaxonomyForAdmin`.

**Media**
`UploadMedia` (validates type/size → `IMediaStorage.SaveAsync` → `MediaAsset.Create`), `SetMediaAltText`, `DeleteMedia` (refused if used), queries `ListMedia` (paged library for the picker), `GetMediaById`.

**Auth (D-09, D-13)**
| Command | What happens |
|---------|--------------|
| `StartRegistration(mobile)` | Mobile not already registered → issue `OtpChallenge(Registration)` → `ISmsSender.SendOtpAsync`. Always answers "code sent" (no account enumeration). Rate limits apply. |
| `CompleteRegistration(mobile, code, password, displayName)` | Verify + consume OTP → `IIdentityService.CreateMemberAsync(mobile, password)` with `PhoneNumberConfirmed = true` → create `UserProfile` → issue tokens. |
| `SignIn(mobile, password)` | `IIdentityService.CheckPasswordAsync` (lockout after 5 failures / 15 min) → tokens. Unconfirmed users cannot sign in. |
| `RefreshToken(refreshToken)` | Rotate refresh token (old one revoked; reuse of a revoked one revokes the whole family). |
| `SignOut(refreshToken)` | Revoke. |
| `StartPasswordReset(mobile)` | If user exists → OTP(PasswordReset). Same neutral answer either way. |
| `CompletePasswordReset(mobile, code, newPassword)` | Verify + consume → `IIdentityService.ResetPasswordAsync` → revoke all refresh tokens of that user. |
| `ChangePassword(current, new)` | For signed-in users. |

**Profiles**: `GetMyProfile`, `UpdateMyProfile(displayName, culture)`.
**Staff**: `SeedFirstAdmin` runs at startup (from config); staff users are created by Admin via `CreateStaffUser` (one person in MVP — D-14 — but the command exists so we never edit the DB by hand).

### 3.4 Anatomy of one use case — `CreateExercise`

```text
Features/Exercises/Commands/CreateExercise/
├── CreateExerciseCommand.cs           record (TitleFa, TitleEn, Slug, TypeId, LevelId) : IRequest<Result<Guid>>
├── CreateExerciseCommandValidator.cs  FluentValidation: not empty, lengths, slug regex, ids exist in Enumeration
└── CreateExerciseCommandHandler.cs
```
```csharp
public sealed class CreateExerciseCommandHandler(IMoveWellUnitOfWork uow)
    : IRequestHandler<CreateExerciseCommand, Result<Guid>>
{
    public async Task<Result<Guid>> Handle(CreateExerciseCommand cmd, CancellationToken ct)
    {
        var title = LocalizedText.Create(cmd.TitleFa, cmd.TitleEn, 150, required: true);
        var slug  = Slug.Create(cmd.Slug);
        var merged = Result.Merge(title, slug);
        if (merged.IsFailed) return merged;

        if (await uow.Exercises.SlugExistsAsync(slug.Value, null, ct))
            return Result.Fail(DomainErrors.Exercise.SlugTaken);

        var created = Exercise.Create(title.Value, slug.Value,
            Enumeration.FromValue<ExerciseType>(cmd.TypeId), Enumeration.FromValue<Level>(cmd.LevelId));
        if (created.IsFailed) return created.ToResult<Guid>();

        await uow.Exercises.AddAsync(created.Value, ct);
        await uow.SaveAndDispatchEventsAsync(ct);          // exactly once per command
        return Result.Ok(created.Value.Id);
    }
}
```
**Validation split:** the validator checks *shape* (empty? too long? regex?). The domain checks *business* (slug unique, publish completeness). Never duplicate a business rule in the validator.

### 3.5 Read side (queries)

Queries do **not** go through aggregates. `IExerciseReadService` (implemented in Infrastructure with `AsNoTracking()` + `Select` projections) returns DTOs directly:
```csharp
Task<PagedResult<ExerciseCardDto>> ListPublishedAsync(ExerciseFilter filter, Culture culture, CancellationToken ct);
Task<ExerciseDetailDto?> GetPublishedBySlugAsync(string slug, Culture culture, CancellationToken ct);
```
- Only `State == Published` and `Access` allowed for the caller (MVP: everyone sees Free; Premium hidden or teaser — decided at subscription gate).
- Culture chosen from `Accept-Language` → DTO fields already in the right language (`Title`, not `TitleFa/TitleEn`). Admin DTOs carry both languages.
- Filtering, paging, sorting done in SQL — never `GetAll()` then filter in memory.

### 3.6 Domain event handlers

| Event | Handler | Action |
|-------|---------|--------|
| `ExercisePublishedEvent` | `InvalidatePublicCachesOnExercisePublished` | clear cache keys: home, list, sitemap, that slug |
| `ExerciseUnpublishedEvent` | same family | same |
| `MediaAssetUploadedEvent` | `GenerateImageVariantsOnUpload` | create WebP thumbnails 320/640/1280 (images only) |

Handlers run **after** the save succeeded (Core's `UnitOfWork`), so a failed cache clear never rolls back content.

### 3.7 Pipeline order (MediatR behaviours)
`LoggingBehavior` (start/finish/duration + correlation id) → `ValidationBehaviorFluentResult` (validators → `Result.Fail`) → handler.

---

## 4. Layer 3 — Infrastructure (`src/MoveWell.Infrastructure`)

Implements every port with real technology.

### 4.1 Folder map
```text
MoveWell.Infrastructure/
├── Persistence/
│   ├── MoveWellDbContext.cs            : IdentityDbContext<ApplicationUser, ApplicationRole, Guid> (see 4.3)
│   ├── MoveWellUnitOfWork.cs           : UnitOfWork<MoveWellDbContext>, IMoveWellUnitOfWork
│   ├── Configurations/{Exercises|Taxonomy|Media|Users|Otp}/…Configuration.cs
│   ├── Converters/                     LocalizedText, Slug, MobileNumber converters
│   ├── ReadServices/                   ExerciseReadService.cs, TaxonomyReadService.cs
│   └── Migrations/
├── Repositories/{Exercises|Taxonomy|Media|Users|Otp}/
├── Identity/       ApplicationUser.cs, ApplicationRole.cs, IdentityService.cs, TokenService.cs, RefreshToken.cs
├── Messaging/Sms/  NullSmsSender.cs, DevSmsSender.cs, KavenegarSmsSender.cs (after Q-12), SmsOptions.cs
├── Security/       OtpCodeGenerator.cs (RandomNumberGenerator, 6 digits), OtpHasher.cs (HMAC-SHA256 + secret)
├── Media/          LocalDiskMediaStorage.cs, ImageSharpProcessor.cs, FileSignatureValidator.cs
├── Caching/        MemoryCacheInvalidator.cs
├── Seeding/        RoleSeeder.cs, FirstAdminSeeder.cs, TaxonomySeeder.cs
├── Logging/        SerilogSetup.cs
├── Health/         HealthChecksSetup.cs
└── DependencyInjection/ InfrastructureServiceCollectionExtensions.cs (AddMoveWellInfrastructure)
```

### 4.2 Database tables (SQL Server)

| Table | Key columns |
|-------|-------------|
| `Exercises` | Id, TitleFa, TitleEn, Slug (unique index), SummaryFa/En, DescriptionFa/En, TypeId, LevelId, AccessId, StateId, PublishedAt, VideoId, ThumbnailId, SeoTitleFa/En, SeoDescriptionFa/En, RowVersion, audit |
| `ExerciseSteps` | Id, ExerciseId (FK cascade), Order, TextFa, TextEn |
| `ExerciseTips` | Id, ExerciseId, KindId, Order, TextFa, TextEn |
| `ExerciseBodyRegions` / `ExercisePosturalIssues` / `ExerciseEquipments` / `ExerciseImages` / `ExerciseRelated` | join tables (ExerciseId, OtherId) |
| `BodyRegions`, `PosturalIssues`, `Equipments` | Id, NameFa/En, Slug (unique), DescriptionFa/En, SortOrder, IsActive, IconMediaId |
| `PosturalIssueBodyRegions` | join |
| `MediaAssets` | Id, FileName, StoredPath, ContentType, SizeBytes, KindId, Width, Height, DurationSeconds, AltTextFa/En |
| `UserProfiles` | Id, UserId (unique FK), DisplayName, PreferredCultureId, AcceptedTermsAt |
| `OtpChallenges` | Id, Mobile, PurposeId, CodeHash, ExpiresAt, Attempts, ConsumedAt — index (Mobile, PurposeId, ExpiresAt) |
| `RefreshTokens` | Id, UserId, TokenHash, FamilyId, ExpiresAt, RevokedAt, ReplacedById, CreatedByIp |
| `AspNetUsers`, `AspNetRoles`, … | Identity tables |

`LocalizedText` maps as an **owned type** → two columns (`TitleFa`, `TitleEn`) on the same table: simple SQL, full-text indexable, no JSON parsing.
Search in MVP: `LIKE N'%…%'` on Title/Summary both languages. If it becomes slow, add SQL Server Full-Text later (same read service, no API change).

### 4.3 DbContext

One database, one context. C# allows only one base class, so — exactly like yildiz — `MoveWellDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` (Identity tables in the same DB) and **re-applies the Core conventions itself**: audit on save (`IAuditStateful` via `ICurrentUserService` reading the `sub` claim + `IDateTimeProvider`), soft-delete query filters, ignoring `DomainEvents`. `OnModelCreating`: `base.OnModelCreating` (Identity) → `ApplyConfigurationsFromAssembly` → conventions. `MoveWellUnitOfWork : UnitOfWork<MoveWellDbContext>` is unchanged Core behaviour (save, then dispatch events).

Migrations: `dotnet ef migrations add <Name> -p src/MoveWell.Infrastructure -s src/MoveWell.Api -o Persistence/Migrations`. In production we apply them with an **EF migration bundle** (`efbundle.exe`) during deploy — not automatically at app start (safer for a first production DB).

### 4.4 Identity & tokens (see ADR-0003, updated proposal)

- `ApplicationUser : IdentityUser<Guid>` — **no extra person fields** (those live in `UserProfile`).
- `UserName = PhoneNumber = MobileNumber.Value`; `RequireConfirmedPhoneNumber = true`.
- Password: min 8, require digit; lockout 5 tries / 15 min.
- `TokenService`: JWT access token (15 min; claims `sub`, `role`, `name`, `culture`), refresh token (30 days, random 64 bytes, stored **hashed**, rotated on every use).
- Same tokens serve the Web (stored server-side in an encrypted cookie — §6) and the future mobile app (stored in secure storage).

### 4.5 OTP flow in Infrastructure

`OtpCodeGenerator` → 6 random digits (`RandomNumberGenerator.GetInt32`). `OtpHasher` → HMAC-SHA256(code + mobile + purpose, secret from config). SMS text from localization ("کد تأیید MoveWell: 123456"). Limits (`OtpOptions`): 1 send per 120 s per mobile, 5 per hour per mobile, 20 per hour per IP; 5 verify attempts per challenge. `DevSmsSender` writes the code to the log so you can test without an SMS account.

### 4.6 Media storage

`LocalDiskMediaStorage` saves to `Media:RootPath` (e.g. `D:\MoveWellData\media\yyyy\MM\{guid}.{ext}`) — **outside** the site folder so a redeploy never deletes uploads. Validation: extension whitelist (jpg, png, webp, mp4), **magic-byte check** (`FileSignatureValidator`), size limits (image 5 MB, video 200 MB — configurable). Images get WebP variants. Videos served by the Api with HTTP range requests (seeking works on phones). When Q-05 is decided, a second adapter (HLS or VOD) replaces it behind the same `IMediaStorage` / `IVideoStorage` port.

### 4.7 Logging, health, caching
- Serilog: console + rolling file `logs/movewell-api-.log` (14 days), enriched with `CorrelationId`, `UserId`, `RequestPath`. No passwords/OTP codes in logs (except `DevSmsSender` in Development).
- Health: `/health/live` (process up) and `/health/ready` (SQL reachable, media folder writable).
- `IMemoryCache` for taxonomy and public lists (5–10 min); invalidated by event handlers.

---

## 5. Layer 4 — Api (`src/MoveWell.Api`) — the only door to data

### 5.1 `Program.cs` order (and why)
```text
builder: Serilog → AddMoveWellApplication → AddMoveWellInfrastructure → AddMoveWellAuth (JWT)
         → AddRateLimiter → AddProblemDetails → AddOpenApi (Development only) → AddHealthChecks
app:     UseCorrelationId → UseExceptionHandler (ProblemDetails) → UseSecurityHeaders
         → UseSerilogRequestLogging → UseAuthentication → UseAuthorization → UseRateLimiter
         → Map endpoint groups → MapHealth → Run
startup: apply seeders (roles, first admin, taxonomy) — migrations only in Development automatically
```

### 5.2 Endpoint catalogue (`/api/v1`)

| Group | Method & route | Policy | Use case |
|-------|----------------|--------|----------|
| Public exercises | GET `/exercises?region=&issue=&equipment=&type=&level=&q=&page=&size=` | anonymous | ListPublishedExercises |
| | GET `/exercises/{slug}` | anonymous | GetPublishedExerciseBySlug |
| | GET `/exercises/{slug}/related` | anonymous | GetRelatedExercises |
| Public taxonomy | GET `/taxonomy` | anonymous | ListTaxonomy |
| | GET `/issues/{slug}` | anonymous | GetPosturalIssueBySlug |
| Media | GET `/media/{id}/{variant?}` | anonymous (Free) | stream file with range support |
| Sitemap | GET `/sitemap-entries` | anonymous | ListExercisesForSitemap |
| Auth | POST `/auth/register/start` · `/auth/register/complete` · `/auth/signin` · `/auth/refresh` · `/auth/signout` · `/auth/password/reset/start` · `/auth/password/reset/complete` · `/auth/password/change` | anonymous / Member | Auth commands |
| Me | GET/PUT `/me/profile` | Member | Profiles |
| Admin exercises | GET `/admin/exercises` · GET `/admin/exercises/{id}` · POST `/admin/exercises` · PUT `/admin/exercises/{id}/content` · `/slug` · `/classification` · `/steps` · `/tips` · `/taxonomy` · `/media` · `/related` · `/seo` · POST `/{id}/publish` · `/unpublish` · `/archive` · `/restore` | ContentEditor | Exercise commands |
| Admin taxonomy | CRUD under `/admin/body-regions`, `/admin/postural-issues`, `/admin/equipments` + `/reorder` | ContentEditor | Taxonomy commands |
| Admin media | POST `/admin/media` (multipart) · GET `/admin/media` · PUT `/admin/media/{id}/alt` · DELETE `/admin/media/{id}` | ContentEditor | Media |
| Admin users | POST `/admin/staff` | AdminOnly | CreateStaffUser |

Small focused PUTs (`/steps`, `/tips`…) instead of one giant "save everything" — each maps to one domain method, errors are precise, and the admin form can save sections independently.

### 5.3 Result → HTTP mapping (one helper, `ResultHttpExtensions.ToHttp()`)
| Result | HTTP |
|--------|------|
| `Ok(value)` | 200 (201 + Location for creates) |
| Fail with `ValidationError` | 400 ProblemDetails `{ errors: { field: [messages] } }` |
| Fail with `NotFoundError` | 404 |
| Fail with `ConflictError` (slug taken, concurrency) | 409 |
| Fail with `ForbiddenError` | 403 |
| Unhandled exception | 500 ProblemDetails with `traceId` only (no stack trace outside Development) |

Error messages are localized keys resolved with `Accept-Language`, so the Web and the mobile app show the same Persian/English message.

### 5.4 Security in the Api
- JWT validation (issuer, audience, lifetime, signing key ≥ 256-bit from secrets).
- Policies in `shared/MoveWell.Common/Identity/Policies.cs`: `AdminOnly` (Admin), `ContentEditor` (Admin or Content), `Member`.
- Rate limiting: auth endpoints (fixed window per IP), OTP send (per mobile + per IP), public GET (generous token bucket).
- Request size limit: 200 MB only on `/admin/media`, 1 MB elsewhere.
- Security headers: `X-Content-Type-Options`, `Referrer-Policy`, no `Server` header.
- The Api listens only on localhost/internal binding in production **unless** the mobile app needs it public — then it gets its own subdomain `api.movewell.ir` with HTTPS.

---

## 6. Layer 5 — Web (`src/MoveWell.Web`) — a client of the Api (BFF)

Web is a **Backend-for-Frontend**: it renders HTML on the server and holds the user's tokens on the server side. The browser never sees a JWT.

### 6.1 How sign-in works on the Web
1. Member submits mobile + password on `/account/signin` (Web form, antiforgery token).
2. Web calls Api `POST /auth/signin` → gets access + refresh tokens.
3. Web creates its own **auth cookie** (`MoveWell.Member`, HttpOnly, Secure, SameSite=Lax) containing the user claims; the tokens are stored in the cookie's encrypted auth properties (ASP.NET Data Protection).
4. `ApiAuthHandler` (a `DelegatingHandler` on every typed client) adds `Authorization: Bearer <access>`; if the Api answers 401 it calls `/auth/refresh` once, updates the cookie, and retries.
5. Staff use a separate cookie `MoveWell.Staff` scoped to `/admin`, same mechanism.

Data Protection keys are persisted to a folder (`D:\MoveWellData\keys`) — otherwise every IIS app-pool recycle logs everyone out.

### 6.2 Typed Api clients (`Web/Services/Api/`)
`IExercisesApiClient`, `ITaxonomyApiClient`, `IAuthApiClient`, `IProfileApiClient`, `IAdminExercisesApiClient`, `IAdminTaxonomyApiClient`, `IAdminMediaApiClient`. Each is an `HttpClient` registered with `AddHttpClient<T>()`, base address from `Api:BaseUrl`, timeout 10 s, sends `Accept-Language` from the current culture and the correlation id header. Contracts (request/response records) come from `shared/MoveWell.Common/Contracts` — the same records the Api returns, so there is no copy-paste of shapes.

### 6.3 Culture & routing
- `/` = fa-IR (RTL), `/en/...` = en-US (LTR) via a path-prefix `RequestCultureProvider` in `shared/MoveWell.Shared.Globalization` (same as yildiz).
- Strings: JSON resource files in `Web/Resources/{area}/{page}/fa.json|en.json` via `shared/MoveWell.Shared.Localization`.
- `<html lang="fa" dir="rtl">` switches per culture; `hreflang` alternates on every page.
- Dates: Jalali (Persian calendar) in fa, Gregorian in en — formatting only in views.

### 6.4 Public controllers & pages
| Route (fa / en) | Controller.Action | Data |
|-----------------|-------------------|------|
| `/` · `/en` | Home.Index | featured exercises, issues, regions |
| `/exercises` | Exercises.Index | filters + paged grid |
| `/exercises/{slug}` | Exercises.Detail | full exercise + related |
| `/issues/{slug}` | Issues.Detail | educational landing for one postural issue + its exercises |
| `/regions/{slug}` | Regions.Detail | exercises for a body region |
| `/search?q=` | Search.Index | text search |
| `/account/register` → `/account/verify` → done | Account.* | OTP registration (2 steps) |
| `/account/signin`, `/account/forgot` → `/account/reset` | Account.* | sign-in, reset with OTP |
| `/account/profile` | Account.Profile | name, language |
| `/about`, `/terms`, `/privacy`, `/disclaimer` | Pages.* | static content (FA/EN) |
| `/sitemap.xml`, `/robots.txt` | Seo.* | generated |

### 6.5 Admin area (`/admin`, staff cookie)
Dashboard (counts: drafts, published, media size) · Exercises list (filters, state badges) · Exercise editor (tabs: Content FA/EN · Steps · Tips · Taxonomy · Media · Related · SEO · Publish panel with checklist of what is missing) · Taxonomy (3 lists with drag-reorder) · Media library (upload, alt text, usage) · Account (change password).

### 6.6 Caching on the Web
Output caching for anonymous public pages (home, lists, detail) for 60 s, varied by culture and query; bypassed for signed-in requests. Static files with long cache + file-version query (`asp-append-version`).

---

## 7. `shared/` projects
- **MoveWell.Common**: `Cultures` (fa-IR, en-US), `Roles`, `Policies`, `Contracts/{Exercises|Taxonomy|Auth|Profiles|Media|Common}` (request/response records, `PagedResponse<T>`, `ApiError`). No dependencies on Domain — the mobile app could reuse it later.
- **MoveWell.Shared.Localization**: JSON `IStringLocalizer` factory.
- **MoveWell.Shared.Globalization**: path-prefix culture provider, `CulturePath.For(culture, path)` helper for links.

---

## 8. Cross-cutting

### 8.1 Configuration & secrets
`appsettings.json` (safe defaults) → `appsettings.Development.json` → **environment variables / user-secrets** for: `ConnectionStrings__Default`, `Jwt__SigningKey`, `Otp__HashSecret`, `Sms__ApiKey`, `Admin__Seed__Mobile/Password`. Nothing secret is ever committed. Options classes are validated on start (`ValidateDataAnnotations().ValidateOnStart()`), so a missing setting stops the app immediately with a clear message instead of failing later.

### 8.2 Testing per layer
| Project | What we test | Example |
|---------|--------------|---------|
| Domain.Tests | every aggregate rule | `Publish_fails_when_english_title_missing` |
| Application.Tests | handlers with fake ports, validators | `CompleteRegistration_rejects_expired_code` |
| ArchitectureTests | layer boundaries (exists, 3 green) | Web never references Infrastructure |
| SmokeTests | real Api in memory (`WebApplicationFactory`) + SQL LocalDB | register → sign in → admin publishes → public GET returns it |

### 8.3 Security checklist (MVP)
HTTPS + HSTS · antiforgery on all Web forms · password hashing by Identity (PBKDF2) · lockout · OTP hashed + rate-limited · no account enumeration · JWT short-lived + refresh rotation · upload magic-byte check · media outside webroot · no secrets in repo · SQL only through EF (parameterized) · Razor auto-encoding (no `Html.Raw` on user content) · CSP header on Web.

---

## 9. Deployment (your first deploy, step by step)

### 9.1 Target server (Windows Server, Iran VPS — D-07)
1. Windows Server 2022, 4 vCPU / 8 GB RAM / SSD ≥ 100 GB (videos!) is a comfortable start.
2. Install **IIS** (with WebSockets off, static content on), **ASP.NET Core 10 Hosting Bundle**, **SQL Server 2022 Express** (free, 10 GB DB limit — enough for text; videos live on disk) or Standard if budget allows.
3. Folders: `D:\Sites\MoveWell.Web`, `D:\Sites\MoveWell.Api`, `D:\MoveWellData\media`, `D:\MoveWellData\keys`, `D:\MoveWellData\logs`, `D:\Backups`.
4. Two IIS sites, each with its own **app pool** (No Managed Code, AlwaysRunning, idle timeout 0):
   - `movewell.ir` + `www` → MoveWell.Web (ports 80/443)
   - Api: internal `localhost:5100` binding; later `api.movewell.ir` when the mobile app ships.
5. App pool identities get **Modify** on `MoveWellData` folders; read on site folders.
6. HTTPS: certificate via **win-acme** (Let's Encrypt, auto-renew) or a purchased certificate; HTTP→HTTPS redirect; `www` → apex redirect.
7. Environment variables per app pool (secrets, `ASPNETCORE_ENVIRONMENT=Production`).

### 9.2 Release process
```text
dotnet test MoveWell.sln                              ← must be green
dotnet publish src/MoveWell.Api -c Release -o out/api
dotnet publish src/MoveWell.Web -c Release -o out/web
dotnet ef migrations bundle -p src/MoveWell.Infrastructure -s src/MoveWell.Api -o out/efbundle.exe
copy to server → app_offline.htm in both sites → backup DB → run efbundle.exe
→ robocopy new files → remove app_offline.htm → open /health/ready → smoke click-through
```
A `scripts/deploy/` PowerShell script will automate this in G3.

### 9.3 Backups
SQL: nightly full backup (SQL Agent isn't in Express → Windows Task Scheduler + `sqlcmd BACKUP DATABASE`), 14 days kept, copied off-server weekly. Media folder: nightly robocopy to a second disk/remote. **One test restore before launch** (exit criterion of G3).

### 9.4 Monitoring
Logs folder + Serilog daily files; `/health/ready` checked by a free uptime monitor; disk-space alert (videos grow).

---

## 10. Build order (how we will actually walk through this)

1. Domain shared VOs + Enumerations + tests
2. Infrastructure DbContext + UoW + first migration (empty tables for Identity, profiles, OTP)
3. Auth vertical slice: StartRegistration → CompleteRegistration → SignIn → Web pages (closes G0)
4. Taxonomy slice (domain → handler → api → admin page)
5. Exercise slice (the big one) + media
6. Public pages + SEO
7. Deploy scripts + server

Every slice follows: **rule → aggregate/VO → test → persistence → handler → endpoint → Web page → smoke test.**
