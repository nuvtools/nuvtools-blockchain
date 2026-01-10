namespace NuvTools.Blockchain;

/// <summary>
/// Defines the contract for blockchain ledger operations.
/// </summary>
public interface ILedgerService
{
    /// <summary>
    /// Writes an entry to the ledger.
    /// </summary>
    /// <typeparam name="T">The type of the entry content.</typeparam>
    /// <param name="entry">The entry to write.</param>
    /// <param name="collectionId">Optional collection/sub-ledger identifier.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The transaction ID of the written entry.</returns>
    Task<string> WriteAsync<T>(T entry, string? collectionId = null, CancellationToken cancellationToken = default)
        where T : class;

    /// <summary>
    /// Reads an entry from the ledger.
    /// </summary>
    /// <typeparam name="T">The type of the entry content.</typeparam>
    /// <param name="transactionId">The transaction ID to read.</param>
    /// <param name="collectionId">Optional collection/sub-ledger identifier.</param>
    /// <param name="maxRetries">Maximum retry attempts for pending entries.</param>
    /// <param name="delayMilliseconds">Base delay between retries in milliseconds.</param>
    /// <param name="cancellationToken">Cancellation token.</param>
    /// <returns>The ledger entry if found; otherwise, null.</returns>
    Task<LedgerEntry<T>?> ReadAsync<T>(
        string transactionId,
        string? collectionId = null,
        int maxRetries = 10,
        int delayMilliseconds = 500,
        CancellationToken cancellationToken = default)
        where T : class;
}
