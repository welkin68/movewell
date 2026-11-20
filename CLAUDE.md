# MoveWell — AI entry point

Website (later + mobile app) that teaches corrective and general exercises. Education only — no therapy, no medical advice (D-02).

## Read first (every session)
1. `docs/roadmap/ROADMAP.md` — current gate
2. `docs/foundation/FOUNDATION.md` — what exists
3. `docs/product/open-questions.md` — what is unknown (never guess these)
4. `docs/product/vision.md` and `docs/architecture/architecture.md` as needed

## Communication
Talk to the user in **Persian**. Code, identifiers, commits, docs, rules: English. UI text: Persian (RTL).

## Rules
Rules live in `.claude/rules/`. Always-on: `00-foundation`, `01-collaboration`, `02-folders`.
Path-scoped — read the matching file before the first edit to that path in a session:

| Touching | Read |
|----------|------|
| `src/MoveWell.Domain/**` | `domain.md` |
| `src/MoveWell.Application/**` | `application.md` |
| `src/MoveWell.Infrastructure/**` | `infrastructure.md` |
| `src/MoveWell.Api/**`, `src/MoveWell.Web/**`, `*.cshtml`, `wwwroot/**` | `presentation.md` |

Reference implementation of the same architecture: `B:\project\yildiz` (read-only).

@.claude/rules/00-foundation.md
@.claude/rules/01-collaboration.md
@.claude/rules/02-folders.md
