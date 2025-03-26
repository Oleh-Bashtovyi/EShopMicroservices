using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ordering.Infrastructure.Data.Interceptors;

namespace Ordering.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("Database");

        // Interceptors in EF Core are middleware-like components that allow you to 
        // intercept and modify Entity Framework operations before they are executed.
        // They provide a way to add cross-cutting concerns like logging, auditing, 
        // or custom behaviors without modifying the core database context.

        // This specific interceptor automatically manages audit fields (CreatedBy, 
        // CreatedAt, LastModifiedBy, LastModified) for entities implementing IEntity 
        // interface. It intercepts save operations to automatically update these 
        // fields whenever an entity is added or modified, including nested owned entities.
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();

        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            // But we must remember to register MediatR before injecting DispatchDomainEventsInterceptor
            // because it's constructor depends on IMediator object.
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
