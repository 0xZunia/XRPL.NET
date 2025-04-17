using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using XRPL.NET.Core.Configuration;
using XRPL.NET.Core.Exceptions;
using XRPL.NET.Core.Interfaces;
using XRPL.NET.Core.Utils;

namespace XRPL.NET.Client;

/// <summary>
/// Client for subscribing to real-time events on the XRP Ledger.
/// </summary>
public class SubscriptionClient : ISubscriptionClient, IDisposable
{
    private readonly XrplClientOptions _options;
    private readonly ILogger<SubscriptionClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private ClientWebSocket? _webSocket;
    private readonly List<string> _activeSubscriptions = new();
    private readonly CancellationTokenSource _processingCts = new();
    private Task? _messageProcessingTask;
    private Uri? _currentServerUri;
    
    /// <inheritdoc/>
    public bool IsConnected => _webSocket?.State == WebSocketState.Open;
    
    /// <inheritdoc/>
    public bool IsSubscribed => _activeSubscriptions.Count > 0;
    
    /// <inheritdoc/>
    public IReadOnlyList<string> ActiveSubscriptions => _activeSubscriptions.AsReadOnly();
    
    /// <inheritdoc/>
    public event EventHandler<LedgerClosedEventArgs>? LedgerClosed;
    
    /// <inheritdoc/>
    public event EventHandler<TransactionEventArgs>? TransactionReceived;
    
    /// <inheritdoc/>
    public event EventHandler<ValidationEventArgs>? ValidationReceived;
    
    /// <inheritdoc/>
    public event EventHandler<PathFindEventArgs>? PathFindUpdate;
    
    /// <inheritdoc/>
    public event EventHandler<OrderBookEventArgs>? OrderBookUpdate;

    /// <summary>
    /// Initializes a new instance of the SubscriptionClient.
    /// </summary>
    /// <param name="options">Configuration options for the XRP Ledger client.</param>
    /// <param name="logger">Logger for capturing client operations.</param>
    public SubscriptionClient(
        XrplClientOptions options,
        ILogger<SubscriptionClient> logger)
    {
        _options = options;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> SubscribeAsync(
        IEnumerable<SubscriptionStream> streams,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsConnected)
            {
                await ConnectAsync(cancellationToken);
            }
            
            var streamList = streams.Select(s => s.ToString().ToLowerInvariant()).ToList();
            _logger.LogInformation($"Subscribing to streams: {string.Join(", ", streamList)}");
            
            var request = new
            {
                command = "subscribe",
                streams = streamList
            };
            
            var response = await SendCommandAsync(request, cancellationToken);
            
            // JsonElement is a struct, so it can't be null
            
            if (response.ValueKind == JsonValueKind.Object && response.TryGetProperty("error", out var error))
            {
                string errorMessage = error.ValueKind == JsonValueKind.String ? error.GetString() ?? "Unknown error" : "Unknown error";
                string errorDetails = "";
                
                if (response.TryGetProperty("error_message", out var errorMsgProp))
                {
                    errorDetails = errorMsgProp.ValueKind == JsonValueKind.String ? errorMsgProp.GetString() ?? "" : "";
                }
                
                return Result.Failure<bool>($"Error from server: {errorMessage} - {errorDetails}");
            }
            
            // Add the streams to the active subscriptions list
            foreach (var stream in streamList.Where(stream => !_activeSubscriptions.Contains(stream)))
            {
                _activeSubscriptions.Add(stream);
            }
            
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to streams");
            return Result.Failure<bool>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> SubscribeLedgerAsync(
        CancellationToken cancellationToken = default)
    {
        return await SubscribeAsync(new[] { SubscriptionStream.Ledger }, cancellationToken);
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> SubscribeAccountAsync(
        string accountId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.NotNullOrEmpty(accountId, nameof(accountId));
            
            if (!IsConnected)
            {
                await ConnectAsync(cancellationToken);
            }
            
            _logger.LogInformation($"Subscribing to account: {accountId}");
            
            var request = new
            {
                command = "subscribe",
                accounts = new[] { accountId }
            };
            
            var response = await SendCommandAsync(request, cancellationToken);
            
            if (response.TryGetProperty("error", out var error))
            {
                string errorMessage = error.GetString() ?? "Unknown error";
                string errorDetails = "";
                
                if (response.TryGetProperty("error_message", out var errorMsgProp))
                {
                    errorDetails = errorMsgProp.GetString() ?? "";
                }
                
                return Result.Failure<bool>($"Error from server: {errorMessage} - {errorDetails}");
            }
            
            // Add the account subscription
            string accountStream = $"account:{accountId}";
            if (!_activeSubscriptions.Contains(accountStream))
            {
                _activeSubscriptions.Add(accountStream);
            }
            
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error subscribing to account {accountId}");
            return Result.Failure<bool>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> SubscribeAccountsAsync(
        IEnumerable<string> accountIds,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var accounts = accountIds.ToList();
            if (accounts.Count == 0)
            {
                return Result.Failure<bool>("No accounts specified");
            }
            
            if (!IsConnected)
            {
                await ConnectAsync(cancellationToken);
            }
            
            _logger.LogInformation($"Subscribing to accounts: {string.Join(", ", accounts)}");
            
            var request = new
            {
                command = "subscribe",
                accounts = accounts
            };
            
            var response = await SendCommandAsync(request, cancellationToken);
            
            if (response.TryGetProperty("error", out var error))
            {
                string errorMessage = error.GetString() ?? "Unknown error";
                string errorDetails = "";
                
                if (response.TryGetProperty("error_message", out var errorMsgProp))
                {
                    errorDetails = errorMsgProp.GetString() ?? "";
                }
                
                return Result.Failure<bool>($"Error from server: {errorMessage} - {errorDetails}");
            }
            
            // Add the account subscriptions
            foreach (var accountId in accounts)
            {
                string accountStream = $"account:{accountId}";
                if (!_activeSubscriptions.Contains(accountStream))
                {
                    _activeSubscriptions.Add(accountStream);
                }
            }
            
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to multiple accounts");
            return Result.Failure<bool>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> SubscribeOrderBookAsync(
        string baseCurrency,
        string? baseIssuer,
        string quoteCurrency,
        string? quoteIssuer,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.NotNullOrEmpty(baseCurrency, nameof(baseCurrency));
            Guard.NotNullOrEmpty(quoteCurrency, nameof(quoteCurrency));
            
            if (baseCurrency != "XRP" && string.IsNullOrEmpty(baseIssuer))
            {
                return Result.Failure<bool>("Base issuer is required for non-XRP currency");
            }
            
            if (quoteCurrency != "XRP" && string.IsNullOrEmpty(quoteIssuer))
            {
                return Result.Failure<bool>("Quote issuer is required for non-XRP currency");
            }
            
            if (!IsConnected)
            {
                await ConnectAsync(cancellationToken);
            }
            
            _logger.LogInformation($"Subscribing to order book: {baseCurrency}/{quoteCurrency}");
            
            // Build the book object
            var book = new Dictionary<string, object>();
            
            // Base currency (taker_pays)
            if (baseCurrency == "XRP")
            {
                book["taker_pays"] = new { currency = "XRP" };
            }
            else
            {
                book["taker_pays"] = new { currency = baseCurrency, issuer = baseIssuer };
            }
            
            // Quote currency (taker_gets)
            if (quoteCurrency == "XRP")
            {
                book["taker_gets"] = new { currency = "XRP" };
            }
            else
            {
                book["taker_gets"] = new { currency = quoteCurrency, issuer = quoteIssuer };
            }
            
            // Set additional parameters
            book["snapshot"] = true;
            book["both"] = true;
            
            var request = new
            {
                command = "subscribe",
                books = new[] { book }
            };
            
            var response = await SendCommandAsync(request, cancellationToken);
            
            if (response.TryGetProperty("error", out var error))
            {
                string errorMessage = error.GetString() ?? "Unknown error";
                string errorDetails = "";
                
                if (response.TryGetProperty("error_message", out var errorMsgProp))
                {
                    errorDetails = errorMsgProp.GetString() ?? "";
                }
                
                return Result.Failure<bool>($"Error from server: {errorMessage} - {errorDetails}");
            }
            
            // Add the book subscription
            string bookStream = $"book:{baseCurrency}.{baseIssuer ?? ""}/{quoteCurrency}.{quoteIssuer ?? ""}";
            if (!_activeSubscriptions.Contains(bookStream))
            {
                _activeSubscriptions.Add(bookStream);
            }
            
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error subscribing to order book");
            return Result.Failure<bool>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> UnsubscribeAsync(
        IEnumerable<SubscriptionStream> streams,
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsConnected)
            {
                return Result.Success(true);
            }
            
            var streamList = streams.Select(s => s.ToString().ToLowerInvariant()).ToList();
            _logger.LogInformation($"Unsubscribing from streams: {string.Join(", ", streamList)}");
            
            var request = new
            {
                command = "unsubscribe",
                streams = streamList
            };
            
            var response = await SendCommandAsync(request, cancellationToken);
            
            if (response.TryGetProperty("error", out var error))
            {
                string errorMessage = error.GetString() ?? "Unknown error";
                string errorDetails = "";
                
                if (response.TryGetProperty("error_message", out var errorMsgProp))
                {
                    errorDetails = errorMsgProp.GetString() ?? "";
                }
                
                return Result.Failure<bool>($"Error from server: {errorMessage} - {errorDetails}");
            }
            
            // Remove the streams from the active subscriptions list
            foreach (var stream in streamList)
            {
                _activeSubscriptions.Remove(stream);
            }
            
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from streams");
            return Result.Failure<bool>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> UnsubscribeAllAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            if (!IsConnected || _activeSubscriptions.Count == 0)
            {
                _activeSubscriptions.Clear();
                return Result.Success(true);
            }
            
            _logger.LogInformation("Unsubscribing from all streams");
            
            // Split subscription types
            var standardStreams = _activeSubscriptions
                .Where(s => !s.StartsWith("account:") && !s.StartsWith("book:"))
                .ToList();
            
            var accountStreams = _activeSubscriptions
                .Where(s => s.StartsWith("account:"))
                .Select(s => s.Substring(8)) // Remove "account:" prefix
                .ToList();
            
            var bookStreams = _activeSubscriptions
                .Where(s => s.StartsWith("book:"))
                .ToList();
            
            // Send unsubscribe commands for each type
            bool success = true;
            
            // Unsubscribe from standard streams
            if (standardStreams.Count > 0)
            {
                var request = new
                {
                    command = "unsubscribe",
                    streams = standardStreams
                };
                
                var response = await SendCommandAsync(request, cancellationToken);
                
                if (response.TryGetProperty("error", out _))
                {
                    success = false;
                }
            }
            
            // Unsubscribe from accounts
            if (accountStreams.Count > 0)
            {
                var request = new
                {
                    command = "unsubscribe",
                    accounts = accountStreams
                };
                
                var response = await SendCommandAsync(request, cancellationToken);
                
                if (response.TryGetProperty("error", out _))
                {
                    success = false;
                }
            }
            
            // Unsubscribe from books
            if (bookStreams.Count > 0)
            {
                // This is a simplification - in a real implementation,
                // we would need to parse the book streams and reconstruct the book objects
                var request = new
                {
                    command = "unsubscribe",
                    books = new[] { new Dictionary<string, object>() }
                };
                
                var response = await SendCommandAsync(request, cancellationToken);
                
                if (response.TryGetProperty("error", out _))
                {
                    success = false;
                }
            }
            
            // Clear all subscriptions regardless of success
            _activeSubscriptions.Clear();
            
            return Result.Success(success);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error unsubscribing from all streams");
            _activeSubscriptions.Clear();
            return Result.Failure<bool>(ex.Message);
        }
    }

    private async Task ConnectAsync(CancellationToken cancellationToken)
    {
        if (IsConnected)
        {
            return;
        }
        
        if (_options.ServerUris.Count == 0)
        {
            throw new InvalidOperationException("No servers configured");
        }
        
        _logger.LogInformation("Connecting to XRP Ledger WebSocket...");
        
        var exceptions = new List<Exception>();
        
        // Stop any existing message processing
        await StopMessageProcessingAsync();
        
        // Try each server in the list until one works
        foreach (var serverUri in _options.ServerUris)
        {
            try
            {
                // Skip non-WebSocket servers
                if (!serverUri.Scheme.StartsWith("ws"))
                {
                    _logger.LogWarning($"Skipping non-WebSocket server {serverUri}");
                    continue;
                }
                
                // Create new WebSocket
                _webSocket = new ClientWebSocket();
                
                // Set timeout
                using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(_options.TimeoutSeconds));
                using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(timeoutCts.Token, cancellationToken);
                
                // Connect to server
                await _webSocket.ConnectAsync(serverUri, linkedCts.Token);
                
                // If we get here, connection succeeded
                _currentServerUri = serverUri;
                _logger.LogInformation($"Connected to WebSocket server {serverUri}");
                
                // Start processing messages
                StartMessageProcessing();
                
                return;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to connect to WebSocket server {serverUri}");
                exceptions.Add(new ConnectionException($"Failed to connect to {serverUri}", serverUri, ex));
                
                // Dispose failed WebSocket
                if (_webSocket != null)
                {
                    _webSocket.Dispose();
                    _webSocket = null;
                }
            }
        }
        
        // If we've tried all servers and none worked, throw an exception
        if (exceptions.Count == 1)
        {
            throw exceptions[0];
        }
        
        throw new AggregateException("Failed to connect to any WebSocket server", exceptions);
    }

    private void StartMessageProcessing()
    {
        if (_messageProcessingTask != null)
        {
            return;
        }
        
        // Reset cancellation token
        _processingCts.TryReset();
        
        // Start message processing task
        _messageProcessingTask = Task.Run(ProcessMessagesAsync);
    }

    private async Task StopMessageProcessingAsync()
    {
        if (_messageProcessingTask == null)
        {
            return;
        }
        
        try
        {
            // Signal cancellation
            _processingCts.Cancel();
            
            // Wait for the task to complete
            await _messageProcessingTask;
        }
        catch (OperationCanceledException)
        {
            // Expected
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error stopping message processing");
        }
        finally
        {
            _messageProcessingTask = null;
        }
    }

    private async Task ProcessMessagesAsync()
    {
        byte[] buffer = new byte[16384]; // 16KB buffer
        
        try
        {
            while (!_processingCts.Token.IsCancellationRequested && 
                   _webSocket != null && 
                   _webSocket.State == WebSocketState.Open)
            {
                // Receive a message
                using var ms = new MemoryStream();
                WebSocketReceiveResult result;
                
                do
                {
                    result = await _webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), _processingCts.Token);
                    
                    if (result.MessageType == WebSocketMessageType.Close)
                    {
                        _logger.LogInformation("WebSocket closed by server");
                        break;
                    }
                    
                    ms.Write(buffer, 0, result.Count);
                } 
                while (!result.EndOfMessage);
                
                if (result.MessageType == WebSocketMessageType.Close)
                {
                    break;
                }
                
                // Process the message
                ms.Seek(0, SeekOrigin.Begin);
                string message = Encoding.UTF8.GetString(ms.ToArray());
                
                try
                {
                    await HandleMessageAsync(message);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error handling WebSocket message");
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
            _logger.LogInformation("WebSocket message processing canceled");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing WebSocket messages");
        }
        finally
        {
            // Close and dispose WebSocket if still open
            if (_webSocket?.State == WebSocketState.Open)
            {
                try
                {
                    await _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Closing", CancellationToken.None);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing WebSocket");
                }
            }
        }
    }

    private async Task HandleMessageAsync(string message)
    {
        try
        {
            var jsonResponse = JsonDocument.Parse(message);
            var root = jsonResponse.RootElement;
            
            // Check if this is a subscription message
            if (root.TryGetProperty("type", out var typeProperty))
            {
                string type = typeProperty.GetString() ?? "";
                
                switch (type)
                {
                    case "ledgerClosed":
                        HandleLedgerClosed(root);
                        break;
                        
                    case "transaction":
                        HandleTransaction(root);
                        break;
                        
                    case "validation":
                        HandleValidation(root);
                        break;
                        
                    case "path_find":
                        HandlePathFind(root);
                        break;
                        
                    case "orderBook":
                        HandleOrderBook(root);
                        break;
                        
                    default:
                        _logger.LogWarning($"Unknown subscription message type: {type}");
                        break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error parsing WebSocket message");
        }
    }

    private void HandleLedgerClosed(JsonElement message)
    {
        try
        {
            var args = new LedgerClosedEventArgs
            {
                LedgerIndex = GetUInt32(message, "ledger_index"),
                LedgerHash = GetString(message, "ledger_hash"),
                TotalCoins = GetString(message, "total_coins"),
                FeeBase = GetUInt64(message, "fee_base"),
                ReserveBase = GetUInt64(message, "reserve_base"),
                ReserveIncrement = GetUInt64(message, "reserve_inc")
            };
            
            // Parse close time
            if (message.TryGetProperty("ledger_time", out var timeProperty))
            {
                var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                uint closeTimeSeconds = timeProperty.GetUInt32();
                args.CloseTime = rippleEpoch.AddSeconds(closeTimeSeconds);
            }
            
            LedgerClosed?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling ledger closed message");
        }
    }

    private void HandleTransaction(JsonElement message)
    {
        try
        {
            if (!message.TryGetProperty("transaction", out var txProperty))
            {
                return;
            }
            
            var args = new TransactionEventArgs
            {
                Transaction = txProperty,
                Validated = GetBool(message, "validated"),
                TransactionId = GetString(txProperty, "hash")
            };
            
            if (message.TryGetProperty("meta", out var metaProperty))
            {
                args.Meta = metaProperty;
                args.EngineResult = GetString(metaProperty, "TransactionResult");
            }
            
            if (txProperty.TryGetProperty("Account", out var accountProperty))
            {
                args.Account = accountProperty.GetString();
            }
            
            if (txProperty.TryGetProperty("TransactionType", out var txTypeProperty))
            {
                args.TransactionType = txTypeProperty.GetString();
            }
            
            if (message.TryGetProperty("ledger_index", out var ledgerIndexProperty))
            {
                args.LedgerIndex = ledgerIndexProperty.GetUInt32();
            }
            
            TransactionReceived?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling transaction message");
        }
    }

    private void HandleValidation(JsonElement message)
    {
        try
        {
            var args = new ValidationEventArgs
            {
                LedgerIndex = GetUInt32(message, "ledger_index"),
                LedgerHash = GetString(message, "ledger_hash"),
                ValidatorPublicKey = GetString(message, "validation_public_key")
            };
            
            // Parse validation time
            if (message.TryGetProperty("ledger_time", out var timeProperty))
            {
                var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                uint validationTimeSeconds = timeProperty.GetUInt32();
                args.ValidationTime = rippleEpoch.AddSeconds(validationTimeSeconds);
            }
            
            ValidationReceived?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling validation message");
        }
    }

    private void HandlePathFind(JsonElement message)
    {
        try
        {
            var args = new PathFindEventArgs
            {
                SourceAccount = GetString(message, "source_account"),
                DestinationAccount = GetString(message, "destination_account")
            };
            
            if (message.TryGetProperty("destination_amount", out var destAmountProperty))
            {
                args.DestinationAmount = destAmountProperty;
            }
            
            if (message.TryGetProperty("alternatives", out var alternativesProperty) && 
                alternativesProperty.ValueKind == JsonValueKind.Array)
            {
                foreach (var altProperty in alternativesProperty.EnumerateArray())
                {
                    if (altProperty.TryGetProperty("paths_computed", out var pathsProperty) && 
                        pathsProperty.ValueKind == JsonValueKind.Array)
                    {
                        args.Paths.Add(pathsProperty);
                    }
                }
            }
            
            PathFindUpdate?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling path find message");
        }
    }

    private void HandleOrderBook(JsonElement message)
    {
        try
        {
            var args = new OrderBookEventArgs();
            
            // Try to parse base currency and issuer
            if (message.TryGetProperty("taker_pays", out var takerPaysProperty))
            {
                if (takerPaysProperty.ValueKind == JsonValueKind.Object)
                {
                    args.BaseCurrency = GetString(takerPaysProperty, "currency");
                    args.BaseIssuer = GetString(takerPaysProperty, "issuer");
                }
                else if (takerPaysProperty.ValueKind == JsonValueKind.String)
                {
                    args.BaseCurrency = "XRP";
                }
            }
            
            // Try to parse quote currency and issuer
            if (message.TryGetProperty("taker_gets", out var takerGetsProperty))
            {
                if (takerGetsProperty.ValueKind == JsonValueKind.Object)
                {
                    args.QuoteCurrency = GetString(takerGetsProperty, "currency");
                    args.QuoteIssuer = GetString(takerGetsProperty, "issuer");
                }
                else if (takerGetsProperty.ValueKind == JsonValueKind.String)
                {
                    args.QuoteCurrency = "XRP";
                }
            }
            
            // Parse bid orders
            if (message.TryGetProperty("bids", out var bidsProperty) && 
                bidsProperty.ValueKind == JsonValueKind.Array)
            {
                foreach (var bidProperty in bidsProperty.EnumerateArray())
                {
                    var order = new Order
                    {
                        Account = GetString(bidProperty, "Account"),
                        OfferId = GetString(bidProperty, "BookDirectory")
                    };
                    
                    // Parse price and amount
                    if (bidProperty.TryGetProperty("TakerGets", out var takerGets) && 
                        bidProperty.TryGetProperty("TakerPays", out var takerPays))
                    {
                        // Logic to calculate price and amount
                        // In a real implementation, this would handle both XRP and issued currencies
                        order.Amount = 0;
                        order.Price = 0;
                    }
                    
                    args.Bids.Add(order);
                }
            }
            
            // Parse ask orders
            if (message.TryGetProperty("asks", out var asksProperty) && 
                asksProperty.ValueKind == JsonValueKind.Array)
            {
                foreach (var askProperty in asksProperty.EnumerateArray())
                {
                    var order = new Order
                    {
                        Account = GetString(askProperty, "Account"),
                        OfferId = GetString(askProperty, "BookDirectory")
                    };
                    
                    // Parse price and amount
                    if (askProperty.TryGetProperty("TakerGets", out var takerGets) && 
                        askProperty.TryGetProperty("TakerPays", out var takerPays))
                    {
                        // Logic to calculate price and amount
                        // In a real implementation, this would handle both XRP and issued currencies
                        order.Amount = 0;
                        order.Price = 0;
                    }
                    
                    args.Asks.Add(order);
                }
            }
            
            OrderBookUpdate?.Invoke(this, args);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error handling order book message");
        }
    }

    private async Task<JsonElement> SendCommandAsync(object command, CancellationToken cancellationToken)
    {
        try
        {
            if (_webSocket == null || _webSocket.State != WebSocketState.Open)
            {
                await ConnectAsync(cancellationToken);
            }
            
            string message = JsonSerializer.Serialize(command, _jsonOptions);
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            
            await _webSocket!.SendAsync(new ArraySegment<byte>(messageBytes), WebSocketMessageType.Text, true, cancellationToken);
            
            // For commands that require a response, we would need to implement a mechanism to wait for the response
            // This is a simplified implementation that just returns success
            using var jsonDoc = JsonDocument.Parse(@"{""status"":""success""}");
            return jsonDoc.RootElement.Clone();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending WebSocket command");
            throw;
        }
    }

    private static string GetString(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            return property.GetString() ?? "";
        }
        return "";
    }

    private static uint GetUInt32(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.GetUInt32();
            }
            if (property.ValueKind == JsonValueKind.String && uint.TryParse(property.GetString(), out uint result))
            {
                return result;
            }
        }
        return 0;
    }

    private static ulong GetUInt64(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.Number)
            {
                return property.GetUInt64();
            }
            if (property.ValueKind == JsonValueKind.String && ulong.TryParse(property.GetString(), out ulong result))
            {
                return result;
            }
        }
        return 0;
    }

    private static bool GetBool(JsonElement element, string propertyName)
    {
        if (element.TryGetProperty(propertyName, out var property))
        {
            if (property.ValueKind == JsonValueKind.True || property.ValueKind == JsonValueKind.False)
            {
                return property.GetBoolean();
            }
            if (property.ValueKind == JsonValueKind.String && bool.TryParse(property.GetString(), out bool result))
            {
                return result;
            }
        }
        return false;
    }

    /// <summary>
    /// Disposes the subscription client.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Disposes the subscription client.
    /// </summary>
    /// <param name="disposing">True if disposing managed resources.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing) return;

        _processingCts.Cancel();
            
        // Dispose WebSocket
        if (_webSocket != null)
        {
            if (_webSocket.State == WebSocketState.Open)
            {
                try
                {
                    _webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Disposing", CancellationToken.None)
                        .GetAwaiter().GetResult();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error closing WebSocket during disposal");
                }
            }
                
            _webSocket.Dispose();
            _webSocket = null;
        }
            
        // Dispose cancellation token source
        _processingCts.Dispose();
    }
}
