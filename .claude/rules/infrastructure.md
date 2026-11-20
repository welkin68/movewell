# Infrastructure (src/MoveWell.Infrastructure/**)

- `MoveWellDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>` and re-applies Core conventions (audit, soft-delete filters, ignore DomainEvents) — same as yildiz; `MoveWellUnitOfWork : UnitOfWork<MoveWellDbContext>`.
- EF configs call `base.Configure(builder)` first; never map Id/audit/RowVersion manually.
- Media behind ports (`IMediaStorage`, `IVideoStorage`) — local disk adapter first (ADR-0002).
- Secrets never in `appsettings.json` committed values; use environment/user-secrets.
- Seeding: roles, first Admin from config, taxonomy starter data — idempotent.
