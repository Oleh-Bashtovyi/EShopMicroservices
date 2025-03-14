namespace Catalog.API.Products.GetProducts;

public record GetProductsQuery() : IQuery<GetProductsResult>;

public record GetProductsResult(IEnumerable<Product> Products);

internal class GetProductsQueryHandler
    (IDocumentSession session) // Logging is handled in MediatR pipeline behavior 
    //(IDocumentSession session, ILogger<GetProductsQueryHandler> logger) 
    : IQueryHandler<GetProductsQuery, GetProductsResult> 
{
    public async Task<GetProductsResult> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        // In ASP.NET Core code with ILogger, the @ symbol in the logging string has a special meaning
        // when used with Serilog (or other structured loggers).
        // 1) The {@Query} in Serilog means that the query object will be written to the log in a structured form (like JSON).
        // 2) Without the @ the query is simply converted to a string using ToString().
        // 3) With @ Serilog serializes the query to JSON, preserving all its fields.
        // For instance, it would be: GetProductsQueryHandler.Handle called with {"PageSize":10,"CategoryId":5}
        // logger.LogInformation("GetProductsQueryHandler.Handle called with {@Query}", query);

        var products = await session.Query<Product>().ToListAsync(cancellationToken);

        return new GetProductsResult(products);
    }
}
