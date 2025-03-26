using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Ordering.Infrastructure.Data.Extensions;

public static class DatabaseExtensions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope(); 
        
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        /*There are some important differences between these two approaches:

        --> context.Database.MigrateAsync().GetAwaiter().GetResult();
        This is a synchronous blocking call that waits for the asynchronous operation to complete
        It can potentially cause deadlocks in certain scenarios, especially in UI or ASP.NET Core context
        It's essentially forcing an async method to behave synchronously
        Useful in scenarios where you can't use await, like in a constructor or synchronous method

        --> await context.Database.MigrateAsync();
        This is the recommended, truly asynchronous approach
        Allows the current thread to be released while waiting for the migration to complete
        Prevents potential deadlocks
        Works best in async methods
        Provides better performance and resource utilization

        --> Best practices:
        Prefer await whenever possible
        Only use GetAwaiter().GetResult() when you absolutely cannot use await
        In ASP.NET Core, it's typically best to use await in your application startup (e.g., in Program.cs)*/
        context.Database.MigrateAsync().GetAwaiter().GetResult();

        await SeedAsync(context);
    }

    private static async Task SeedAsync(ApplicationDbContext context)
    {
        await SeedCustomerAsync(context);
        await SeedProductAsync(context);
        await SeedOrdersWithItemsAsync(context);
    }

    private static async Task SeedCustomerAsync(ApplicationDbContext context)
    {
        if (!await context.Customers.AnyAsync())
        {
            await context.Customers.AddRangeAsync(InitialData.Customers);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedProductAsync(ApplicationDbContext context)
    {
        if (!await context.Products.AnyAsync())
        {
            await context.Products.AddRangeAsync(InitialData.Products);
            await context.SaveChangesAsync();
        }
    }

    private static async Task SeedOrdersWithItemsAsync(ApplicationDbContext context)
    {
        if (!await context.Orders.AnyAsync())
        {
            await context.Orders.AddRangeAsync(InitialData.OrdersWithItems);
            await context.SaveChangesAsync();
        }
    }
}