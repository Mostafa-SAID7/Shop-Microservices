# API Gateway (YARP) — Feature Changelog

## Feature: YARP API Gateway Routes

### Summary
Configures the YARP Reverse Proxy API Gateway to route all microservices
through a single unified entry point.

### Routes Added
| Route ID | Match Path | Backend Cluster |
|----------|-----------|-----------------|
| `catalog-route` | `/catalog-service/{**catch-all}` | `catalog.api:8080` |
| `cart-route` | `/cart-service/{**catch-all}` | `cart.api:8080` |
| `ordering-route` | `/ordering-service/{**catch-all}` | `ordering.api:8080` |
| `payment-route` | `/payment-service/{**catch-all}` | `payment.api:8080` |
| `tracking-route` | `/tracking-service/{**catch-all}` | `tracking.api:8080` |
| `identity-route` | `/identity-service/{**catch-all}` | `identity.api:8080` |
| `notification-route` | `/notification-service/{**catch-all}` | `notification.api:8080` |

### Port Map
| Service | Port |
|---------|------|
| YARP Gateway | `6000` |
| Catalog API | `6001` |
| Cart API | `6003` |
| Ordering API | `6004` |
| Payment API | `6007` |
| Tracking API | `6006` |
| Identity API | `6068` |
| Notification API | `6069` |
