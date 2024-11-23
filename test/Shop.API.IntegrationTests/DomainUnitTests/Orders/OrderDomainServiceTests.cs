using FluentAssertions;
using Shop.Domain.Orders;
using Shop.Domain.Services;

namespace Shop.API.IntegrationTests.DomainUnitTests.Orders
{
    public class OrderDomainServiceTests
    {
        [Fact]
        public void CreateOrder_ShouldSucceed_WhenOrderLinesAreValid()
        {
            // Arrange
            var orderLines = new[]
            {
                new OrderLine("SKU1", 2, 50, 1)
            };

            // Act
            var order = OrderDomainService.Create(DateTime.UtcNow, orderLines);

            // Assert
            order.Should().NotBeNull();
            order.OrderLines.Should().HaveCount(1);
            order.TotalPrice.Should().Be(100);
        }

        [Fact]
        public void CreateOrder_ShouldThrowException_WhenOrderLinesAreEmpty()
        {
            // Arrange
            var emptyOrderLines = Array.Empty<OrderLine>();

            // Act
            var act = () => OrderDomainService.Create(DateTime.UtcNow, emptyOrderLines);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Order must contain at least one order line.");
        }

        [Fact]
        public void CreateOrder_ShouldThrowException_WhenOrderLinesAreNull()
        {
            // Arrange
            OrderLine[]? nullOrderLines = null;

            // Act
            var act = () => OrderDomainService.Create(DateTime.UtcNow, nullOrderLines);

            // Assert
            act.Should().Throw<ArgumentException>()
                .WithMessage("Order must contain at least one order line.");
        }
    }
}
