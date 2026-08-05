# Shop Microservices - Complete Analysis & Build Guide

## 📋 Project Overview

This is a comprehensive .NET 8 microservices e-commerce platform demonstrating modern architecture patterns including DDD, CQRS, Vertical Slice Architecture, and Event-Driven Communication.

**Repository:** https://github.com/Mostafa-SAID7/Shop-Microservices

---

## 🏗️ Architecture Overview

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                    Shopping Web UI (Port 6065)              │
│                  (ASP.NET Core Razor App)                   │
└────────────┬────────────────────────────────────────────────┘
             │
             ▼
┌─────────────────────────────────────────────────────────────┐
│            YARP API Gateway (Port 6064 HTTPS)               │
│         (Reverse Proxy with Rate Limiting)                  │
└──┬──────────────┬──────────────┬────────────┬──────────────┘
   │              │              │            │
   ▼              ▼              ▼            ▼
┌────────┐  ┌────────┐  ┌────────┐  ┌────────────┐
│Catalog│  │Basket  │  │Order   │  │Tracking    │
│API    │  │API     │  │API     │  │API         │
│6060   │  │6061    │  │6063    │  │6066        │
└────┬──┘  └──┬─────┘  └──┬─────┘  └──┬─────────┘
     │         │          │           │
  gRPC+       Sync         │         Async
  Async       gRPC         │        Events
     │         │          │           │
     ▼         ▼          ▼           ▼
  ┌─────────────────────────────────────────┐
  │          RabbitMQ Message Broker        │
  │      (Event-Driven Communication)       │
  │   Admin Portal: localhost:15672         │
  └─────────────────────────────────────────┘
     │         │          │           │
     ▼         ▼          ▼           ▼
  ┌────────┐ ┌─────────┐ ┌──────┐ ┌────────┐
  │Catalog │ │Basket   │ │Order │ │Payment │
  │ DB     │ │ DB      │ │ DB   │ │ DB     │
  │PostgreS│ │PostgreS │ │MSSQL │ │MSSQL   │
  │5432    │ │5433     │ │1433  │ │1434    │
  └────────┘ └─────────┘ └──────┘ └────────┘

Additional:
- Redis Cache (Port 6379) - Distributed Cache for Cart
- Discount gRPC (Port 6062) - Synchronous Service
```

---

## 📦 Microservices Breakdown

### 1. **Catalog API** (Port 6060)
- **Framework:** ASP.NET Core Web API (.NET 8)
- **Database:** PostgreSQL (Port 5432)
- **Architecture:** Vertical Slice Architecture
- **Key Features:**
  - Minimal APIs with Carter
  - CQRS with MediatR
  - Marten (Document DB on PostgreSQL)
  - Health Checks
  - Async Event Publishing to RabbitMQ

### 2. **Cart API** (Port 6061)
- **Framework:** ASP.NET Core Web API (.NET 8)
- **Database:** PostgreSQL (Port 5433)
- **Cache:** Redis (Port 6379)
- **Architecture:** REST API with Caching
- **Key Features:**
  - Redis Distributed Cache
  - Cache-Aside Pattern
  - gRPC Client (calls Discount Service)
  - Marten for event storage
  - MassTransit for Publishing Events
  - Health Checks

### 3. **Discount gRPC** (Port 6062)
- **Framework:** ASP.NET Core gRPC (.NET 8)
- **Database:** SQLite
- **Architecture:** gRPC Server
- **Key Features:**
  - High-Performance Inter-Service Communication
  - Protobuf Serialization
  - Entity Framework Core
  - Lightweight SQLite database

### 4. **Ordering API** (Port 6063)
- **Framework:** ASP.NET Core Web API (.NET 8)
- **Database:** SQL Server (Port 1433)
- **Architecture:** Clean Architecture (Domain → Application → Infrastructure)
- **Key Features:**
  - DDD (Domain-Driven Design)
  - CQRS Implementation
  - MassTransit RabbitMQ Consumer
  - Entity Framework Core with Auto-Migration
  - Complex Domain Logic
  - Feature Toggle Support

### 5. **Tracking API** (Port 6066) - NEW
- **Framework:** ASP.NET Core Web API (.NET 8)
- **Database:** PostgreSQL (Port 5434)
- **Architecture:** Event-Driven
- **Key Features:**
  - RabbitMQ Event Consumer
  - Order Tracking Functionality

### 6. **Payment API** (Port 6067) - NEW
- **Framework:** ASP.NET Core Web API (.NET 8)
- **Database:** SQL Server (Port 1434)
- **Architecture:** Event-Driven
- **Key Features:**
  - RabbitMQ Event Consumer
  - Payment Processing

### 7. **YARP API Gateway** (Port 6064)
- **Framework:** ASP.NET Core (.NET 8)
- **Architecture:** Reverse Proxy Pattern
- **Key Features:**
  - Route Configuration
  - Cluster Management
  - Rate Limiting (FixedWindowLimiter)
  - Request/Response Transformation
  - Unified API Endpoint

### 8. **Shopping Web UI** (Port 6065)
- **Framework:** ASP.NET Core Web App (.NET 8)
- **Templating:** Razor Pages + Bootstrap 4
- **Key Features:**
  - Calls APIs via Yarp Gateway
  - Refit for HTTP Client Factory
  - Product Browsing, Cart Management
  - Order Checkout

---

## 🗄️ Databases & Infrastructure

| Service | Database | Type | Port | Purpose |
|---------|----------|------|------|---------|
| Catalog | PostgreSQL | Relational | 5432 | Product Catalog Storage |
| Basket | PostgreSQL | Relational | 5433 | Cart Data |
| Discount | SQLite | Embedded | - | Discount Rules (File-based) |
| Ordering | SQL Server | Relational | 1433 | Orders & Business Logic |
| Tracking | PostgreSQL | Relational | 5434 | Order Tracking |
| Payment | SQL Server | Relational | 1434 | Payment Records |
| Cache | Redis | In-Memory | 6379 | Distributed Cache |
| Message Broker | RabbitMQ | Message Queue | 5672, 15672 | Event Streaming |

---

## 📚 Key Technologies & Libraries

### Core Framework
- **.NET 8** - Latest LTS version
- **C# 12** - Modern language features

### Data Access
- **Entity Framework Core 8.0.2** - ORM
- **Marten 6.4.1** - Document DB on PostgreSQL
- **SQL Server, PostgreSQL, SQLite** - Various databases

### API Design
- **Carter 8.0.0** - Minimal API endpoint definition
- **Yarp** - Reverse Proxy
- **gRPC** - High-performance RPC
- **Refit** - HTTP client factory

### Architecture & Design Patterns
- **MediatR** - CQRS Pattern
- **FluentValidation** - Validation Pipeline
- **Scrutor 4.2.2** - Dependency Injection extensions
- **MassTransit** - Service Bus abstraction

### Message Bus & Events
- **RabbitMQ** - Message Broker
- **MassTransit** - Service Bus patterns
- **Event-Driven Architecture**

### Monitoring & Health
- **AspNetCore.HealthChecks.*** - Health check packages
- **Global Exception Handling**
- **Structured Logging**

### Caching
- **Redis** - Distributed Cache
- **StackExchangeRedis** - Redis client

### Containerization
- **Docker & Docker Compose**
- **Linux containers**

---

## ✅ Prerequisites

### System Requirements

| Requirement | Version | Status | Notes |
|------------|---------|--------|-------|
| .NET SDK | 8.0 or later | ✅ INSTALLED (10.0.301) | Supports all microservices |
| Docker Desktop | Latest | ⚠️ INSTALLED BUT NOT RUNNING | Need to start it manually |
| Visual Studio 2022 | Latest | Optional | Can use VS Code + CLI |
| RAM | 4GB minimum | - | Docker Compose requires memory |
| Disk Space | 10GB+ | - | For Docker images & containers |

### Installation Steps Needed

1. **Start Docker Desktop** (Currently NOT running)
   - Click Docker Desktop icon in system tray
   - Wait for daemon to start (~30-60 seconds)
   - Verify: Run `docker ps` in terminal

2. **Check Network Ports** (Should be available)
   - Port 5432, 5433, 5434 (PostgreSQL)
   - Port 1433, 1434 (SQL Server)
   - Port 6379 (Redis)
   - Port 5672, 15672 (RabbitMQ)
   - Port 6000-6067 (Application ports)

---

## 🚀 Build & Run Instructions

### Option 1: Docker Compose (Recommended - All Services)

```powershell
# Navigate to src directory
cd Shop-Microservices\src

# Start all microservices and databases
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Monitor logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes
docker-compose down -v
```

**Expected Output:**
- 8 services running in containers
- All databases initialized
- RabbitMQ ready for messages
- Redis cache available

### Option 2: Visual Studio Solution (.sln)

```powershell
# Open solution in Visual Studio
.\src\eshop-microservices.sln

# In Visual Studio:
# 1. Right-click "docker-compose" project
# 2. Select "Set as Startup Project"
# 3. Run (F5) without debugging
```

### Option 3: CLI Build & Run Individual Services

```powershell
# Restore dependencies
dotnet restore .\src\eshop-microservices.sln

# Build entire solution
dotnet build .\src\eshop-microservices.sln

# Run individual services (after databases are running)
cd .\src\Services\Catalog\Catalog.API
dotnet run

# In another terminal
cd .\src\Services\Basket\Basket.API
dotnet run
```

---

## 🔍 Project Structure

```
Shop-Microservices/
├── src/
│   ├── Services/
│   │   ├── Catalog/
│   │   │   └── Catalog.API/
│   │   ├── Basket/
│   │   │   └── Basket.API/
│   │   ├── Discount/
│   │   │   └── Discount.Grpc/
│   │   ├── Ordering/
│   │   │   ├── Ordering.API/
│   │   │   ├── Ordering.Application/
│   │   │   ├── Ordering.Domain/
│   │   │   └── Ordering.Infrastructure/
│   │   ├── Tracking/
│   │   │   └── Tracking.API/
│   │   └── Payment/
│   │       └── Payment.API/
│   ├── ApiGateways/
│   │   └── YarpApiGateway/
│   ├── WebApps/
│   │   └── Shopping.Web/
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks/          (Common utilities)
│   │   └── BuildingBlocks.Messaging/ (Event definitions)
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   └── eshop-microservices.sln
├── .git/
├── README.md
└── LICENSE
```

---

## 🧪 Testing the System

### 1. Check All Services Status

```powershell
# List running containers
docker ps

# Check service health
docker-compose logs

# Test specific service
curl http://localhost:6000/api/products  # Catalog API
```

### 2. Access Web Interfaces

| Service | URL | Purpose |
|---------|-----|---------|
| Shopping Web UI | https://localhost:6065 | Main application |
| RabbitMQ Admin | http://localhost:15672 | Message broker (guest/guest) |
| API Gateway | http://localhost:6064 | Unified API endpoint |

### 3. API Endpoints (via Yarp Gateway)

```
GET  http://localhost:6064/api/products              # List products
POST http://localhost:6064/api/baskets               # Create basket
GET  http://localhost:6064/api/discounts/{productId} # Get discount
POST http://localhost:6064/api/orders                # Create order
```

### 4. Monitor Message Queue

- Open RabbitMQ Dashboard: http://localhost:15672
- Username: guest
- Password: guest
- Watch messages in queues during checkout

---

## ⚠️ Important Configuration

### Environment Variables

All services use these environment variables (configured in docker-compose.override.yml):

```yaml
# Database Connections
ConnectionStrings__Database=Server=xxx;...
ConnectionStrings__Redis=distributedcache:6379

# Message Broker
MessageBroker__Host=amqp://ecommerce-mq:5672
MessageBroker__UserName=guest
MessageBroker__Password=guest

# gRPC Settings
GrpcSettings__DiscountUrl=https://discount.grpc:8081

# API Configuration
ApiSettings__GatewayAddress=http://yarpapigateway:8080
```

### Database Credentials

| Database | User | Password | Port |
|----------|------|----------|------|
| PostgreSQL | postgres | postgres | 5432-5434 |
| SQL Server | sa | SwN12345678 | 1433-1434 |
| RabbitMQ | guest | guest | 5672 |

---

## 🔧 Common Tasks

### Restart All Services

```powershell
cd src
docker-compose restart
```

### View Logs for Specific Service

```powershell
docker-compose logs catalog.api -f
docker-compose logs basket.api -f
```

### Clear and Rebuild

```powershell
cd src
docker-compose down -v
docker-compose up -d --build
```

### Access Database Directly

```powershell
# PostgreSQL
docker exec -it catalogdb psql -U postgres -d CatalogDb

# SQL Server
docker exec -it orderdb sqlcmd -S localhost -U sa -P SwN12345678
```

### Stop Individual Service

```powershell
docker-compose stop catalog.api
docker-compose start catalog.api
```

---

## 📊 Service Dependencies

```
Shopping.Web
    ↓
YarpApiGateway
    ├→ Catalog.API
    ├→ Basket.API → Discount.Grpc
    ├→ Ordering.API ← (consumes events from RabbitMQ)
    └→ Tracking.API ← (consumes events from RabbitMQ)

Event Flow:
Basket.API ---publishes BasketCheckout--→ RabbitMQ
                                            ├→ Ordering.API (subscribe)
                                            └→ Tracking.API (subscribe)
```

---

## 🚨 Troubleshooting

### Docker Not Running
**Solution:** Start Docker Desktop from system tray

### Port Already in Use
```powershell
# Find process using port
netstat -ano | findstr :6065

# Kill process
taskkill /PID <PID> /F
```

### Connection Timeout to Database
**Solution:** Wait 30-60 seconds for databases to initialize

### RabbitMQ Connection Failed
**Solution:** Ensure messagebroker service is running
```powershell
docker-compose logs messagebroker
```

### Out of Memory
**Solution:** Increase Docker Desktop memory to 4GB+ in settings

---

## 📝 Key Architectural Patterns

1. **Vertical Slice Architecture** - Features organized by domain
2. **CQRS** - Command Query Responsibility Segregation
3. **DDD** - Domain-Driven Design (Ordering service)
4. **Event-Driven Architecture** - RabbitMQ async communication
5. **Microservices Pattern** - Independent services with own data
6. **API Gateway Pattern** - Yarp for unified entry point
7. **Circuit Breaker & Resilience** - Built into MassTransit
8. **Health Check Pattern** - Liveness & readiness probes

---

## 🎓 Learning Resources

- **Repository:** https://github.com/Mostafa-SAID7/Shop-Microservices
- **Course:** https://www.udemy.com/course/microservices-architecture-and-implementation-on-dotnet/
- **Medium Article:** https://medium.com/@mehmetozkaya/net-8-microservices-ddd-cqrs-vertical-clean-architecture-2dd7ebaaf4bd

---

## 📋 Checklist for First Run

- [ ] Docker Desktop installed and running
- [ ] .NET 8 SDK installed (`dotnet --version` ≥ 8.0)
- [ ] Ports 5432, 5433, 1433, 6379, 5672 available
- [ ] Run `docker-compose up -d` in `src/` directory
- [ ] Wait 60 seconds for all services to initialize
- [ ] Access https://localhost:6065 in browser
- [ ] Check RabbitMQ dashboard: http://localhost:15672
- [ ] Browse products and test checkout
- [ ] Verify order appears in database

---

Generated: August 5, 2026
Version: v1.0
