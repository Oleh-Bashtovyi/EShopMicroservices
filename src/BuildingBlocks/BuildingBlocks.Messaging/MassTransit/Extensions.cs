using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace BuildingBlocks.Messaging.MassTransit;

public static class Extensions
{
    public static IServiceCollection AddMessageBroker
    (this IServiceCollection services, IConfiguration configuration, Assembly? assembly = null)
    {
        services.AddMassTransit(config =>
        {
            config.SetKebabCaseEndpointNameFormatter();

            if (assembly != null)
                config.AddConsumers(assembly);

            config.UsingRabbitMq((context, configurator) =>
            {
                var host = configuration["MessageBroker:Host"]!;
                var username = configuration["MessageBroker:UserName"] ?? "guest";
                var password = configuration["MessageBroker:Password"] ?? "guest";

                Console.WriteLine($"Connecting to RabbitMQ at {host} with username {username}");

                configurator.Host(new Uri(host), h =>
                {
                    h.Username(username);
                    h.Password(password);
                });
                
                configurator.ConfigureEndpoints(context);
            });
        });

        return services;
    }
}