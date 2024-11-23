using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RepoDb;
using Shop.Core.DataEF;
using Shop.Core.DataEF.Models;
using Shop.Core.Extensions;
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
            try
            {
                using var memoryStream = new MemoryStream();
                await file.CopyToAsync(memoryStream);
                memoryStream.Seek(0, SeekOrigin.Begin);

                var updates = memoryStream.ReadCsv<ProductPriceUpdateDto>().ToList();
                return OperationResult<List<ProductPriceUpdateDto>>.Success(updates);
            }
            catch (Exception ex)
            {
                return OperationResult<List<ProductPriceUpdateDto>>
                    .Failure($"CSV parsing error: {ex.Message}", OperationErrorType.Validation);
            }
        }

        public async Task<OperationResult<int>> BulkUpdateAsync(IList<ProductPriceUpdateDto> updates)
        {
            if (updates == null || updates.Count == 0)
            {
                return OperationResult<int>.Failure("No updates provided.", OperationErrorType.Validation);
            }

            var domainProducts = updates.Select(dto =>
                new Product(
                    title: dto.Title,
                    description: dto.Description,
                    price: dto.Price,
                    sku: dto.SKU
                )
            ).ToList();

            var dbProducts = domainProducts.Select(product => new ProductModel
            {
                Title = product.Title,
                Description = product.Description,
                Price = product.Price,
                SKU = product.SKU
            }).ToList();

            try
            {
                using var connection = new SqlConnection(_dbContext.Database.GetConnectionString());
                await connection.EnsureOpenAsync();

                var mergedRows = await connection.BulkMergeAsync(
                    tableName: "Products",
                    entities: dbProducts,
                    qualifiers: product => new { product.SKU }
                );

                return OperationResult<int>.Success(mergedRows);
            }
            catch (Exception ex)
            {
                return OperationResult<int>.Failure($"Error during products update: {ex.Message}", OperationErrorType.Unexpected);
            }
        }

        public sealed record ProductPriceUpdateDto
        {
            public string SKU { get; init; }
            public decimal Price { get; init; }
            public string Title { get; init; } = default!;
            public string Description { get; init; } = default!;
        }
    }
}
