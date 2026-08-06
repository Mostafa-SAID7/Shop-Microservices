<div align="center">

# 🛒 Shop Microservices

**Production-grade .NET 8 e-commerce platform built with microservices architecture**

[![CI — Build & Test](https://github.com/Mostafa-SAID7/Shop-Microservices/actions/workflows/ci.yml/badge.svg)](https://github.com/Mostafa-SAID7/Shop-Microservices/actions/workflows/ci.yml)
[![Docker — Build & Push](https://github.com/Mostafa-SAID7/Shop-Microservices/actions/workflows/docker-build.yml/badge.svg)](https://github.com/Mostafa-SAID7/Shop-Microservices/actions/workflows/docker-build.yml)
[![Security Scan](https://github.com/Mostafa-SAID7/Shop-Microservices/actions/workflows/security.yml/badge.svg)](https://github.com/Mostafa-SAID7/Shop-Microservices/actions/workflows/security.yml)
[![.NET](https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/download/dotnet/8.0)
[![Docker](https://img.shields.io/badge/Docker-Compose-2496ED?logo=docker)](https://www.docker.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](./LICENSE)

*DDD · CQRS · Vertical Slice · Event-Driven · gRPC · RabbitMQ · YARP · Identity (JWT) · Notification (MassTransit)*

</div>

---

## 🏗️ Architecture Overview

```
                        Browser / Mobile Clients
                                   │
                                   ▼
                    🔀 YARP API Gateway (Port 6000)
    ┌──────────┬──────────┬──────────┼──────────┬──────────┬──────────┐
    ▼          ▼          ▼          ▼          ▼          ▼          ▼
 Catalog      Cart     Ordering   Tracking   Payment   Identity Notification
  (6001)     (6003)     (6004)     (6006)     (6007)     (6068)     (6069)
    │          │          │          │          │          │          │
 PostgreSQL PostgreSQL SQL Server PostgreSQL SQL Server  MongoDB    MongoDB
            + Redis      │                               (Users)   (Log Audit)
                         ▼ (gRPC)                           │          │
                      Discount                              └────┬─────┘
                       (6002)                                    ▼
                                                             RabbitMQ
```

---

## 📦 Service Registry & Stack

| Service | Container Port | External Port | Stack / Architecture | Data Store | Key Responsibilities |
|---------|----------------|---------------|----------------------|------------|----------------------|
| **YARP Gateway** | 8080 | **6000** | ASP.NET Core + YARP Proxy | — | Centralized reverse proxy, routing, rate limiting |
| **Catalog.API** | 8080 | **6001** | Minimal API + Carter + CQRS | PostgreSQL | Product catalog, categories, brand filtering |
| **Discount.Grpc**| 8080 | **6002** | gRPC Service + Protobuf | SQLite | Product discount codes & rule calculations |
| **Cart.API** | 8080 | **6003** | Web API + Marten | PostgreSQL + Redis | Shopping cart storage, cache-aside, checkout trigger |
| **Ordering.API** | 8080 | **6004** | Web API + Clean Arch + DDD | SQL Server | Order processing, domain events, saga orchestration |
| **Shopping.Web** | 8080 | **6065** | ASP.NET Core Razor Pages | — | Responsive e-commerce Web UI frontend |
| **Tracking.API** | 8080 | **6006** | Minimal API + Carter | PostgreSQL | Order tracking status, fulfillment updates |
| **Payment.API** | 8080 | **6007** | Web API + Carter | SQL Server | Payment gateway integration, transaction processing |
| **Identity.API** | 8080 | **6068** | Minimal API + Carter + JWT | MongoDB / In-Memory | User registration, authentication, JWT token issuance |
| **Notification.API**| 8080 | **6069** | Web API + MassTransit Consumers | MongoDB | Event-driven welcome emails, order confirmation SMS, audit logs |

---

## ⚡ Quick Start Guide

> **Prerequisites:** [Docker Desktop](https://www.docker.com/products/docker-desktop) · [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

```bash
# 1 — Clone the repository
git clone https://github.com/Mostafa-SAID7/Shop-Microservices.git
cd Shop-Microservices

# 2 — Build & Start all services via Docker Compose
docker-compose -f src/docker-compose.yml -f src/docker-compose.override.yml up -d --build

# 3 — Verify all containers are running
docker ps
```

### 🌐 Access Endpoints

| Resource | URL | Description | Credentials |
|----------|-----|-------------|-------------|
| **Shopping Web UI** | [http://localhost:6065](http://localhost:6065) | Web Application Interface | — |
| **API Gateway** | [http://localhost:6000](http://localhost:6000) | YARP Unified Gateway | — |
| **Identity API** | [http://localhost:6068](http://localhost:6068) | Auth Endpoints | — |
| **Notification API** | [http://localhost:6069](http://localhost:6069) | Notification Service | — |
| **RabbitMQ Management** | [http://localhost:15672](http://localhost:15672) | Message Broker Portal | `guest` / `guest` |

---

## 🧪 Test Suite & Quality Assurance

The repository features automated Unit and Integration tests using **xUnit**, **FluentAssertions**, and **WebApplicationFactory**.

```bash
# Run Unit Tests across all microservices
dotnet test tests/Services/Identity/Identity.API.Tests/Identity.API.Tests.csproj --filter "FullyQualifiedName~Unit"
dotnet test tests/Services/Notification/Notification.API.Tests/Notification.API.Tests.csproj --filter "FullyQualifiedName~Unit"
dotnet test tests/Services/Cart/Cart.API.Tests/Cart.API.Tests.csproj --filter "FullyQualifiedName~Unit"

# Run In-Memory Integration Tests
dotnet test tests/Services/Identity/Identity.API.Tests/Identity.API.Tests.csproj --filter "FullyQualifiedName~Integration"
```

| Test Project | Unit Tests | Integration Tests | Total |
|--------------|------------|-------------------|-------|
| `Identity.API.Tests` | 7 (TokenService, UserStore) | 5 (HTTP endpoints via WebApplicationFactory) | **12** |
| `Notification.API.Tests` | 4 (EventHandlers, Models) | 2 (Contract validation) | **6** |
| `Cart.API.Tests` | 2 (Cart model calculation) | 5 (Contract & HTTP spec) | **7** |

---

## 🌿 Gitflow & Branching Strategy

This project enforces **Gitflow** for structured collaborative development:

- `master`: Production-ready, stable releases (tagged with `vX.Y.Z`).
- `develop`: Main integration branch for active development.
- `feature/*`: Feature development branches (`feature/identity-service`, `feature/cart-rename-from-basket`, etc.).
- `release/*`: Release preparation and final stabilization.
- `hotfix/*`: Emergency production patches branched directly from `master`.

Detailed workflows, merge policies, and commit standards are documented in [GITFLOW.md](./docs/GITFLOW.md).

---

## 🔄 CI/CD Workflows (GitHub Actions)

The repository includes enterprise-grade GitHub Actions workflows located in `.github/workflows/`:

1. **`ci.yml` — Build & Test Pipeline**:
   - Triggers on push/PR across all branches.
   - Restores, builds, and executes unit & integration tests.
   - Publishes test reports via TRX reporter.
2. **`docker-build.yml` — Container Orchestration**:
   - Detects modified services using path filtering.
   - Builds and publishes Docker images to GitHub Container Registry (GHCR).
3. **`security.yml` — Security & Compliance**:
   - CodeQL C# static application security testing (SAST).
   - NuGet dependency vulnerability audits.
   - Secret scanning via Gitleaks.
4. **`release.yml` — Automated Release Management**:
   - Auto-generates release notes and changelogs from Conventional Commits.

---

## 📁 Repository Directory Structure

```
Shop-Microservices/
├── .github/
│   └── workflows/                # CI/CD Workflows (CI, Docker, Security, Release)
├── docs/                         # System architecture & Gitflow documentation
│   ├── GITFLOW.md
│   ├── PROJECT_ANALYSIS.md
│   ├── TECHNICAL_DETAILS.md
│   └── STARTUP_GUIDE.md
├── src/
│   ├── ApiGateways/
│   │   └── YarpApiGateway/       # YARP Reverse Proxy Gateway
│   ├── BuildingBlocks/
│   │   ├── BuildingBlocks/       # Core Building Blocks (CQRS, Behaviors, Exceptions)
│   │   └── BuildingBlocks.Messaging/ # MassTransit & RabbitMQ Event Models
│   ├── Services/
│   │   ├── Cart/                 # Cart API (Refactored from Basket)
│   │   ├── Catalog/              # Catalog API
│   │   ├── Discount/             # Discount gRPC Service
│   │   ├── Identity/             # Identity API (Authentication & User Management)
│   │   ├── Notification/         # Notification API (Event Consumers)
│   │   ├── Ordering/             # Ordering Microservice
│   │   ├── Payment/              # Payment API
│   │   └── Tracking/             # Tracking API
│   └── WebApps/
│       └── Shopping.Web/         # Razor Pages Web Frontend
└── tests/
    └── Services/                 # Unit & Integration Tests by Microservice
        ├── Cart/Cart.API.Tests/
        ├── Identity/Identity.API.Tests/
        └── Notification/Notification.API.Tests/
```

---

## 🤝 Authors & Maintenance

- **Author / Lead**: Mostafa SAID (`m.ssaid356@gmail.com` / `samirsaid3560@gmail.com`)
- **License**: MIT License — see [LICENSE](./LICENSE) for details.
