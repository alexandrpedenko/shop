using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Shop.Core.DataEF;
using Shop.Core.DataEF.Models;
using Shop.Core.Helpers.OperationResult;
using Shop.Domain.Products;

namespace Shop.Core.Services.Products
{
    /// <summary>
    /// Product management service
    /// </summary>
    /// <param name="dbContext"></param>
    /// <param name="mapper"></param>
    public sealed class ProductService(ShopContext dbContext, IMapper mapper)
    {
        private readonly ShopContext _dbContext = dbContext;
        private readonly IMapper _mapper = mapper;

        public async Task<Product> CreateProductAsync(Product product)
        {
            var dbModel = _mapper.Map<ProductModel>(product);

            _dbContext.Products.Add(dbModel);
            await _dbContext.SaveChangesAsync();

            return _mapper.Map<Product>(dbModel);
        }

        public async Task<bool> CheckIfExistBySkuAsync(string sku)
        {
            return await _dbContext.Products.AnyAsync(p => p.SKU == sku);
        }

        public async Task<OperationResult<List<ProductPriceUpdateDto>>> ParseCsvAsync(IFormFile file)
        {
            using var stream = file.OpenReadStream();
            using var reader = new StreamReader(stream);
            var updates = new List<ProductPriceUpdateDto>();

            while (!reader.EndOfStream)
            {
                var line = await reader.ReadLineAsync();
                if (string.IsNullOrWhiteSpace(line)) continue;

                var parts = line.Split(',');
                if (parts.Length != 2)
                {

                    return OperationResult<List<ProductPriceUpdateDto>>
                        .Failure("Invalid CSV format", OperationErrorType.Validation);
                }

                var sku = parts[0].Trim();
                if (!decimal.TryParse(parts[1].Trim(), out var price))
                {
                    return OperationResult<List<ProductPriceUpdateDto>>
                        .Failure($"Invalid price format for SKU {sku}", OperationErrorType.Validation);
                }

                updates.Add(new ProductPriceUpdateDto { SKU = sku, Price = price });
            }

            return OperationResult<List<ProductPriceUpdateDto>>.Success(updates);
        }

        public async Task<OperationResult<int>> BulkUpdatePricesAsync(IList<ProductPriceUpdateDto> updates)
        {
            var SKUs = updates.Distinct().Select(u => u.SKU).ToList();

            var products = await _dbContext.Products
                .Where(p => SKUs.Contains(p.SKU))
                .ToDictionaryAsync(p => p.SKU);

            var invalidPriceUpdates = updates.Where(u => u.Price < 0).ToList();
            if (invalidPriceUpdates.Any())
            {
                var invalidSkus = string.Join(", ", invalidPriceUpdates.Select(u => u.SKU));
                var errorMessage = $"Products with SKUs {invalidSkus} have negative prices.";
                return OperationResult<int>.Failure(errorMessage, OperationErrorType.Validation);
            }

            if (products.Count != SKUs.Count)
            {
                var missingProductSKUs = SKUs.Except(products.Keys).ToList();
                var errorMessage = $"Products with SKUs {string.Join(", ", missingProductSKUs)} not found.";
                return OperationResult<int>.Failure(errorMessage, OperationErrorType.NotFound);
            }

            using var transaction = await _dbContext.Database.BeginTransactionAsync();
            try
            {
                foreach (var update in updates)
                {
                    if (products.TryGetValue(update.SKU, out var product))
                    {
                        product.Price = update.Price;

                        // NOTE: !Just for transaction testing!
                        if (update.SKU == "invalidProductSKUForUpdate")
                        {
                            throw new InvalidOperationException("Test rollback error");
                        }
                    }
                }

                var savedResult = await _dbContext.SaveChangesAsync();
                await transaction.CommitAsync();

                return OperationResult<int>.Success(savedResult);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return OperationResult<int>.Failure(ex.Message, OperationErrorType.Unexpected);
            }
        }

        public sealed record ProductPriceUpdateDto
        {
            public string SKU { get; init; }
            public decimal Price { get; init; }
        }
    }
}
