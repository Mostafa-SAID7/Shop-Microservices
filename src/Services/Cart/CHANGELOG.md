# Cart Service — Feature Changelog

## v1.3.0 — Unit & Integration Tests Added

### Testing Structure
- Unit tests: `tests/Services/Cart/Cart.API.Tests/Unit/`
  - `CartModelTests.cs` — 2 tests (ShoppingCart TotalPrice calculation, empty cart)
- Integration tests: `tests/Services/Cart/Cart.API.Tests/Integration/`
  - `CartEndpointsContractTests.cs` — 5 HTTP endpoint contract specification tests

### CI/CD
- Cart unit and integration tests included in `ci.yml` GitHub Actions workflow.
- Tests run without requiring PostgreSQL, Redis, or gRPC connection.

---

## v1.0.0 — Feature: Cart Rename from Basket

### Summary
Complete systemic refactoring of the Basket microservice → Cart microservice.

### Changes
- Renamed all C# namespaces: `Basket.*` → `Cart.*`
- Renamed all class names: `BasketRepository` → `CartRepository`, `BasketCheckoutEvent` → `CartCheckoutEvent`, etc.
- Renamed all physical files and folders:
  - `CheckoutBasket/` → `CheckoutCart/`
  - `DeleteBasket/` → `DeleteCart/`
  - `GetBasket/` → `GetCart/`
  - `StoreBasket/` → `StoreCart/`
  - `BasketRepository.cs` → `CartRepository.cs`
  - `CachedBasketRepository.cs` → `CachedCartRepository.cs`
  - `IBasketRepository.cs` → `ICartRepository.cs`
  - `BasketCheckoutDto.cs` → `CartCheckoutDto.cs`
  - `BasketNotFoundException.cs` → `CartNotFoundException.cs`
- Updated Docker service name: `basket.api` → `cart.api`
- Updated YARP routes: `/basket-service/` → `/cart-service/`
- Updated docker-compose volumes: `postgres_basket` → `postgres_cart`
- Updated Shopping.Web service clients and models

### Testing
- `dotnet build` — 0 errors ✅
