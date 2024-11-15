using Microsoft.EntityFrameworkCore;
using Shop.Core.DataEF;
using Shop.Core.DataEF.Models;
using Shop.Core.DTOs.Orders;
using Shop.Core.Helpers.OperationResult;
using Shop.Domain.Common;
using Shop.Domain.Orders;
using Shop.Domain.Services;

namespace Shop.Core.Services.Orders
{
    public sealed class OrdersService(ShopContext context)
    {
        private readonly ShopContext _context = context;

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
                return OperationResult<int>.Success(orderModel.Id);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return OperationResult<int>.Failure(failingError, OperationErrorType.Unexpected);
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
                productSKU: new SKU(p.ProductSKU),
                quantity: new Quantity(p.Quantity),
                price: new Price(existingProducts[p.ProductSKU].Price),
                productId: existingProducts[p.ProductSKU].Id
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
