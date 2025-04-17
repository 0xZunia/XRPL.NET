using System;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XRPL.NET.Core.Configuration;
using XRPL.NET.Core.Exceptions;
using XRPL.NET.Core.Interfaces;
using XRPL.NET.Core.Utils;
using XRPL.NET.Serialization;

namespace XRPL.NET.Client;

/// <summary>
/// Client for submitting and tracking transactions on the XRP Ledger.
/// </summary>
public class TransactionClient : ITransactionClient
{
    private readonly XrplClientOptions _options;
    private readonly ILogger<TransactionClient> _logger;
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly HttpClient _httpClient;
    private readonly ISerializer _serializer;

    /// <summary>
    /// Initializes a new instance of the TransactionClient.
    /// </summary>
    /// <param name="options">Configuration options for the XRP Ledger client.</param>
    /// <param name="logger">Logger for capturing client operations.</param>
    public TransactionClient(
        XrplClientOptions options, 
        ILogger<TransactionClient> logger)
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
        _serializer = new BinarySerializer();
    }

    /// <inheritdoc/>
    public async Task<Result<SubmitResult>> SubmitTransactionAsync(
        string signedTransaction, 
        bool failHard = false, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Submitting signed transaction");
            
            var request = new
            {
                method = "submit",
                @params = new[]
                {
                    new
                    {
                        tx_blob = signedTransaction,
                        fail_hard = failHard
                    }
                }
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<SubmitResult>("Failed to get response from server");
            }

            var submitResult = new SubmitResult
            {
                TransactionId = response.result?.tx_json?.hash?.ToString() ?? "",
                EngineResult = response.result?.engine_result?.ToString() ?? "",
                EngineResultMessage = response.result?.engine_result_message?.ToString() ?? "",
                Accepted = response.result?.engine_result?.ToString()?.StartsWith("tes") ?? false
            };

            if (response.result?.tx_json == null) return Result.Success(submitResult);
            if (response.result.tx_json.Sequence != null)
            {
                submitResult.Sequence = Convert.ToUInt32(response.result.tx_json.Sequence.ToString());
            }
                
            if (response.result.tx_json.Fee != null)
            {
                submitResult.Fee = Convert.ToUInt64(response.result.tx_json.Fee.ToString());
            }

            return Result.Success(submitResult);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error submitting transaction");
            return Result.Failure<SubmitResult>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<SubmitResult>> SubmitAsync(
        object transaction, 
        string secret, 
        bool failHard = false, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Signing and submitting transaction");
            
            if (_options.AutoComputeFee)
            {
                var feeResult = await CalculateFeeAsync(transaction, cancellationToken);
                if (feeResult.IsSuccess)
                {
                    var txType = transaction.GetType();
                    var feeProperty = txType.GetProperty("Fee");
                    if (feeProperty != null && feeProperty.GetValue(transaction) == null)
                    {
                        feeProperty.SetValue(transaction, feeResult.Value.ToString());
                    }
                }
            }
            
            var request = new
            {
                method = "sign",
                @params = new[]
                {
                    new
                    {
                        tx_json = transaction,
                        secret = secret
                    }
                }
            };

            var signResponse = await SendRequestAsync(request, cancellationToken);
            if (signResponse == null)
            {
                return Result.Failure<SubmitResult>("Failed to get response from server");
            }

            if (signResponse.error != null)
            {
                return Result.Failure<SubmitResult>($"Error signing transaction: {signResponse.error} - {signResponse.error_message ?? "Unknown error"}");
            }

            var signedTx = signResponse.result?.tx_blob?.ToString();
            if (string.IsNullOrEmpty(signedTx))
            {
                return Result.Failure<SubmitResult>("Failed to sign transaction");
            }

            return await SubmitTransactionAsync(signedTx, failHard, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error signing and submitting transaction");
            return Result.Failure<SubmitResult>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<TransactionStatus>> GetTransactionStatusAsync(
        string transactionId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Getting status for transaction {transactionId}");
            
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
                return Result.Failure<TransactionStatus>("Failed to get response from server");
            }

            if (response.error != null)
            {
                return Result.Success(new TransactionStatus
                {
                    TransactionId = transactionId,
                    Validated = false,
                    InLedger = false
                });
            }

            var status = new TransactionStatus
            {
                TransactionId = transactionId,
                Validated = Convert.ToBoolean(response.result?.validated?.ToString() ?? "false"),
                Result = response.result?.meta?.TransactionResult?.ToString(),
                InLedger = response.result?.ledger_index != null
            };

            if (response.result?.ledger_index != null)
            {
                status.LedgerIndex = Convert.ToUInt32(response.result.ledger_index.ToString());
            }

            if (response.result?.date == null) return Result.Success(status);
            var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            uint seconds = Convert.ToUInt32(response.result.date.ToString() ?? "0");
            status.Date = rippleEpoch.AddSeconds(seconds);

            return Result.Success(status);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting status for transaction {transactionId}");
            return Result.Failure<TransactionStatus>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<TransactionStatus>> WaitForTransactionAsync(
        string transactionId, 
        TimeSpan? timeout = null, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Waiting for transaction {transactionId} to be validated");
            
            var maxWaitTime = timeout ?? TimeSpan.FromSeconds(30);
            var startTime = DateTime.UtcNow;
            
            while (DateTime.UtcNow - startTime < maxWaitTime)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return Result.Failure<TransactionStatus>("Operation was canceled");
                }
                
                var statusResult = await GetTransactionStatusAsync(transactionId, cancellationToken);
                if (statusResult.IsFailure)
                {
                    return statusResult;
                }
                
                if (statusResult.Value.Validated)
                {
                    return statusResult;
                }
                
                await Task.Delay(1000, cancellationToken);
            }
            
            // Timeout reached
            return Result.Failure<TransactionStatus>("Timeout waiting for transaction validation");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error waiting for transaction {transactionId}");
            return Result.Failure<TransactionStatus>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<bool>> VerifyTransactionAsync(
        string transactionId, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation($"Verifying transaction {transactionId}");
            
            var statusResult = await GetTransactionStatusAsync(transactionId, cancellationToken);
            if (statusResult.IsFailure)
            {
                return Result.Failure<bool>(statusResult.Errors);
            }
            
            return Result.Success(statusResult.Value.Validated && statusResult.Value.IsSuccessful);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error verifying transaction {transactionId}");
            return Result.Failure<bool>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<ulong>> CalculateFeeAsync(
        object transaction, 
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Calculating transaction fee");
            
            // First, get the current fee from the server
            var feeEstimate = await EstimateFeeAsync(cancellationToken);
            if (feeEstimate.IsFailure)
            {
                return Result.Failure<ulong>(feeEstimate.Errors);
            }
            
            // Serialize the transaction to get its exact size
            var serializeResult = await _serializer.SerializeTransactionAsync(transaction);
            if (serializeResult.IsFailure)
            {
                // If serialization fails, return the recommended fee
                return Result.Success(feeEstimate.Value.RecommendedFee);
            }
            
            // Calculate fee based on transaction size
            var txSize = serializeResult.Value.Length;
            var baseFee = feeEstimate.Value.MinimumFee;
            
            // Adjust fee based on size - larger transactions require higher fees
            ulong fee = txSize <= 1024 ? baseFee : baseFee * (ulong)Math.Ceiling(txSize / 1024.0);
            
            // Apply fee multiplier for faster inclusion
            fee = (ulong)(fee * (double)_options.FeeMultiplier);
            
            return Result.Success(fee);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating transaction fee");
            return Result.Failure<ulong>(ex.Message);
        }
    }

    /// <inheritdoc/>
    public async Task<Result<FeeEstimate>> EstimateFeeAsync(
        CancellationToken cancellationToken = default)
    {
        try
        {
            _logger.LogInformation("Estimating transaction fee");
            
            var request = new
            {
                method = "fee",
                @params = new object[] { }
            };

            var response = await SendRequestAsync(request, cancellationToken);
            if (response == null)
            {
                return Result.Failure<FeeEstimate>("Failed to get response from server");
            }

            // Parse the fee information
            var drops = response.result?.drops;
            if (drops == null)
            {
                return Result.Failure<FeeEstimate>("Fee information not found in response");
            }

            var feeEstimate = new FeeEstimate
            {
                MinimumFee = Convert.ToUInt64(drops.minimum_fee?.ToString() ?? "10"),
                MedianFee = Convert.ToUInt64(drops.median_fee?.ToString() ?? "10000"),
                OpenLedgerFee = Convert.ToUInt64(drops.open_ledger_fee?.ToString() ?? "10")
            };

            // Calculate recommended fee
            feeEstimate.RecommendedFee = (ulong)(feeEstimate.OpenLedgerFee * (double)_options.FeeMultiplier);
            
            // Ensure minimum fee
            if (feeEstimate.RecommendedFee < feeEstimate.MinimumFee)
            {
                feeEstimate.RecommendedFee = feeEstimate.MinimumFee;
            }

            return Result.Success(feeEstimate);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error estimating transaction fee");
            return Result.Failure<FeeEstimate>(ex.Message);
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
                
                var response = await _httpClient.PostAsync(endpoint, content, cancellationToken);
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogWarning($"Server {serverUri} returned status code {response.StatusCode}");
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
        
        throw new ConnectionException("Failed to connect to any server");
    }
}