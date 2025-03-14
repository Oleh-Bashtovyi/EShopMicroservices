namespace Catalog.API.Products.DeleteProduct;


public record DeleteProductCommand(Guid Id) : ICommand<DeleteProductResult>;


public record DeleteProductResult(bool IsSuccess);


public class DeleteProductValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Product ID is required");
    }
}


internal class DeleteProductCommandHandler
    (IDocumentSession session) // Logging is handled in MediatR pipeline behavior 
    //(IDocumentSession session, ILogger<DeleteProductCommandHandler> logger)
    : ICommandHandler<DeleteProductCommand, DeleteProductResult>
{
    public async Task<DeleteProductResult> Handle(DeleteProductCommand command, CancellationToken cancellationToken)
    {
        // Logging is handled in MediatR pipeline behavior, so just comment or delete next line
        // logger.LogInformation("DeleteProductCommandHandler.Handle called with {@Command}", command);

        session.Delete<Product>(command.Id);
        await session.SaveChangesAsync(cancellationToken);

        return new DeleteProductResult(true);
    }
}