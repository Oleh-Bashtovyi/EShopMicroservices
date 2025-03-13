namespace Catalog.API.Exceptions;
public class ProductNotFoundException(Guid productId) : Exception("Product not found!")
{
    public Guid ProductId { get; set; } = productId;
}
