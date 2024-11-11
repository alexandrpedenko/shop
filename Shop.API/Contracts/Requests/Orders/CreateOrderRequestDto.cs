namespace Shop.API.Contracts.Requests.Orders
{
    /// <summary>
    /// Create order request
    /// </summary>
    public sealed class CreateOrderRequestDto
    {
        /// <summary>
        /// List of product's ids
        /// </summary>
        public List<ProductForOrderDto> Products { get; set; }
    }
}
