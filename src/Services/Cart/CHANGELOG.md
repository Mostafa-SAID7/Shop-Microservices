# Cart Service — Feature Changelog

## Feature: Cart Rename from Basket

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
