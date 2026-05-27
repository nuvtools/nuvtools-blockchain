# NuvTools.Blockchain

A suite of .NET libraries for abstracting tamper-evident, append-only blockchain ledger operations, with comprehensive support for Microsoft Azure Confidential Ledger.

[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![.NET](https://img.shields.io/badge/.NET-8%20%7C%209%20%7C%2010-512BD4)](https://dotnet.microsoft.com/)

## Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Installation](#installation)
- [Quick Start](#quick-start)
- [Usage Examples](#usage-examples)
- [Configuration](#configuration)
- [Target Frameworks](#target-frameworks)
- [Building from Source](#building-from-source)
- [Documentation](#documentation)
- [License](#license)

## Overview

NuvTools.Blockchain provides a clean, intuitive abstraction layer for writing and reading typed records to a blockchain ledger. The library follows a provider pattern, allowing you to implement custom ledger backends while maintaining a consistent API.

### Libraries

#### NuvTools.Blockchain (Core)

The core library defines the foundational abstractions:

- **`ILedgerService`** - Interface for writing entries to and reading entries from a ledger
- **`LedgerEntry<T>`** - Typed entry returned from the ledger, including `TransactionId` and optional `CollectionId`
- **`LedgerEntryBase<T>`** - Base wrapper carrying the original `Content` and `CommitTimeUtc`

#### NuvTools.Blockchain.Azure

Azure Confidential Ledger implementation:

- **`AzureConfidentialLedgerService`** - Full `ILedgerService` implementation backed by `Azure.Security.ConfidentialLedger`
- **`BlockchainSection`** - Strongly-typed configuration POCO bound to the `NuvTools.Blockchain` configuration section
- **`BlockchainExtensions.AddBlockchain()`** - Dependency injection registration extension
- Authentication via `DefaultAzureCredential` (managed identity, environment, Visual Studio, Azure CLI, etc.)
- Built-in retry with linear backoff while entries are in the `Loading` state (eventual consistency)
- Automatic JSON serialization of payloads
- Lazy client initialization

## Features

✅ **Provider Pattern** - Easy to implement custom ledger backends
✅ **Async/Await** - Fully asynchronous API with `CancellationToken` support
✅ **Generic Type Support** - Write and read any serializable type
✅ **Azure Integration** - Production-ready Azure Confidential Ledger implementation
✅ **Collection / Sub-Ledger Support** - Organize entries by `collectionId`
✅ **Eventual-Consistency Retry** - Configurable retry while the ledger commits the entry
✅ **Automatic Serialization** - Records are serialized to JSON transparently
✅ **Comprehensive Documentation** - Full XML documentation for IntelliSense
✅ **Multi-Targeting** - Supports .NET 8, 9, and 10

## Installation

Install via NuGet Package Manager:

```bash
# Core library
dotnet add package NuvTools.Blockchain

# Azure Confidential Ledger implementation
dotnet add package NuvTools.Blockchain.Azure
```

Or via Package Manager Console:

```powershell
Install-Package NuvTools.Blockchain
Install-Package NuvTools.Blockchain.Azure
```

## Quick Start

### Azure Confidential Ledger Example

**1. Configure `appsettings.json`:**

```json
{
  "NuvTools.Blockchain": {
    "LedgerEndpoint": "https://your-ledger-name.confidential-ledger.azure.com"
  }
}
```

**2. Register services in `Program.cs`:**

```csharp
using NuvTools.Blockchain.Azure;

builder.Services.AddBlockchain(builder.Configuration);
```

**3. Inject `ILedgerService` and use it:**

```csharp
using NuvTools.Blockchain;

public class AuditService(ILedgerService ledgerService)
{
    public async Task RecordAsync(AuditRecord record, CancellationToken ct)
    {
        var transactionId = await ledgerService.WriteAsync(record, cancellationToken: ct);

        var entry = await ledgerService.ReadAsync<AuditRecord>(transactionId, cancellationToken: ct);

        Console.WriteLine($"Committed at: {entry?.CommitTimeUtc}");
    }
}
```

## Usage Examples

### Writing an Entry

```csharp
public record AuditRecord(string UserId, string Action, DateTime Timestamp);

var record = new AuditRecord("user-42", "DocumentSigned", DateTime.UtcNow);

// Write to the default ledger
var transactionId = await ledgerService.WriteAsync(record);
```

### Reading an Entry

```csharp
// Read with default retry settings (10 attempts, 500ms base delay)
var entry = await ledgerService.ReadAsync<AuditRecord>(transactionId);

if (entry is not null)
{
    Console.WriteLine($"Transaction:  {entry.TransactionId}");
    Console.WriteLine($"Committed at: {entry.CommitTimeUtc}");
    Console.WriteLine($"User:         {entry.Content.UserId}");
    Console.WriteLine($"Action:       {entry.Content.Action}");
}
```

### Working with Collections (Sub-Ledgers)

```csharp
const string CollectionId = "audit-2026";

// Write to a specific collection
var transactionId = await ledgerService.WriteAsync(record, CollectionId);

// Read from the same collection
var entry = await ledgerService.ReadAsync<AuditRecord>(
    transactionId,
    collectionId: CollectionId);
```

### Tuning Retry Behavior

Entries can briefly be in a `Loading` state while the ledger commits and seals them. The reader retries with linear backoff (`delayMilliseconds * attempt`) until the entry is available or `maxRetries` is exceeded.

```csharp
var entry = await ledgerService.ReadAsync<AuditRecord>(
    transactionId,
    collectionId: null,
    maxRetries: 20,
    delayMilliseconds: 250,
    cancellationToken: ct);
```

If the ledger does not finish loading within the retry budget, a `TimeoutException` is thrown.

### Custom Configuration Section

```csharp
// Bind to a non-default section name
builder.Services.AddBlockchain(builder.Configuration, "MyApp:Ledger");
```

```json
{
  "MyApp": {
    "Ledger": {
      "LedgerEndpoint": "https://my-ledger.confidential-ledger.azure.com"
    }
  }
}
```

## Configuration

| Setting           | Type     | Required | Description                                              |
|-------------------|----------|----------|----------------------------------------------------------|
| `LedgerEndpoint`  | `string` | Yes      | The Azure Confidential Ledger endpoint URL.              |

### Authentication

`AzureConfidentialLedgerService` uses `DefaultAzureCredential`, which transparently tries the following credential sources in order:

1. Environment variables
2. Workload identity (Kubernetes)
3. Managed identity
4. Visual Studio / Visual Studio Code
5. Azure CLI / Azure PowerShell / Azure Developer CLI
6. Interactive browser (where enabled)

Ensure the identity running the application has the appropriate role on the ledger resource (typically **Confidential Ledger Contributor**).

## Target Frameworks

- .NET 8.0
- .NET 9.0
- .NET 10.0

All libraries enable nullable reference types and implicit usings.

## Building from Source

This solution uses the modern `.slnx` (XML-based) solution format.

### Prerequisites

- .NET 10 SDK or later
- Visual Studio 2022 17.0+ or Visual Studio Code with C# extension

### Build Commands

```bash
# Clone the repository
git clone https://github.com/nuvtools/nuvtools-blockchain.git
cd nuvtools-blockchain

# Build the solution
dotnet build NuvTools.Blockchain.slnx

# Run tests
dotnet test NuvTools.Blockchain.slnx

# Create NuGet packages
dotnet pack NuvTools.Blockchain.slnx -c Release
```

The solution structure:
```
nuvtools-blockchain/
├── src/
│   ├── NuvTools.Blockchain/             # Core abstractions
│   └── NuvTools.Blockchain.Azure/       # Azure Confidential Ledger implementation
├── tests/
│   └── NuvTools.Blockchain.Azure.Test/  # NUnit tests
└── NuvTools.Blockchain.slnx             # Solution file
```

## Documentation

All public APIs include comprehensive XML documentation comments. IntelliSense in Visual Studio, Visual Studio Code, and Rider will display:

- Detailed descriptions of all types, methods, and properties
- Parameter information with expected values
- Return value descriptions
- Exception documentation
- Usage examples and best practices

XML documentation files are included in the NuGet packages for seamless integration.

## License

Licensed under the [MIT License](LICENSE).

Copyright © 2026 Nuv Tools
