using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Shop.API.Contracts.Requests.Products;
using Shop.Core.Exceptions.Common;
using Shop.Core.Services.Products;
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
                request.Title,
                request.Description,
                request.Price,
                request.SKU
            );

            var createdProduct = await _productService.CreateProductAsync(product);

            return CreatedAtAction(nameof(CreateProduct), createdProduct);
        }
    }
}
