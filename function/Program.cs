using System;
using AoaiStreamFunction;
using Azure.Identity;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Azure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((hostContext,services) => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        services.AddOptions<OpenAIOptions>().Configure<IConfiguration>((options, configuration) =>
        {
            configuration.GetSection("OpenAIOptions").Bind(options);
        });

        services.AddAzureClients(clientBuilder =>
        {
            clientBuilder.AddOpenAIClient(new Uri(hostContext.Configuration.GetValue<string>("OpenAIEndpoint")));
            clientBuilder.UseCredential(new AzureCliCredential());
        });

    })
    .Build();

host.Run();