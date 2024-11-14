using Shop.Domain.Common;

namespace Shop.Domain.Products
{
    public sealed class Product
    {
        public int Id { get; private set; }
        public Title Title { get; }
        public Description Description { get; }
        public Price Price { get; }
        public SKU SKU { get; }

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
