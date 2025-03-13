using Microsoft.CodeAnalysis;
using Weasel.Core;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
// ==================================================================================

// Endpoints were created using Carter library.
// Currently if we install Carter to BuildingBlocks project,
// it will not Map any classes that implements ICarterModule interface.
// That`s because it will scan only BuildingBlocks project for those classes.
// Unfortunately Marten library currently does not have ability
// To config assembly scan, as MediatR do in next line.
// So only solution is to install Carter in this project, and not in BuildingBlocks.
builder.Services.AddCarter();
builder.Services.AddMediatR(config =>
{
    config.RegisterServicesFromAssembly(typeof(Program).Assembly);
});

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

app.Run();
