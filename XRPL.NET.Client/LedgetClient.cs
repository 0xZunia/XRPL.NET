using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using XRPL.NET.Core.Configuration;
using XRPL.NET.Core.Interfaces;
using XRPL.NET.Core.Utils;
using XRPL.NET.Models.Transactions.Common;
using IssuedCurrencyAmount = XRPL.NET.Core.Interfaces.IssuedCurrencyAmount;

namespace XRPL.NET.Client;

/// <summary>
/// Client for interacting with the XRP Ledger to retrieve ledger data.
/// </summary>
public class LedgerClient : ILedgerClient
{
    private readonly XrplClientOptions _options;
    private readonly ILogger<LedgerClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the LedgerClient.
    /// </summary>
    /// <param name="options">Configuration options for the XRP Ledger client.</param>
    /// <param name="logger">Logger for capturing client operations.</param>
    public LedgerClient(
        XrplClientOptions options,
        ILogger<LedgerClient> logger)
    {
        _options = options;
        _logger = logger;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true
        };
        _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(_options.TimeoutSeconds)
        };
    }

    /// <inheritdoc/>
    public async Task<Result<AccountInfo>> GetAccountInfoAsync(
        string accountId,
        uint? ledgerIndex = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.NotNullOrEmpty(accountId, nameof(accountId));
            
            _logger.LogInformation($"Getting account info for {accountId}");
            
            var request = new
            {
                id = 2,
                command = "account_info",
                account = accountId,
                ledgerIndex = ledgerIndex?.ToString() ?? "current",
                queue = true
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<AccountInfo>("Failed to get response from server");
            }

            if (response.error != null)
            {
                return Result.Failure<AccountInfo>($"Error from server: {response.error} - {response.error_message ?? "Unknown error"}");
            }

            if (response.result?.account_data == null)
            {
                return Result.Failure<AccountInfo>("Account data not found in response");
            }

            var accountData = response.result.account_data;
            
            var accountInfo = new AccountInfo
            {
                AccountId = accountId,
                Balance = XrpAmount.ToXrp(accountData.Balance?.ToString() ?? "0"),
                Sequence = Convert.ToUInt32(accountData.Sequence?.ToString() ?? "0"),
                OwnerCount = Convert.ToUInt32(accountData.OwnerCount?.ToString() ?? "0"),
                LedgerIndex = Convert.ToUInt32(response.result.ledger_index?.ToString() ?? "0"),
                Flags = Convert.ToUInt32(accountData.Flags?.ToString() ?? "0")
            };

            if (accountData.Domain != null)
            {
                accountInfo.Domain = accountData.Domain.ToString();
            }

            if (accountData.EmailHash != null)
            {
                accountInfo.EmailHash = accountData.EmailHash.ToString();
            }

            if (accountData.RegularKey != null)
            {
                accountInfo.RegularKey = accountData.RegularKey.ToString();
            }

            if (accountData.PreviousTxnID != null)
            {
                accountInfo.PreviousTxnId = accountData.PreviousTxnID.ToString();
            }

            return Result.Success(accountInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting account info for {accountId}");
            return Result.Failure<AccountInfo>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AccountTransactions>> GetAccountTransactionsAsync(
        string accountId,
        uint? limit = null,
        object? marker = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.NotNullOrEmpty(accountId, nameof(accountId));
            
            _logger.LogInformation($"Getting account transactions for {accountId}");
            
            var parameters = new Dictionary<string, object>
            {
                ["account"] = accountId,
                ["ledger_index_min"] = -1, // Use -1 for all available history
                ["ledger_index_max"] = -1,
                ["binary"] = false,
                ["forward"] = false
            };

            if (limit.HasValue)
            {
                parameters["limit"] = limit.Value;
            }

            if (marker != null)
            {
                parameters["marker"] = marker;
            }

            var request = new
            {
                method = "account_tx",
                @params = new[] { parameters }
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<AccountTransactions>("Failed to get response from server");
            }

            if (response.error != null)
            {
                return Result.Failure<AccountTransactions>($"Error from server: {response.error} - {response.error_message ?? "Unknown error"}");
            }

            if (response.result?.transactions == null)
            {
                return Result.Failure<AccountTransactions>("Transactions not found in response");
            }

            var accountTransactions = new AccountTransactions
            {
                AccountId = accountId
            };

            if (response.result.marker != null)
            {
                accountTransactions.Marker = response.result.marker;
            }

            foreach (var txItem in response.result.transactions)
            {
                if (txItem.tx == null && txItem.tx_blob == null)
                {
                    continue;
                }

                var tx = txItem.tx ?? DeserializeBinaryTransaction(txItem.tx_blob.ToString());
                
                var transactionInfo = new TransactionInfo
                {
                    TransactionId = tx.hash?.ToString() ?? "",
                    Transaction = tx,
                    Meta = txItem.meta,
                    Validated = Convert.ToBoolean(txItem.validated?.ToString() ?? "false")
                };

                if (txItem.meta?.ledger_index != null)
                {
                    transactionInfo.LedgerIndex = Convert.ToUInt32(txItem.meta.ledger_index.ToString());
                }

                if (tx.date != null)
                {
                    var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                    uint seconds = Convert.ToUInt32(tx.date.ToString() ?? "0");
                    transactionInfo.Date = rippleEpoch.AddSeconds(seconds);
                }

                accountTransactions.Transactions.Add(transactionInfo);
            }

            return Result.Success(accountTransactions);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting account transactions for {accountId}");
            return Result.Failure<AccountTransactions>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AccountBalances>> GetAccountBalancesAsync(
        string accountId,
        uint? ledgerIndex = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // First get the XRP balance from account_info
            var accountInfoResult = await GetAccountInfoAsync(accountId, ledgerIndex, cancellationToken);
            if (accountInfoResult.IsFailure)
            {
                return Result.Failure<AccountBalances>(accountInfoResult.Errors);
            }

            var balances = new AccountBalances
            {
                AccountId = accountId,
                XrpBalance = accountInfoResult.Value.Balance,
                LedgerIndex = accountInfoResult.Value.LedgerIndex
            };

            // Then get issued currency balances from account_lines
            var linesResult = await GetAccountLinesAsync(accountId, null, null, null, cancellationToken);
            if (linesResult.IsSuccess)
            {
                foreach (var line in linesResult.Value.Lines)
                {
                    balances.IssuedCurrencies.Add(new IssuedCurrencyAmount
                    {
                        Currency = line.Currency,
                        Issuer = line.Account,
                        Value = line.Balance
                    });
                }
            }

            return Result.Success(balances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting account balances for {accountId}");
            return Result.Failure<AccountBalances>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AccountLines>> GetAccountLinesAsync(
        string accountId,
        string? currency = null,
        uint? limit = null,
        object? marker = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.NotNullOrEmpty(accountId, nameof(accountId));
            
            _logger.LogInformation($"Getting account lines for {accountId}");
            
            var parameters = new Dictionary<string, object>
            {
                ["account"] = accountId,
                ["ledger_index"] = "validated"
            };

            if (!string.IsNullOrEmpty(currency))
            {
                parameters["currency"] = currency;
            }

            if (limit.HasValue)
            {
                parameters["limit"] = limit.Value;
            }

            if (marker != null)
            {
                parameters["marker"] = marker;
            }

            var request = new
            {
                method = "account_lines",
                @params = new[] { parameters }
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<AccountLines>("Failed to get response from server");
            }

            if (response.error != null)
            {
                return Result.Failure<AccountLines>($"Error from server: {response.error} - {response.error_message ?? "Unknown error"}");
            }

            if (response.result?.lines == null)
            {
                return Result.Failure<AccountLines>("Trust lines not found in response");
            }

            var accountLines = new AccountLines
            {
                AccountId = accountId,
                LedgerIndex = Convert.ToUInt32(response.result.ledger_index?.ToString() ?? "0")
            };

            if (response.result.marker != null)
            {
                accountLines.Marker = response.result.marker;
            }

            foreach (var line in response.result.lines)
            {
                var trustLine = new TrustLine
                {
                    Account = line.account?.ToString() ?? "",
                    Currency = line.currency?.ToString() ?? "",
                    Balance = Convert.ToDecimal(line.balance?.ToString() ?? "0"),
                    Limit = Convert.ToDecimal(line.limit?.ToString() ?? "0"),
                    LimitPeer = Convert.ToDecimal(line.limit_peer?.ToString() ?? "0"),
                    Authorized = Convert.ToBoolean(line.authorized?.ToString() ?? "false"),
                    PeerAuthorized = Convert.ToBoolean(line.peer_authorized?.ToString() ?? "false"),
                    Freeze = Convert.ToBoolean(line.freeze?.ToString() ?? "false"),
                    FreezePeer = Convert.ToBoolean(line.freeze_peer?.ToString() ?? "false")
                };

                if (line.quality_in != null)
                {
                    trustLine.QualityIn = Convert.ToDecimal(line.quality_in.ToString());
                }

                if (line.quality_out != null)
                {
                    trustLine.QualityOut = Convert.ToDecimal(line.quality_out.ToString());
                }

                accountLines.Lines.Add(trustLine);
            }

            return Result.Success(accountLines);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting account lines for {accountId}");
            return Result.Failure<AccountLines>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AccountNfts>> GetAccountNftsAsync(
        string accountId,
        uint? limit = null,
        object? marker = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.NotNullOrEmpty(accountId, nameof(accountId));
            
            _logger.LogInformation($"Getting account NFTs for {accountId}");
            
            var parameters = new Dictionary<string, object>
            {
                ["account"] = accountId
            };

            if (limit.HasValue)
            {
                parameters["limit"] = limit.Value;
            }

            if (marker != null)
            {
                parameters["marker"] = marker;
            }

            var request = new
            {
                method = "account_nfts",
                @params = new[] { parameters }
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<AccountNfts>("Failed to get response from server");
            }

            if (response.error != null)
            {
                return Result.Failure<AccountNfts>($"Error from server: {response.error} - {response.error_message ?? "Unknown error"}");
            }

            var accountNfts = new AccountNfts
            {
                AccountId = accountId,
                LedgerIndex = Convert.ToUInt32(response.result?.ledger_index?.ToString() ?? "0")
            };

            if (response.result?.marker != null)
            {
                accountNfts.Marker = response.result.marker;
            }

            if (response.result?.account_nfts != null)
            {
                foreach (var nftItem in response.result.account_nfts)
                {
                    var nft = new Nft
                    {
                        TokenId = nftItem.TokenID?.ToString() ?? "",
                        Owner = accountId,
                        Taxon = Convert.ToUInt32(nftItem.NFTokenTaxon?.ToString() ?? "0"),
                        Sequence = Convert.ToUInt32(nftItem.nft_serial?.ToString() ?? "0"),
                        Flags = Convert.ToUInt32(nftItem.Flags?.ToString() ?? "0")
                    };

                    if (nftItem.URI != null)
                    {
                        nft.Uri = nftItem.URI.ToString();
                    }

                    accountNfts.Nfts.Add(nft);
                }
            }

            return Result.Success(accountNfts);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting account NFTs for {accountId}");
            return Result.Failure<AccountNfts>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<AccountObjects>> GetAccountObjectsAsync(
        string accountId,
        string? objectType = null,
        uint? limit = null,
        object? marker = null,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.NotNullOrEmpty(accountId, nameof(accountId));
            
            _logger.LogInformation($"Getting account objects for {accountId}");
            
            var parameters = new Dictionary<string, object>
            {
                ["account"] = accountId,
                ["ledger_index"] = "validated"
            };

            if (!string.IsNullOrEmpty(objectType))
            {
                parameters["type"] = objectType;
            }

            if (limit.HasValue)
            {
                parameters["limit"] = limit.Value;
            }

            if (marker != null)
            {
                parameters["marker"] = marker;
            }

            var request = new
            {
                method = "account_objects",
                @params = new[] { parameters }
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<AccountObjects>("Failed to get response from server");
            }

            if (response.error != null)
            {
                return Result.Failure<AccountObjects>($"Error from server: {response.error} - {response.error_message ?? "Unknown error"}");
            }

            var accountObjects = new AccountObjects
            {
                AccountId = accountId,
                LedgerIndex = Convert.ToUInt32(response.result?.ledger_index?.ToString() ?? "0")
            };

            if (response.result?.marker != null)
            {
                accountObjects.Marker = response.result.marker;
            }

            if (response.result?.account_objects != null)
            {
                foreach (var obj in response.result.account_objects)
                {
                    accountObjects.Objects.Add(obj);
                }
            }

            return Result.Success(accountObjects);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting account objects for {accountId}");
            return Result.Failure<AccountObjects>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<TransactionInfo>> GetTransactionAsync(
        string transactionId,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.NotNullOrEmpty(transactionId, nameof(transactionId));
            
            _logger.LogInformation($"Getting transaction {transactionId}");
            
            var request = new
            {
                method = "tx",
                @params = new[]
                {
                    new
                    {
                        transaction = transactionId,
                        binary = false
                    }
                }
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<TransactionInfo>("Failed to get response from server");
            }

            if (response.error != null)
            {
                return Result.Failure<TransactionInfo>($"Error from server: {response.error} - {response.error_message ?? "Unknown error"}");
            }

            var transactionInfo = new TransactionInfo
            {
                TransactionId = transactionId,
                Transaction = response.result,
                Meta = response.result?.meta,
                Validated = Convert.ToBoolean(response.result?.validated?.ToString() ?? "false")
            };

            if (response.result?.ledger_index != null)
            {
                transactionInfo.LedgerIndex = Convert.ToUInt32(response.result.ledger_index.ToString());
            }

            if (response.result?.date != null)
            {
                var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                uint seconds = Convert.ToUInt32(response.result.date.ToString() ?? "0");
                transactionInfo.Date = rippleEpoch.AddSeconds(seconds);
            }

            return Result.Success(transactionInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting transaction {transactionId}");
            return Result.Failure<TransactionInfo>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<LedgerInfo>> GetLedgerAsync(
        uint? ledgerIndex = null,
        bool includeTxs = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Getting ledger {ledgerIndex?.ToString() ?? "validated"}");
            
            var parameters = new Dictionary<string, object>
            {
                ["transactions"] = includeTxs,
                ["expand"] = includeTxs
            };

            if (ledgerIndex.HasValue)
            {
                parameters["ledger_index"] = ledgerIndex.Value;
            }
            else
            {
                parameters["ledger_index"] = "validated";
            }

            var request = new
            {
                method = "ledger",
                @params = new[] { parameters }
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<LedgerInfo>("Failed to get response from server");
            }

            if (response.error != null)
            {
                return Result.Failure<LedgerInfo>($"Error from server: {response.error} - {response.error_message ?? "Unknown error"}");
            }

            if (response.result?.ledger == null)
            {
                return Result.Failure<LedgerInfo>("Ledger not found in response");
            }

            var ledger = response.result.ledger;
            
            var ledgerInfo = new LedgerInfo
            {
                LedgerIndex = Convert.ToUInt32(ledger.ledger_index?.ToString() ?? "0"),
                LedgerHash = ledger.ledger_hash?.ToString() ?? "",
                ParentLedgerHash = ledger.parent_hash?.ToString() ?? "",
                CloseTime = Convert.ToUInt32(ledger.close_time?.ToString() ?? "0"),
                CloseTimeResolution = Convert.ToUInt32(ledger.close_time_resolution?.ToString() ?? "0"),
                Closed = Convert.ToBoolean(ledger.closed?.ToString() ?? "false"),
                TotalCoins = ledger.total_coins?.ToString() ?? "0"
            };

            // Convert Ripple epoch time to DateTime
            var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            ledgerInfo.CloseTimeUtc = rippleEpoch.AddSeconds(ledgerInfo.CloseTime);

            // Parse transactions if included
            if (includeTxs && ledger.transactions != null)
            {
                ledgerInfo.Transactions = new List<TransactionInfo>();
                
                foreach (var tx in ledger.transactions)
                {
                    var txInfo = new TransactionInfo
                    {
                        TransactionId = tx.hash?.ToString() ?? "",
                        Transaction = tx,
                        LedgerIndex = ledgerInfo.LedgerIndex,
                        Validated = true
                    };

                    if (tx.metaData != null)
                    {
                        txInfo.Meta = tx.metaData;
                    }

                    if (tx.date != null)
                    {
                        uint seconds = Convert.ToUInt32(tx.date.ToString() ?? "0");
                        txInfo.Date = rippleEpoch.AddSeconds(seconds);
                    }

                    ledgerInfo.Transactions.Add(txInfo);
                }
            }

            return Result.Success(ledgerInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting ledger {ledgerIndex?.ToString() ?? "validated"}");
            return Result.Failure<LedgerInfo>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<LedgerInfo>> GetLedgerByHashAsync(
        string ledgerHash,
        bool includeTxs = false,
        CancellationToken cancellationToken = default)
    {
        try
        {
            Guard.NotNullOrEmpty(ledgerHash, nameof(ledgerHash));
            
            _logger.LogInformation($"Getting ledger by hash {ledgerHash}");
            
            var parameters = new Dictionary<string, object>
            {
                ["ledger_hash"] = ledgerHash,
                ["transactions"] = includeTxs,
                ["expand"] = includeTxs
            };

            var request = new
            {
                method = "ledger",
                @params = new[] { parameters }
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<LedgerInfo>("Failed to get response from server");
            }

            if (response.error != null)
            {
                return Result.Failure<LedgerInfo>($"Error from server: {response.error} - {response.error_message ?? "Unknown error"}");
            }

            if (response.result?.ledger == null)
            {
                return Result.Failure<LedgerInfo>("Ledger not found in response");
            }

            var ledger = response.result.ledger;
            
            var ledgerInfo = new LedgerInfo
            {
                LedgerIndex = Convert.ToUInt32(ledger.ledger_index?.ToString() ?? "0"),
                LedgerHash = ledger.ledger_hash?.ToString() ?? "",
                ParentLedgerHash = ledger.parent_hash?.ToString() ?? "",
                CloseTime = Convert.ToUInt32(ledger.close_time?.ToString() ?? "0"),
                CloseTimeResolution = Convert.ToUInt32(ledger.close_time_resolution?.ToString() ?? "0"),
                Closed = Convert.ToBoolean(ledger.closed?.ToString() ?? "false"),
                TotalCoins = ledger.total_coins?.ToString() ?? "0"
            };

            // Convert Ripple epoch time to DateTime
            var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            ledgerInfo.CloseTimeUtc = rippleEpoch.AddSeconds(ledgerInfo.CloseTime);

            // Parse transactions if included
            if (includeTxs && ledger.transactions != null)
            {
                ledgerInfo.Transactions = new List<TransactionInfo>();
                
                foreach (var tx in ledger.transactions)
                {
                    var txInfo = new TransactionInfo
                    {
                        TransactionId = tx.hash?.ToString() ?? "",
                        Transaction = tx,
                        LedgerIndex = ledgerInfo.LedgerIndex,
                        Validated = true
                    };

                    if (tx.metaData != null)
                    {
                        txInfo.Meta = tx.metaData;
                    }

                    if (tx.date != null)
                    {
                        uint seconds = Convert.ToUInt32(tx.date.ToString() ?? "0");
                        txInfo.Date = rippleEpoch.AddSeconds(seconds);
                    }

                    ledgerInfo.Transactions.Add(txInfo);
                }
            }

            return Result.Success(ledgerInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting ledger by hash {ledgerHash}");
            return Result.Failure<LedgerInfo>(ex.Message);
        }
    }

    private async Task<dynamic?> SendRequestAsync(object request, CancellationToken cancellationToken)
    {
        foreach (var serverUri in _options.ServerUris)
        {
            try
            {
                string endpoint = serverUri.ToString();
                if (!endpoint.EndsWith('/'))
                {
                    endpoint += "/";
                }
                
                // For WebSocket servers, use HTTP endpoint for JSON-RPC
                if (endpoint.StartsWith("ws"))
                {
                    endpoint = endpoint.Replace("ws", "http");
                }
                
                var content = new StringContent(
                    JsonSerializer.Serialize(request, _jsonOptions),
                    Encoding.UTF8,
                    "application/json");
                
                _logger.LogInformation($"Sending request to {endpoint}");
                _logger.LogInformation($"Request: {content.ReadAsStringAsync(cancellationToken).Result}");
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Server {serverUri} returned status code {response.StatusCode}");
                    _logger.LogWarning($"Response: {await response.Content.ReadAsStringAsync(cancellationToken)}");
                    continue;
                }
                
                var jsonResponse = await response.Content.ReadAsStringAsync(cancellationToken);
                return JsonSerializer.Deserialize<dynamic>(jsonResponse, _jsonOptions);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, $"Failed to connect to server {serverUri}");
            }
        }
        
        return null;
    }

    private dynamic DeserializeBinaryTransaction(string binaryTx)
    {
        try
        {
            // This is a placeholder for actual binary deserialization
            // In a real implementation, this would use the serializer to deserialize the tx_blob
            return new { };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deserializing binary transaction");
            throw;
        }
    }
}
