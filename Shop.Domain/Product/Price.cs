namespace Shop.Domain.Product;

public readonly record struct Price
{
    public readonly decimal Value;

    public Price(decimal value)
    {
        if (value <= 0)
        {
            throw new ArgumentException("Price must be greater than zero");
        }

        Value = value;
    }
}

