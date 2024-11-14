namespace Shop.API.Contracts.Responses.Products
{
    public sealed record GetProductResponseDto
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public decimal Price { get; set; }

        public string SKU { get; set; }
    }
}
