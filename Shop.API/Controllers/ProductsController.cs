using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Shop.API.Contracts.Requests.Products;
using Shop.API.Contracts.Responses.Products;
using Shop.Core.Exceptions.Common;
using Shop.Core.Helpers.OperationResult;
using Shop.Core.Services.Products;
using Shop.Domain.Common;
using Shop.Domain.Products;

namespace Shop.API.Controllers
{
    /// <summary>
    /// ProductController
    /// </summary>
    [ApiVersion(1)]
    [ApiController]
    [Route("api/v{v:apiVersion}/[controller]")]
    public sealed class ProductsController(ProductService productService) : ControllerBase
    {
        private readonly ProductService _productService = productService;

        /// <summary>
        /// Adds a new product.
        /// </summary>
        /// <body name="product">The product to add.</body>
        /// <returns>The newly created product.</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateProduct([FromBody] CreateProductRequestDto request)
        {
            if (await _productService.CheckIfExistBySkuAsync(request.SKU))
            {
                return BadRequest($"SKU '{request.SKU}' must be unique.");
            }

            var product = new Product(
                title: new Title(request.Title),
                description: new Description(request.Description),
                price: new Price(request.Price),
                sku: new SKU(request.SKU)
            );

            Product createdProduct = await _productService.CreateProductAsync(product);

            GetProductResponseDto productResponse = new()
            {
                Description = createdProduct.Description,
                Title = createdProduct.Title,
                Price = createdProduct.Price,
                SKU = createdProduct.SKU,
                Id = createdProduct.Id
            };

            return CreatedAtAction(nameof(CreateProduct), productResponse);
        }

        /// <summary>
        /// Update products price
        /// </summary>
        /// <file name="file">The product to add.</file>
        /// <returns>Number of updated products</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        [HttpPost("update-prices")]
        public async Task<IActionResult> BulkUpdateProducts(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest("File is empty");

            var parsedProductsResult = await _productService.ParseCsvAsync(file);
            var parsedProductsError = HandleOperationResultForAction(parsedProductsResult);

            if (parsedProductsError != null)
            {
                return parsedProductsError;
            }

            if (parsedProductsResult.Value == null)
            {
                return StatusCode(500, "Unexpected error during file parsing");
            }

            var updatedCount = await _productService.BulkUpdatePricesAsync(parsedProductsResult.Value);
            var updateProductError = HandleOperationResultForAction(updatedCount);

            if (updateProductError != null)
            {
                return updateProductError;
            }

            return Ok(new { UpdatedCount = updatedCount.Value });
        }

        // TODO: Move to the BaseController?
        private IActionResult? HandleOperationResultForAction<T>(OperationResult<T> result)
        {
            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    OperationErrorType.NotFound => NotFound(result.ErrorMessage),
                    OperationErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return null;
        }

    }
}
