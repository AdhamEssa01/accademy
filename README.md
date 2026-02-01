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
Local development uses SQL Server LocalDB by default.

SSMS connection:
- Server name: `(localdb)\MSSQLLocalDB`
- Database: `AcademyDev`

Apply migrations:
```powershell
dotnet ef database update -p src/Academy.Infrastructure -s src/Academy.Api
```

## Dev Admin Credentials (DEV ONLY)
- Email: `admin@local.test`
- Password: `Admin123$`

## JWT Settings (DEV ONLY)
Set in `src/Academy.Api/appsettings.Development.json`:
- Jwt:Issuer
- Jwt:Audience
- Jwt:AccessTokenMinutes
- Jwt:RefreshTokenDays

Set the signing key via user-secrets:
```powershell
dotnet user-secrets --project src/Academy.Api set "Jwt:Key" "dev-only-key-change-me"
```

For CI/containers, set `Jwt__Key` as an environment variable.

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
