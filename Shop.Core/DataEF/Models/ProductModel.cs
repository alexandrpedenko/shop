namespace Shop.Core.DataEF.Models
{
    /// <summary>
    /// Product model
    /// </summary>
    public sealed class ProductModel
    {
        /// <summary>
        ///  Product id
        /// </summary>
        public int Id { get; set; }

        /// <summary>
        ///  Title
        /// </summary>
        public required string Title { get; set; }

        /// <summary>
        ///  Product description
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        ///  Product price
        /// </summary>
        public decimal Price { get; set; }

        public required string SKU { get; set; }

        public ICollection<OrderLineModel> OrderLines { get; set; } = [];
    }
}
