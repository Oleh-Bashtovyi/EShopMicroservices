using Discount.Grpc.Protos;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);
var assembly = typeof(Program).Assembly;

// Add services to the container
// ==================================================================================


// Application services
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});


// Data services
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    // Set UserName as primary key for UserName table, or use [Identity] attribute
    opts.Schema.For<ShoppingCart>().Identity(x => x.UserName); 
}).UseLightweightSessions();

// Scrutor is used to extend the standard DI container of ASP.NET Core
// and to decorate the IBasketRepository interface with two implementations:
// BasketRepository (the primary implementation) and CachedBasketRepository (the decorator).
// This allows adding caching functionality without modifying the primary repository,
// adhering to the Single Responsibility Principle.
// Registration process:
// 1) First, register the primary implementation, BasketRepository
// 2) Then, use Scrutor to decorate IBasketRepository
// 3) Set CachedBasketRepository as a decorator over BasketRepository
// 4) CachedBasketRepository receives IBasketRepository and IDistributedCache in its constructor
// 5) When the IBasketRepository service is requested, the system injects CachedBasketRepository
// 6) CachedBasketRepository checks the cache and delegates the request to BasketRepository only if necessary
// So basically we implemented decorator and proxy pattern simultaneously:
// - BasketRepository was replaced with CachedBasketRepository (Proxy)
// - BasketRepository was decorated using cache capabilities (Decorator) 
// Last instance of Decorate will be passed to IBasketRepository parameter
builder.Services.AddScoped<IBasketRepository, BasketRepository>();
builder.Services.Decorate<IBasketRepository, CachedBasketRepository>();

builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = builder.Configuration.GetConnectionString("Redis");
    options.InstanceName = "Basket";
});


// Grpc services
builder.Services.AddGrpcClient<DiscountProtoService.DiscountProtoServiceClient>(options =>
{
    options.Address = new Uri(builder.Configuration["GrpcSettings:DiscountUrl"]!);
})
//Used to bypass untrusted development certificate. USE IN DEVELOPMENT PROCESS ONLY
.ConfigurePrimaryHttpMessageHandler(() =>
{
    var handler = new HttpClientHandler
    {
        ServerCertificateCustomValidationCallback =
            HttpClientHandler.DangerousAcceptAnyServerCertificateValidator
    };

    return handler;
});


// Cross-cutting services
builder.Services.AddExceptionHandler<CustomExceptionHandler>();
builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!)
    .AddRedis(builder.Configuration.GetConnectionString("Redis")!);



var app = builder.Build();


// Configure the HTTP pipeline
// ==================================================================================
app.MapCarter();
app.UseExceptionHandler(options => {});
app.UseHealthChecks("/health", 
    new HealthCheckOptions
    {
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    });
app.Run();
