namespace Catalog.API.Products.GetProductById;


public record GetProductByIdQuery(Guid Id) : IQuery<GetProductByIdResult>;

public record GetProductByIdResult(Product Product);

internal class GetProductByIdQueryHandler
    (IDocumentSession session, ILogger<GetProductByIdQueryHandler> logger) 
    : IQueryHandler<GetProductByIdQuery, GetProductByIdResult>
{
    public async Task<GetProductByIdResult> Handle(GetProductByIdQuery query, CancellationToken cancellationToken)
    {
        // In ASP.NET Core code with ILogger, the @ symbol in the logging string has a special meaning
        // when used with Serilog (or other structured loggers).
        // 1) The {@Query} in Serilog means that the query object will be written to the log in a structured form (like JSON).
        // 2) Without the @ the query is simply converted to a string using ToString().
        // 3) With @ Serilog serializes the query to JSON, preserving all its fields.
        // For instance, it would be: GetProductsQueryHandler.Handle called with {"PageSize":10,"CategoryId":5}
        logger.LogInformation("GetProductByIdQueryHandler.Handle called with {@Query}", query);

        var product = await session.LoadAsync<Product>(query.Id, cancellationToken);

        if (product == null)
        {
            throw new ProductNotFoundException(query.Id);
        }

        return new GetProductByIdResult(product);
    }
}