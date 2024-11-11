using Microsoft.EntityFrameworkCore;
using Shop.Core.DataEF;
using Shop.Core.DataEF.Models;
using Shop.Core.Helpers.OperationResult;
using Shop.Domain.Orders;

namespace Shop.Core.Services.Orders
{
    public sealed class OrdersService(ShopContext context)
    {
        private readonly ShopContext _context = context;

        public async Task<OperationResult<int>> CreateOrderAsync(Order order)
        {
            if (order.OrderLines.Count == 0)
            {
                return OperationResult<int>.Failure("Order must contain at least one product", OperationErrorType.Validation);
            }

            var productSKUs = order.OrderLines.Select(ol => ol.ProductSKU).ToList();
            var existingProducts = await _context.Products
                .Where(p => productSKUs.Contains(p.SKU))
                .Select(p => new
                {
                    p.SKU,
                    p.Price,
                    p.Id
                })
                .ToDictionaryAsync(p => p.SKU, p => new { p.Price, p.Id });

            if (existingProducts.Count != productSKUs.Count)
            {
                var missingProductSKUs = productSKUs.Except(existingProducts.Keys).ToList();

                return OperationResult<int>.Failure(
                    $"Products with SKUs {string.Join(", ", missingProductSKUs)} not found.",
                    OperationErrorType.NotFound);
            }

            foreach (var orderLine in order.OrderLines)
            {
                var price = existingProducts[orderLine.ProductSKU].Price;
                orderLine.SetPrice(price);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                OrderModel orderModel = new()
                {
                    OrderDate = order.OrderDate,
                    OrderLines = order.OrderLines.Select(ol => new OrderLineModel
                    {
                        ProductSKU = ol.ProductSKU,
                        Quantity = ol.Quantity,
                        Price = ol.Price,
                        ProductId = existingProducts[ol.ProductSKU].Id
                    }).ToList()
                };

                _context.Orders.Add(orderModel);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return OperationResult<int>.Success(orderModel.Id);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                await transaction.RollbackAsync();
                return OperationResult<int>.Failure("An error occurred while creating the order.", OperationErrorType.Unexpected);
            }
        }
    }

}
