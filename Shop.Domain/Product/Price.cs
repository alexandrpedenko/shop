namespace Shop.Domain.Product;

public class Price
{
    public decimal Value { get; }

    public Price(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Price must be greater than zero");
        }

        Value = value;
    }
}

