namespace Shop.Domain.Product;

public sealed class Product
{
    public int Id { get; private set; }

    public Title _title;
    public Description _description;
    public Price _price;

    public string Title => _title.Value;
    public string Description => _description.Value;
    public decimal Price => _price.Value;

    public Product(Title title, Description description, Price price)
    {
        ArgumentNullException.ThrowIfNull(title);
        ArgumentNullException.ThrowIfNull(description);
        ArgumentNullException.ThrowIfNull(price);

        _title = title;
        _description = description;
        _price = price;
    }
}
