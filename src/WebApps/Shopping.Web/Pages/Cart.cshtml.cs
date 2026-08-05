namespace Shopping.Web.Pages
{
    public class CartModel(ICartService basketService, ILogger<CartModel> logger)
        : PageModel
    {
        public ShoppingCartModel Cart { get; set; } = new ShoppingCartModel();

        public async Task<IActionResult> OnGetAsync()
        {
            Cart = await basketService.LoadUserCart();

            return Page();
        }

        public async Task<IActionResult> OnPostRemoveToCartAsync(Guid productId)
        {
            logger.LogInformation("Remove to cart button clicked");
            Cart = await basketService.LoadUserCart();

            Cart.Items.RemoveAll(x => x.ProductId == productId);

            await basketService.StoreCart(new StoreCartRequest(Cart));

            return RedirectToPage();
        }
    }
}
