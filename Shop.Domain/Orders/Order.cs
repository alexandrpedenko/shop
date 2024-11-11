namespace Shop.Domain.Orders
{
    public sealed class Order(DateTime orderDate)
    {
        private readonly List<OrderLine> _orderLines = [];

        public int Id { get; private set; }

        public DateTime OrderDate { get; private set; } = orderDate;

        public IReadOnlyCollection<OrderLine> OrderLines => _orderLines;

        public void AddOrderLine(OrderLine newOrderLine)
        {
            _orderLines.Add(newOrderLine);
        }
    }
}
