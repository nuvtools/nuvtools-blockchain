using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace NuvTools.Blockchain.Azure;

/// <summary>
/// Extension methods for registering blockchain services.
/// </summary>
public static class BlockchainExtensions
{
    /// <summary>
    /// Default configuration section name.
    /// </summary>
    public const string DefaultSectionName = "NuvTools.Blockchain";

    /// <summary>
    /// Adds Azure Confidential Ledger blockchain services to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="configuration">The configuration.</param>
    /// <param name="sectionName">The configuration section name. Defaults to "NuvTools.Blockchain".</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddBlockchain(this IServiceCollection services, IConfiguration configuration, string sectionName = DefaultSectionName)
    {
        services.Configure<BlockchainSection>(configuration.GetSection(sectionName));
        services.AddScoped<ILedgerService, AzureConfidentialLedgerService>();

        return services;
    }
}
