# Frontend Phase 3 (Angular) — CMS-driven site + Polish + Performance + Release

## Goal of Phase 3
Make the product feel production-grade:
- Landing/Home driven by CMS content from backend
- Achievements loaded dynamically
- Better forms UX (autosave where needed, better validation)
- Real dashboards (KPIs) for Admin/Instructor/Parent
- Offline-friendly caching for read-only content
- Performance, security, and deployment readiness

## Testing policy
- Keep tests minimal.
- Focus on manual QA flows + build checks.
- Optional: 1 end-to-end smoke (Cypress/Playwright) ONLY if requested.

---

## Progress
- [x] Milestone F3.0 - Production readiness (envs, base-href, build configs)
- [x] Milestone F3.1 - CMS integration for Landing/Home/About (public)
- [x] Milestone F3.2 - Achievements integration (public + admin management UI)
- [x] Milestone F3.3 - Real dashboards (Admin/Instructor/Parent) using backend dashboard endpoints
- [x] Milestone F3.4 - UX polish (skeletons, empty states, error states, accessibility)
- [x] Milestone F3.5 - Performance (lazy loading, prefetch, caching)
- [x] Milestone F3.6 - Security UI (session expiry UX, logout everywhere, safer storage options)
- [x] Milestone F3.9 - Release checklist + docs + final cleanup

---

# Milestone F3.0 - Production readiness

## Goal
Make the Angular app build and deploy cleanly.

## Requirements
- Add environments:
  - environment.development.ts -> apiBaseUrl "/api" (proxy)
  - environment.production.ts -> apiBaseUrl "https://<your-domain>/api" OR relative "/api"
- Add build scripts and ensure:
  - `npm run build` works
- Configure base href and routing strategy:
  - prefer PathLocationStrategy
- Add Dockerfile (optional) or Nginx config (optional)

## Verification
- Build succeeds and output is correct.

---

# Milestone F3.1 - CMS integration for Landing/Home/About (public)

## Goal
Landing becomes dynamic content from CMS endpoints.

## Requirements
- Public API calls:
  - GET /api/v1/public/cms/pages/home
  - GET /api/v1/public/cms/pages/landing
  - GET /api/v1/public/cms/pages/about
- Render CMS sections by type:
  - hero, stats, programs, features, testimonials
- Use a simple renderer:
  - map section.type -> component
  - section.jsonContent -> inputs
- Add fallback static content if CMS returns 404/unpublished.

## Verification
- Update CMS content in admin -> public landing reflects changes.

---

# Milestone F3.2 - Achievements integration (public + admin UI)

## Goal
Show achievements on public landing and admin can manage them.

## Requirements
Public:
- GET /api/v1/public/achievements?page=&pageSize=
- display grid + filters (tag)
Admin:
- /app/admin/achievements:
  - list/create/edit/delete
  - image upload (if supported) or URL input

## Verification
- Achievements created in admin appear publicly.

---

# Milestone F3.3 - Real dashboards (Admin/Instructor/Parent)

## Goal
Replace dashboard placeholders with real KPIs.

## Requirements
Endpoints (assumed from backend phase later):
- GET /api/v1/dashboards/admin
- GET /api/v1/dashboards/instructor
- GET /api/v1/dashboards/parent

UI:
- Summary cards
- Charts (Recharts-like for Angular: ng2-charts/Chart.js OR lightweight SVG)
- Lists: risky students, pending grading, unread announcements, upcoming sessions

## Verification
- Role-based dashboards show correct data.

---

# Milestone F3.4 - UX polish

## Goal
Make the app feel premium.

## Requirements
- Skeleton loaders for lists
- Empty states with CTA
- Better error states:
  - 401 -> login
  - 403 -> forbidden page
  - 404 -> not found
- Accessibility basics:
  - keyboard nav, aria labels, focus outline
- Mobile improvements for tables -> cards

---

# Milestone F3.5 - Performance

## Goal
Make it fast.

## Requirements
- Lazy-load feature routes/modules
- Use OnPush change detection where appropriate
- Cache read-only endpoints (CMS, achievements) with TTL in service
- Reduce bundle size (analyze, remove unused deps)

---

# Milestone F3.6 - Security UI

## Goal
Harden session UX.

## Requirements
- Use refresh token flow robustly
- Logout on refresh failure
- Add "session expiring" banner (optional)
- Avoid storing sensitive info beyond tokens
- Consider switching access token storage to memory + refresh token in localStorage (optional; depends on your threat model)

---

# Milestone F3.9 - Release checklist + docs

## Requirements
- Update README:
  - run dev (proxy)
  - production build
  - env variables
- Remove unused components/files
- Final manual QA checklist:
  - login/logout
  - role routing
  - main CRUD flows
  - mobile views

Stop after completion.
