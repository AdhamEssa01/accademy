# Academy

## Architecture
This solution follows a clean-architecture layout:
- **Academy.Domain**: Core business entities and rules (no dependencies).
- **Academy.Shared**: Cross-cutting/shared types (no dependencies).
- **Academy.Application**: Use cases and application services (depends on Domain + Shared).
- **Academy.Infrastructure**: External concerns (depends on Application + Domain + Shared).
- **Academy.Api**: ASP.NET Core entry point (depends on Application + Infrastructure + Shared).

## Build
```powershell
dotnet restore
dotnet build
```

## Test
```powershell
dotnet test
```

## Run
```powershell
dotnet run --project src/Academy.Api
```

In Development, the API applies migrations and seeds default data on startup.

## Migrations
Create a migration (from repo root):
```powershell
dotnet ef migrations add AddRefreshTokens -p src/Academy.Infrastructure -s src/Academy.Api -o Data/Migrations
```

## Development Database
The SQLite file is created at `academy_dev.db` in the working directory (repo root when running from the root).

## Dev Admin Credentials (DEV ONLY)
- Email: `admin@local.test`
- Password: `Admin123$`

## JWT Settings (DEV ONLY)
Set in `src/Academy.Api/appsettings.Development.json`:
- Jwt:Issuer
- Jwt:Audience
- Jwt:Key
- Jwt:AccessTokenMinutes
- Jwt:RefreshTokenDays

Replace the dev key before production deployments.

## Google Login (DEV ONLY)
Set in `src/Academy.Api/appsettings.Development.json`:
- GoogleAuth:ClientId

Client flow:
1) Client obtains a Google `id_token`.
2) Client POSTs to `/api/v1/auth/google` with `{ idToken, academyId? }`.
3) API validates the token and returns the standard AuthResponse.

## Auth Endpoints (v1)
- POST `/api/v1/auth/register`
- POST `/api/v1/auth/login`
- POST `/api/v1/auth/refresh`
- POST `/api/v1/auth/logout`
- POST `/api/v1/auth/google`
- GET  `/api/v1/auth/me`