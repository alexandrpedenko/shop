namespace Shop.Core.DataEF.Models
{
    public sealed class OrderLineModel
    {
        public int Id { get; set; }

        public decimal Price { get; set; }

        public int Quantity { get; set; }

        public int OrderId { get; set; }

        public string ProductSKU { get; set; }

        public int ProductId { get; set; }

        public ProductModel Product { get; set; }

        public OrderModel Order { get; set; }
    }
}
