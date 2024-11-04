using Shop.API.IntegrationTests.Infrastructure;
using Shop.Core.DataEF.Models;
using System.Net;
using System.Net.Http.Json;

namespace Shop.API.IntegrationTests.Product
{
    public class CreateProductControllerTests(
        CustomWebApplicationFactory<Program> factory) : IClassFixture<CustomWebApplicationFactory<Program>>
    {
        private readonly HttpClient _client = factory.CreateClient();

        [Fact]
        public async Task CreateProduct_ShouldReturnCreated_WhenValidData()
        {
            // Arrange
            var newProduct = new
            {
                Title = "Test Product",
                Description = "Test Description",
                Price = 100
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/products", newProduct);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            var product = await response.Content.ReadFromJsonAsync<ProductModel>();
            Assert.NotNull(product);
            Assert.Equal("Test Product", product.Title);
        }

        [Theory]
        [InlineData("", "Valid Description", 100.0, "Title cannot be empty")]
        [InlineData("Valid Title", "", 100.0, "Description cannot be empty")]
        [InlineData("Valid Title", "Valid Description", -1.0, "Price must be greater than zero")]
        public async Task CreateProduct_ShouldReturnBadRequest_WhenInvalidEmptyData(
            string title, string description, decimal price, string expectedErrorMessage)
        {
            // Arrange
            var invalidProduct = new
            {
                Title = title,
                Description = description,
                Price = price
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedErrorMessage, errorResponse);
        }

        [Theory]
        [InlineData("long_title", "Valid Description", 100.0, "Title cannot exceed 100 characters")]
        [InlineData("Valid Title", "long_description", 100.0, "Description cannot exceed 500 characters")]
        public async Task CreateProduct_ShouldReturnBadRequest_WhenInvalidLongData(
            string title, string description, decimal price, string expectedErrorMessage)
        {
            if (title == "long_title")
            {
                title = new string('A', 101);
            }

            if (description == "long_description")
            {
                description = new string('A', 501);
            }

            // Arrange
            var invalidProduct = new
            {
                Title = title,
                Description = description,
                Price = price
            };

            // Act
            var response = await _client.PostAsJsonAsync("/api/v1/products", invalidProduct);

            // Assert
            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
            var errorResponse = await response.Content.ReadAsStringAsync();
            Assert.Contains(expectedErrorMessage, errorResponse);
        }
    }
}
