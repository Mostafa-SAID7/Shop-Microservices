# 🚀 Quick Start Guide - Shop Microservices

**Status:** ✅ Solution built successfully with no errors (12 warnings - non-critical)  
**Framework:** .NET 8.0  
**Build Time:** ~2 minutes

---

## 📌 Current Status

### ✅ What's Ready
- ✅ Repository cloned successfully
- ✅ All 10 projects restored
- ✅ Solution compiled (Release mode)
- ✅ No build errors (only minor warnings)
- ✅ All dependencies downloaded

### ⚠️ What Needs Manual Action
- ⚠️ Docker Desktop needs to be **STARTED** manually
- ⚠️ Databases need to be initialized
- ⚠️ Services need to be started

---

## 🔴 CRITICAL: Start Docker Desktop First

**Docker is installed but NOT running.**

### How to Start Docker Desktop:

**Windows - Method 1 (Recommended):**
```powershell
# Option A: Click the Docker icon in system tray
# Look for Docker icon in bottom right corner → Click it → Wait 30-60 seconds

# Option B: Start from command line
Start-Process "C:\Program Files\Docker\Docker\Docker.exe"

# Wait for daemon to start (you'll see "Docker is running" notification)
```

**Verify Docker is Running:**
```powershell
docker ps
# Should return empty list (no containers yet), NOT an error
```

---

## 🐳 Option 1: Run All Services with Docker Compose (RECOMMENDED)

**This is the easiest way to run everything.**

### Step 1: Open PowerShell in src directory

```powershell
cd "Shop-Microservices\src"
```

### Step 2: Start All Services

```powershell
# Start all microservices and databases in background
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# Expected output:
# Creating network "src_default" with the default driver
# Creating catalogdb ... done
# Creating cartdb ... done
# Creating orderdb ... done
# Creating mongodb ... done
# Creating messagebroker ... done
# ... etc
```

### Step 3: Wait for Services to Initialize

```powershell
# Watch the logs
docker-compose logs -f

# Or check individual service
docker-compose logs catalog.api

# Press Ctrl+C to stop watching logs
```

**⏱️ Expected wait time: 1-2 minutes for all services to be ready**

### Step 4: Verify All Services

```powershell
# List all running containers
docker ps

# Should see running services:
# - catalogdb, cartdb, distributedcache, orderdb, messagebroker, mongodb
# - catalog.api, cart.api, discount.grpc, ordering.api, yarpapigateway, shopping.web
# - trackingdb, paymentdb, tracking.api, payment.api, identity.api, notification.api
```

### Step 5: Access Applications

| Service | URL | Purpose |
|---------|-----|---------|
| 🛒 **Shopping Web UI** | https://localhost:6065 | Main app - Browse products, add to cart, checkout |
| 🏛️ **API Gateway** | http://localhost:6064 | Unified API endpoint |
| 📊 **RabbitMQ Admin** | http://localhost:15672 | Message broker dashboard (guest/guest) |

### Step 6: Test the Application

1. **Open Shopping Web UI:** https://localhost:6065
2. **Browse Products** - Should see catalog items
3. **Add to Cart** - Add items to cart
4. **Checkout** - Process order
5. **Watch RabbitMQ** - Open http://localhost:15672 to see events being processed

---

## 💻 Option 2: Run Individual Services Locally (.NET CLI)

**For development - only works if databases are running in Docker**

### Step 1: Start Infrastructure Only

```powershell
cd "Shop-Microservices\src"

# Start only databases and message broker
docker-compose up -d catalogdb cartdb distributedcache orderdb messagebroker mongodb

# Wait 30 seconds for databases to initialize
```

### Step 2: Run Services Individually

**In separate terminal windows, run each service:**

```powershell
# Terminal 1 - Catalog API
cd "Shop-Microservices\src\Services\Catalog\Catalog.API"
dotnet run

# Terminal 2 - Cart API
cd "Shop-Microservices\src\Services\Basket\Basket.API"
dotnet run

# Terminal 3 - Discount gRPC
cd "Shop-Microservices\src\Services\Discount\Discount.Grpc"
dotnet run

# Terminal 4 - Ordering API
cd "Shop-Microservices\src\Services\Ordering\Ordering.API"
dotnet run

# Terminal 5 - API Gateway
cd "Shop-Microservices\src\ApiGateways\YarpApiGateway"
dotnet run

# Terminal 6 - Shopping Web
cd "Shop-Microservices\src\WebApps\Shopping.Web"
dotnet run
```

**Access at:** https://localhost:6065

---

## 🛑 Stopping All Services

```powershell
cd "Shop-Microservices\src"

# Stop all containers (data preserved)
docker-compose stop

# Stop and remove containers (data preserved)
docker-compose down

# Stop, remove, and delete all data
docker-compose down -v

# Remove all Docker images built for this project
docker-compose down -v --rmi all
```

---

## 🔧 Common Tasks

### View Service Logs

```powershell
# All logs
docker-compose logs -f

# Specific service
docker-compose logs catalog.api -f
docker-compose logs basket.api -f
docker-compose logs ordering.api -f

# Last 50 lines
docker-compose logs --tail=50
```

### Restart a Service

```powershell
docker-compose restart catalog.api

# Or stop and start
docker-compose stop catalog.api
docker-compose start catalog.api
```

### Rebuild Services

```powershell
# Rebuild specific service
docker-compose build catalog.api

# Rebuild and restart
docker-compose up -d --build catalog.api

# Rebuild all
docker-compose build --no-cache
docker-compose up -d
```

### Access Database CLI

```powershell
# PostgreSQL Catalog DB
docker exec -it catalogdb psql -U postgres -d CatalogDb

# SQL Server Order DB
docker exec -it orderdb sqlcmd -S localhost -U sa -P SwN12345678
```

### Check Service Health

```powershell
# Check which services are running
docker ps

# See all (including stopped)
docker ps -a

# Check service resource usage
docker stats

# View service logs for errors
docker-compose logs basket.api | Select-String "error"
```

---

## 📋 API Endpoints (via Gateway)

```
GET    http://localhost:6064/api/products                    # List all products
GET    http://localhost:6064/api/products/{id}               # Get product
GET    http://localhost:6064/api/baskets/{username}          # Get cart
POST   http://localhost:6064/api/baskets/{username}/items    # Add item to cart
POST   http://localhost:6064/api/orders                      # Create order
GET    http://localhost:6064/api/orders/{userId}             # Get user orders
```

### Example with Postman

1. Download Postman collection: `EShopMicroservices.postman_collection.json`
2. Import into Postman
3. Set environment variables (if any)
4. Start testing endpoints

---

## ⚠️ Troubleshooting

### Problem: "Docker daemon is not running"

**Solution:**
```powershell
# Start Docker Desktop
Start-Process "C:\Program Files\Docker\Docker\Docker.exe"

# Wait 1-2 minutes for daemon to start
Start-Sleep -Seconds 90

# Verify
docker ps
```

### Problem: "Port 6065 already in use"

**Solution:**
```powershell
# Find process using port
netstat -ano | findstr :6065

# Kill process (replace PID)
taskkill /PID 12345 /F

# Or just use docker-compose, which handles this
```

### Problem: "Connection refused" when accessing services

**Solution:**
```powershell
# Services take time to start, wait longer
Start-Sleep -Seconds 60

# Check logs
docker-compose logs shopping.web

# Verify container is running
docker ps | findstr shopping.web
```

### Problem: "Database connection failed"

**Solution:**
```powershell
# Check if database container is running
docker ps | findstr catalogdb

# If not, restart
docker-compose restart catalogdb

# Check logs
docker-compose logs catalogdb
```

### Problem: "gRPC connection failed" (Cart → Discount)

**Solution:**
```powershell
# Restart discount service
docker-compose restart discount.grpc

# Check if it's running
docker ps | findstr discount.grpc

# View logs
docker-compose logs discount.grpc
```

### Problem: Build failed with warnings

**Note:** Build succeeded with 12 warnings - these are non-critical (mostly null-checking).

---

## 🧪 Testing Workflow

### 1. Start All Services
```powershell
cd Shop-Microservices\src
docker-compose up -d
```

### 2. Wait for Services (60-90 seconds)
```powershell
# Watch logs until everything stabilizes
docker-compose logs -f
# Press Ctrl+C when stable
```

### 3. Open Web UI
```
https://localhost:6065
```

### 4. Test Product Browsing
- Page should load with products
- If not, check logs: `docker-compose logs catalog.api`

### 5. Test Cart Operations
- Add products to cart
- Modify quantities
- If not working, check: `docker-compose logs basket.api`

### 6. Test Checkout
- Complete order
- Should process without errors
- If not working, check: `docker-compose logs ordering.api`

### 7. Monitor Events
- Open RabbitMQ: http://localhost:15672
- Login: guest / guest
- Go to Queues tab
- Should see `BasketCheckout` events being processed

---

## 📊 Architecture Quick Reference

```
Customer
   ↓
Shopping.Web (Port 6065)
   ↓
YarpApiGateway (Port 6064) - Rate Limited
   ├→ Catalog.API (Port 6000)
   ├→ Basket.API (Port 6001)
   │  ├→ [Call] Discount.Grpc (Port 6002)
   │  └→ [Publish] RabbitMQ
   ├→ Ordering.API (Port 6003)
   │  └→ [Subscribe] RabbitMQ
   └→ Tracking.API (Port 6006)
      └→ [Subscribe] RabbitMQ

Databases:
  - PostgreSQL: Catalog (5432), Cart (5433), Tracking (5434)
  - SQL Server: Orders (1433), Payments (1434)
  - Redis: Cache (6379)
  - RabbitMQ: Messages (5672, Admin: 15672)
```

---

## 🎯 Key Features to Test

### Catalog Service
- [x] View all products
- [x] Product details with descriptions
- [x] Add/remove products

### Cart Service
- [x] Add items to cart
- [x] Update quantities
- [x] Remove items
- [x] Get discount from Discount service

### Ordering Service
- [x] Place orders
- [x] Process orders via RabbitMQ
- [x] Store orders in SQL Server

### Discount Service
- [x] Calculate discounts via gRPC
- [x] Fast inter-service communication

### API Gateway
- [x] Route requests to correct service
- [x] Rate limiting
- [x] Unified endpoint

### Shopping Web
- [x] Product listing
- [x] Shopping cart
- [x] Order management
- [x] Order tracking

---

## 📝 Configuration Files

### Key Files to Know

```
src/
├── docker-compose.yml              # Service definitions
├── docker-compose.override.yml      # Environment variables & ports
├── eshop-microservices.sln          # Solution file
├── Services/
│   ├── Catalog/Catalog.API/         # Catalog microservice
│   ├── Basket/Basket.API/           # Cart Microservice
│   ├── Discount/Discount.Grpc/      # Discount microservice
│   ├── Ordering/Ordering.API/       # Ordering microservice
│   ├── Tracking/Tracking.API/       # Tracking microservice
│   └── Payment/Payment.API/         # Payment microservice
├── ApiGateways/YarpApiGateway/      # API Gateway
├── WebApps/Shopping.Web/            # Web UI
└── BuildingBlocks/                  # Shared libraries
```

---

## 🔑 Database Credentials

| Database | User | Password | Port |
|----------|------|----------|------|
| PostgreSQL | postgres | postgres | 5432-5434 |
| SQL Server | sa | SwN12345678 | 1433-1434 |
| RabbitMQ | guest | guest | 5672 |

---

## 📞 Support Resources

- **GitHub:** https://github.com/Mostafa-SAID7/Shop-Microservices
- **Udemy Course:** https://www.udemy.com/course/microservices-architecture-and-implementation-on-dotnet/
- **Article:** https://medium.com/@mehmetozkaya/net-8-microservices-ddd-cqrs-vertical-clean-architecture-2dd7ebaaf4bd

---

## ✅ Next Steps

1. **Start Docker Desktop**
2. **Run:** `docker-compose up -d`
3. **Wait:** 60-90 seconds
4. **Open:** https://localhost:6065
5. **Test:** Browse products → Add to cart → Checkout
6. **Monitor:** RabbitMQ dashboard at http://localhost:15672

---

**Ready to start?** 🚀

Run this command:
```powershell
cd Shop-Microservices\src ; docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

Then open: https://localhost:6065

