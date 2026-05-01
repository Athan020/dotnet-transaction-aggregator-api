using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Scalar.AspNetCore;
using Shared.Core.Observability;
using Shared.Entities;
using Transaction.Aggregator.Api.Extensions;
using Transaction.Aggregator.Api.Middleware;
using Transaction.Aggregator.Application;
using Transaction.Aggregator.Application.Configuration;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;
using Transaction.Aggregator.Infrastructure;
using Transaction.Aggregator.Infrastructure.Sources;

var builder = WebApplication.CreateBuilder(args);

{
    builder.Configuration.AddEnvironmentVariables();
    builder.Services
        .Configure<CategorizationRuleSet>(builder.Configuration.GetSection("CategorizationRules"));
    builder.Services.Configure<Settings>(builder.Configuration.GetSection("Toggles"));

    builder.AddDistributedCache();

    // Add services to the container.
    builder.Services.AddScoped<ITransactionSource, DatabaseTransactionsSource>();
    builder.Services.AddScoped<ITransactionManager, TransactionManager>();

    builder.AddObservability(builder.Configuration);

    // builder.Services.Decorate<ITransactionAggregator,CategorizationAggregator>();
    builder.Services.AddDbContext<TransactionsContext>(options =>
    {
        options.UseNpgsql(builder.Configuration.GetConnectionString("Postgres"), providerOptions =>
        {
            providerOptions.EnableRetryOnFailure();
            providerOptions.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
        } );

        options.EnableSensitiveDataLogging(builder.Environment.IsDevelopment());
        options.EnableDetailedErrors(builder.Environment.IsDevelopment());
        options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);

    });

    builder.Services.AddControllers()
        .AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.WhenWritingNull;;
        });
    
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.AddCustomRateLimiting();
    builder.AddCustomHealthChecks();


    builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

    builder.Services.AddProblemDetails();
}

var app = builder.Build();
{
    // Configure the HTTP request pipeline.
    if (app.Environment.IsDevelopment())
    {
        app.MapOpenApi();

        app.MapScalarApiReference(options =>
        {
            options.Title = "Transaction Aggregator API";
            options.ShowSidebar = true;
            options.Theme = ScalarTheme.Moon;

        });
    }
    
    app.UseMiddleware<CorrelationMiddleware>();
    app.UseRateLimiter();
    app.UseExceptionHandler();

    app.MapControllers();

    app.MapPrometheusScrapingEndpoint();

    app.MapHealthChecks("/health", new HealthCheckOptions
    {
        Predicate = _ => true,
    });
}

await app.RunAsync();
