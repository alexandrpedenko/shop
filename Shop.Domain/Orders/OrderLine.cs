using Shop.Domain.Common;

namespace Shop.Domain.Orders
{
    public readonly record struct OrderLine(SKU ProductSKU, Quantity Quantity, Price Price, int ProductId)
    {
        public readonly int ProductId = ProductId;
        public readonly Price Price = Price;
        public readonly SKU ProductSKU = ProductSKU;
        public readonly Quantity Quantity = Quantity;
    }
}
