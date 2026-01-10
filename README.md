# NuvTools.Blockchain

Core abstractions for blockchain ledger operations with support for multiple cloud providers.

## Installation

```bash
# Core abstractions
dotnet add package NuvTools.Blockchain

# Azure Confidential Ledger implementation
dotnet add package NuvTools.Blockchain.Azure
```

## Usage

### Configuration

Add the following to your `appsettings.json`:

```json
{
  "NuvTools.Blockchain": {
    "LedgerEndpoint": "https://your-ledger-name.confidential-ledger.azure.com"
  }
}
```

### Register Services

```csharp
using NuvTools.Blockchain.Azure;

// Uses default section "NuvTools.Blockchain"
builder.Services.AddBlockchain(builder.Configuration);

// Or specify a custom section name
builder.Services.AddBlockchain(builder.Configuration, "CustomSectionName");
```

### Inject and Use

```csharp
using NuvTools.Blockchain;

public class MyService(ILedgerService ledgerService)
{
    public async Task WriteToLedgerAsync(MyRecord record)
    {
        // Write an entry to the ledger
        var transactionId = await ledgerService.WriteAsync(record);

        // Read back the entry
        var entry = await ledgerService.ReadAsync<MyRecord>(transactionId);

        Console.WriteLine($"Committed at: {entry?.CommitTimeUtc}");
    }
}
```

## Features

- **Write entries** to blockchain ledger with automatic serialization
- **Read entries** with built-in retry mechanism for eventual consistency
- **Generic type support** for any serializable content
- **Collection/sub-ledger support** for organizing entries

## Supported Providers

- **Azure Confidential Ledger** - `NuvTools.Blockchain.Azure`

## License

MIT License - see [LICENSE](LICENSE) for details.
