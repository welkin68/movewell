# Frontend & UI design guide

> v0.2 · 2026-11-20 · Status: **Accepted (D-18)** — direction **A "Calm Clinic" (D-15)**, light only (D-17); Q-15…Q-17 still open.
> Backend contract for the pages: `backend-guide.md` §6.

---

## 1. What the UI must achieve (derived from decisions)

| Need | From | UI consequence |
|------|------|----------------|
| People learn a movement correctly | D-01 | Video first, then numbered steps, then mistakes/cautions. Loop + slow-motion on the player. |
| Education, not therapy | D-02 | Calm, clinical-but-friendly tone; a visible but light disclaimer; no "diagnose yourself" UI. |
| Mostly phones, often mid-exercise | product nature | Mobile-first; big tap targets (≥ 44 px); readable from 1 m away in "follow mode". |
| FA (RTL) + EN (LTR) at launch | D-12 | Every layout written with CSS **logical properties**; icons that point (arrows) mirror. |
| Free now, subscription later | D-03 | Card/detail components already have a "premium" badge/lock state (hidden in MVP). |
| Iran hosting, no CDN | rules | All fonts/icons/JS local; small page weight for mobile data. |
| One specialist runs Admin alone | D-14 | Admin must be fast and forgiving: autosave drafts, a "what's missing to publish" checklist. |

---

## 2. Design direction — three options

No brand exists yet (D-10). Three directions, each fully workable; I recommend **A**.

### A. "Calm Clinic" — **[Decided] D-15**
Light, airy, lots of white space; soft teal/green primary (movement, health) with a warm coral accent for actions; rounded 12–16 px cards; line illustrations of the body. Feels trustworthy and educational — fits D-02 best and ages well.
- Primary `#0F766E` (teal 700) · Accent `#F97362` (coral) · Ink `#1F2937` · Surface `#F7FAF9` · Muted `#64748B`

### B. "Active Studio"
Dark hero sections, bold condensed headings, energetic orange/lime, full-bleed workout photos. Feels like a gym/fitness brand — strong for "general exercises", weaker for corrective/rehab trust.

### C. "Soft Wellness"
Pastel beige/sage, serif headings, yoga-like calm, organic shapes. Very gentle; can feel less "expert" and is harder to keep readable with Persian serif fonts.

**Why A:** corrective exercise users are often people with back/neck pain — they need calm and clarity, not hype. A also gives the strongest contrast for accessibility and a dark mode that is easy to derive.

---

## 3. Design system (tokens first)

All colours, spacing, radii and fonts are **CSS custom properties** in `wwwroot/css/shared/tokens.css`. Components only use tokens — changing the brand later = editing one file.

```css
:root {
  /* colour */
  --color-primary: #0F766E;  --color-primary-ink: #ffffff;
  --color-accent:  #F97362;  --color-ink: #1F2937;  --color-muted: #64748B;
  --color-surface: #F7FAF9;  --color-card: #ffffff; --color-border: #E2E8F0;
  --color-success: #15803D;  --color-warning: #B45309; --color-danger: #B91C1C;
  /* type */
  --font-body: "Vazirmatn", system-ui, sans-serif;
  --step--1: 0.875rem; --step-0: 1rem; --step-1: 1.25rem; --step-2: 1.563rem; --step-3: 1.953rem; --step-4: 2.441rem;
  --leading-body: 1.8;   /* Persian needs more line height than English */
  /* space (4-pt scale) */
  --space-1: .25rem; --space-2: .5rem; --space-3: .75rem; --space-4: 1rem; --space-6: 1.5rem; --space-8: 2rem; --space-12: 3rem;
  /* shape & depth */
  --radius-sm: 8px; --radius-md: 12px; --radius-lg: 16px; --shadow-card: 0 1px 2px rgb(0 0 0 / .06), 0 4px 12px rgb(0 0 0 / .05);
}
/* Dark mode: not at launch (D-17). Later: add dark tokens under [data-theme="dark"] only. */
```

**Font:** Vazirmatn (self-hosted, variable WOFF2, already used in yildiz) for both languages — its Latin glyphs are good, so one font file family serves FA and EN (less download). Persian digits in fa (`font-feature-settings: "ss01"` or formatting in Razor), Latin digits in en.

**RTL/LTR rule:** never write `margin-left`, `padding-right`, `left:`, `text-align: left`. Use `margin-inline-start`, `padding-inline-end`, `inset-inline-start`, `text-align: start`. Directional icons get `.icon--flip-rtl { transform: scaleX(-1) }` under `[dir=rtl]`. One stylesheet works for both directions.

**Icons:** an SVG sprite (`wwwroot/icons/sprite.svg`) built from an open-licence set (e.g. Lucide, ISC licence), used as `<svg><use href="/icons/sprite.svg#play"/></svg>`. No icon font, no CDN.

---

## 4. Component inventory

Built as Razor **partials / ViewComponents** in `Views/Shared/Ui/` + one CSS file each in `wwwroot/css/shared/components/` + small JS modules where needed.

| Component | Used on | Notes |
|-----------|---------|-------|
| `SiteHeader` | all public | logo, search icon, language switch (فا/EN), account icon; collapses to a drawer on mobile |
| `BottomNav` (mobile) | public | Home · Library · Issues · Account — thumb-reachable |
| `SiteFooter` | all public | links, disclaimer link, social |
| `ExerciseCard` | lists, home, related | thumbnail 16:9 (or 4:5, Q-15), title, level chip, region chips, duration; premium lock state (hidden in MVP) |
| `FilterBar` / `FilterSheet` | library | chips on desktop, bottom sheet on mobile; region · issue · equipment · type · level; filters live in the URL (shareable, SEO-safe) |
| `BodyMap` | home, library | **interactive SVG body (front/back)** — tap a region → filtered library. Signature feature, very intuitive for "where does it hurt / what do I train" without being diagnosis |
| `VideoPlayer` | detail | native `<video>` + custom controls: play, **loop on by default**, **speed 0.5× / 0.75× / 1×**, fullscreen, poster image; `preload="metadata"` to save data |
| `StepList` | detail | big numbered steps; each step optionally linked to a video timestamp |
| `TipBlock` | detail | three styles: ✅ cue, ⚠️ common mistake, 🛈 caution (colour + icon + text, never colour alone) |
| `Disclaimer` | detail, issue pages | short line + link to full disclaimer page |
| `IssueHero` | issue landing | illustration + plain-language explanation + list of exercises |
| `Breadcrumbs` | detail, issue, region | also emitted as JSON-LD |
| `Pagination` / "Load more" | lists | server-rendered links (SEO) + progressive enhancement |
| `OtpInput` | register, reset | 6 boxes, auto-advance, paste support, `autocomplete="one-time-code"` (Android/iOS auto-fill), resend countdown 120 s |
| `PasswordField` | auth | show/hide toggle, strength hint |
| `FormField`, `Button`, `Chip`, `Badge`, `Toast`, `Modal`, `EmptyState`, `Skeleton` | everywhere | base kit |
| Admin: `AdminShell`, `DataTable`, `TabPanel`, `BilingualField`, `SortableList`, `MediaPicker`, `PublishChecklist` | admin | see §6 |

---

## 5. Public pages (wireframes, mobile first)

### 5.1 Home
```text
┌──────────────────────────┐
│ ☰  MoveWell        🔍 فا │  header
├──────────────────────────┤
│ Learn to move well.      │  hero: one sentence + search box
│ [ Search exercises… ]    │
├──────────────────────────┤
│   [ BodyMap front/back ] │  tap a region
├──────────────────────────┤
│ Common issues  →         │  horizontal scroll of issue cards
│ [Kyphosis][Flat feet]…   │  (kyphosis, lordosis, rounded shoulders…)
├──────────────────────────┤
│ New exercises  →         │  ExerciseCard row
├──────────────────────────┤
│ How MoveWell works (3 steps) · disclaimer line │
└──────────────────────────┘
│ 🏠  📚  🧍  👤 │  bottom nav (mobile)
```

### 5.2 Library (`/exercises`)
Search field + filter chips (sticky) → result count → grid of `ExerciseCard` (1 col phone, 2 tablet, 3–4 desktop) → pagination. Empty state suggests removing a filter.

### 5.3 Exercise detail (`/exercises/{slug}`) — the most important page
```text
Breadcrumb: Library › Neck › Chin tuck
[ VIDEO  ▶   ⟳ loop  0.75× ]          ← sticky on desktop side, top on mobile
Chin Tuck                               H1
Beginner · Stretch · No equipment       chips
Short summary paragraph
── How to do it ──
 ① Sit tall …
 ② Gently pull your chin back …
 ③ Hold 5 s …
── Common mistakes ──  ⚠️ …
── Be careful ──       🛈 …
[ Start follow mode ]                   ← full-screen: video + one big step at a time, swipe/next
Related exercises → cards
Disclaimer line
```
**Follow mode:** a full-screen, big-type, one-step-at-a-time view with the looping video, keeps the screen awake (`navigator.wakeLock`, progressive enhancement). This is what makes the site useful *during* exercise, not only before.

### 5.4 Issue landing (`/issues/{slug}`)
Illustration · "What is it?" plain text · "Which areas are involved" chips · exercises list · disclaimer emphasised ("see a professional if you have pain") — strong SEO pages (people search "حرکات اصلاحی کایفوز").

### 5.5 Account
Register: mobile → (OTP screen) → password + name → done. Sign in: mobile + password, "forgot password?". Reset: mobile → OTP → new password. Profile: name, language. All forms server-rendered with inline validation messages from the Api (same FA/EN messages as the mobile app).

### 5.6 Static
About, Terms, Privacy, Disclaimer — text pages with a readable width (`max-inline-size: 70ch`).

---

## 6. Admin UI (for one specialist)

- **Shell:** sidebar (Dashboard, Exercises, Body regions, Postural issues, Equipment, Media, Account) + top bar; collapses to a drawer on mobile.
- **Exercises list:** table with search, state filter (Draft/Published/Archived), "missing EN" badge, last edited.
- **Exercise editor** — tabs, each saved independently (matches the small Api PUTs):
  1. *Content* — `BilingualField`: FA and EN side-by-side on desktop, stacked on mobile; character counters.
  2. *Steps* — sortable list (drag + up/down buttons), each step FA/EN.
  3. *Tips* — kind selector + FA/EN.
  4. *Taxonomy* — multi-select chips.
  5. *Media* — `MediaPicker` (library dialog + upload), thumbnail preview, video preview.
  6. *Related* — search & pick.
  7. *SEO* — Google-preview box (title/description length meter).
  8. *Publish* — `PublishChecklist`: ✔ FA title ✔ EN title ✖ thumbnail missing … → Publish button enabled only when the domain would accept it (the Api is still the final judge).
- Unsaved-changes warning; toast after save; concurrency conflict (409) shows "someone else saved — reload".
- Admin uses the same tokens but denser spacing (`[data-density="compact"]`).

---

## 7. Frontend technology

| Choice | Decision | Why |
|--------|----------|-----|
| Rendering | **Razor SSR** (MVC views) | SEO, fast first paint on phones, same stack as yildiz, no SPA build chain. |
| Interactivity | **Vanilla JS ES modules** per component (`wwwroot/js/shared/*.js`), progressive enhancement | Everything works without JS; JS only adds filters-without-reload, OTP auto-advance, player controls, follow mode. No framework to learn or update. |
| Partial updates | `fetch` + server-rendered partials (filters, load-more) | Keeps HTML rendering on the server. |
| CSS | Plain CSS with tokens + `@layer` (reset, tokens, base, components, utilities) | No Tailwind/Sass build needed; easy for a first project. |
| Bundling | ASP.NET static assets + `asp-append-version`; optional minify on publish | No Node.js on the server. |
| Assets | all local (`wwwroot/fonts`, `icons`, `img`) | Iran hosting, no CDN rule. |

Folder layout in `MoveWell.Web/wwwroot`:
```text
css/shared/{tokens,reset,base,layout,utilities}.css
css/shared/components/{header,card,player,filter,otp,…}.css
css/pages/{home,library,exercise,issue,account}.css
css/admin/{shell,editor,table}.css
js/shared/{player,filters,otp-input,toast,modal,body-map,follow-mode}.js
js/admin/{editor-tabs,sortable,media-picker,publish-checklist}.js
fonts/vazirmatn/  icons/sprite.svg  img/
```

---

## 8. Quality bars

- **Performance (mobile, 4G):** LCP < 2.5 s, CLS < 0.1, JS < 50 KB per page, CSS < 60 KB; images WebP with `srcset`; video never autoplays with sound, `preload="metadata"`.
- **Accessibility:** WCAG 2.2 AA contrast; visible focus ring; all controls keyboard-reachable; captions track (`<track>`) slot ready for videos; `lang`/`dir` correct per page; tip types use icon + text, not colour alone.
- **SEO:** unique `<title>`/meta per page and language, `hreflang` fa/en + `x-default`, canonical URLs, JSON-LD (`HowTo` / `ExercisePlan`-style + `BreadcrumbList` + `VideoObject`), XML sitemap, clean slugs, server-rendered content.
- **Testing:** smoke tests check each public page returns 200 in fa and en, has one H1, has `hreflang`, and no external (CDN) URLs.

---

## 9. Build order for the UI

1. Tokens + base + header/footer + bottom nav (G0 shell, both directions)
2. Auth pages + OtpInput (G0)
3. Admin shell + taxonomy pages (G1)
4. Exercise editor (G1)
5. ExerciseCard, library, filters (G2)
6. Exercise detail + player + follow mode (G2)
7. BodyMap + issue landings + home (G2)
8. SEO pass, accessibility pass, Lighthouse pass (G2 exit)
