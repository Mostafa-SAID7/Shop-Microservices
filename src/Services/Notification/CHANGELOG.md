# Notification Service — Feature Changelog

## Feature: Notification.API Microservice

### Summary
New Notification microservice that subscribes to domain events via RabbitMQ
and dispatches emails and SMS notifications to customers.

### Event Consumers
| Event | Queue | Action |
|-------|-------|--------|
| `UserRegisteredEvent` | `user-registered-notification-queue` | Send welcome email |
| `CartCheckoutEvent` | `cart-checkout-notification-queue` | Send order confirmation email + SMS |

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
