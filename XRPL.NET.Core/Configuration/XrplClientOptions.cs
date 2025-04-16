namespace XRPL.NET.Core.Configuration;

/// <summary>
/// Configuration options for the XRP Ledger client.
/// </summary>
public class XrplClientOptions
{
    private readonly List<Uri> _serverUris = new();
    
    /// <summary>
    /// Gets the server URIs to connect to.
    /// </summary>
    public IReadOnlyList<Uri> ServerUris => _serverUris.AsReadOnly();
    
    /// <summary>
    /// Gets or sets the timeout for network operations in seconds.
    /// </summary>
    /// <remarks>
    /// Default value is 30 seconds.
    /// </remarks>
    public int TimeoutSeconds { get; set; } = 30;
    
    /// <summary>
    /// Gets or sets the maximum number of retry attempts for failed operations.
    /// </summary>
    /// <remarks>
    /// Default value is 3 retries.
    /// </remarks>
    public int MaxRetries { get; set; } = 3;
    
    /// <summary>
    /// Gets or sets a value indicating whether to automatically retry failed requests.
    /// </summary>
    /// <remarks>
    /// Default value is true.
    /// </remarks>
    public bool AutoRetry { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the network type to connect to.
    /// </summary>
    public NetworkType Network { get; set; } = NetworkType.MainNet;
    
    /// <summary>
    /// Gets or sets a value indicating whether to use secure connections when available.
    /// </summary>
    /// <remarks>
    /// Default value is true.
    /// </remarks>
    public bool UseSecureConnection { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether to verify SSL certificates.
    /// </summary>
    /// <remarks>
    /// Default value is true. Set to false only in development environments.
    /// </remarks>
    public bool VerifySslCertificate { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether to use WebSocket for communication.
    /// </summary>
    /// <remarks>
    /// Default value is true. If false, uses JSON-RPC over HTTP instead.
    /// </remarks>
    public bool UseWebSocket { get; set; } = true;
    
    /// <summary>
    /// Gets or sets a value indicating whether to automatically compute the transaction fee.
    /// </summary>
    /// <remarks>
    /// Default value is true. If false, uses the default minimum fee.
    /// </remarks>
    public bool AutoComputeFee { get; set; } = true;
    
    /// <summary>
    /// Gets or sets the fee multiplier to use when auto-computing fees.
    /// </summary>
    /// <remarks>
    /// Default value is 1.2, which means 20% higher than the minimum required fee.
    /// </remarks>
    public decimal FeeMultiplier { get; set; } = 1.2m;
    
    /// <summary>
    /// Gets or sets the client name sent to the server.
    /// </summary>
    public string ClientName { get; set; } = "XrplDotNet";
    
    /// <summary>
    /// Gets or sets the client version sent to the server.
    /// </summary>
    public string ClientVersion { get; set; } = "1.0.0";
    
    /// <summary>
    /// Adds a server URI to the list of servers to connect to.
    /// </summary>
    /// <param name="serverUri">The server URI to add.</param>
    /// <returns>The configuration options instance for method chaining.</returns>
    public XrplClientOptions AddServer(Uri serverUri)
    {
        _serverUris.Add(serverUri);
        return this;
    }
    
    /// <summary>
    /// Adds a server URI to the list of servers to connect to.
    /// </summary>
    /// <param name="serverUrl">The server URL to add.</param>
    /// <returns>The configuration options instance for method chaining.</returns>
    public XrplClientOptions AddServer(string serverUrl)
    {
        _serverUris.Add(new Uri(serverUrl));
        return this;
    }
    
    /// <summary>
    /// Sets the network type and adds default servers for that network.
    /// </summary>
    /// <param name="networkType">The network type to use.</param>
    /// <returns>The configuration options instance for method chaining.</returns>
    public XrplClientOptions UseNetwork(NetworkType networkType)
    {
        Network = networkType;
        _serverUris.Clear();
        
        switch (networkType)
        {
            case NetworkType.MainNet:
                AddServer("wss://xrplcluster.com");
                AddServer("wss://s1.ripple.com");
                AddServer("wss://s2.ripple.com");
                break;
            case NetworkType.TestNet:
                AddServer("wss://s.altnet.rippletest.net:51233");
                break;
            case NetworkType.DevNet:
                AddServer("wss://s.devnet.rippletest.net:51233");
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(networkType), networkType, "Unsupported network type");
        }
        
        return this;
    }
    
    /// <summary>
    /// Creates a new instance of the <see cref="XrplClientOptions"/> class 
    /// with default configuration for the specified network.
    /// </summary>
    /// <param name="networkType">The network type to use.</param>
    /// <returns>A new configuration options instance.</returns>
    public static XrplClientOptions CreateDefault(NetworkType networkType = NetworkType.MainNet)
    {
        return new XrplClientOptions().UseNetwork(networkType);
    }
}

/// <summary>
/// Represents the different XRP Ledger networks.
/// </summary>
public enum NetworkType
{
    /// <summary>
    /// The main production network.
    /// </summary>
    MainNet,
    
    /// <summary>
    /// The test network (altnet).
    /// </summary>
    TestNet,
    
    /// <summary>
    /// The development network.
    /// </summary>
    DevNet,
    
    /// <summary>
    /// A custom network.
    /// </summary>
    Custom
}