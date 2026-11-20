# Application (src/MoveWell.Application/**)

- Handlers inject `IMoveWellUnitOfWork` / repository ports — never DbContext.
- Commands: call domain factory/behaviour → repository → `SaveAndDispatchEventsAsync()` once.
- Queries: read-only, filtering/paging pushed to the DB; no `GetAll()` + in-memory filter. Public queries return only `Published` content and respect `AccessLevel`.
- FluentValidation = input shape; domain = business invariants.
- Return `Result`/`Result<T>`; never expose domain entities — map to DTOs.
- FluentValidation messages are **error codes** (`ValidationCodes`), never sentences; Core turns them into errors whose message is the code.
- Every new error code (domain or application) gets a FA and EN text in the Api error files.
