using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shop.API.IntegrationTests.Infrastructure;
using Shop.Core.DataEF.Models;
using System.Net;
using System.Net.Http.Json;

namespace Shop.API.IntegrationTests.ApiIntegrationTests.Product
{
    [Collection("Database Tests")]
    public sealed class ProductBulkPriceUpdate_Tests
        (CustomWebApplicationFactory<Program> factory)
        : ApiTestsBase(factory)
    {
        [Fact]
        public async Task ProductBulkPriceUpdate_Succeeds_WhenAllSkusMatch()
        {
            // Arrange
            await AuthorizeAdmin();

            SeedDatabaseWithProducts();
            var csvFilePath = GetCsvFilePath("products.csv");

            using var content = GetFileFormContent(csvFilePath);

            // Act
            var response = await _client.PostAsync("/api/v1/products/update-prices", content);

            // Assert
            response.EnsureSuccessStatusCode();

            var responseData = await response.Content.ReadFromJsonAsync<BulkUpdateResponse>();
            responseData.UpdatedCount.Should().Be(4);

            await VerifySuccessfulInsertAndUpdate();
        }

        [Fact]
        public async Task ProductBulkPriceUpdate_Fails_WhenUserIsNotAuthorized()
        {
            // Arrange
            SeedDatabaseWithProducts();
            var csvFilePath = GetCsvFilePath("products.csv");

            using var content = GetFileFormContent(csvFilePath);

            // Act
            var response = await _client.PostAsync("/api/v1/products/update-prices", content);

            // Assert
            response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
        }

        [Fact]
        public async Task ProductBulkPriceUpdate_Fails_WhenFileIsEmpty()
        {
            // Arrange
            await AuthorizeAdmin();

            SeedDatabaseWithProducts();
            var emptyFilePath = GetCsvFilePath("empty.csv");
            using var content = GetFileFormContent(emptyFilePath);

            // Act
            var response = await _client.PostAsync("/api/v1/products/update-prices", content);

            // Assert
            response.ShouldFail()
                .WithErrors("File is empty");
        }

        [Fact]
        public async Task ProductBulkPriceUpdate_Fails_WhenMissedField()
        {
            // Arrange
            await AuthorizeAdmin();

            SeedDatabaseWithProducts();

            var csvFilePath = GetCsvFilePath("products_with_missed_field.csv");

            using var content = GetFileFormContent(csvFilePath);

            // Act
            var response = await _client.PostAsync("/api/v1/products/update-prices", content);

            // Assert
            response.ShouldFail()
                .WithErrors("CSV parsing error: Field at index '3' does not exist");
        }

        [Fact]
        public async Task ProductBulkPriceUpdate_Fails_WhenMissedHeader()
        {
            // Arrange
            await AuthorizeAdmin();

            SeedDatabaseWithProducts();
            var invalidCsvPath = GetCsvFilePath("products_with_missed_header.csv");
            using var content = GetFileFormContent(invalidCsvPath);

            // Act
            var response = await _client.PostAsync("/api/v1/products/update-prices", content);

            // Assert
            response.ShouldFail()
                .WithErrors("CSV parsing error: Header with name 'Description'[0] was not found");
        }

        [Fact]
        public async Task ProductBulkPriceUpdate_Fails_WhenNegativePrice()
        {
            // Arrange
            await AuthorizeAdmin();

            SeedDatabaseWithProducts();

            var csvFilePath = GetCsvFilePath("products_with_negative_price.csv");

            using var content = GetFileFormContent(csvFilePath);

            // Act
            var response = await _client.PostAsync("/api/v1/products/update-prices", content);

            // Assert
            response.ShouldFail()
                .WithErrors("Price must be greater than zero");
        }

        private void SeedDatabaseWithProducts()
        {
            InitializeDatabase(context =>
            {
                context.Products.AddRange(
                    new ProductModel { SKU = "testSKU1", Title = "Product 1", Description = "Description 1", Price = 20.00m },
                    new ProductModel { SKU = "testSKU2", Title = "Product 2", Description = "Description 2", Price = 30.00m },
                    new ProductModel { SKU = "testSKU3", Title = "Product 3", Description = "Description 3", Price = 40.00m }
                );
            });
        }

        private async Task VerifySuccessfulInsertAndUpdate()
        {
            using var context = GetDbContext();

            var product1 = await context.Products.SingleAsync(p => p.SKU == "testSKU1");
            product1.Price.Should().Be(45.99m);
            product1.Title.Should().Be("Updated Product 1");

            var product2 = await context.Products.SingleAsync(p => p.SKU == "testSKU2");
            product2.Price.Should().Be(99.99m);

            var newProduct1 = await context.Products.SingleAsync(p => p.SKU == "newSKU1");
            newProduct1.Title.Should().Be("New Product 1");
            newProduct1.Price.Should().Be(25.00m);
        }

        private async Task VerifyDatabaseStateUnchanged()
        {
            using var context = GetDbContext();

            var product1 = await context.Products.SingleAsync(p => p.SKU == "testSKU1");
            var product2 = await context.Products.SingleAsync(p => p.SKU == "testSKU2");
            var product3 = await context.Products.SingleAsync(p => p.SKU == "invalidProductSKUForUpdate");

            product1.Price.Should().Be(20.00m);
            product2.Price.Should().Be(30.00m);
            product3.Price.Should().Be(40.00m);
        }

        private static string GetCsvFilePath(string fileName)
        {
            var testFilesDirectory = Path.Combine(AppContext.BaseDirectory, "ApiIntegrationTests", "Assets", "ProductBulkPriceUpdate");

            if (!Directory.Exists(testFilesDirectory))
            {
                throw new DirectoryNotFoundException($"Test files directory not found: {testFilesDirectory}");
            }

            var filePath = Path.Combine(testFilesDirectory, fileName);

            if (!File.Exists(filePath))
            {
                throw new FileNotFoundException($"File not found: {filePath}");
            }

            return filePath;
        }

        private static MultipartFormDataContent GetFileFormContent(string csvFilePath)
        {
            return new MultipartFormDataContent
            {
                { new StreamContent(new FileStream(csvFilePath, FileMode.Open, FileAccess.Read)), "file", "products_file.csv" }
            };
        }

        public record BulkUpdateResponse
        {
            public int UpdatedCount { get; init; }
        }
    }
}
