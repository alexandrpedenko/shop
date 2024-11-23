namespace Shop.Domain.Common
{
    public readonly record struct SKU
    {
        public string Value { get; }

        public SKU(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("SKU cannot be empty");
            }

            if (value.Length > 50)
            {
                throw new ArgumentException("SKU cannot exceed 50 characters");
            }

            Value = value;
        }

        public static implicit operator string(SKU title) => title.Value;
        public static implicit operator SKU(string value) => new(value);
    }
}
