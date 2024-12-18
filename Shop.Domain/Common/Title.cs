﻿namespace Shop.Domain.Common
{
    public readonly record struct Title
    {
        public string Value { get; }

        public Title(string value)
        {
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

        public static implicit operator string(Title title) => title.Value;
        public static implicit operator Title(string value) => new(value);
    }
}
