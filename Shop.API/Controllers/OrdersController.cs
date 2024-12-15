using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Shop.API.Contracts.Requests.Orders;
using Shop.API.Contracts.Responses.Orders;
using Shop.Core.DTOs.Orders;
using Shop.Core.Exceptions.Common;
using Shop.Core.Helpers.OperationResult;
using Shop.Core.Services.Orders;

namespace Shop.API.Controllers
{
    /// <summary>
    /// Order controller
    /// </summary>
    [ApiVersion(1)]
    [ApiController]
    [Authorize(Roles = "Customer")]
    [Route("api/v{v:apiVersion}/[controller]")]
    public sealed class OrdersController(OrdersService orderService) : ControllerBase
    {
        private readonly OrdersService _orderService = orderService;

        /// <summary>
        /// Creates a Order based on Product's ids
        /// </summary>
        /// <body name="orderRequest">List of products ids for Order</body>
        /// <returns>The Order Id</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequestDto orderRequest)
        {
            if (orderRequest.Products.Count == 0)
            {
                return BadRequest("Order must contain at least one product");
            }

            CreateOrderDto orderDto = new()
            {
                Products = orderRequest.Products,
            };

            var result = await _orderService.CreateOrderAsync(orderDto);

            var createdOrderAction = result.CheckForAction();

            if (createdOrderAction != null)
            {
                return createdOrderAction;
            }

            return CreatedAtAction(nameof(CreateOrder), new { id = result.Value });
        }

        /// <summary>
        /// Apply discount
        /// </summary>
        /// <body name="request">Discount percentage</body>
        /// <returns>Updated Order</returns>
        /// <exception cref="InternalServerErrorException"></exception>
        [HttpPost("{orderId}/apply-discount")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ApplyDiscount(int orderId, [FromBody] ApplyDiscountRequestDto request)
        {
            var result = await _orderService.ApplyDiscountAsync(orderId, request.DiscountPercentage);

            var applyDiscountAction = result.CheckForAction();

            if (applyDiscountAction != null)
            {
                return applyDiscountAction;
            }

            return Ok(new ApplyDiscountResponseDto { TotalPrice = result.Value.TotalPrice });
        }
    }
}
