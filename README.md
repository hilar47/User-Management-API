# User Management API (.NET 8)

A REST API for managing users, built with ASP.NET Core 8 controllers.

## Features
- **CRUD**: `GET /api/users`, `GET /api/users/{id}`, `POST /api/users`, `PUT /api/users/{id}`, `DELETE /api/users/{id}`
- **Validation**: DataAnnotations on request DTOs (name length, email format, age 18-120, phone regex, valid role) -> automatic `400`; duplicate email -> `409`
- **Middleware**
  - `ExceptionHandlingMiddleware` - maps errors to ProblemDetails (404 / 409 / 500)
  - `RequestLoggingMiddleware` - logs method, path, status, duration + `X-Correlation-ID`
  - `ApiKeyAuthMiddleware` - requires `X-API-KEY` header (Swagger and `/health` are public)
- Paging + search: `GET /api/users?page=1&pageSize=10&search=ada`
- Swagger UI with API-key support
- xUnit integration tests
- In-memory storage behind `IUserService` (easy to swap for EF Core)

## Run
```bash
dotnet restore
dotnet run --project src/UserManagementApi
# Swagger: http://localhost:5080/swagger
```
Click **Authorize** in Swagger and enter `dev-secret-key-change-me` (set in `appsettings.json`;
override with the env var `Authentication__ApiKey` in real deployments).

## Test
```bash
dotnet test
```

## Layout
```
src/UserManagementApi/
  Controllers/   UsersController
  DTOs/          request/response models + validation attributes
  Exceptions/    NotFoundException, ConflictException
  Middleware/    logging, API-key auth, exception handling
  Models/        User, UserRole
  Services/      IUserService, UserService
tests/UserManagementApi.Tests/   integration tests
docs/COPILOT_DEBUGGING.md        Copilot debugging guide
```

## Sample request
```bash
curl -X POST http://localhost:5080/api/users \
  -H "X-API-KEY: dev-secret-key-change-me" -H "Content-Type: application/json" \
  -d '{"fullName":"Ada Lovelace","email":"ada@example.com","age":36,"role":2}'
```
