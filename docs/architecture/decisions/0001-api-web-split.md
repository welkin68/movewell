# ADR-0001 — Separate Api and Web hosts

- **Status:** Accepted (D-18) · 2026-11-20
- **Context:** A mobile app is planned (D-08). Yildiz already uses Web → Api over HTTP.
- **Decision:** `MoveWell.Api` (REST `/api/v1`) is the only host that touches Application/Infrastructure. `MoveWell.Web` (Razor SSR + Admin) calls Api through typed HTTP clients. The future mobile app uses the same Api.
- **Consequences:** + mobile needs no new backend; + one place for authorization and premium checks. − two IIS sites/app pools; − one HTTP hop for Web pages (mitigated by output caching on public pages).
- **Alternative rejected:** single MVC host calling MediatR directly — faster now, but the mobile app would force extracting an Api later.
