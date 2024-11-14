using Shop.Domain.Orders;
namespace Shop.Domain.Services
{
    public sealed class OrderDomainService
    {
        public static Order Create(DateTime orderDate, OrderLine[] orderLines)
        {
            return new Order(orderDate, orderLines);
        }
    }
}
