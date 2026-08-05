# API Gateway (YARP) — Feature Changelog

## v1.3.0 — CI/CD Routing Verification

### CI/CD
- `ci.yml` now validates gateway-dependent builds on every push to `master`, `develop`, `feature/*`.
- `docker-build.yml` detects changes under `src/ApiGateways/**` and rebuilds only when modified.

---

## v1.0.0 — Initial Feature: YARP API Gateway Routes

### Summary
Configures the YARP Reverse Proxy API Gateway to route all 10 microservices
through a single unified entry point on port `6000`.

### Routes Added
| Route ID | Match Path | Backend Cluster | Port |
|----------|-----------|-----------------|------|
| `catalog-route` | `/catalog-service/{**catch-all}` | `catalog.api:8080` | 6001 |
| `cart-route` | `/cart-service/{**catch-all}` | `cart.api:8080` | 6003 |
| `ordering-route` | `/ordering-service/{**catch-all}` | `ordering.api:8080` | 6004 |
| `payment-route` | `/payment-service/{**catch-all}` | `payment.api:8080` | 6007 |
| `tracking-route` | `/tracking-service/{**catch-all}` | `tracking.api:8080` | 6006 |
| `identity-route` | `/identity-service/{**catch-all}` | `identity.api:8080` | 6068 |
| `notification-route` | `/notification-service/{**catch-all}` | `notification.api:8080` | 6069 |

### Complete Port Map
| Service | External Port | Internal Port |
|---------|--------------|---------------|
| **YARP Gateway** | **6000** | 8080 |
| Catalog API | 6001 | 8080 |
| Discount gRPC | 6002 | 8080 |
| Cart API | 6003 | 8080 |
| Ordering API | 6004 | 8080 |
| Shopping Web | 6005 / 6065 | 8080 |
| Tracking API | 6006 | 8080 |
| Payment API | 6007 | 8080 |
| Identity API | 6068 | 8080 |
| Notification API | 6069 | 8080 |

### Infrastructure
- Rate limiting configured
- Health check passthrough to all downstream services

### Testing
- `dotnet build` — 0 errors ✅
