namespace Shop.Domain.Common;

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

    public static implicit operator decimal(Price price) => price.Value;
}

