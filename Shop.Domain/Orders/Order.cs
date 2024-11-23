using Shop.Domain.Common;

namespace Shop.Domain.Orders
{
    public sealed class Order
    {
        public int Id { get; private set; }

        public readonly DateTime OrderDate;

        public readonly IReadOnlyCollection<OrderLine> OrderLines;

        public decimal TotalPrice;

        public DiscountPercentage DiscountPercentage;

        public Order(DateTime orderDate, IReadOnlyCollection<OrderLine> orderLines)
        {
            if (orderLines == null || !orderLines.Any())
            {
                throw new ArgumentException("Order must contain at least one order line.");
            }

            OrderDate = orderDate;
            OrderLines = orderLines;
            TotalPrice = orderLines.Sum(line => line.Price.Value * line.Quantity.Value);
            DiscountPercentage = 0;
        }

        public void ApplyDiscountPercentage(decimal percentage)
        {
            if (percentage < 0 || percentage > 100)
            {
                throw new ArgumentException("Discount percentage must be between 0 and 100.");
            }

            DiscountPercentage = percentage;
            UpdateTotalPriceWithDiscount();
        }

        private void UpdateTotalPriceWithDiscount()
        {
            TotalPrice -= GetDiscountAmount();
        }

        private decimal GetDiscountAmount()
        {
            return TotalPrice * (DiscountPercentage / 100);
        }
    }
}
