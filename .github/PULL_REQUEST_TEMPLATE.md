## 📋 Description

<!-- Summarize what this PR does and why -->

Closes #<!-- issue number -->

---

## 🔧 Type of Change

- [ ] 🐛 Bug fix (non-breaking)
- [ ] ✨ New feature (non-breaking)
- [ ] 💥 Breaking change (fix or feature that changes existing behavior)
- [ ] 📝 Documentation update
- [ ] 🔨 Refactoring / code cleanup
- [ ] ⚡ Performance improvement
- [ ] 🔒 Security fix
- [ ] 🐳 Docker / infrastructure change

---

## 🏗️ Affected Service(s)

- [ ] Catalog API
- [ ] Cart API
- [ ] Discount gRPC
- [ ] Ordering API
- [ ] Tracking API
- [ ] Payment API
- [ ] Notification API
- [ ] Identity API
- [ ] YARP Gateway
- [ ] Shopping Web UI
- [ ] BuildingBlocks / Shared
- [ ] Docker / Infra

---

## 🧪 How Has This Been Tested?

<!-- Describe the tests you ran -->

- [ ] Built solution locally: `dotnet build src/eshop-microservices.sln`
- [ ] Unit tests pass: `dotnet test --filter FullyQualifiedName~Unit`
- [ ] Integration tests pass: `dotnet test --filter FullyQualifiedName~Integration`
- [ ] Code is formatted: `dotnet format src/eshop-microservices.sln --verify-no-changes`
- [ ] Ran services with Docker Compose
- [ ] Manually tested the affected endpoints / UI flows
- [ ] Verified RabbitMQ events flow correctly (if messaging changed)

---

## 🔒 Security Checklist

- [ ] No secrets, credentials, or API keys are hardcoded or committed
- [ ] No new `catch (Exception ex)` generic handlers — use specific exception types
- [ ] All user inputs that reach DB queries / commands are validated or parameterised
- [ ] JWT / auth changes reviewed for token expiry and scope correctness
- [ ] MongoDB write errors are translated to appropriate HTTP status codes (e.g. 409 for E11000)

---

## ✅ General Checklist

- [ ] My code follows the coding style of this project (`.editorconfig` / `dotnet format`)
- [ ] I have performed a self-review of my code
- [ ] I have commented my code where necessary
- [ ] I have updated the relevant documentation in `/docs`
- [ ] My changes generate no new compiler warnings
- [ ] I have added or updated unit/integration tests for new behaviour
- [ ] CODEOWNERS are satisfied — no manual reviewer override needed

---

## 📷 Screenshots (if applicable)

<!-- Add screenshots or recordings for UI changes -->

---

## 🤖 CI Status

All of the following checks **must be green** before merge:

| Check | Required for |
|-------|-------------|
| 🔨 Build Solution | All PRs |
| 🧪 Unit Tests | All PRs |
| 🔗 Integration Tests | `master` only |
| 🎨 dotnet-format Gate | All PRs |
| 🔍 CodeQL Analysis | All PRs |
| 📦 NuGet Vulnerability Audit | All PRs |
| 🔐 Secret Scan (Gitleaks) | All PRs |
