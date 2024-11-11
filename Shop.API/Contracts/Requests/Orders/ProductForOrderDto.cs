namespace Shop.API.Contracts.Requests.Orders
{
    /// <summary>
    /// Product for order
    /// </summary>
    public sealed record ProductForOrderDto
    {
        /// <summary>
        /// Product id
        /// </summary>
        public string ProductSKU { get; set; }

        /// <summary>
        /// Product quantity
        /// </summary>
        public int Quantity { get; set; }
    }
}
