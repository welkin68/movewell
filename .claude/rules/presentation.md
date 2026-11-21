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

## Web conventions (learned in the G0 shell)
- Language: `/en` becomes PathBase (`CulturePrefixMiddleware`) - never put `/en` in routes; `asp-*` and `Url.*` keep the language. Language switch and hreflang use `CulturePath.For`.
- Texts: `Ui.For("public/account/account")` reads `Resources/public/account/account.{fa|en}.json`. Add every key to BOTH files.
- No inline `style=` or `<script>` (CSP is `'self'` only): use utility classes and `wwwroot/js/shared/*.js` modules.
- Sprite icons: `<use href="@(Url.Content("~/icons/sprite.svg"))#name" />` (`~/` is not resolved inside `<use>`).
- Numbers/codes inside Persian sentences: wrap in LRI/PDI (U+2066 ... U+2069) or `<bdi dir="ltr">` so they keep their order.
- Personal data (mobile) never in URLs or hidden fields: keep in-flow state in TempData.
- Admin uses an area route (`MapAreaControllerRoute`), so plain `RedirectToAction` never lands in /admin.
