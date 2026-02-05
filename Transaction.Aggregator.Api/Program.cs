using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.RateLimiting;
using Scalar.AspNetCore;
using Transaction.Aggregator.Api.Middleware;
using Transaction.Aggregator.Application;
using Transaction.Aggregator.Application.Configuration;
using Transaction.Aggregator.Application.Contracts;
using Transaction.Aggregator.Domain.Models;
using Transaction.Aggregator.Infrastructure;
using Transaction.Aggregator.Infrastructure.RuleEngine;

var builder = WebApplication.CreateBuilder(args);

{
    builder.Services
        .Configure<CategorizationRuleSet>(builder.Configuration.GetSection("CategorizationRules"));
    builder.Services.Configure<Settings>(builder.Configuration.GetSection("Toggles"));

    // Add services to the container.
    builder.Services.AddSingleton<IResiliencePipelineFactory, ResiliencePipelineFactory>();
    builder.Services.AddScoped<ITransactionManager, TransactionManager>();
    builder.Services.AddScoped<ITransactionSource, RewardTransactionSource>();
    builder.Services.AddScoped<ITransactionSource, PrepaidTransactionSource>();
    builder.Services.AddScoped<ITransactionSource, CardTransactionSource>();
    builder.Services.AddScoped<ICategorizerEngine, CustomRuleCategorizer>();

    builder.Services.AddScoped<TransactionAggregator>();


    builder.Services.AddScoped<ITransactionAggregator>(sp =>
    {
        var transactionAggregator = sp.GetRequiredService<TransactionAggregator>();
        var logger = sp.GetRequiredService<ILogger<CategorizationAggregator>>();
        var categorizerEngine = sp.GetRequiredService<ICategorizerEngine>();
        return new CategorizationAggregator(transactionAggregator, categorizerEngine, logger);
    });

    builder.Services.AddControllers();
    // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
    builder.Services.AddOpenApi();

    builder.Services.AddRateLimiter(options =>
    {
        options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

        options.OnRejected = async (context, cancellationToken) =>
        {

            if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
            {
                context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                var problemdetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                var problemDetails = problemdetailsFactory.CreateProblemDetails(
                    context.HttpContext,
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Too Many Requests",
                    detail: $"You have exceeded the allowed request limit. Please try again in {retryAfter.TotalSeconds:N0} seconds."
                );

                await context.HttpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);
            }
        };

        options.AddPolicy("Fixed", context =>
            RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: context.Connection.RemoteIpAddress?.ToString(), factory: _ =>
                new FixedWindowRateLimiterOptions
                {
                    PermitLimit = 50,
                    Window = TimeSpan.FromMinutes(1),
                }));
    });

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

    app.MapControllers();
}

await app.RunAsync();
