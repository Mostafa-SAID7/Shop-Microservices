# Identity Service — Feature Changelog

## v1.4.0 — MongoDB User Store Integration

### New: MongoDB Persistence (Production-Ready)
- `MongoUserStore` added — full `IUserStore` implementation backed by MongoDB (`identitydb → Users` collection).
- Inner `MongoUserDocument` class isolates MongoDB schema from the `User` domain model (Guid stored as `domainId` string).
- Unique indexes automatically created on `email` and `userName` fields.
- Auto-fallback: if `DatabaseSettings__ConnectionString` is **not** set, `InMemoryUserStore` is used (safe for unit/integration tests).
- Packages added: `MongoDB.Driver 2.25.0`, `AspNetCore.HealthChecks.MongoDb 8.0.0`.

### Infrastructure Updates
- Docker: `identity.api` now depends on `mongodb` container.
- Connection string: `mongodb://mongodb:27017/identitydb` via `DatabaseSettings__ConnectionString` env var.

---

## v1.3.0 — CI/CD & Test Reorganization

### Testing Structure
- Unit tests moved to `tests/Services/Identity/Identity.API.Tests/Unit/`
  - `TokenServiceTests.cs` — 4 tests (PBKDF2 hash, salt, JWT generation & verification)
  - `UserStoreTests.cs` — 3 tests (AddAsync, GetByEmailAsync, UpdateAsync)
- Integration tests: `tests/Services/Identity/Identity.API.Tests/Integration/`
  - `IdentityEndpointsTests.cs` — 5 HTTP endpoint tests using `WebApplicationFactory` with in-memory MassTransit bus

### CI/CD
- `ci.yml` GitHub Actions workflow: Build → Unit Tests → Integration Tests → Gate job
- Identity tests run in CI on every push/PR without requiring Docker or live RabbitMQ

---

## v1.0.0 — Initial Feature: Identity.API Microservice

### Summary
JWT-based authentication and user management microservice for the entire Shop-Microservices platform.

### Endpoints
| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/identity/register` | Register a new user — publishes `UserRegisteredEvent` |
| POST | `/api/identity/login` | Authenticate user — returns JWT token |
| GET  | `/api/identity/users/{id}` | Get user profile |
| PUT  | `/api/identity/users/{id}/profile` | Update name / username |
| PUT  | `/api/identity/users/{id}/change-password` | Change password (PBKDF2 verified) |
| GET  | `/api/identity/users` | List all users (Admin) |
| PUT  | `/api/identity/users/{id}/role` | Assign role: Customer / Admin / Manager |
| GET  | `/health` | Health check |

### Security
- **Password Hashing**: PBKDF2/SHA256 with random 16-byte salt, 100,000 iterations
- **JWT**: HS256, 24h expiry, includes `role` + OIDC-compatible claims
- **Middleware**: `UseAuthentication()` + `UseAuthorization()`

### Event Integration
- Publishes `UserRegisteredEvent` → RabbitMQ → `Notification.API`

### Infrastructure
- Docker port: `6068:8080`
- YARP route: `/identity-service/{**catch-all}`
- Health endpoint: `/health`

### Testing
- `dotnet build` — 0 errors ✅
