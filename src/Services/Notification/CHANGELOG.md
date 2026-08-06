# Notification Service — Feature Changelog

## v1.4.0 — MongoDB Audit Trail Integration

### New: MongoDB Notification Log Persistence
- `INotificationRepository` / `MongoNotificationRepository` service added.
- Every dispatched email and SMS is now persisted to **MongoDB** (`notificationdb → NotificationLogs` collection).
- Logs include: `eventType`, `recipient`, `channel` (Email/SMS), `subject`, `message`, `status`, `timestamp`, and event `metadata`.
- New REST endpoint: `GET /api/notifications/logs` — returns up to 50 recent notification logs sorted by timestamp.
- MongoDB health check added via `AspNetCore.HealthChecks.MongoDb`.

### Infrastructure Updates
- Docker service: `mongodb` container added (port `27017:27017`, volume `mongo_data:/data/db`).
- `notification.api` depends on `mongodb` and `messagebroker` in Docker Compose.
- Connection string: `mongodb://mongodb:27017/notificationdb` (via `DatabaseSettings__ConnectionString` env var).

---

## v1.0.0 — Initial Feature: Notification.API Microservice

### Summary
Notification microservice that subscribes to domain events via RabbitMQ
and dispatches emails and SMS notifications to customers.

### Event Consumers
| Event | Queue | Action |
|-------|-------|--------|
| `UserRegisteredEvent` | `user-registered-notification-queue` | Send welcome email + log to MongoDB |
| `CartCheckoutEvent` | `cart-checkout-notification-queue` | Send order confirmation email + SMS + log to MongoDB |

### Notification Templates
**Welcome Email** (on user registration):
- Greets user by first + last name
- Confirms account creation
- Includes User ID

**Order Confirmation Email** (on cart checkout):
- Full order summary: customer name, total, shipping address
- Payment card last 4 digits
- Estimated delivery note

**SMS Simulation** (on cart checkout):
- Logged as simulated outbound SMS

### Reliability
- Named RabbitMQ receive endpoints (not auto-generated queue names)
- Retry policy: **3 retries × 5 second interval**

### Infrastructure
- Docker port: `6069:8080`
- YARP route: `/notification-service/{**catch-all}`
- Status endpoint: `GET /api/notifications/status`
- Health endpoint: `GET /health`

### Testing
- `dotnet build` — 0 errors ✅

