# Contributing to Shop Microservices

Thank you for your interest in contributing! 🎉  
This document outlines everything you need to know to get started.

---

## 📋 Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Branch Conventions](#branch-conventions)
- [Commit Style](#commit-style)
- [Pull Request Process](#pull-request-process)
- [Coding Standards](#coding-standards)
- [Running Locally](#running-locally)

---

## Code of Conduct

This project follows the [Contributor Covenant Code of Conduct](./CODE_OF_CONDUCT.md).  
By participating, you agree to uphold these standards.

---

## Getting Started

1. **Fork** the repository on GitHub
2. **Clone** your fork locally:
   ```bash
   git clone https://github.com/<your-username>/Shop-Microservices.git
   cd Shop-Microservices
   ```
3. Add the **upstream remote**:
   ```bash
   git remote add upstream https://github.com/Mostafa-SAID7/Shop-Microservices.git
   ```
4. Create a **feature branch** (see conventions below)

---

## Branch Conventions

| Type | Pattern | Example |
|------|---------|---------|
| Feature | `feat/<short-description>` | `feat/add-payment-webhook` |
| Bug fix | `fix/<short-description>` | `fix/cart-redis-timeout` |
| Docs | `docs/<short-description>` | `docs/update-startup-guide` |
| Refactor | `refactor/<short-description>` | `refactor/ordering-clean-arch` |
| Chore | `chore/<short-description>` | `chore/upgrade-masstransit` |

> Always branch from `main` and keep branches short-lived.

---

## Commit Style

Use [Conventional Commits](https://www.conventionalcommits.org/):

```
<type>(scope): <short description>

[optional body]
[optional footer]
```

**Types:** `feat`, `fix`, `docs`, `style`, `refactor`, `test`, `chore`, `ci`

**Examples:**
```
feat(cart): add cache TTL configuration via appsettings
fix(ordering): resolve null reference in CreateOrderHandler
docs(readme): add architecture diagram
ci: add dotnet build workflow
```

---

## Pull Request Process

1. Sync with upstream before opening a PR:
   ```bash
   git fetch upstream
   git rebase upstream/main
   ```
2. Ensure the solution **builds without errors**:
   ```bash
   dotnet build src/eshop-microservices.sln
   ```
3. **Fill out the PR template** completely
4. Link the related issue (`Closes #123`)
5. Request a review from `@Mostafa-SAID7`
6. Squash commits if requested before merge

---

## Coding Standards

- **C# 12** features are encouraged where they improve clarity
- Follow **Vertical Slice Architecture** for new features in Catalog/Cart Services
- Follow **Clean Architecture** layers for changes to the Ordering service
- Use **MediatR** commands/queries — do not put business logic in controllers or endpoints
- Always add **FluentValidation** for any new command/query inputs
- Register health checks for any new infrastructure dependency
- Do not commit connection strings or secrets — use environment variables

---

## Running Locally

### Full stack (recommended)

```bash
cd src
docker-compose -f docker-compose.yml -f docker-compose.override.yml up -d
```

### Individual service (development)

```bash
# Start only infrastructure
docker-compose up -d catalogdb basketdb distributedcache orderdb messagebroker

# Run a specific service
cd src/Services/Catalog/Catalog.API
dotnet run
```

### Useful commands

```bash
# Build solution
dotnet build src/eshop-microservices.sln

# Watch logs
docker-compose logs -f

# Restart a single service
docker-compose restart catalog.api
```

---

## Questions?

Open a [Discussion](https://github.com/Mostafa-SAID7/Shop-Microservices/discussions) or check the [docs/](../docs/) folder.
