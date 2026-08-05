using System.Net;

namespace Shopping.Web.Services;

public interface ICartService
{
    [Get("/cart-service/cart/{userName}")]
    Task<GetCartResponse> GetCart(string userName);

    [Post("/cart-service/cart")]
    Task<StoreCartResponse> StoreCart(StoreCartRequest request);

    [Delete("/cart-service/cart/{userName}")]
    Task<DeleteCartResponse> DeleteCart(string userName);

    [Post("/cart-service/cart/checkout")]
    Task<CheckoutCartResponse> CheckoutCart(CheckoutCartRequest request);

    public async Task<ShoppingCartModel> LoadUserCart()
    {
        // Get Basket If Not Exist Create New Basket with Default Logged In User Name: swn
        var userName = "swn";
        ShoppingCartModel basket;

        try
        {
            var getBasketResponse = await GetCart(userName);
            basket = getBasketResponse.Cart;
        }
        catch (ApiException apiException) when (apiException.StatusCode == HttpStatusCode.NotFound)
        {
            basket = new ShoppingCartModel
            {
                UserName = userName,
                Items = []
            };
        }

        return basket;
    }
}
