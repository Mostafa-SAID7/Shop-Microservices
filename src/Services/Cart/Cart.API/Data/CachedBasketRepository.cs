using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Cart.API.Data;

public class CachedCartRepository
    (ICartRepository repository, IDistributedCache cache) 
    : ICartRepository
{
    public async Task<ShoppingCart> GetCart(string userName, CancellationToken cancellationToken = default)
    {
        var cachedBasket = await cache.GetStringAsync(userName, cancellationToken);
        if (!string.IsNullOrEmpty(cachedBasket))
            return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket)!;

        var basket = await repository.GetCart(userName, cancellationToken);
        await cache.SetStringAsync(userName, JsonSerializer.Serialize(basket), cancellationToken);
        return basket;
    }

    public async Task<ShoppingCart> StoreCart(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        await repository.StoreCart(basket, cancellationToken);

        await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket), cancellationToken);

        return basket;
    }

    public async Task<bool> DeleteCart(string userName, CancellationToken cancellationToken = default)
    {
        await repository.DeleteCart(userName, cancellationToken);

        await cache.RemoveAsync(userName, cancellationToken);

        return true;
    }
}
