﻿namespace Shop.Core.DataEF.Models;

public class OrderModel
{
    public int Id { get; set; }

    public DateTime OrderDate { get; set; }

    public decimal TotalPrice { get; set; }

    public decimal DiscountPercentage { get; set; }

    public required ICollection<OrderLineModel> OrderLines { get; set; } = new List<OrderLineModel>();
}

