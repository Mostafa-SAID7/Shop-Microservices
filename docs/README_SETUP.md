# 🎯 Shop Microservices - Setup & Deployment Guide

**Current Status:** ✅ **READY FOR DEPLOYMENT**

---

## ⚡ TL;DR - Quick Start (3 steps)

### Step 1: Start Docker Desktop
```powershell
# Open Docker Desktop (click icon in system tray)
# OR run:
Start-Process "C:\Program Files\Docker\Docker\Docker.exe"

# Wait 1-2 minutes, then verify:
docker ps
```

### Step 2: Start Services
```powershell
cd Shop-Microservices\src
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

### Step 3: Access Web UI
```
Open in browser: https://localhost:6065
```

**That's it!** The system will initialize in 1-2 minutes.

---

## 📊 What's Included

### ✅ Already Done
- Repository cloned and ready
- Solution built successfully (0 errors)
- All dependencies resolved
- Ready to deploy

### ⚠️ Manual Required
- **Start Docker Desktop** (see Step 1 above)
- **Run docker-compose** (see Step 2 above)

### ❌ Not Needed
- Install .NET SDK (already at 10.0.301)
- Fix build errors (build succeeded)
- Configure databases (automated)

---

## 📚 Documentation Files

| Document | Purpose |
|----------|---------|
| **BUILD_STATUS_REPORT.md** | Detailed build results & deployment checklist |
| **STARTUP_GUIDE.md** | Step-by-step startup instructions with troubleshooting |
| **PROJECT_ANALYSIS.md** | Complete project overview & architecture |
| **TECHNICAL_DETAILS.md** | Deep dive into each microservice & technologies |
| **README_SETUP.md** | This file - quick reference |

---

## 🏗️ Architecture at a Glance

```
┌─────────────────────────────────────────┐
│     Shopping Web UI (Port 6065)         │
│   Browse Products → Add Cart → Checkout │
└─────────────┬───────────────────────────┘
              │
┌─────────────▼───────────────────────────┐
│  YARP API Gateway (Port 6064) - Routes  │
│            & Rate Limiting              │
└─────────────┬───────────────────────────┘
              │
    ┌─────────┼─────────┬─────────┐
    │         │         │         │
    ▼         ▼         ▼         ▼
  Catalog  Basket   Ordering  Tracking
  (6060)   (6061)   (6063)    (6066)
    │         │         │         │
    └─────────┼─────────┼─────────┘
              ▼
    ┌─────────────────────────┐
    │  RabbitMQ Message Queue │
    │ Event-Driven Async Work │
    └─────────────────────────┘
    
Databases:
- PostgreSQL (Catalog, Basket, Tracking)
- SQL Server (Orders, Payments)
- Redis (Distributed Cache)
- SQLite (Discount)
```

---

## 🚀 Services Overview

| Service | Port | Type | Database | Purpose |
|---------|------|------|----------|---------|
| **Catalog** | 6060 | REST API | PostgreSQL | Product catalog management |
| **Basket** | 6061 | REST API | PostgreSQL + Redis | Shopping cart with caching |
| **Discount** | 6062 | gRPC | SQLite | Fast discount calculations |
| **Ordering** | 6063 | REST API | SQL Server | Order processing & DDD |
| **Tracking** | 6066 | REST API | PostgreSQL | Order status tracking |
| **Payment** | 6067 | REST API | SQL Server | Payment processing |
| **Gateway** | 6064 | Proxy | N/A | Unified API endpoint |
| **Web UI** | 6065 | Web | N/A | Shopping application |

---

## 📋 System Requirements

| Component | Required | Installed | Status |
|-----------|----------|-----------|--------|
| Docker Desktop | Latest | ✅ 29.6.1 | ⚠️ Not running |
| .NET SDK | 8.0+ | ✅ 10.0.301 | ✅ Ready |
| RAM | 4GB+ | ✅ Available | ✅ Ready |
| Disk Space | 10GB+ | ✅ Available | ✅ Ready |
| Windows | 10/11 | ✅ Yes | ✅ Ready |

---

## 🎯 What to Test After Startup

1. **Browse Products** (Catalog Service)
   - Open https://localhost:6065
   - Should see product list
   
2. **Add to Basket** (Basket Service + Discount gRPC)
   - Add items to cart
   - Should apply discounts automatically

3. **Checkout** (Ordering Service + RabbitMQ Events)
   - Complete order
   - Should process in background

4. **Monitor Events** (RabbitMQ Dashboard)
   - Open http://localhost:15672
   - Login: guest / guest
   - Should see order events in queues

---

## 🔧 Common Commands

### Docker Management
```powershell
# Start all services
docker-compose up -d

# View running services
docker ps

# View logs
docker-compose logs -f

# Stop all services
docker-compose stop

# Restart specific service
docker-compose restart catalog.api

# View service logs
docker-compose logs catalog.api -f

# Stop and remove everything
docker-compose down -v
```

### Service Health
```powershell
# Check all services
docker-compose ps

# Check specific service logs
docker-compose logs ordering.api

# Monitor resource usage
docker stats

# Check network connectivity
docker-compose exec catalog.api ping basketdb
```

---

## ⚠️ Troubleshooting Quick Guide

### Docker Not Running
**Error:** "Docker daemon is not running"
```powershell
# Start it
Start-Process "C:\Program Files\Docker\Docker\Docker.exe"

# Wait 1-2 minutes, then:
docker ps
```

### Port Already in Use
**Error:** "Port 6065 already in use"
```powershell
# Find what's using the port
netstat -ano | findstr :6065

# Kill the process (replace PID with actual number)
taskkill /PID 12345 /F
```

### Services Not Starting
**Error:** "Connection refused" or "Service unavailable"
```powershell
# Give it more time (1-2 minutes)
Start-Sleep -Seconds 120

# Check logs
docker-compose logs

# Restart the service
docker-compose restart catalog.api
```

### Database Connection Failed
**Error:** "Cannot connect to database"
```powershell
# Check database container is running
docker ps | findstr postgres

# Check logs
docker-compose logs catalogdb

# Restart database
docker-compose restart catalogdb

# Wait 30 seconds then retry
```

---

## 🌐 URLs After Startup

| Service | URL | Purpose |
|---------|-----|---------|
| Shopping App | https://localhost:6065 | Main application |
| API Gateway | http://localhost:6064 | API endpoint |
| RabbitMQ | http://localhost:15672 | Message broker (guest/guest) |
| Catalog API | http://localhost:6000 | Direct catalog API |
| Basket API | http://localhost:6001 | Direct basket API |
| Ordering API | http://localhost:6003 | Direct ordering API |

---

## 📝 Database Credentials

### PostgreSQL
- **Servers:** catalogdb, basketdb, trackingdb
- **Username:** postgres
- **Password:** postgres
- **Ports:** 5432, 5433, 5434

### SQL Server
- **Servers:** orderdb, paymentdb
- **Username:** sa
- **Password:** SwN12345678
- **Ports:** 1433, 1434

### Redis
- **Host:** distributedcache
- **Port:** 6379
- **Password:** None

### RabbitMQ
- **Host:** messagebroker
- **Port:** 5672 (AMQP), 15672 (Admin)
- **Username:** guest
- **Password:** guest

---

## 🎓 Learning Resources

### Official Documentation
- **Project:** https://github.com/Mostafa-SAID7/Shop-Microservices
- **Course:** https://www.udemy.com/course/microservices-architecture-and-implementation-on-dotnet/
- **Article:** https://medium.com/@mehmetozkaya

### Technology Docs
- **Docker:** https://docs.docker.com/
- **.NET 8:** https://docs.microsoft.com/en-us/dotnet/
- **RabbitMQ:** https://www.rabbitmq.com/
- **PostgreSQL:** https://www.postgresql.org/
- **SQL Server:** https://docs.microsoft.com/en-us/sql/

---

## 📊 Project Structure

```
Shop-Microservices/
├── src/
│   ├── Services/
│   │   ├── Catalog/
│   │   ├── Basket/
│   │   ├── Discount/
│   │   ├── Ordering/ (with Domain, Application, Infrastructure layers)
│   │   ├── Tracking/
│   │   └── Payment/
│   ├── ApiGateways/
│   │   └── YarpApiGateway/
│   ├── WebApps/
│   │   └── Shopping.Web/
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks/ (common utilities)
│   │   └── BuildingBlocks.Messaging/ (shared events)
│   ├── docker-compose.yml
│   ├── docker-compose.override.yml
│   └── eshop-microservices.sln
├── BUILD_STATUS_REPORT.md
├── STARTUP_GUIDE.md
├── PROJECT_ANALYSIS.md
├── TECHNICAL_DETAILS.md
└── README_SETUP.md (this file)
```

---

## ✅ Pre-Flight Checklist

Before starting services:
- [ ] Docker Desktop installed
- [ ] Docker Desktop will be started (Step 1 above)
- [ ] .NET 8 SDK available
- [ ] Ports 5432-5434, 6000-6067 are free
- [ ] At least 4GB RAM available
- [ ] At least 10GB free disk space

---

## 🚀 Ready?

### Start Here:
```powershell
# 1. Start Docker Desktop (if not running)
Start-Process "C:\Program Files\Docker\Docker\Docker.exe"
Start-Sleep -Seconds 90

# 2. Navigate to src directory
cd Shop-Microservices\src

# 3. Start all services
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# 4. Wait for services to initialize (1-2 minutes)
Start-Sleep -Seconds 90

# 5. Open in browser
Start-Process "https://localhost:6065"
```

---

## 📞 Support

- **Build Issues:** Check `BUILD_STATUS_REPORT.md`
- **Startup Issues:** Check `STARTUP_GUIDE.md`
- **Architecture Questions:** Check `TECHNICAL_DETAILS.md`
- **Detailed Info:** Check `PROJECT_ANALYSIS.md`

---

## 🎉 You're All Set!

The project is **fully built and ready to deploy**. Just follow the 3-step TL;DR at the top of this file.

**Estimated time to full setup:** 5-10 minutes (including Docker startup and service initialization)

Happy coding! 🚀

---

**Last Updated:** August 5, 2026  
**Status:** ✅ Ready for Deployment  
**Build:** Release | No Errors | 12 Non-Critical Warnings

