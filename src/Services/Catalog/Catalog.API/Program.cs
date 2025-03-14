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


var app = builder.Build();

// Configure the HTTP pipeline
// ==================================================================================

app.MapCarter();

// The bes practice to handle global
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



app.Run();
