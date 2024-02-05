using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Serilog;

namespace Common.Books.Extensions;

public static class ConfigurationExtensions
{
    public static IConfigurationRoot AddKeyVault(this IConfiguration configuration)
    {
        var configBuilder = new ConfigurationBuilder().AddConfiguration(configuration);
        var keyVaultEndpoint = configuration["AzureKeyVaultEndpoint"];
        if (string.IsNullOrEmpty(keyVaultEndpoint))
            throw new InvalidOperationException("AzureKeyVaultEndpoint is not set in configuration");

        var usingDefaultCredential = true;
        try
        {
            configBuilder.AddAzureKeyVault(new Uri(keyVaultEndpoint), new DefaultAzureCredential());
        }
        catch (Exception e)
        {
            usingDefaultCredential = false;
            Log.Information(e, "Failed to add Azure Key Vault configuration");
        }

        if (!usingDefaultCredential)
        {
            var tenantId = Environment.GetEnvironmentVariable("AZURE_TENANT_ID");
            var clientId = Environment.GetEnvironmentVariable("AZURE_CLIENT_ID");
            var clientSecret = Environment.GetEnvironmentVariable("AZURE_CLIENT_SECRET");
            configBuilder.AddAzureKeyVault(new Uri(keyVaultEndpoint),
                new ClientSecretCredential(tenantId, clientId, clientSecret));
        }

        var config = configBuilder.Build();
        return config;
    }
}