using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XRPL.NET.Core.Configuration;
using XRPL.NET.Core.Interfaces;
using XRPL.NET.Core.Utils;

namespace XRPL.NET.Client;

/// <summary>
/// Primary client for interacting with the XRP Ledger.
/// </summary>
public class XrplClient : IXrplClient, IDisposable
{
    private readonly ILogger<XrplClient> _logger;
    private readonly XrplClientOptions _options;

    /// <summary>
    /// Initializes a new instance of the XrplClient.
    /// </summary>
    /// <param name="options">Configuration options for the XRP Ledger client.</param>
    /// <param name="logger">Logger for capturing client operations.</param>
    public XrplClient(
        XrplClientOptions? options = null, 
        ILogger<XrplClient>? logger = null)
    {
        _options = options ?? XrplClientOptions.CreateDefault();
        _logger = logger ?? new LoggerFactory().CreateLogger<XrplClient>();

        // Initialize client components
        Ledger = new LedgerClient(_options, _logger);
        Transactions = new TransactionClient(_options, _logger);
        Subscriptions = new SubscriptionClient(_options, _logger);
    }

    /// <inheritdoc/>
    public ILedgerClient Ledger { get; }

    /// <inheritdoc/>
    public ITransactionClient Transactions { get; }

    /// <inheritdoc/>
    public ISubscriptionClient Subscriptions { get; }

    /// <inheritdoc/>
    public XrplClientOptions Options => _options;

    /// <inheritdoc/>
    public bool IsConnected { get; private set; }

    /// <inheritdoc/>
    public Uri? CurrentServerUri { get; private set; }

    /// <inheritdoc/>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        if (IsConnected)
        {
            _logger.LogInformation("Client is already connected.");
            return;
        }

        try 
        {
            // TODO: Implement connection logic
            CurrentServerUri = _options.ServerUris.Count > 0 
                ? _options.ServerUris[0] 
                : throw new InvalidOperationException("No servers configured");

            IsConnected = true;
            _logger.LogInformation($"Connected to {CurrentServerUri}");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Connection failed");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            _logger.LogInformation("Client is not connected.");
            return;
        }

        try 
        {
            // TODO: Implement disconnection logic
            IsConnected = false;
            CurrentServerUri = null;
            _logger.LogInformation("Disconnected from server");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Disconnection failed");
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task ReconnectAsync(bool forceSwitch = false, CancellationToken cancellationToken = default)
    {
        await DisconnectAsync(cancellationToken);
        await ConnectAsync(cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<ServerInfoResponse> GetServerInfoAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement server info retrieval logic
        throw new NotImplementedException("Server info retrieval not yet implemented");
    }

    /// <inheritdoc/>
    public async Task<FeeResponse> GetFeeAsync(CancellationToken cancellationToken = default)
    {
        // TODO: Implement fee retrieval logic
        throw new NotImplementedException("Fee retrieval not yet implemented");
    }

    /// <inheritdoc/>
    public async Task<TResponse> SendCommandAsync<TResponse>(object command, CancellationToken cancellationToken = default)
    {
        // TODO: Implement command sending logic
        throw new NotImplementedException("Raw command sending not yet implemented");
    }

    /// <summary>
    /// Disposes the client and releases resources.
    /// </summary>
    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }
}