using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using Azure.AI.OpenAI;
using Microsoft.Extensions.Azure;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using System;

[assembly: FunctionsStartup(typeof(AoaiStreamFunction.Startup))]

namespace AoaiStreamFunction
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        { 
            builder.Services.AddOptions<OpenAIOptions>().Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection("OpenAIOptions").Bind(options);
            });

            builder.Services.AddAzureClients(clientBuilder =>
            {
                clientBuilder.AddOpenAIClient(new Uri(Environment.GetEnvironmentVariable("OpenAIEndpoint")));
                // Use Managed ID
                clientBuilder.UseCredential(new DefaultAzureCredential());
            });
        }
    }
}