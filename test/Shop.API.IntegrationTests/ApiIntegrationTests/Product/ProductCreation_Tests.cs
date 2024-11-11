using FluentAssertions;
using Shop.API.IntegrationTests.Infrastructure;
using Shop.Core.DataEF.Models;
using System.Net.Http.Json;

namespace Shop.API.IntegrationTests.ApiIntegrationTests.Product
{
    public sealed class ProductCreation_Tests
        (CustomWebApplicationFactory<Program> factory)
        : ApiTestsBase(factory)
    {
        private const int TitleMaxLength = 100;
        private const int DescriptionMaxLength = 500;
        private const int SkuMaxLength = 50;

        private readonly TestProduct Product = new TestProduct
        {
            Title = "Low-Poly Potato",
            Description = "Raw, roughly peeled potato with bits of skin and uneven edges.",
            Price = 42.0m,
            SKU = "testSKU"
        };

        [Fact]
        public async Task Product_Created_WithValidData()
        {
            var protuctToCreate = Product;

            var response = await _client.PostAsJsonAsync("/api/v1/products", protuctToCreate);

            response.EnsureSuccessStatusCode();
            var createdProduct = await response.Content.ReadFromJsonAsync<ProductModel>();
            createdProduct.Should().BeEquivalentTo(protuctToCreate);
        }

        [Fact]
        public async Task Product_CreationFails_WithNegativePrice()
        {
            var invalidProduct = Product with { Price = -1 };

            var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

            response.ShouldFail()
                .WithErrors($"{nameof(invalidProduct.Price)} must be greater than zero");
        }

        [Fact]
        public async Task Product_CreationFails_NameIsTooLong()
        {
            var tooLongTitle = GetStringOverTheLimit(TitleMaxLength);

            var invalidProduct = Product with { Title = tooLongTitle };

            var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

            response.ShouldFail()
                .WithErrors($"Title cannot exceed {TitleMaxLength} characters");
        }

        [Fact]
        public async Task Product_CreationFails_DescriptionIsTooLong()
        {
            var tooLongDescription = GetStringOverTheLimit(DescriptionMaxLength);
            var invalidProduct = Product with { Description = tooLongDescription };

            var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

            response.ShouldFail()
                .WithErrors($"Description cannot exceed {DescriptionMaxLength} characters");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\r \t \n")]
        public async Task Product_CreationFails_IfTitle_NulOrEmptyOrWhiteSpaces(string wrongTitle)
        {
            var invalidProduct = Product with { Title = wrongTitle };

            var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

            response.ShouldFail()
                .WithErrors("Title cannot be empty", "The Title field is required");
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        [InlineData("\t")]
        [InlineData("\r \t \n")]
        public async Task Product_CreationFails_IfDescription_NulOrEmptyOrWhiteSpaces(string wrongDescription)
        {
            var invalidProduct = Product with { Description = wrongDescription };

            var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

            response.ShouldFail()
                .WithErrors(["Description cannot be empty", "The Description field is required"]);
        }

        [Fact]
        public async Task Product_CreationFails_WithSkuTooLong()
        {
            var invalidProduct = Product with { SKU = GetStringOverTheLimit(SkuMaxLength + 1) };

            var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

            response.ShouldFail()
                .WithErrors($"SKU cannot exceed {SkuMaxLength} characters");
        }

        //[Fact]
        //public async Task Product_CreationFails_WithDuplicateSKU()
        //{
        //    SeedDatabaseWithProducts();

        //    var duplicateSkuProduct = Product;

        //    var response = await _client.PostAsJsonAsync("/api/v1/products", duplicateSkuProduct);

        //    response.ShouldFail()
        //        .WithErrors($"SKU '{duplicateSkuProduct.SKU}' must be unique.");
        //}

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task Product_CreationFails_IfSku_NullOrWhiteSpace(string invalidSku)
        {
            var invalidProduct = Product with { SKU = invalidSku };

            var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

            response.ShouldFail()
                .WithErrors(["SKU cannot be empty", "The SKU field is required"]);
        }

        private void SeedDatabaseWithProducts()
        {
            InitializeDatabase(context =>
            {
                context.Products.AddRange(
                [
                    new ProductModel {
                        SKU = "testSKU",
                        Title = "Another Product",
                        Description = "Description for another product",
                        Price = 15.0m
                    },
                ]);
            });
        }

        private static string GetStringOverTheLimit(int maxLimit)
            => new string('A', maxLimit + 1);

        private sealed record TestProduct
        {
            public string Title { get; init; }
            public string Description { get; init; }
            public decimal Price { get; init; }
            public string SKU { get; init; }
        }
    }
}
