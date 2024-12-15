namespace Shop.Domain.Users
{
    // NOTE: not used yet
    public sealed class User
    {
        public string Id { get; }
        public string Name { get; }
        public string Email { get; }
        public string Role { get; }

        public User(string id, string name, string email, string role)
        {
            if (string.IsNullOrWhiteSpace(id))
                throw new ArgumentException("ID cannot be empty.", nameof(id));

            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                throw new ArgumentException("Invalid email.", nameof(email));

            if (role != "Admin" && role != "Customer")
                throw new ArgumentException("Invalid role. Role must be either 'Admin' or 'Customer'.", nameof(role));

            Id = id;
            Name = name;
            Email = email;
            Role = role;
        }
    }
}
