namespace Shop.Domain.Common;

public readonly record struct Quantity
{
    private const int MinQuantity = 1;
    private const int MaxQuantity = 100;

    public readonly int Value;

    public Quantity(int value)
    {
        if (value < MinQuantity)
        {
            throw new ArgumentException($"Quantity must be at least {MinQuantity}");
        };
        if (value > MaxQuantity)
        {
            throw new ArgumentException($"Quantity cannot exceed {MaxQuantity}");
        }

        Value = value;
    }

    public static implicit operator int(Quantity price) => price.Value;
    public static implicit operator Quantity(int value) => new(value);
}

