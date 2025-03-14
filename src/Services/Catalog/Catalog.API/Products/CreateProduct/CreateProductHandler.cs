using Microsoft.Extensions.Logging;

namespace Catalog.API.Products.CreateProduct;


public record CreateProductCommand(
    string Name,
    List<string> Category,
    string Description,
    string ImageFile,
    decimal Price) : ICommand<CreateProductResult>;


public record CreateProductResult(Guid Id);


public class CrateProductValidator : AbstractValidator<CreateProductCommand>
{
    public CrateProductValidator()
    {
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


internal class CreateProductCommandHandler
    (IDocumentSession session) // Logging is handled in MediatR pipeline behavior 
    //(IDocumentSession session, ILogger<CreateProductCommandHandler> logger) 
    //(IDocumentSession session, IValidator<CreateProductCommand> validator) 
    : ICommandHandler<CreateProductCommand, CreateProductResult>
{
    public async Task<CreateProductResult> Handle(CreateProductCommand command, CancellationToken cancellationToken)
    {
        // This approach is more code prone and require redundant code.
        // Instead, MediatR pipeline behavior was used.
        // ValidationBehavior is located in BuildingBlocks project
        // ========================================================================
        //var result = await validator.ValidateAsync(command, cancellationToken);
        //var errors = result.Errors.Select(x => x.ErrorMessage).ToList();
        //if (errors.Any())
        //{
        //    throw new ValidationException(errors.FirstOrDefault());
        //}
        // ========================================================================

        // Logging is handled in MediatR pipeline behavior, so just comment or delete it
        // logger.LogInformation("CreateProductCommandHandler.Handle called with {@Command}", command);

        // Create product entity from command object
        var product = new Product()
        {
            Name = command.Name,
            Category = command.Category,
            Description = command.Description,
            ImageFile = command.ImageFile,
            Price = command.Price,
        };

        // Save to database
        session.Store(product);
        await session.SaveChangesAsync(cancellationToken);

        // Return CreateProductResult result
        return new CreateProductResult(product.Id);
    }
}