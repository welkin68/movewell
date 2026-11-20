# Application (src/MoveWell.Application/**)

- Handlers inject `IMoveWellUnitOfWork` / repository ports — never DbContext.
- Commands: call domain factory/behaviour → repository → `SaveAndDispatchEventsAsync()` once.
- Queries: read-only, filtering/paging pushed to the DB; no `GetAll()` + in-memory filter. Public queries return only `Published` content and respect `AccessLevel`.
- FluentValidation = input shape; domain = business invariants.
- Return `Result`/`Result<T>`; never expose domain entities — map to DTOs.
