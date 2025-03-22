using Discount.Grpc.Data;
using Discount.Grpc.Services;
using Mapster;
using Microsoft.EntityFrameworkCore;

//Mapster, by default, assumes that property names follow PascalCase conventions. 
// This can cause issues when using lower case property names, as it may not recognize the properties correctly for mapping. 
// Here's how you can configure Mapster to work with lowercase property names:
// Configuration in Program.cs
//TypeAdapterConfig.GlobalSettings.Default.NameMatchingStrategy(NameMatchingStrategy.IgnoreCase);

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddDbContext<DiscountContext>(opts =>
    opts.UseSqlite(builder.Configuration.GetConnectionString("Database")));

var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseMigration();
app.MapGrpcService<DiscountService>();
app.MapGet("/", () => "Communication with gRPC endpoints must be made through a gRPC client.");

app.Run();
