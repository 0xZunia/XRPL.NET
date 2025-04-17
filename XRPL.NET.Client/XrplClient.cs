using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using XRPL.NET.Core.Configuration;
using XRPL.NET.Core.Exceptions;
using XRPL.NET.Core.Interfaces;

namespace XRPL.NET.Client;

/// <summary>
/// Primary client for interacting with the XRP Ledger.
/// </summary>
public class XrplClient : IXrplClient
{
    private readonly ILogger<XrplClient> _logger;
    private XrplClientOptions _options;

    /// <summary>
    /// Initializes a new instance of the XrplClient.
    /// </summary>
    /// <param name="options">Configuration options for the XRP Ledger client.</param>
    /// <param name="logger">Logger for capturing client operations.</param>
    public XrplClient(
        XrplClientOptions? options = null, 
        ILoggerFactory? loggerFactory = null)
    {
        _options = options ?? XrplClientOptions.CreateDefault();
    
        // Create logger factory if not provided
        loggerFactory ??= new LoggerFactory();
        _logger = loggerFactory.CreateLogger<XrplClient>();

        // Initialize client components with their own loggers
        Ledger = new LedgerClient(_options, loggerFactory.CreateLogger<LedgerClient>());
        Transactions = new TransactionClient(_options, loggerFactory.CreateLogger<TransactionClient>());
        Subscriptions = new SubscriptionClient(_options, loggerFactory.CreateLogger<SubscriptionClient>());
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

        if (_options.ServerUris.Count == 0)
        {
            throw new InvalidOperationException("No servers configured");
        }

        _logger.LogInformation("Connecting to XRP Ledger...");
        
        var exceptions = new List<Exception>();
        
        // Try each server in the list until one works
        foreach (var serverUri in _options.ServerUris)
        {
            try
            {
                // Create an HTTP client for testing connectivity
                using var httpClient = new HttpClient
                {
                    Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
                };
                
                // Determine the appropriate endpoint
                string endpoint = serverUri.ToString();
                if (!endpoint.EndsWith('/'))
                {
                    endpoint += "/";
                }
                
                // For WebSocket servers, use HTTP endpoint for testing
                string httpEndpoint = endpoint;
                if (httpEndpoint.StartsWith("ws"))
                {
                    httpEndpoint = httpEndpoint.Replace("ws", "http");
                }
                
                // Create a simple server_info request to test connectivity
                var command = new
                {
                    method = "server_info",
                    @params = new object[] { }
                };
                
                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(command, jsonOptions),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await httpClient.PostAsync(httpEndpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                using var doc = JsonDocument.Parse(jsonResponse);
                var root = doc.RootElement;

                if (root.TryGetProperty("result", out var resultElement) &&
                    resultElement.TryGetProperty("info", out var infoElement))
                {
                    // Server responded successfully, set as current
                    CurrentServerUri = serverUri;
                    IsConnected = true;
                    _logger.LogInformation($"Connected to {serverUri}");
                    return;
                }
                
                _logger.LogWarning($"Server {serverUri} returned invalid response");
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to connect to server {serverUri}");
                exceptions.Add(new ConnectionException($"Failed to connect to {serverUri}", serverUri, ex));
            }
        }
        
        // If we've tried all servers and none worked, throw an exception
        if (exceptions.Count == 1)
        {
            throw exceptions[0];
        }
        
        throw new AggregateException("Failed to connect to any server", exceptions);
    }

    /// <inheritdoc/>
    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            _logger.LogInformation("Client is not connected.");
            return;
        }

        _logger.LogInformation($"Disconnecting from {CurrentServerUri}...");
    
        // For HTTP-based connections, there's no actual connection to close
        // For WebSocket connections, we would need to close the socket
        if (_options.UseWebSocket && CurrentServerUri != null && CurrentServerUri.Scheme.StartsWith("ws"))
        {
            // In a real implementation, close the WebSocket connection here
            // For now, just log and reset connection status
            _logger.LogInformation("Closing WebSocket connection");
        }
    
        // Reset connection state
        IsConnected = false;
        CurrentServerUri = null;
        _logger.LogInformation("Disconnected from server");
    }

    /// <inheritdoc/>
    public async Task ReconnectAsync(bool forceSwitch = false, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Reconnecting to XRP Ledger...");
        
        Uri? previousUri = CurrentServerUri;
        
        await DisconnectAsync(cancellationToken);
        
        if (forceSwitch && previousUri != null)
        {
            // If we're forcing a switch, reorder the server list to try other servers first
            var servers = _options.ServerUris.ToList();
            if (servers.Count > 1)
            {
                // Try to find the previous server's index
                int index = servers.FindIndex(u => u.ToString() == previousUri.ToString());
                
                if (index >= 0)
                {
                    // Move the previous server to the end of the list
                    var previous = servers[index];
                    servers.RemoveAt(index);
                    servers.Add(previous);
                    
                    // Create a new options object with the reordered servers
                    var newOptions = new XrplClientOptions()
                    {
                        TimeoutSeconds = _options.TimeoutSeconds,
                        MaxRetries = _options.MaxRetries,
                        AutoRetry = _options.AutoRetry,
                        Network = _options.Network,
                        UseSecureConnection = _options.UseSecureConnection,
                        VerifySslCertificate = _options.VerifySslCertificate,
                        UseWebSocket = _options.UseWebSocket,
                        AutoComputeFee = _options.AutoComputeFee,
                        FeeMultiplier = _options.FeeMultiplier,
                        ClientName = _options.ClientName,
                        ClientVersion = _options.ClientVersion
                    };
                    
                    foreach (var server in servers)
                    {
                        newOptions.AddServer(server);
                    }
                    
                    // Update the options
                    _options = newOptions;
                }
            }
        }
        
        // Try to connect using the potentially reordered server list
        await ConnectAsync(cancellationToken);
}

    /// <inheritdoc/>
public async Task<ServerInfoResponse> GetServerInfoAsync(CancellationToken cancellationToken = default)
{
    if (!IsConnected)
    {
        await ConnectAsync(cancellationToken);
    }
    
    _logger.LogInformation("Retrieving server information...");
    
    var command = new
    {
        method = "server_info",
        @params = new object[] { }
    };
    
    try
    {
        var response = await SendCommandAsync<JsonDocument>(command, cancellationToken);
        var root = response.RootElement;
        
        if (!root.TryGetProperty("result", out var resultElement))
        {
            throw new XrplException("Invalid response: 'result' property not found");
        }
        
        if (!resultElement.TryGetProperty("info", out var infoElement))
        {
            throw new XrplException("Invalid response: 'info' property not found");
        }
        
        var serverInfo = new ServerInfoResponse
        {
            BuildVersion = GetPropertyString(infoElement, "build_version", ""),
            CompleteLedgers = GetPropertyString(infoElement, "complete_ledgers", ""),
            HostId = GetPropertyString(infoElement, "hostid", null),
            LoadFactor = GetPropertyDecimal(infoElement, "load_factor", 1.0m),
            ValidatorPublicKey = GetPropertyString(infoElement, "pubkey_validator", null),
            IsValidator = infoElement.TryGetProperty("validation_quorum", out _)
        };
        
        // Parse uptime
        if (infoElement.TryGetProperty("uptime", out var uptimeElement) && uptimeElement.ValueKind == JsonValueKind.Number)
        {
            var seconds = uptimeElement.GetInt64();
            serverInfo.Uptime = TimeSpan.FromSeconds(seconds);
        }
        
        // Parse validated ledger
        if (infoElement.TryGetProperty("validated_ledger", out var validatedLedger) && 
            validatedLedger.ValueKind == JsonValueKind.Object &&
            validatedLedger.TryGetProperty("seq", out var seqElement))
        {
            serverInfo.ValidatedLedgerIndex = seqElement.GetUInt32();
        }
        
        // Parse closed ledger
        if (infoElement.TryGetProperty("closed_ledger", out var closedLedger) && 
            closedLedger.ValueKind == JsonValueKind.Object &&
            closedLedger.TryGetProperty("seq", out var closedSeqElement))
        {
            serverInfo.CurrentLedgerIndex = closedSeqElement.GetUInt32();
        }
        
        return serverInfo;
    }
    catch (Exception ex) when (!(ex is XrplException))
    {
        throw new XrplException($"Failed to retrieve server information: {ex.Message}", ex);
    }
}

    /// <inheritdoc/>
    public async Task<FeeResponse> GetFeeAsync(CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }
    
        _logger.LogInformation("Retrieving fee information...");
    
        var command = new
        {
            method = "fee",
            @params = new object[] { }
        };
    
        try
        {
            var response = await SendCommandAsync<JsonDocument>(command, cancellationToken);
            var root = response.RootElement;
        
            if (!root.TryGetProperty("result", out var resultElement))
            {
                throw new XrplException("Invalid response: 'result' property not found");
            }
        
            if (!resultElement.TryGetProperty("drops", out var dropsElement))
            {
                throw new XrplException("Invalid response: 'drops' property not found");
            }
        
            var feeResponse = new FeeResponse
            {
                BaseFee = GetPropertyUInt64(dropsElement, "base_fee", 10),
                MinimumFee = GetPropertyUInt64(dropsElement, "minimum_fee", 10),
                MedianFee = GetPropertyUInt64(dropsElement, "median_fee", 10000),
                OpenLedgerFee = GetPropertyUInt64(dropsElement, "open_ledger_fee", 10)
            };
        
            return feeResponse;
        }
        catch (Exception ex) when (!(ex is XrplException))
        {
            throw new XrplException($"Failed to retrieve fee information: {ex.Message}", ex);
        }
    }

    /// <inheritdoc/>
    public async Task<TResponse> SendCommandAsync<TResponse>(object command, CancellationToken cancellationToken = default)
    {
        if (!IsConnected)
        {
            await ConnectAsync(cancellationToken);
        }
        
        if (CurrentServerUri == null)
        {
            throw new ConnectionException("Not connected to any server");
        }
        
        // Determine the appropriate endpoint
        string endpoint = CurrentServerUri.ToString();
        if (!endpoint.EndsWith('/'))
        {
            endpoint += "/";
        }
        
        // For WebSocket servers, use HTTP endpoint for JSON-RPC
        if (endpoint.StartsWith("ws"))
        {
            endpoint = endpoint.Replace("ws", "http");
        }
        
        int retryCount = 0;
        Exception? lastException = null;
        
        while (retryCount <= _options.MaxRetries)
        {
            try
            {
                using var httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds);

                var jsonOptions = new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true
                };
                
                var content = new StringContent(
                    JsonSerializer.Serialize(command, jsonOptions),
                    Encoding.UTF8,
                    "application/json");
                
                var response = await httpClient.PostAsync(endpoint, content, cancellationToken);
                response.EnsureSuccessStatusCode();
                
                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                var result = JsonSerializer.Deserialize<TResponse>(jsonResponse, jsonOptions);
                
                if (Equals(result, default(TResponse)))
                {
                    throw new XrplException("Failed to deserialize response from server");
                }
                
                return result;
            }
            catch (Exception ex)
            {
                lastException = ex;
                retryCount++;
                
                if (retryCount <= _options.MaxRetries && _options.AutoRetry)
                {
                    _logger.LogWarning(ex, $"Error sending command to server (attempt {retryCount}/{_options.MaxRetries + 1})");
                    await Task.Delay(100 * retryCount, cancellationToken);
                }
                else
                {
                    break;
                }
            }
        }
        
        if (lastException != null)
        {
            if (lastException is HttpRequestException)
            {
                throw new ConnectionException($"Failed to send command to server: {lastException.Message}", CurrentServerUri, lastException);
            }
            else
            {
                throw new XrplException($"Error processing command: {lastException.Message}", lastException);
            }
        }
        
        throw new XrplException("Unknown error sending command to server");
    }
    
    private string? GetPropertyString(JsonElement element, string propertyName, string? defaultValue = null)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            return property.GetString() ?? defaultValue;
        }
        return defaultValue;
    }

    private decimal GetPropertyDecimal(JsonElement element, string propertyName, decimal defaultValue = 0)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
        {
            return property.GetDecimal();
        }
        return defaultValue;
    }
    
    private ulong GetPropertyUInt64(JsonElement element, string propertyName, ulong defaultValue = 0)
    {
        if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Number)
        {
            return property.GetUInt64();
        }
        return defaultValue;
    }

    /// <summary>
    /// Disposes the client and releases resources.
    /// </summary>
    public void Dispose()
    {
        DisconnectAsync().GetAwaiter().GetResult();
    }
}