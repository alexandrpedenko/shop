using Shop.Core.Exceptions.Common;

namespace Shop.Core.Exceptions.Products
{
    public class ProductNotFoundException(int id) : NotFoundException($"Product with id: {id} not found")
    {
    }
}
