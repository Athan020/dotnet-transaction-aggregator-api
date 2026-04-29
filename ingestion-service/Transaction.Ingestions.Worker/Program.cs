using Microsoft.EntityFrameworkCore;
using Shared.Core.Observability;
using Shared.Entities;
using Transaction.Ingestions.Worker.Extensions;
using Transaction.Ingestions.Worker.SourceAdapters;
using Transaction.Ingestions.Worker.Workers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.AddObservability(builder.Configuration);

builder.Services.ConfigureGrpcClient(builder.Configuration);

builder.Services.AddScoped<ISourceAdapter, MockSourceAdapter>();

builder.Services.AddHostedService<CategorizationWorker>();
builder.Services.AddHostedService<IngestionWorker>();

builder.Services.AddDbContext<TransactionsContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres")));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// app.MapHealthChecks("/health");

app.Run();