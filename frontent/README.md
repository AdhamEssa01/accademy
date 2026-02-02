# Frontend (Angular)

## Prerequisites
- Node LTS
- Angular CLI (use `npx` if not installed globally)

## Install
```
cd frontent/web
npm install
```

## Run (dev)
```
cd frontent/web
npx ng serve --proxy-config proxy.conf.json
```

The app proxies `/api` to `http://localhost:5134` by default.  
To change the backend port, edit `frontent/web/proxy.conf.json`.

## Build
```
cd frontent/web
npm run build
```

## Environments
- `src/environments/environment.development.ts` uses `/api` (proxy).
- `src/environments/environment.production.ts` uses `/api` (update if your API is hosted elsewhere).

## Backend endpoints used
- `/api/v1/auth/login`
- `/api/v1/auth/register`
- `/api/v1/auth/refresh`
- `/api/v1/auth/logout`
- `/api/v1/auth/me`
- `/api/v1/public/cms/pages/landing`
- `/api/v1/public/cms/pages/home`
- `/api/v1/public/cms/pages/about`
- `/api/v1/public/achievements`
- `/api/v1/academies/me`
- `/api/v1/branches`
- `/api/v1/programs`
- `/api/v1/courses`
- `/api/v1/levels`
- `/api/v1/groups`
- `/api/v1/groups/mine`
- `/api/v1/sessions`
- `/api/v1/sessions/mine`
- `/api/v1/students`
- `/api/v1/students/{id}/photo`
- `/api/v1/sessions/{sessionId}/attendance`
- `/api/v1/attendance`
- `/api/v1/parent/me/children`
- `/api/v1/parent/me/attendance`
- `/api/v1/parent/me/assignments`
- `/api/v1/parent/me/announcements`
- `/api/v1/instructor/assignments`
- `/api/v1/dashboards/admin`
- `/api/v1/dashboards/instructor`
- `/api/v1/dashboards/parent`
- `/api/v1/achievements`

## App routes
- Public
- `/`
- `/about`
- `/login`
- `/register`
- Admin
  - `/app/admin/academy`
  - `/app/admin/branches`
  - `/app/admin/programs`
  - `/app/admin/courses`
  - `/app/admin/levels`
  - `/app/admin/groups`
  - `/app/admin/sessions`
  - `/app/admin/students`
  - `/app/admin/students/:id`
  - `/app/admin/attendance`
- Instructor
  - `/app/instructor/groups`
  - `/app/instructor/sessions`
  - `/app/instructor/attendance`
  - `/app/instructor/assignments`
- Parent
  - `/app/parent/children`
  - `/app/parent/attendance`
  - `/app/parent/assignments`
  - `/app/parent/announcements`
