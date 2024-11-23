using FluentAssertions;
using Shop.Domain.Common;
using Shop.Domain.Orders;
using Shop.Domain.Services;

namespace Shop.API.IntegrationTests.DomainUnitTests.Orders
{
    public class OrderDiscountServiceTests
    {
        [Fact]
        public void ApplyDiscount_ShouldCorrectlyApplyDiscount_WhenPercentageIsValid()
        {
            // Arrange
            var order = CreateOrder();
            var discountPercentage = new DiscountPercentage(10);

            // Act
            OrderDiscountService.ApplyDiscount(order, discountPercentage);

            // Assert
            order.DiscountPercentage.Value.Should().Be(10);
            order.TotalPrice.Should().Be(90);
        }

        [Theory]
        [InlineData(-5)]
        [InlineData(150)]
        public void ApplyDiscount_ShouldThrowException_WhenPercentageIsInvalid(decimal invalidPercentage)
        {
            // Arrange
            var order = CreateOrder();

            // Act
            var act = () => OrderDiscountService.ApplyDiscount(order, new DiscountPercentage(invalidPercentage));

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Discount percentage must be between 0 and 100.");
        }

        private static Order CreateOrder()
        {
            OrderLine[] orderLines =
            [
                new("SKU1", 1, 50, 1),
                new("SKU2", 1, 50, 2)
            ];

            return new Order(DateTime.UtcNow, orderLines);
        }
    }
}
