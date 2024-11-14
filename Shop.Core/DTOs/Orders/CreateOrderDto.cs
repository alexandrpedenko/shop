namespace Shop.Core.DTOs.Orders
{
    /// <summary>
    /// Create order request
    /// </summary>
    public sealed class CreateOrderDto
    {
        /// <summary>
        /// List of product's ids
        /// </summary>
        public List<ProductForOrderDto> Products { get; set; }
    }
}
