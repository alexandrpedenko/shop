using Shop.Domain.Orders;
namespace Shop.Domain.Services
{
    public sealed class OrderDomainService
    {
        public static Order Create(DateTime orderDate, OrderLine[] orderLines)
        {
            if (orderLines == null || !orderLines.Any())
            {
                throw new ArgumentException("Order must contain at least one order line.");
            }

            return new Order(orderDate, orderLines);
        }
    }
}
