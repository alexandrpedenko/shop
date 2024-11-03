namespace Shop.API.Contracts.Requests.Products
{
    /// <summary>
    /// Create product dto
    /// </summary>
    public class CreateProductRequestDto
    {
        public required string Title { get; set; }

        public required string Description { get; set; }

        public decimal Price { get; set; }
    }
}
