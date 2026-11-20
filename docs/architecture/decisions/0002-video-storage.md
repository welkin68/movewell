# ADR-0002 — Video storage and delivery

- **Status:** Open (needs Q-05) · 2026-11-20
- **Context:** Videos are the core content. Free at launch (D-03), subscription later → premium videos may need protection. Host is an Iran VPS (D-07); users are mostly on mobile data.
- **Options:**
  1. **Self-hosted MP4** on VPS disk — simplest; easy to download; heavy on VPS bandwidth.
  2. **Self-hosted HLS** (ffmpeg transcode to 360/720p + signed URLs) — adaptive on mobile, harder to download; needs a transcode job and disk.
  3. **VOD service (e.g. ArvanCloud VOD)** — CDN in Iran, adaptive, signed links; monthly cost, external dependency.
- **Proposal:** code against an `IVideoStorage` port now; start with option 1 for development, decide 2 vs 3 before G3 launch. Switching later only changes the Infrastructure adapter.
