# Docker Orchestration — Feature Changelog

## Feature: Docker Compose Full Orchestration

### Summary
Complete Docker Compose setup orchestrating all microservices, databases,
message broker, caching, and API gateway in a single `docker-compose up` command.

### Services in docker-compose.yml
| Service | Image | Port |
|---------|-------|------|
| `catalogdb` | postgres | 5432 |
| `cartdb` | postgres | 5433 |
| `orderdb` | mcr.microsoft.com/mssql/server | 1433 |
| `trackingdb` | postgres | 5434 |
| `paymentdb` | mcr.microsoft.com/mssql/server | 1435 |
| `distributedcache` | redis | 6379 |
| `messagebroker` | rabbitmq:management | 5672, 15672 |
| `catalog.api` | catalogapi | 6001/6061 |
| `cart.api` | cartapi | 6003/6063 |
| `discount.grpc` | discountgrpc | 6002/6062 |
| `ordering.api` | orderingapi | 6004/6064 |
| `tracking.api` | trackingapi | 6006/6066 |
| `payment.api` | paymentapi | 6007/6067 |
| `identity.api` | identityapi | **6068** |
| `notification.api` | notificationapi | **6069** |
| `yarpapigateway` | yarpapigateway | 6000/6060 |
| `shopping.web` | shoppingweb | 6005/6065 |

### Start the whole platform
```bash
cd src
docker-compose up -d
```

### Environment Overrides (docker-compose.override.yml)
- All services use `ASPNETCORE_ENVIRONMENT=Development`
- Database connection strings injected via environment variables
- RabbitMQ credentials: `guest/guest`
- JWT Secret injected via env for Identity.API
