# Presentation (Api + Web)

## Api
- Minimal-API endpoint groups under `/api/v1/{system}`; thin: map request → MediatR → `Result` to HTTP.
- Authorization policies from `shared/MoveWell.Common` (AdminOnly, ContentEditor, Member).
- Designed for a future mobile client: no Razor, no cookies-only assumptions in contracts.

## Web
- Razor SSR, RTL, Persian UI text from localization resources (no hard-coded strings in views).
- Calls Api only through typed HTTP clients; no business logic in controllers, views or JS.
- Mobile-first; Vazirmatn and all assets local (no CDN).
- Every exercise page shows the educational disclaimer (D-02).
- SEO: unique title/meta per page, slug URLs, JSON-LD on exercise pages.
