namespace Shop.Domain.Common
{
    public class Title
    {
        public string Value { get; }

        public Title(string value)
        {
            // ArgumentException.ThrowIfNullOrWhiteSpace(value, nameof(value));

            if (string.IsNullOrWhiteSpace(value))
            {
                throw new ArgumentException("Title cannot be empty");
            }

            if (value.Length > 100)
            {
                throw new ArgumentException("Title cannot exceed 100 characters");
            }

            Value = value;
        }
    }
}
