using Shop.Domain.Common;

namespace Shop.Domain.Products
{
    public sealed class Product
    {
        public int Id { get; private set; }
        public Title _title;
        public Description _description;
        public Price _price;
        public SKU _sku;

        public string Title => _title.Value;
        public string Description => _description.Value;
        public decimal Price => _price.Value;
        public string SKU => _sku.Value;

        public Product(string title, string description, decimal price, string sku)
        {
            ArgumentNullException.ThrowIfNull(title);
            ArgumentNullException.ThrowIfNull(description);
            ArgumentNullException.ThrowIfNull(price);
            ArgumentNullException.ThrowIfNull(sku);

            _title = new Title(title);
            _description = new Description(description);
            _price = new Price(price);
            _sku = new SKU(sku);
        }
    }
}
