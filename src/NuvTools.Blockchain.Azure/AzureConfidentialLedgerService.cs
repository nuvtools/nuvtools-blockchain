using Azure;
using Azure.Core;
using Azure.Identity;
using Azure.Security.ConfidentialLedger;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace NuvTools.Blockchain.Azure;

/// <summary>
/// Azure Confidential Ledger implementation of <see cref="ILedgerService"/>.
/// </summary>
public class AzureConfidentialLedgerService(IOptions<BlockchainSection> options) : ILedgerService
{
    private readonly Lazy<ConfidentialLedgerClient> _client = new(() =>
    {
        var settings = options.Value;
        var ledgerUri = new Uri(settings.LedgerEndpoint);
        return new ConfidentialLedgerClient(ledgerUri, new DefaultAzureCredential());
    });

    private ConfidentialLedgerClient Client => _client.Value;

    /// <inheritdoc/>
    public async Task<string> WriteAsync<T>(T entry, string? collectionId = null, CancellationToken cancellationToken = default) where T : class
    {
        ArgumentNullException.ThrowIfNull(entry);
        cancellationToken.ThrowIfCancellationRequested();

        var ledgerEntryBase = new LedgerEntryBase<T> { Content = entry, CommitTimeUtc = DateTime.UtcNow };
        var entryJson = JsonSerializer.Serialize(ledgerEntryBase);

        var payload = new { contents = entryJson };

        var postOperation = await Client.PostLedgerEntryAsync(
            WaitUntil.Completed,
            RequestContent.Create(payload), collectionId
        );

        return postOperation.Id;
    }

    /// <inheritdoc/>
    public async Task<LedgerEntry<T>?> ReadAsync<T>(
        string transactionId,
        string? collectionId = null,
        int maxRetries = 10,
        int delayMilliseconds = 500,
        CancellationToken cancellationToken = default) where T : class
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(transactionId);

        for (var attempt = 1; attempt <= maxRetries; attempt++)
        {
            cancellationToken.ThrowIfCancellationRequested();

            var response = await Client.GetLedgerEntryAsync(transactionId, collectionId);

            using var json = JsonDocument.Parse(response.Content);
            var entry = json.RootElement;

            if (entry.TryGetProperty("state", out var state) && state.GetString() == "Loading")
            {
                if (attempt == maxRetries)
                    throw new TimeoutException($"Ledger entry {transactionId} still in 'Loading' state after {maxRetries} retries.");

                await Task.Delay(delayMilliseconds * attempt, cancellationToken);
                continue;
            }

            entry = entry.GetProperty("entry");

            var txId = entry.GetProperty("transactionId").GetString()!;

            entry.TryGetProperty("collectionId", out var c);
            var entryCollectionId = c.ValueKind == JsonValueKind.String ? c.GetString() : null;

            var contentJson = entry.GetProperty("contents").GetString()!;
            var ledgerEntryBase = JsonSerializer.Deserialize<LedgerEntryBase<T>>(contentJson);

            return ledgerEntryBase is null
                ? null
                : new LedgerEntry<T>
                {
                    TransactionId = txId,
                    CollectionId = entryCollectionId,
                    Content = ledgerEntryBase.Content,
                    CommitTimeUtc = ledgerEntryBase.CommitTimeUtc,
                };
        }

        // This should never be reached, but required for compiler
        throw new InvalidOperationException($"Ledger entry {transactionId} retrieval failed unexpectedly.");
    }
}
