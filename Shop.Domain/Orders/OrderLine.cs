using Shop.Domain.Common;

namespace Shop.Domain.Orders
{
    public sealed class OrderLine(SKU productSKU, Quantity quantity, Price price, int productId)
    {
        public int Id { get; private set; }

        public int ProductId { get; private set; } = productId;

        public Price Price { get; private set; } = price;

        public SKU ProductSKU { get; private set; } = productSKU;

        public Quantity Quantity { get; private set; } = quantity;
    }
}
