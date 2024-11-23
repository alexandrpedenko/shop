namespace Shop.Domain.Common;

public readonly record struct DiscountPercentage
{
    public readonly decimal Value;

    public DiscountPercentage(decimal percentage)
    {
        if (percentage < 0 || percentage > 100)
        {
            throw new ArgumentException("Discount percentage must be between 0 and 100.");
        }

        Value = percentage;
    }

    public static implicit operator decimal(DiscountPercentage price) => price.Value;
    public static implicit operator DiscountPercentage(decimal value) => new(value);
}

