using Microsoft.EntityFrameworkCore;
using Shared.Entities;
using Transaction.Categorization.Engine.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddGrpc(options =>
{
    options.EnableDetailedErrors = builder.Environment.IsDevelopment();
    options.MaxReceiveMessageSize = 5 * 1024 * 1024; // 5 MB
});

builder.Services.AddDbContext<TransactionsContext>(options =>
{
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"), providerOptions =>
    {
        providerOptions.EnableRetryOnFailure();
        providerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
    });

    options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
    options.EnableDetailedErrors(builder.Environment.IsDevelopment());
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

});

builder.Services.AddGrpcReflection();

var app = builder.Build();

// Configure the HTTP request pipeline.
app.MapGrpcService<CategorizationService>();

if (app.Environment.IsDevelopment())
{
    app.MapGrpcReflectionService();
}

app.Run();
