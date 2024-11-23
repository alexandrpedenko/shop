namespace Shop.API.Contracts.Requests.Orders
{
    /// <summary>
    /// Apply order discount request
    /// </summary>
    public sealed class ApplyDiscountRequestDto
    {
        /// <summary>
        /// Discount percentage
        /// </summary>
        public decimal DiscountPercentage { get; init; }
    }
}
