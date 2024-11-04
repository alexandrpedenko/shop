using Shop.Core.Exceptions.Common;

namespace Shop.Core.Exceptions.Products
{
    // Move to API
    public sealed class ProductNotFoundException(int id) : NotFoundException($"Product with id: {id} not found")
    {
    }
}
