using Microsoft.Extensions.Caching.Distributed;
using System.Text.Json;

namespace Basket.API.Data;


// Cache-aside pattern implementation:
// The Cache-Aside (or Lazy Loading) pattern provides an efficient caching strategy
// where the application first checks for data in the cache (Redis). If the data is missing,
// it is fetched from the primary source (DB), stored in the cache for future requests,
// and returned to the client. This reduces database load and speeds up responses.
// Implementation:
// 1) First, check for data in the Redis cache using a unique key
// 2) If the data is found in the cache, deserialize it and return it to the client
// 3) If the data is missing, fetch it from the primary data source (DB)
// 4) Serialize the retrieved data and store it in Redis with an appropriate TTL (Time To Live) 
// 5) Return the data to the client
// 6) To invalidate the cache upon data updates, remove the corresponding key from Redis
// 7) Implement error handling: if Redis is unavailable, fall back to the database
public class CachedBasketRepository
    (IBasketRepository repository, IDistributedCache cache)
    : IBasketRepository
{
    public async Task<ShoppingCart> GetBasket(string userName, CancellationToken cancellationToken = default)
    {
        var cachedBasket = await cache.GetStringAsync(userName, cancellationToken);

        if (!string.IsNullOrEmpty(cachedBasket))
        {
            return JsonSerializer.Deserialize<ShoppingCart>(cachedBasket)!;
        }

        var basket = await repository.GetBasket(userName, cancellationToken);
        await cache.SetStringAsync(userName, JsonSerializer.Serialize(basket), cancellationToken);

        return basket;
    }

    public async Task<ShoppingCart> StoreBasket(ShoppingCart basket, CancellationToken cancellationToken = default)
    {
        await repository.StoreBasket(basket, cancellationToken);

        await cache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket), cancellationToken);

        return basket;
    }

    public async Task<bool> DeleteBasket(string userName, CancellationToken cancellationToken = default)
    {
        await repository.DeleteBasket(userName, cancellationToken);

        await cache.RemoveAsync(userName, cancellationToken);

        return true;
    }

}