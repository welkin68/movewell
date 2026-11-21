# Folders (always)

Never create a file outside a defined folder. First file of a new concern → create its folder first.

- Docs → `docs/{product|architecture|architecture/decisions|roadmap|foundation}/`. Only `README.md` at `docs/` root.
- Aggregate → `Domain/Aggregates/{Aggregate}/` with `Entities/ ValueObjects/ Events/ Enumerations/` as needed.
- Repository port → `Domain/Repositories/{System}/I{Aggregate}Repository.cs`; implementation → `Infrastructure/Repositories/{System}/`.
- CQRS → `Application/Features/{Feature}/{Commands|Queries}/{Op}/{Op}Command.cs | Handler.cs | Validator.cs`.
- DTOs → `Application/DTOs/{System}/`; public API contracts → `shared/MoveWell.Common/Contracts/{System}/`.
- Domain event handlers → `Application/EventHandlers/{System}/`.
- Application services shared by several handlers → `Application/Services/{System}/`; app-level error codes → `Application/Common/Errors/`.
- Api error texts → `Api/Localization/Errors/errors.{fa|en}.json` (every new error code gets both).
- EF config → `Infrastructure/Persistence/Configurations/{System}/`.
- Api endpoints → `Api/Endpoints/{System}/`.
- Web → `Web/Controllers/`, `Web/Areas/Admin/{Controllers|Views}/`, `Web/Views/`, shared UI partials `Web/Views/Shared/Ui/`, form/view models `Web/Models/{Area}/`, Api clients `Web/Services/{System}/` (plumbing in `Web/Services/Api/`), cookies/session `Web/Authentication/`, middleware `Web/Middleware/`.
- Web assets → `wwwroot/css/{shared|pages|admin}/`, `wwwroot/js/shared/`, `wwwroot/icons/sprite.svg`, `wwwroot/img/{area}/`, `wwwroot/fonts/`.
- Localization → `Web/Resources/{area}/{page}/`.
- Tests mirror source.
