# Security Policy

## Supported Versions

| Version | Supported |
|---------|-----------|
| `main` branch | ✅ Active |
| Older branches | ❌ Not supported |

> This repository targets **.NET 8** (LTS). Please ensure you are on the latest commit of `main` before reporting a vulnerability.

---

## Reporting a Vulnerability

**Please do NOT open a public GitHub issue for security vulnerabilities.**

Report security issues privately using one of these methods:

### Option A — GitHub Private Security Advisory (Recommended)
1. Go to the [Security tab](https://github.com/Mostafa-SAID7/Shop-Microservices/security/advisories/new)
2. Click **"Report a vulnerability"**
3. Fill in the details

### Option B — Email
Send details to the repository owner via GitHub profile: [@Mostafa-SAID7](https://github.com/Mostafa-SAID7)

---

## What to Include in Your Report

- **Description** of the vulnerability
- **Steps to reproduce** (proof of concept if possible)
- **Affected service(s)** (Catalog, Cart, Ordering, etc.)
- **Potential impact** (data exposure, RCE, SSRF, etc.)
- **Suggested fix** (optional but appreciated)

---

## Response Timeline

| Step | Timeline |
|------|----------|
| Acknowledgment | Within 48 hours |
| Assessment | Within 7 days |
| Fix / Patch | Within 30 days (critical), 90 days (others) |
| Public disclosure | After fix is released |

---

## Scope

### In Scope
- SQL injection, XXE, SSRF, RCE in any microservice
- Authentication/authorization bypass in the API Gateway
- Sensitive data exposure via API endpoints
- Insecure deserialization in RabbitMQ consumers
- Container escape / Docker misconfigurations

### Out of Scope
- Vulnerabilities in third-party libraries (report to the library maintainer)
- Issues requiring physical access to the host machine
- Social engineering attacks

---

## Security Notes for Production Deployments

> ⚠️ This project is designed as a **learning/development** platform. Before deploying to production:

- Replace all default credentials (`postgres/postgres`, `guest/guest`, `sa/SwN12345678`)
- Enable HTTPS for all inter-service communication
- Add authentication/authorization (OAuth2 / OpenID Connect) at the API Gateway
- Use managed secrets (Azure Key Vault, AWS Secrets Manager, etc.)
- Enable encrypted connections to SQL Server and PostgreSQL
- Apply principle of least privilege to all service accounts
