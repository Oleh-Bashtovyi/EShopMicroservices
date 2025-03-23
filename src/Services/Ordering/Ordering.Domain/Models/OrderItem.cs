namespace Ordering.Domain.Models;

public class OrderItem : Entity<OrderItemId>
{

    // STRONGLY TYPED IDs
    // --> Creating distinct types for each kind of ID in your domain
    // --> This makes code more expressive and less error-prone
    // Primitives obsession is a code smell where primitives (like string, int, Guid)
    // are used for domain concepts, leading to ambiguity and errors.
    // To be sure that we pass orderId and not id for product, we can encapsulate
    // orderId into separate object that has class "OrderId".
    // =========================================================
    // This constructor MUST be internal, because it should be 
    // reached only in domain layer in "Order" class
    internal OrderItem(OrderId orderId, ProductId productId, int quantity, decimal price)
    {
        Id = OrderItemId.Of(Guid.NewGuid());
        OrderId = orderId;
        ProductId = productId;
        Quantity = quantity;
        Price = price;
    }

    public OrderId OrderId { get; private set; } = default!;
    public ProductId ProductId { get; private set; } = default!;
    public int Quantity { get; private set; } = default!;
    public decimal Price { get; private set;} = default!;
}