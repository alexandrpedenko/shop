using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using Shop.API.Contracts.Requests.Orders;
using Shop.Core.Exceptions.Common;
using Shop.Core.Helpers.OperationResult;
using Shop.Core.Services.Orders;
using Shop.Domain.Orders;

namespace Shop.API.Controllers
{
    /// <summary>
    /// Order controller
    /// </summary>
    [ApiVersion(1)]
    [ApiController]
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
            var order = new Order(DateTime.UtcNow);

            foreach (var request in orderRequest.Products)
            {
                var orderLine = new OrderLine(request.ProductSKU, request.Quantity);
                order.AddOrderLine(orderLine);
            }

            var result = await _orderService.CreateOrderAsync(order);

            if (!result.IsSuccess)
            {
                return result.ErrorType switch
                {
                    OperationErrorType.NotFound => NotFound(result.ErrorMessage),
                    OperationErrorType.Validation => BadRequest(result.ErrorMessage),
                    _ => StatusCode(500, result.ErrorMessage)
                };
            }

            return CreatedAtAction(nameof(CreateOrder), new { id = result.Value });
        }
    }
}
