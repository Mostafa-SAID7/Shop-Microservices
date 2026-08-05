using Cart.API.Models;
using Xunit;

namespace Cart.API.Tests;

public class CartModelTests
{
    [Fact]
    public void ShoppingCart_TotalPrice_ShouldCalculateSumOfAllItemPrices()
    {
        // Arrange
        var cart = new ShoppingCart("test_user")
        {
            Items = new List<ShoppingCartItem>
            {
                new ShoppingCartItem { Quantity = 2, Price = 50.00m, ProductName = "Item 1" },
                new ShoppingCartItem { Quantity = 1, Price = 25.50m, ProductName = "Item 2" }
            }
        };

        // Act
        var total = cart.TotalPrice;

        // Assert
        Assert.Equal(125.50m, total);
    }

    [Fact]
    public void ShoppingCart_TotalPrice_ShouldReturnZero_WhenCartIsEmpty()
    {
        // Arrange
        var cart = new ShoppingCart("empty_user");

        // Act
        var total = cart.TotalPrice;

        // Assert
        Assert.Equal(0m, total);
    }
}
