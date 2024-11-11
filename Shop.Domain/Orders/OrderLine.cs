using Shop.Domain.Common;

namespace Shop.Domain.Orders
{
    public sealed class OrderLine
    {
        private const int MinQuantity = 1;
        private const int MaxQuantity = 100;

        private Price _price;

        public int Id { get; private set; }

        public string ProductSKU { get; private set; }

        public int Quantity { get; private set; }

        public decimal Price => _price.Value;

        public OrderLine(string productSKU, int quantity)
        {
            if (quantity < MinQuantity)
            {
                throw new ArgumentException($"Quantity must be at least {MinQuantity}");
            };
            if (quantity > MaxQuantity)
            {
                throw new ArgumentException($"Quantity cannot exceed {MaxQuantity}");
            }

            ProductSKU = productSKU;
            Quantity = quantity;
        }

        public void SetPrice(decimal price)
        {
            if (price <= 0)
            {
                throw new ArgumentException("Price must be greater than zero.");
            }

            _price = new Price(price);
        }
    }
}
