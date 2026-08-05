# Identity Service — Feature Changelog

## Feature: Identity.API Microservice

### Summary
New Identity microservice providing JWT-based authentication and user management
for the entire Shop-Microservices platform.

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
| GET  | `/api/identity/health` | Health check |

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
