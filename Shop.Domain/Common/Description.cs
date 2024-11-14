﻿namespace Shop.Domain.Common
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

        public static implicit operator string(Description description) => description.Value;
    }
}
