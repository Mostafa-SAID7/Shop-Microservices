# 🔬 Technical Deep Dive - Shop Microservices Architecture

---

## 1️⃣ Catalog Microservice

### Overview
- **Technology:** ASP.NET Core Minimal APIs (.NET 8)
- **Database:** PostgreSQL with Marten (Document DB)
- **Architecture:** Vertical Slice Architecture
- **Port:** 6000/6060

### Key Components

#### Database Layer
- **Marten 6.4.1:** Document database on PostgreSQL
  - Events stored as JSON documents
  - Full audit trail capability
  - Transactional guarantees

#### API Layer
- **Carter 8.0.0:** Minimal API endpoint definition
  - Clean endpoint organization
  - Reduces boilerplate code
  - Feature-based folder structure

#### Business Logic
- **MediatR:** CQRS implementation
  - Commands for state changes
  - Queries for reads
  - Decoupled handlers

#### Cross-Cutting Concerns
- **FluentValidation:** Input validation
- **Global Exception Handling:** Centralized error handling
- **Health Checks:** Service health monitoring
- **Structured Logging:** Diagnostic logging

### API Endpoints

```csharp
GET    /api/products              // List all products
GET    /api/products/{id}         // Get product details
POST   /api/products              // Create product (if applicable)
PUT    /api/products/{id}         // Update product
DELETE /api/products/{id}         // Delete product
```

### Event Publishing
```
Product Events → RabbitMQ → Other Microservices
```

### Database Connection
```
Connection String:
Server=catalogdb;Port=5432;Database=CatalogDb;User Id=postgres;Password=postgres
```

---

## 2️⃣ Cart Microservice

### Overview
- **Technology:** ASP.NET Core Web API (.NET 8)
- **Databases:** PostgreSQL (data) + Redis (cache)
- **Architecture:** REST API with Caching
- **Port:** 6001/6061

### Key Components

#### Data Layer
- **PostgreSQL:** Persistent cart storage
- **Marten:** Event sourcing for cart history
- **Redis:** Distributed cache for performance
  - Cache-Aside Pattern
  - TTL-based expiration

#### Service Integration
- **gRPC Client:** Calls Discount.Grpc service synchronously
  - Gets discount percentages
  - Updates final prices

#### Messaging
- **MassTransit:** Event bus abstraction
- **RabbitMQ:** Publishes BasketCheckout events
  - Other services subscribe to checkout events

#### Health Checks
```csharp
- NpgSQL (PostgreSQL connection)
- Redis connection
- Readiness probes
```

### Key Patterns

#### Cache-Aside Pattern
```
1. Check Redis cache
2. If miss:
   a. Load from PostgreSQL
   b. Store in Redis
   c. Return data
3. If hit: Return from cache
```

#### gRPC Calls
```csharp
GrpcSettings__DiscountUrl = "https://discount.grpc:8081"

// Client code
var discountResponse = await discountClient.GetDiscountAsync(productId);
```

#### Event Publishing
```csharp
// Create checkout event
var basketCheckoutEvent = new BasketCheckoutEvent { ... };

// Publish to RabbitMQ via MassTransit
await publishEndpoint.Publish(basketCheckoutEvent);
```

### Database Connections
```
PostgreSQL:
Server=cartdb;Port=5432;Database=CartDb;User Id=postgres;Password=postgres

Redis:
Host: distributedcache:6379 (no password required)
```

---

## 3️⃣ Discount Microservice

### Overview
- **Technology:** ASP.NET Core gRPC (.NET 8)
- **Database:** SQLite (file-based)
- **Protocol:** Protocol Buffers (Protobuf)
- **Port:** 6002/6062

### Key Components

#### gRPC Server
```protobuf
service DiscountProtoService {
  rpc GetDiscount (GetDiscountRequest) returns (DiscountModel);
}

message GetDiscountRequest {
  string productId = 1;
}

message DiscountModel {
  string productId = 1;
  string productName = 2;
  string description = 3;
  int32 amount = 4;
}
```

#### Data Layer
- **SQLite:** Lightweight embedded database
  - File stored in container
  - No server required
  - Perfect for small reference data

#### ORM
- **Entity Framework Core 8.0.2**
  - SQLite provider
  - Migrations support
  - Query capabilities

### Performance Characteristics
- **High-speed:** Binary protocol (Protobuf)
- **Low latency:** Direct connection (not HTTP)
- **Efficient:** Compact message format

### Typical Usage
```
Basket.API calls Discount.Grpc to:
1. Get product discount information
2. Calculate final basket price
3. Include discount in order submission
```

---

## 4️⃣ Ordering Microservice

### Overview
- **Technology:** ASP.NET Core Web API (.NET 8)
- **Database:** SQL Server (full relational DB)
- **Architecture:** Clean Architecture + DDD + CQRS
- **Port:** 6003/6063

### Layered Architecture

#### Domain Layer (`Ordering.Domain`)
- **Entities:** Order, OrderItem, OrderStatus
- **Value Objects:** OrderNumber, Address, ShippingAddress
- **Aggregates:** Order as root aggregate
- **Domain Events:** OrderCreated, OrderConfirmed, OrderShipped
- **Repository Interfaces:** Defined but not implemented here

```csharp
public class Order : Aggregate
{
    public OrderNumber OrderNumber { get; }
    public CustomerId CustomerId { get; }
    public OrderStatus Status { get; }
    public List<OrderItem> OrderItems { get; }
    public Address ShippingAddress { get; }
    public decimal TotalPrice { get; }
}
```

#### Application Layer (`Ordering.Application`)
- **CQRS Handlers:** MediatR command/query handlers
- **DTOs:** Data transfer objects for API contracts
- **Validations:** FluentValidation rules
- **Mappings:** Mapster for DTO ↔ Domain conversion
- **Exception Handling:** Custom exception classes

```csharp
// Command
public class CreateOrderCommand : ICommand<CreateOrderResult>
{
    public string CustomerId { get; set; }
    public List<OrderItemDto> OrderItems { get; set; }
    public AddressDto ShippingAddress { get; set; }
}

// Query
public class GetOrdersByCustomerQuery : IQuery<List<OrderDto>>
{
    public string CustomerId { get; set; }
}
```

#### Infrastructure Layer (`Ordering.Infrastructure`)
- **EF Core:** SQL Server context
- **Repository Implementation:** IOrderRepository
- **Unit of Work:** Transaction management
- **Migrations:** Database schema management
- **Message Broker:** RabbitMQ consumer configuration

```csharp
public class OrderContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
}
```

#### API Layer (`Ordering.API`)
- **Carter Modules:** Endpoint definitions
- **Health Checks:** SQL Server and RabbitMQ
- **Global Exception Handling:** Converts exceptions to HTTP responses
- **Startup Configuration:** DI container setup

### RabbitMQ Integration

#### Event Consumption
```csharp
// Subscribe to BasketCheckout event from Cart Service
public class BasketCheckoutEventConsumer : IConsumer<BasketCheckoutEvent>
{
    public async Task Consume(ConsumeContext<BasketCheckoutEvent> context)
    {
        // Create order from basket checkout
        var createOrderCommand = new CreateOrderCommand { ... };
        await mediator.Send(createOrderCommand);
    }
}
```

#### Event Publishing
```csharp
// Publish order events for other services to consume
await publishEndpoint.Publish(new OrderCreatedEvent { ... });
```

### Database Schema
```sql
Orders Table:
- OrderId (Primary Key)
- OrderNumber (Unique)
- CustomerId (Foreign Key)
- OrderDate
- Status (Enum)
- TotalPrice
- ShippingAddress
- BillingAddress

OrderItems Table:
- OrderItemId (Primary Key)
- OrderId (Foreign Key)
- ProductId
- Quantity
- Price
```

### Database Connection
```
Connection String:
Server=orderdb;Database=OrderDb;User Id=sa;Password=SwN12345678;Encrypt=False;TrustServerCertificate=True
```

### Auto-Migration
```csharp
// On application startup:
// 1. Check if database exists
// 2. If not, create it
// 3. Apply pending migrations
// 4. Seed initial data (if needed)
```

---

## 5️⃣ Tracking Microservice

### Overview
- **Technology:** ASP.NET Core Web API (.NET 8)
- **Database:** PostgreSQL
- **Architecture:** Event-Driven
- **Port:** 6006/6066

### Responsibilities
- Consumes order events from RabbitMQ
- Maintains order tracking history
- Provides order status updates to customers

### Event Flow
```
OrderCreated (from Ordering)
    ↓
Tracking.API receives event
    ↓
Creates tracking record in PostgreSQL
    ↓
Customers can check status via API
```

---

## 6️⃣ Payment Microservice

### Overview
- **Technology:** ASP.NET Core Web API (.NET 8)
- **Database:** SQL Server
- **Architecture:** Event-Driven
- **Port:** 6007/6067

### Responsibilities
- Consumes order creation events
- Processes payments
- Stores payment records
- Updates order status based on payment result

### Event Flow
```
OrderCreated (from Ordering)
    ↓
Payment.API receives event
    ↓
Processes payment through payment provider
    ↓
Updates order status (Paid/Failed)
    ↓
Publishes OrderPaid/OrderPaymentFailed event
```

---

## 7️⃣ YARP API Gateway

### Overview
- **Technology:** ASP.NET Core with YARP Reverse Proxy
- **Port:** 6004/6064
- **Purpose:** Unified entry point for all client requests

### Architecture Pattern
```
Client Request
    ↓
YARP Gateway (Port 6064)
    ├→ Rate Limiter (FixedWindowLimiter)
    │   └→ Max X requests per Y seconds
    ├→ Route Matcher
    ├→ Cluster Selection
    ├→ Load Balancing
    └→ Request Transformation
    ↓
Appropriate Microservice
```

### Configuration
```json
{
  "ReverseProxy": {
    "Routes": [
      {
        "RouteId": "catalog-route",
        "ClusterId": "catalog-cluster",
        "Match": {
          "Path": "/api/products{**catch-all}"
        }
      }
    ],
    "Clusters": [
      {
        "ClusterId": "catalog-cluster",
        "Destinations": {
          "catalog": {
            "Address": "https://catalog.api:8081"
          }
        }
      }
    ]
  }
}
```

### Rate Limiting
```csharp
// FixedWindowLimiter Configuration
options.GlobalLimiter = new FixedWindowRateLimiter(
    new FixedWindowRateLimiterOptions 
    { 
        Window = TimeSpan.FromSeconds(10),
        PermitLimit = 100 // 100 requests per 10 seconds
    }
);
```

### Benefits
1. **Single Entry Point:** Clients only need one URL
2. **Rate Limiting:** Prevent abuse
3. **Request Transformation:** Modify headers/paths
4. **Load Balancing:** Distribute traffic
5. **Security:** Authentication/authorization layer
6. **Caching:** Response caching

---

## 8️⃣ Shopping Web Application

### Overview
- **Technology:** ASP.NET Core Razor Pages (.NET 8)
- **UI Framework:** Bootstrap 4
- **HTTP Client:** Refit
- **Port:** 6005/6065

### Architecture
```
User Interface (Razor Pages + Bootstrap)
    ↓
Refit HTTP Client Factory
    ↓
YARP API Gateway
    ↓
Microservices
```

### Key Features

#### Refit Client
```csharp
[BaseAddress("http://gateway:8080")]
public interface IGatewayClient
{
    [Get("/api/products")]
    Task<IEnumerable<ProductDto>> GetProductsAsync();

    [Post("/api/baskets/{userId}/items")]
    Task AddToBasketAsync(string userId, [Body] AddItemRequest request);

    [Post("/api/orders")]
    Task<CreateOrderResponse> CreateOrderAsync([Body] CreateOrderRequest request);
}
```

#### Razor Pages
- **Index:** Product listing
- **ProductDetail:** Single product view
- **Basket:** Shopping cart
- **Order:** Checkout
- **OrderList:** Order history

#### Session Management
- Stores current user context
- Tracks basket items
- Maintains order history

---

## 🏗️ BuildingBlocks

### BuildingBlocks Library
Shared utilities and abstractions used by multiple services:
- Common exception types
- Base classes for entities
- Logging utilities
- Extension methods

### BuildingBlocks.Messaging Library
Event and command definitions shared across services:

```csharp
public class BasketCheckoutEvent : IntegrationEvent
{
    public string UserId { get; set; }
    public List<CartItem> CartItems { get; set; }
    public string FirstName { get; set; }
    public string LastName { get; set; }
    public string EmailAddress { get; set; }
    public string InvoiceAddress { get; set; }
    public string ShippingAddress { get; set; }
}
```

---

## 📡 Event-Driven Communication

### Message Broker: RabbitMQ

#### RabbitMQ Components
- **Exchange:** Topic exchange (publish/subscribe)
- **Queue:** Durable queues for each consumer
- **Routing:** Message routing based on topic

#### Flow
```
Service A publishes event
    ↓
Event sent to RabbitMQ Topic Exchange
    ↓
RabbitMQ routes to relevant queues
    ↓
Service B consumes from queue
    ↓
Service B processes event
    ↓
Service B publishes response event
```

### Event Types

#### BasketCheckout Event
```
Published by: Basket.API
Consumed by: Ordering.API, Tracking.API
Contains: User, items, addresses
```

#### OrderCreated Event
```
Published by: Ordering.API
Consumed by: Tracking.API, Payment.API
Contains: Order details
```

#### OrderPaid Event
```
Published by: Payment.API
Consumed by: Tracking.API, Ordering.API
Contains: Payment confirmation
```

### MassTransit Configuration
```csharp
// Publisher setup
var busControl = Bus.Factory.CreateUsingRabbitMq(cfg =>
{
    cfg.Host(host, port, virtualHost, h =>
    {
        h.Username(username);
        h.Password(password);
    });
    
    cfg.Publish<BasketCheckoutEvent>(x => x.ExchangeType = "topic");
});

// Consumer setup
endpointConfigurator.Consumer<BasketCheckoutEventConsumer>();
```

---

## 🔄 Service Communication Patterns

### Synchronous: gRPC
```
Client (Basket.API)
    ↓ (gRPC)
Server (Discount.Grpc)
    ↓ (Response)
Client receives immediate response
```

**Use cases:**
- Get discount information
- Need immediate response
- Tightly coupled services
- High performance required

### Asynchronous: RabbitMQ
```
Service A publishes event
    ↓ (RabbitMQ)
Event Queue
    ↓
Service B consumes at own pace
    ↓
Service B processes independently
```

**Use cases:**
- Order processing
- Long-running operations
- Loosely coupled services
- Resilience to failures

---

## 🗄️ Database Strategies

### PostgreSQL (Catalog, Cart, Tracking)
- **Use case:** Document storage, event sourcing
- **Advantages:** JSONB support, scalability, open source
- **Marten:** JSONB document database
- **Price:** Free, open source

### SQL Server (Ordering, Payment)
- **Use case:** Complex relational data, transactions
- **Advantages:** ACID compliance, powerful queries, reporting
- **EF Core:** Full ORM support
- **Price:** Enterprise features available

### SQLite (Discount)
- **Use case:** Small reference data
- **Advantages:** File-based, zero configuration
- **EF Core:** Full support
- **Price:** Free

### Redis (Cache)
- **Use case:** Distributed caching
- **Advantages:** High speed, TTL support
- **Use:** Cache-aside pattern
- **Price:** Free, cloud options available

---

## 🔐 Security Considerations

### Current Implementation
- No authentication/authorization (development mode)
- HTTP and HTTPS both supported
- Database credentials in environment variables

### Production Recommendations
1. **API Gateway Authentication:** OAuth2/OpenID Connect
2. **Service-to-Service:** mTLS or API keys
3. **Database:** Encrypted connections, managed identities
4. **Secrets Management:** Azure Key Vault or similar
5. **Rate Limiting:** Implement per-user limits
6. **Input Validation:** FluentValidation in place
7. **Logging:** Audit all transactions

---

## 🚀 Scalability Considerations

### Current Deployment
- Single instance of each service
- Shared databases
- Single RabbitMQ instance

### Scaling Strategies

#### Horizontal Scaling (Multiple instances)
```
Load Balancer
├→ Basket.API Instance 1
├→ Basket.API Instance 2
└→ Basket.API Instance 3

Each instance can serve different requests
RabbitMQ consumer groups handle distribution
```

#### Database Scaling
- **Read Replicas:** For read-heavy queries
- **Sharding:** Partition data by customer
- **Caching:** Redis for frequently accessed data

#### Message Queue Scaling
- **Consumer Groups:** Multiple instances consume same queue
- **Multiple Brokers:** RabbitMQ clustering
- **Topic Partitioning:** Different topics for different consumer groups

---

## 📊 Monitoring & Observability

### Health Checks
```csharp
// Each service exposes health checks
GET /health/live     // Liveness probe
GET /health/ready    // Readiness probe
GET /health/startup  // Startup probe
```

### Logging
```csharp
// Structured logging with Serilog pattern
logger.LogInformation("Order {OrderId} created for customer {CustomerId}", 
    orderId, customerId);
```

### Diagnostics Points
- API request/response timing
- Database query performance
- RabbitMQ message throughput
- Cache hit/miss ratios
- Service dependencies health

---

## 🔧 Configuration Management

### Environment Variables
Controlled via `docker-compose.override.yml`:
```yaml
- ASPNETCORE_ENVIRONMENT=Development
- ConnectionStrings__Database=...
- MessageBroker__Host=...
- GrpcSettings__DiscountUrl=...
```

### Configuration Files
- `appsettings.json`: Default configuration
- `appsettings.Development.json`: Dev overrides
- Environment variables: Runtime overrides

---

## 📦 NuGet Dependencies Summary

| Package | Version | Purpose |
|---------|---------|---------|
| MediatR | Latest | CQRS pattern |
| FluentValidation | Latest | Input validation |
| Carter | 8.0.0 | Minimal APIs |
| Marten | 6.4.1 | Document DB |
| MassTransit | Latest | Service bus |
| Refit | Latest | HTTP client |
| EF Core | 8.0.2 | ORM |
| Grpc.AspNetCore | 2.60.0 | gRPC server |
| StackExchangeRedis | 8.0.1 | Redis client |

---

## 🎯 Key Design Principles Applied

1. **SOLID Principles**
   - Single Responsibility
   - Open/Closed
   - Liskov Substitution
   - Interface Segregation
   - Dependency Inversion

2. **Domain-Driven Design**
   - Entities and Value Objects
   - Aggregates
   - Repository Pattern
   - Domain Events

3. **Architectural Patterns**
   - Microservices
   - API Gateway
   - Event-Driven Architecture
   - Cache-Aside Pattern

4. **CQRS**
   - Separation of read and write concerns
   - Optimized models for each operation
   - Independent scaling

5. **Vertical Slice Architecture**
   - Features organized by domain
   - Complete feature in one place
   - Easier to understand and modify

---

## 🔌 Extension Points

### Adding New Microservice
1. Create new project in Services folder
2. Define domain entities
3. Implement repository
4. Add API endpoints (Carter or Controllers)
5. Subscribe to events from RabbitMQ
6. Update docker-compose.yml
7. Add gateway routes in YARP

### Adding New Event
1. Define class in BuildingBlocks.Messaging
2. Update publishers (who sends)
3. Register consumer in service
4. Update RabbitMQ configuration

### Adding Database
1. Update docker-compose.yml
2. Add connection string in override
3. Create DbContext
4. Create migrations
5. Update service configuration

---

## ✅ Testing Strategy (Recommended)

### Unit Tests
- Test domain logic
- Test validators
- Test mappers

### Integration Tests
- Test repository with real database
- Test API endpoints with test server

### End-to-End Tests
- Docker Compose test environment
- Test complete workflows
- RabbitMQ event processing

---

**Generated:** August 5, 2026  
**Version:** 1.0  
**Status:** ✅ Complete Analysis

