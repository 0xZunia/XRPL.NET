using XRPL.NET.Models.Transactions;
using XRPL.NET.Models.Transactions.Common;

namespace XRPL.NET.Transactions.Factory;

/// <summary>
/// Base class for transaction builders.
/// </summary>
/// <typeparam name="TBuilder">The type of the builder.</typeparam>
/// <typeparam name="TTransaction">The type of the transaction.</typeparam>
public abstract class BuilderBase<TBuilder, TTransaction>
    where TBuilder : BuilderBase<TBuilder, TTransaction>
    where TTransaction : TransactionBase, new()
{
    /// <summary>
    /// The transaction being built.
    /// </summary>
    protected readonly TTransaction Transaction;
    
    /// <summary>
    /// Initializes a new instance of the <see cref="BuilderBase{TBuilder, TTransaction}"/> class.
    /// </summary>
    protected BuilderBase()
    {
        Transaction = new TTransaction();
    }
    
    /// <summary>
    /// Sets the account that initiates the transaction.
    /// </summary>
    /// <param name="account">The account address.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithAccount(string account)
    {
        Transaction.Account = account;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the sequence number for the transaction.
    /// </summary>
    /// <param name="sequence">The sequence number.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithSequence(uint sequence)
    {
        Transaction.Sequence = sequence;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the fee for the transaction.
    /// </summary>
    /// <param name="fee">The fee in drops of XRP.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithFee(string fee)
    {
        Transaction.Fee = fee;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the flags for the transaction.
    /// </summary>
    /// <param name="flags">The flags value.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithFlags(uint flags)
    {
        Transaction.Flags = flags;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the source tag for the transaction.
    /// </summary>
    /// <param name="sourceTag">The source tag.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithSourceTag(uint sourceTag)
    {
        Transaction.SourceTag = sourceTag;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the last ledger sequence for the transaction.
    /// </summary>
    /// <param name="lastLedgerSequence">The last ledger sequence.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithLastLedgerSequence(uint lastLedgerSequence)
    {
        Transaction.LastLedgerSequence = lastLedgerSequence;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the expiration for the transaction.
    /// </summary>
    /// <param name="ledgerOffset">The number of ledgers after which the transaction expires.</param>
    /// <param name="currentLedgerIndex">The current ledger index.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithExpiration(uint ledgerOffset, uint currentLedgerIndex)
    {
        Transaction.SetExpiration(ledgerOffset, currentLedgerIndex);
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Adds a memo to the transaction.
    /// </summary>
    /// <param name="memo">The memo to add.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithMemo(Memo memo)
    {
        Transaction.AddMemo(memo);
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Adds a text memo to the transaction.
    /// </summary>
    /// <param name="text">The text to add as a memo.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithMemo(string text)
    {
        Transaction.AddMemo(text);
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Adds a signer to the transaction for multi-signing.
    /// </summary>
    /// <param name="signer">The signer to add.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithSigner(Signer signer)
    {
        Transaction.AddSigner(signer);
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the signing public key for the transaction.
    /// </summary>
    /// <param name="signingPubKey">The signing public key.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithSigningPublicKey(string signingPubKey)
    {
        Transaction.SigningPubKey = signingPubKey;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the transaction signature.
    /// </summary>
    /// <param name="txnSignature">The transaction signature.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithTransactionSignature(string txnSignature)
    {
        Transaction.TxnSignature = txnSignature;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the account transaction ID.
    /// </summary>
    /// <param name="accountTxnId">The account transaction ID.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithAccountTransactionId(string accountTxnId)
    {
        Transaction.AccountTxnID = accountTxnId;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the destination tag for the transaction.
    /// </summary>
    /// <param name="destinationTag">The destination tag.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithDestinationTag(uint destinationTag)
    {
        Transaction.DestinationTag = destinationTag;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Sets the ticket to use for the transaction.
    /// </summary>
    /// <param name="ticketSequence">The ticket sequence number.</param>
    /// <returns>The builder for method chaining.</returns>
    public TBuilder WithTicket(uint ticketSequence)
    {
        Transaction.TicketSequence = ticketSequence;
        return (TBuilder)this;
    }
    
    /// <summary>
    /// Builds the transaction.
    /// </summary>
    /// <returns>The built transaction.</returns>
    public TTransaction Build()
    {
        Validate();
        return Transaction;
    }
    
    /// <summary>
    /// Validates the transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected virtual void Validate()
    {
        if (string.IsNullOrEmpty(Transaction.Account))
        {
            throw new InvalidOperationException("Account is required.");
        }
    }
}