using Shop.Domain.Common;
using Shop.Domain.Orders;

namespace Shop.Domain.Services
{
    public static class OrderDiscountService
    {
        public static void ApplyDiscount(Order order, DiscountPercentage discountPercentage)
        {
            order.ApplyDiscountPercentage(discountPercentage);
        }
    }
}
