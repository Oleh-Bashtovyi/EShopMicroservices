namespace Catalog.API.Products.UpdateProduct;

public record UpdateProductCommand(Guid Id, string Name, List<string> Category, string Description, string ImageFile, decimal Price)
    : ICommand<UpdateProductResult>;


public record UpdateProductResult(bool IsSuccess);


public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductValidator()
    {
        RuleFor(c => c.Id)
            .NotEmpty().WithMessage("Product ID is required");
        RuleFor(c => c.Name)
            .NotEmpty().WithMessage("Name is required")
            .Length(2, 150).WithMessage("Name must be between 2 and 150 characters"); 
        RuleFor(c => c.Category)
            .NotEmpty().WithMessage("Category is required");
        RuleFor(c => c.ImageFile)
            .NotEmpty().WithMessage("ImageFile is required");
        RuleFor(c => c.Price)
            .GreaterThan(0).WithMessage("Price must be grater than 0");
    }
}


internal class UpdateProductCommandHandler
    (IDocumentSession session) // Logging is handled in MediatR pipeline behavior 
    //(IDocumentSession session, ILogger<UpdateProductCommandHandler> logger)
    : ICommandHandler<UpdateProductCommand, UpdateProductResult>
{
    public async Task<UpdateProductResult> Handle(UpdateProductCommand command, CancellationToken cancellationToken)
    {
        // Logging is handled in MediatR pipeline behavior, so just comment or delete it
        //logger.LogInformation("UpdateProductCommandHandler.Handle called with {@Command}", command);

        var product = await session.LoadAsync<Product>(command.Id, cancellationToken);

        if (product is null)
        {
            throw new ProductNotFoundException(command.Id);
        }

        product.Name = command.Name;
        product.Category = command.Category;
        product.Description = command.Description;
        product.ImageFile = command.ImageFile;
        product.Price = command.Price;

        session.Update(product);
        await session.SaveChangesAsync(cancellationToken);

        return new UpdateProductResult(true);
    }
}