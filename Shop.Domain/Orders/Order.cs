namespace Shop.Domain.Orders
{
    public sealed class Order(DateTime orderDate, IReadOnlyCollection<OrderLine> orderLines)
    {
        private readonly List<OrderLine> _orderLines = [];

        public int Id { get; private set; }

        public DateTime OrderDate { get; private set; } = orderDate;

        public IReadOnlyCollection<OrderLine> OrderLines { get; private set; } = orderLines;

        public decimal TotalPrice => _orderLines.Sum(line => line.Price.Value * line.Quantity.Value);
    }
}
