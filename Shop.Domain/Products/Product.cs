using Shop.Domain.Common;

namespace Shop.Domain.Products
{
    public record struct Product
    {
        public int Id { get; private set; }

        public readonly Title Title;
        public readonly Description Description;
        public readonly Price Price;
        public readonly SKU SKU;

        public Product(Title title, Description description, Price price, SKU sku)
        {
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(description);
            ArgumentNullException.ThrowIfNull(price);
            ArgumentNullException.ThrowIfNull(sku);

            Title = title;
            Description = description;
            Price = price;
            SKU = sku;
        }
    }
}
