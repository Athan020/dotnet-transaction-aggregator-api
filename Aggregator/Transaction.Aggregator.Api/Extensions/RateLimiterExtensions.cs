using System;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Transaction.Aggregator.Api.Extensions;

public static class RateLimiterExtensions
{

    public static TBuilder AddCustomRateLimiting<TBuilder>(this TBuilder builder)
        where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.OnRejected = async (context, cancellationToken) =>
            {

                if (context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter))
                {
                    context.HttpContext.Response.Headers.RetryAfter = ((int)retryAfter.TotalSeconds).ToString();

                    var problemDetailsFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

                    var problemDetails = problemDetailsFactory.CreateProblemDetails(
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

        return builder;
    }
}
