using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Shop.API.Contracts.Requests.Orders;
using Shop.API.IntegrationTests.ApiIntegrationTests.Product;
using Shop.API.IntegrationTests.Infrastructure;
using Shop.Core.DataEF.Models;
using Shop.Core.DTOs.Orders;
using System.Net.Http.Json;

namespace Shop.API.IntegrationTests.ApiIntegrationTests.Order
{
    [Collection("Database Tests")]
    public sealed class OrderCreation_Tests
        (CustomWebApplicationFactory<Program> factory)
        : ApiTestsBase(factory)
    {
        private const string InvalidProductSKU = "notExistingSKU";
        private const int MinQuantity = 1;
        private const int MaxQuantity = 100;

        private readonly CreateOrderRequestDto InvalidOrderRequest = new()
        {
            Products =
            [
                new()
                {
                    ProductSKU = InvalidProductSKU,
                    Quantity = 1
                }
            ]
        };

        [Fact]
        public async Task Order_Created_WithValidData()
        {
            SeedDatabaseWithProducts();

            CreateOrderRequestDto validRequest = CreateOrderRequest(1);

            var response = await _client.PostAsJsonAsync("/api/v1/orders", validRequest);

            response.EnsureSuccessStatusCode();

            var createdOrderId = await response.Content.ReadFromJsonAsync<CreateOrderResponseDto>();
            createdOrderId!.Id.Should().BeGreaterThan(0);

            await VerifyOrderInDatabase(createdOrderId!.Id, validRequest);
        }

        [Fact]
        public async Task Order_CreationFails_WithNonExistentProduct()
        {
            SeedDatabaseWithProducts();

            var response = await _client.PostAsJsonAsync("/api/v1/orders", InvalidOrderRequest);

            response.ShouldFail()
                .WithErrors($"Products with SKUs {InvalidProductSKU} not found.");
        }

        [Fact]
        public async Task Order_CreationFails_IfQuantity_ZeroOrNegative()
        {
            SeedDatabaseWithProducts();
            var invalidRequest = CreateOrderRequest(0);

            var response = await _client.PostAsJsonAsync("/api/v1/orders", invalidRequest);

            response.ShouldFail()
                .WithErrors($"Quantity must be at least {MinQuantity}");
        }

        [Fact]
        public async Task Order_CreationFails_IfQuantity_ExceedsMaximum()
        {
            SeedDatabaseWithProducts();
            var invalidRequest = CreateOrderRequest(MaxQuantity + 1);

            var response = await _client.PostAsJsonAsync("/api/v1/orders", invalidRequest);

            response.ShouldFail()
                .WithErrors($"Quantity cannot exceed {MaxQuantity}");
        }

        [Fact]
        public async Task Order_CreationFails_IfOrderHasNoItems()
        {
            SeedDatabaseWithProducts();

            CreateOrderRequestDto emptyOrderItems = new()
            {
                Products = []
            };

            var response = await _client.PostAsJsonAsync("/api/v1/orders", emptyOrderItems);

            response.ShouldFail()
                .WithErrors("Order must contain at least one product");
        }

        private void SeedDatabaseWithProducts()
        {

            InitializeDatabase(context =>
            {
                context.Products.AddRange(
                [
                    new ProductModel { SKU = "A0101", Title = "Product A", Description = "Description A", Price = 10.99m },
                    new ProductModel { SKU = "B0101", Title = "Product B", Description = "Description B", Price = 20.99m }
                ]);
            });
        }

        private static CreateOrderRequestDto CreateOrderRequest(int quantity)
        {
            return new CreateOrderRequestDto
            {
                Products =
                [
                    new ProductForOrderDto { ProductSKU = "A0101", Quantity = quantity },
                    new ProductForOrderDto { ProductSKU = "B0101", Quantity = quantity }
                ]
            };
        }

        private async Task VerifyOrderInDatabase(int orderId, CreateOrderRequestDto request)
        {
            using var context = GetDbContext();

            var orderInDb = await context.Orders
                .Include(o => o.OrderLines)
                .SingleOrDefaultAsync(o => o.Id == orderId);

            orderInDb.Should().NotBeNull();
            orderInDb!.OrderLines.Should().HaveCount(request.Products.Count);

            foreach (var product in request.Products)
            {
                orderInDb.OrderLines.Should().Contain(ol =>
                    ol.ProductSKU == product.ProductSKU &&
                    ol.Quantity == product.Quantity &&
                    ol.Price == context.Products.Single(p => p.SKU == product.ProductSKU).Price
                );
            }
        }

        public record CreateOrderResponseDto
        {
            public int Id { get; set; }
        }
    }
}
