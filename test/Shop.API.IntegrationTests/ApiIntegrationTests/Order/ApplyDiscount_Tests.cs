using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shop.API.Contracts.Responses.Orders;
using Shop.API.IntegrationTests.Infrastructure;
using Shop.Core.DataEF.Models;
using System.Net;
using System.Net.Http.Json;

namespace Shop.API.IntegrationTests.ApiIntegrationTests.Order
{
    [Collection("Database Tests")]
    public sealed class ApplyDiscount_Tests(CustomWebApplicationFactory<Program> factory)
        : ApiTestsBase(factory)
    {
        [Fact]
        public async Task ApplyDiscount_Succeeds_WhenValidDiscountIsApplied()
        {
            // Arrange
            SeedDatabaseWithOrders();
            int orderId = await GetOrderID();
            var request = new { DiscountPercentage = 10 };

            // Act
            var response = await _client.PostAsJsonAsync(ApplyDiscountUrl(orderId), request);

            // Assert
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<ApplyDiscountResponseDto>();
            responseData.Should().NotBeNull();
            responseData!.TotalPrice.Should().Be(90.00m);

            // Verify database state
            await VerifyOrderState(orderId, expectedDiscount: 10, expectedTotalPrice: 90.00m);
        }

        [Fact]
        public async Task ApplyDiscount_Fails_WhenOrderDoesNotExist()
        {
            // Arrange
            var request = new { DiscountPercentage = 10 };

            // Act
            var response = await _client.PostAsJsonAsync(ApplyDiscountUrl(999), request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.NotFound);

            var errorMessage = await response.Content.ReadAsStringAsync();
            errorMessage.Should().Contain("Order with ID 999 not found.");
        }

        [Theory]
        [InlineData(-10)]
        [InlineData(110)]
        public async Task ApplyDiscount_Fails_WhenInvalidDiscountIsProvided(decimal invalidDiscount)
        {
            // Arrange
            SeedDatabaseWithOrders();
            int orderId = await GetOrderID();

            var request = new { DiscountPercentage = invalidDiscount };

            // Act
            var response = await _client.PostAsJsonAsync(ApplyDiscountUrl(orderId), request);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

            var errorMessage = await response.Content.ReadAsStringAsync();
            errorMessage.Should().Contain("Discount percentage must be between 0 and 100.");
        }

        private static string? ApplyDiscountUrl(int orderId)
        {
            return $"/api/v1/orders/{orderId}/apply-discount";
        }

        private void SeedDatabaseWithOrders()
        {
            InitializeDatabase(context =>
            {
                var products = new[]
                {
                    new ProductModel { SKU = "SKU1", Title = "Product A", Description = "Description A", Price = 10.99m },
                    new ProductModel { SKU = "SKU2", Title = "Product B", Description = "Description B", Price = 20.99m }
                };

                context.Products.AddRange(products);
                context.SaveChanges();

                var product1Id = context.Products.First(p => p.SKU == "SKU1").Id;
                var product2Id = context.Products.First(p => p.SKU == "SKU2").Id;

                context.Orders.Add(new OrderModel
                {
                    OrderDate = DateTime.UtcNow,
                    TotalPrice = 100.00m,
                    DiscountPercentage = 0,
                    OrderLines =
                    [
                        new OrderLineModel { ProductSKU = "SKU1", ProductId = product1Id, Price = 50.00m, Quantity = 1 },
                        new OrderLineModel { ProductSKU = "SKU2", ProductId = product2Id, Price = 50.00m, Quantity = 1 }
                    ]
                });
            });
        }

        private async Task VerifyOrderState(int orderId, decimal expectedDiscount, decimal expectedTotalPrice)
        {
            using var context = GetDbContext();
            var order = await context.Orders.Include(o => o.OrderLines).FirstOrDefaultAsync(o => o.Id == orderId);

            order.DiscountPercentage.Should().Be(expectedDiscount);
            order.TotalPrice.Should().Be(expectedTotalPrice);
        }

        private async Task<int> GetOrderID()
        {
            using var context = GetDbContext();
            var order = await context.Orders.FirstOrDefaultAsync();

            return order.Id;
        }
    }
}
