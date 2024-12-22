using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Shop.Core.DataEF;
using Shop.Core.DataEF.Models;
using Shop.Core.DTOs.Orders;
using Shop.Core.Helpers.OperationResult;
using Shop.Core.Services.Redis;
using Shop.Domain.Orders;
using Shop.Domain.Services;
using System.Text.Json;

namespace Shop.Core.Services.Orders
{
    public sealed class OrdersService(
        ShopContext context,
        RedisPublisher redisPublisher,
        ILogger<OrdersService> logger)
    {
        private readonly ShopContext _context = context;
        private readonly RedisPublisher _redisPublisher = redisPublisher;
        private readonly ILogger<OrdersService> _logger = logger;

        private const string failingError = "An error occurred while creating the order.";

        public async Task<OperationResult<int>> CreateOrderAsync(CreateOrderDto createOrderRequest)
        {
            var orderResult = await PrepareOrderPayload(createOrderRequest);

            if (orderResult.ErrorMessage != null)
            {
                return OperationResult<int>.Failure(orderResult.ErrorMessage, orderResult.ErrorType);
            }

            if (orderResult.Value == null)
            {
                return OperationResult<int>.Failure(failingError, OperationErrorType.Unexpected);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var orderModel = MapOrderToOrderModel(orderResult.Value);
                _context.Orders.Add(orderModel);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                var message = JsonSerializer.Serialize(new { OrderId = orderModel.Id, Timestamp = DateTime.UtcNow });
                await _redisPublisher.PublishAsync("order_channel", message);

                _logger.LogInformation("Order created with ID {OrderId} and TotalPrice {TotalPrice}", orderModel.Id, orderModel.TotalPrice);

                return OperationResult<int>.Success(orderModel.Id);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to create order - {ErrorMessage}", ex.Message);

                await transaction.RollbackAsync();
                return OperationResult<int>.Failure($"{failingError}: {ex.Message}", OperationErrorType.Unexpected);
            }
        }

        public async Task<OperationResult<Order>> ApplyDiscountAsync(int orderId, decimal discountPercentage)
        {
            var orderModel = await _context.Orders
                .Include(o => o.OrderLines)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (orderModel == null)
            {
                return OperationResult<Order>.Failure($"Order with ID {orderId} not found.", OperationErrorType.NotFound);
            }

            var order = new Order(
                orderDate: orderModel.OrderDate,
                orderLines: orderModel.OrderLines.Select(ol => new OrderLine(
                    ProductSKU: ol.ProductSKU,
                    Quantity: ol.Quantity,
                    Price: ol.Price,
                    ProductId: ol.ProductId)).ToHashSet()
            );

            OrderDiscountService.ApplyDiscount(order, discountPercentage);

            try
            {
                orderModel.DiscountPercentage = order.DiscountPercentage;
                orderModel.TotalPrice = order.TotalPrice;

                var result = await _context.SaveChangesAsync();

                return OperationResult<Order>.Success(order);
            }
            catch (Exception ex)
            {
                return OperationResult<Order>.Failure($"Failed to apply discount: {ex.Message}", OperationErrorType.Unexpected);
            }
        }

        private async Task<OperationResult<Order>> PrepareOrderPayload(CreateOrderDto createOrderRequest)
        {
            var productSKUs = createOrderRequest.Products.Select(ol => ol.ProductSKU).Distinct().ToList();
            var existingProducts = await FetchExistingProducts(productSKUs);

            if (existingProducts.Count != productSKUs.Count)
            {
                var missingProductSKUs = productSKUs.Except(existingProducts.Keys).ToList();
                var errorMessage = $"Products with SKUs {string.Join(", ", missingProductSKUs)} not found.";
                return OperationResult<Order>.Failure(errorMessage, OperationErrorType.NotFound);
            }

            var orderLines = createOrderRequest.Products.Select(p => new OrderLine(
                    ProductSKU: p.ProductSKU,
                    Quantity: p.Quantity,
                    Price: existingProducts[p.ProductSKU].Price,
                    ProductId: existingProducts[p.ProductSKU].Id
                    )).ToArray();

            var order = OrderDomainService.Create(DateTime.UtcNow, orderLines);

            return OperationResult<Order>.Success(order);
        }

        private async Task<Dictionary<string, ProductForLine>> FetchExistingProducts(IEnumerable<string> productSKUs)
        {
            return await _context.Products
                .Where(p => productSKUs.Contains(p.SKU))
                .Select(p => new ProductForLine
                {
                    SKU = p.SKU,
                    Price = p.Price,
                    Id = p.Id
                })
                .ToDictionaryAsync(p => p.SKU);
        }

        private async Task<Dictionary<string, ProductForLine>> FetchExistingProducts(CreateOrderDto createOrderRequest)
        {
            var productSKUs = createOrderRequest.Products.Select(p => p.ProductSKU).Distinct().ToList();
            return await FetchExistingProducts(productSKUs);
        }

        private static OrderModel MapOrderToOrderModel(Order order)
        {
            return new OrderModel
            {
                OrderDate = order.OrderDate,
                TotalPrice = order.TotalPrice,
                OrderLines = order.OrderLines.Select(ol => new OrderLineModel
                {
                    ProductSKU = ol.ProductSKU,
                    Quantity = ol.Quantity,
                    Price = ol.Price,
                    ProductId = ol.ProductId,
                }).ToList()
            };
        }

        private record ProductForLine
        {
            public string SKU { get; init; }
            public decimal Price { get; init; }
            public int Id { get; init; }
        }
    }

}
