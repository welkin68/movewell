# Domain (src/MoveWell.Domain/**)

- Reuse CookieBytes.Core bases: `AggregateRoot`, `Entity`, `ValueObject`, `Enumeration`. Never redeclare `Id`, audit fields, `RowVersion`, `DomainEvents`, soft-delete fields.
- Two ctors: `protected` parameterless (EF) + private full ctor (assignment only).
- Factories `static Create(...) → Result<T>`; behaviour methods return `Result`. No throws for business rules.
- `private set` properties; child collections private, changed only through the root.
- `Enumeration` instead of C# `enum`.
- Only aggregate roots raise events (`Events/{Aggregate}{Action}Event.cs`).
- No EF, MediatR, FluentValidation or HTTP references.
- Value objects for: `MobileNumber`, `Slug`, `ExerciseStep` ordering, `MediaRef`.
