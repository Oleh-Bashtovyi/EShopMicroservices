using Catalog.API.Data;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var assembly = typeof(Program).Assembly;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// ==================================================================================

builder.Services.AddValidatorsFromAssembly(assembly);

builder.Services.AddExceptionHandler<CustomExceptionHandler>();

builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(assembly);

    // Add validation behavior as a pipeline behavior into mediatR
    config.AddOpenBehavior(typeof(ValidationBehavior<,>));
    config.AddOpenBehavior(typeof(LoggingBehavior<,>));
});

// Endpoints were created using Carter library.
// Currently, if we install Carter to BuildingBlocks project,
// it will not Map any classes that implements ICarterModule interface.
// That`s because it will scan only BuildingBlocks project for those classes.
// Unfortunately Marten library currently does not have ability
// To config assembly scan, as MediatR do in next line.
// So only solution is to install Carter in this project, and not in BuildingBlocks.
builder.Services.AddCarter();

// For products storage we will use PostgreSQL.
// PostgreSQL has some NoSql db features:
// (json column features that allows to store and query data as json document)
// With Marten library we can utilize them.
// This setup is ideal for handling complex data structures like product entity,
// Which includes list of the categories and other varied data types
// and product characteristics.
// * LightweightSession - fast saving without competition control
// * DirtyTrackedSession - automatic tracking of changes
// * OptimisticConcurrencySession - control of competitiveness
// * QuerySession - fast reading without transaction
builder.Services.AddMarten(opts =>
{
    opts.Connection(builder.Configuration.GetConnectionString("Database")!);
    //opts.AutoCreateSchemaObjects = AutoCreate.CreateOrUpdate;
}).UseLightweightSessions();

if (builder.Environment.IsDevelopment())
{
    builder.Services.InitializeMartenWith<CatalogInitialData>();
}

// HEALTH CHECKS
builder.Services
    .AddHealthChecks()
    // Next line of code will be performed the health check for the PostgreSQL
    // database within the catalog microservice
    .AddNpgSql(builder.Configuration.GetConnectionString("Database")!);


var app = builder.Build();

// Configure the HTTP pipeline
// ==================================================================================

app.MapCarter();

// The best practice to handle global
// exceptions is to use IExceptionHandler interface
//==========================================================
//app.UseExceptionHandler(exceptionHandlerApp =>
//{
//    exceptionHandlerApp.Run(async context =>
//    {
//        var exception = context.Features.Get<IExceptionHandlerFeature>()?.Error;
//        if (exception == null)
//        {
//            return;
//        }

//        var problemDetails = new ProblemDetails
//        {
//            Title = exception.Message,
//            Status = StatusCodes.Status500InternalServerError,
//            Detail = exception.StackTrace
//        };

//        var logger = context.RequestServices.GetRequiredService<ILogger<Program>>();
//        logger.LogError(exception, exception.Message);

//        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
//        context.Response.ContentType = "application/problem+json";

//        await context.Response.WriteAsJsonAsync(problemDetails);
//    });
//});
//==========================================================

// The empty option parameter indicates that we are relying on custom configured handler
app.UseExceptionHandler(opts => {});


// HEALTH CHECKS
// =================================================================================================
// Repository that offers a wide collection of ASP.NET Core
// Health Check packages for widely used services and platforms:
// https://github.com/Xabaril/AspNetCore.Diagnostics.HealthChecks
// =================================================================================================
// HealthCheck is a mechanism for monitoring the application's health and its dependencies.  
// It is responsible for checking the status of services, databases, external APIs, and other components.  
// Used for automatic issue detection, integration with orchestrators  
// (Kubernetes, Docker) for automatic restarts, as well as load balancers.  
// Example use case: monitoring database or Redis cache availability,  
// checking access to external APIs, and redirecting traffic only to healthy instances.  
// For example, if a database connection fails or an API becomes unresponsive,  
// the health check can detect the failure and report it.  
// Orchestrators like Kubernetes can then restart the failing container,  
// while load balancers can redirect traffic from the failing instance to a healthy instance,  
// ensuring minimal downtime and high availability of the application. 
//
// app.UseHealthChecks("/health"); - This is the minimum setting that creates the /health endpoint,
//                                   which returns a 200 OK status if the program is running.
//
app.UseHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.Run();
