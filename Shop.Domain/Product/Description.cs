namespace Shop.Domain.Product
{
    public class Description
    {
        public string Value { get; }

        public Description(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Description cannot be empty");

            if (value.Length > 500)
                throw new ArgumentException("Description cannot exceed 500 characters");

            Value = value;
        }
    }
}
