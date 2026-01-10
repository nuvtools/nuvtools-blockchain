namespace NuvTools.Blockchain.Azure;

/// <summary>
/// Configuration options for Azure Confidential Ledger.
/// </summary>
public class BlockchainSection
{
    /// <summary>
    /// The Azure Confidential Ledger endpoint URL.
    /// </summary>
    public required string LedgerEndpoint { get; set; }
}
