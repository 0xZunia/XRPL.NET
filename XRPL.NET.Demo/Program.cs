using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using XRPL.NET.Client;
using XRPL.NET.Core.Configuration;
using XRPL.NET.Core.Interfaces;
using XRPL.NET.Models.Transactions.Payment;
using XRPL.NET.Transactions.Factory;
using XRPL.NET.Wallet;

namespace XRPL.NET.Demo
{
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("XRPL.NET Demo Program");
            Console.WriteLine("-----------------------");

            // Setup logging
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole()
                       .SetMinimumLevel(LogLevel.Information);
            });
            var logger = loggerFactory.CreateLogger<Program>();

            // Initialize XRP Ledger client for DevNet
            logger.LogInformation("Initializing XRPL client for DevNet...");
            var clientOptions = new XrplClientOptions()
                .UseNetwork(NetworkType.DevNet);

            using var client = new XrplClient(clientOptions, loggerFactory);

            try
            {
                // Connect to the XRP Ledger
                logger.LogInformation("Connecting to XRPL...");
                await client.ConnectAsync();
                logger.LogInformation("Connected to {0}", client.CurrentServerUri);

                // Get server info
                var serverInfo = await client.GetServerInfoAsync();
                logger.LogInformation("Server version: {0}", serverInfo.BuildVersion);
                logger.LogInformation("Ledger range: {0}", serverInfo.CompleteLedgers);
                logger.LogInformation("Validated ledger: {0}", serverInfo.ValidatedLedgerIndex);

                // Create a wallet
                logger.LogInformation("Creating a new wallet...");
                var wallet = XrplWallet.Generate();
                logger.LogInformation("Generated wallet address: {0}", wallet.Address);
                logger.LogInformation("Secret: {0}", wallet.GetSecret());
                logger.LogInformation("Public key: {0}", wallet.PublicKey);
                
                // We need to fund the wallet on DevNet
                logger.LogInformation("Please fund this wallet at https://test.xrplexplorer.com/en/faucet");
                logger.LogInformation("Press any key after funding the wallet...");
                Console.ReadKey();
                
                // TODO: here ftf
                logger.LogError("Demonstration stops here, the rest of the code is not yet functional.");
                logger.LogInformation("Press any key to exit...");
                Console.ReadKey();
                Environment.Exit(0);

                // Get account info
                try 
                {
                    logger.LogInformation("Fetching account info...");
                    var accountInfoResult = await client.Ledger.GetAccountInfoAsync(wallet.Address);
                    
                    if (accountInfoResult.IsSuccess)
                    {
                        var accountInfo = accountInfoResult.Value;
                        logger.LogInformation("Account balance: {0} XRP", accountInfo.Balance);
                        logger.LogInformation("Account sequence: {0}", accountInfo.Sequence);
                    }
                    else
                    {
                        logger.LogWarning("Failed to get account info: {0}", accountInfoResult.Error);
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error getting account info");
                }

                // Subscribe to ledger closures
                logger.LogInformation("Subscribing to ledger closures...");
                client.Subscriptions.LedgerClosed += (sender, e) =>
                {
                    logger.LogInformation("Ledger closed: {0}", e.LedgerIndex);
                };
                await client.Subscriptions.SubscribeLedgerAsync();

                // Create and submit a payment
                logger.LogInformation("Enter destination address for payment:");
                string destinationAddress = Console.ReadLine() ?? "rPT1Sjq2YGrBMTttX4GZHjKu9dyfzbpAYe"; // Default test address

                if (!XrplWallet.ValidateAddress(destinationAddress))
                {
                    logger.LogError("Invalid destination address");
                    return;
                }

                logger.LogInformation("Creating a payment transaction...");
                var payment = TransactionFactory.Payment()
                    .WithAccount(wallet.Address)
                    .WithDestination(destinationAddress)
                    .WithAmount(1.0m) // 1 XRP
                    .Build();

                logger.LogInformation("Signing and submitting payment...");
                var submitResult = await client.Transactions.SubmitAsync(payment, wallet.GetSecret());

                if (submitResult.IsSuccess)
                {
                    logger.LogInformation("Payment submitted successfully");
                    logger.LogInformation("Transaction ID: {0}", submitResult.Value.TransactionId);
                    logger.LogInformation("Engine result: {0}", submitResult.Value.EngineResult);
                    logger.LogInformation("Engine message: {0}", submitResult.Value.EngineResultMessage);

                    // Wait for validation
                    logger.LogInformation("Waiting for transaction to be validated...");
                    var statusResult = await client.Transactions.WaitForTransactionAsync(
                        submitResult.Value.TransactionId, 
                        TimeSpan.FromSeconds(10));

                    if (statusResult.IsSuccess)
                    {
                        var status = statusResult.Value;
                        if (status.Validated)
                        {
                            logger.LogInformation("Transaction validated!");
                            logger.LogInformation("Result: {0}", status.Result);
                            logger.LogInformation("In ledger: {0}", status.LedgerIndex);
                        }
                        else
                        {
                            logger.LogWarning("Transaction not validated within timeout");
                        }
                    }
                    else
                    {
                        logger.LogWarning("Failed to get transaction status: {0}", statusResult.Error);
                    }

                    // Get updated account info
                    logger.LogInformation("Fetching updated account info...");
                    var updatedAccountInfo = await client.Ledger.GetAccountInfoAsync(wallet.Address);
                    
                    if (updatedAccountInfo.IsSuccess)
                    {
                        logger.LogInformation("Updated balance: {0} XRP", updatedAccountInfo.Value.Balance);
                    }
                }
                else
                {
                    logger.LogError("Failed to submit payment: {0}", submitResult.Error);
                }

                // Listen for more ledger closures
                logger.LogInformation("Listening for 5 more ledger closures...");
                int ledgerCount = 0;
                var ledgerClosedHandler = new EventHandler<LedgerClosedEventArgs>((sender, e) =>
                {
                    ledgerCount++;
                    logger.LogInformation("Ledger {0} closed at {1}", e.LedgerIndex, e.CloseTime);
                    
                    if (ledgerCount >= 5)
                    {
                        // We can't unsubscribe from within the handler, so we'll do it outside
                        logger.LogInformation("Received 5 ledger closures, demo complete.");
                    }
                });

                client.Subscriptions.LedgerClosed += ledgerClosedHandler;
                
                // Wait for user to end demo
                logger.LogInformation("Press any key to end the demo...");
                Console.ReadKey();
                
                // Unsubscribe and disconnect
                logger.LogInformation("Unsubscribing from ledgers...");
                client.Subscriptions.LedgerClosed -= ledgerClosedHandler;
                await client.Subscriptions.UnsubscribeAsync(new[] { SubscriptionStream.Ledger });
                
                logger.LogInformation("Disconnecting...");
                await client.DisconnectAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in demo program");
            }

            logger.LogInformation("Demo program completed");
        }
    }
}