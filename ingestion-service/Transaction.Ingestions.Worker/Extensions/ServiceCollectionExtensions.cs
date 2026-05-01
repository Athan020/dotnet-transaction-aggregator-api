using System;
using Grpc.Net.Client;
using Grpc.Core;
using Categorization;
using static Categorization.Categorization;

namespace Transaction.Ingestions.Worker.Extensions;

public static class ServiceCollectionExtensions
{

    public static IServiceCollection ConfigureGrpcClient(this IServiceCollection services, IConfiguration configuration)
    {
        var categorizationServiceUrl = configuration.GetValue<string>("CategorizationService:Url");

        ArgumentNullException.ThrowIfNull(categorizationServiceUrl, "Categorization service URL must be provided in configuration.");
    
        var builder = services.AddGrpcClient<CategorizationClient>(options =>
        {
            options.Address = new Uri(categorizationServiceUrl);

        });

        builder.ConfigureChannel(channel =>
        {
            channel.ServiceConfig = new Grpc.Net.Client.Configuration.ServiceConfig
            {
                MethodConfigs =
                {
                    new Grpc.Net.Client.Configuration.MethodConfig
                    {
                        Names = { Grpc.Net.Client.Configuration.MethodName.Default },
                        RetryPolicy = new Grpc.Net.Client.Configuration.RetryPolicy
                        {
                            MaxAttempts = 5,
                            InitialBackoff = TimeSpan.FromSeconds(1),
                            MaxBackoff = TimeSpan.FromSeconds(5),
                            BackoffMultiplier = 2,
                            RetryableStatusCodes = { StatusCode.Unavailable }
                        }
                    }
                }
            };
            
        });
        
        return services;
    }


}
