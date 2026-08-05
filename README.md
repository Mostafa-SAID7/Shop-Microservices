<div align="center">

# 🛒 Shop Microservices

**Production-grade .NET 8 e-commerce platform built with microservices architecture**

[![Build](https://github.com/Mostafa-SAID7/Shop-Microservices/actions/workflows/dotnet-build.yml/badge.svg)](https://github.com/Mostafa-SAID7/Shop-Microservices/actions/workflows/dotnet-build.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)

*DDD · CQRS · Vertical Slice · Event-Driven · gRPC · RabbitMQ · YARP*

</div>

---

## 🏗️ Architecture

```
Browser → Shopping.Web (6065)
              ↓
         YARP Gateway (6064) — rate limiting
    ┌────┬────┬────┬────┐
    ▼    ▼    ▼    ▼    ▼
 Catalog Basket Order Track Payment
 (6060) (6061)(6063)(6066)(6067)
    │    │ ↕gRPC
    │    │ Discount(6062)
    └────┴────── RabbitMQ ──────────┘
```

---

## 📦 Services

| Service | Port | Stack | Database | Pattern |
|---------|------|-------|----------|---------|
| **Catalog** | 6060 | Minimal API | PostgreSQL | Vertical Slice + CQRS |
| **Cart** | 6061 | Web API | PostgreSQL + Redis | Cache-Aside |
| **Discount** | 6062 | gRPC | SQLite | Protobuf RPC |
| **Ordering** | 6063 | Web API | SQL Server | DDD + Clean Arch |
| **Tracking** | 6066 | Web API | PostgreSQL | Event-Driven |
| **Payment** | 6067 | Web API | SQL Server | Event-Driven |
| **Gateway** | 6064 | YARP | — | Reverse Proxy |
| **Web UI** | 6065 | Razor Pages | — | Bootstrap 4 |

---

## ⚡ Quick Start

> **Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop) · [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) · 4 GB RAM

```bash
# 1 — Clone
git clone https://github.com/Mostafa-SAID7/Shop-Microservices.git
cd Shop-Microservices/src

# 2 — Launch all services
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d

# 3 — Open the app (wait ~2 min for services to initialize)
open https://localhost:6065
```

| URL | Purpose |
|-----|---------|
| https://localhost:6065 | 🛒 Shopping Web UI |
| http://localhost:6064 | 🔀 API Gateway |
| http://localhost:15672 | 📊 RabbitMQ Admin (`guest/guest`) |

---

## 🛠️ Tech Stack

**Core:** .NET 8 · C# 12 · ASP.NET Core  
**Data:** Entity Framework Core · Marten · PostgreSQL · SQL Server · SQLite · Redis  
**Messaging:** RabbitMQ · MassTransit  
**API:** Carter · YARP · gRPC · Refit  
**Patterns:** MediatR · FluentValidation · Scrutor · Mapster  
**Infra:** Docker Compose · Health Checks · Structured Logging  

---

## 📁 Project Structure

```
src/
├── Services/
│   ├── Catalog/Catalog.API
│   ├── Basket/Basket.API
│   ├── Discount/Discount.Grpc
│   ├── Ordering/{Domain,Application,Infrastructure,API}
│   ├── Tracking/Tracking.API
│   └── Payment/Payment.API
├── ApiGateways/YarpApiGateway
├── WebApps/Shopping.Web
└── BuildingBlocks/{Core,Messaging}
```

---

## 📚 Documentation

| Doc | Description |
|-----|-------------|
| [Project Analysis](./docs/PROJECT_ANALYSIS.md) | Full architecture overview & service breakdown |
| [Technical Details](./docs/TECHNICAL_DETAILS.md) | Deep dive — patterns, code, DB schemas |
| [Startup Guide](./docs/STARTUP_GUIDE.md) | Step-by-step run & troubleshooting |
| [Build Status](./docs/BUILD_STATUS_REPORT.md) | Build results & deployment checklist |

---

## 🤝 Contributing

See [CONTRIBUTING.md](./.github/CONTRIBUTING.md) for guidelines. Please read our [Code of Conduct](./.github/CODE_OF_CONDUCT.md).

---

## 📖 Resources

- 🎓 [Udemy Course](https://www.udemy.com/course/microservices-architecture-and-implementation-on-dotnet/?couponCode=MARC26)
- 📝 [Medium Article — .NET 8 Microservices: DDD, CQRS, Clean Architecture](https://medium.com/@mehmetozkaya/net-8-microservices-ddd-cqrs-vertical-clean-architecture-2dd7ebaaf4bd)
- 👤 [Author: Mehmet Ozkaya](https://github.com/mehmetozkaya)

---

<div align="center">
<sub>Built with ❤️ · MIT License</sub>
</div>
