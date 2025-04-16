using XRPL.NET.Models.Transactions.Common;
using XRPL.NET.Models.Transactions.Escrow;
using XRPL.NET.Transactions.Factory;

namespace XRPL.NET.Transactions.Builders;

/// <summary>
/// Builder for creating escrow create transactions.
/// </summary>
public class EscrowCreateBuilder : BuilderBase<EscrowCreateBuilder, EscrowCreateTransaction>
{
    /// <summary>
    /// Sets the destination account for the escrowed payment.
    /// </summary>
    /// <param name="destination">The destination account address.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowCreateBuilder WithDestination(string destination)
    {
        Transaction.Destination = destination;
        return this;
    }
    
    /// <summary>
    /// Sets the amount to escrow as XRP.
    /// </summary>
    /// <param name="xrpAmount">The amount of XRP to escrow.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowCreateBuilder WithAmount(decimal xrpAmount)
    {
        Transaction.Amount = XrpAmount.ToDrops(xrpAmount);
        return this;
    }
    
    /// <summary>
    /// Sets the amount to escrow as XRP using drops.
    /// </summary>
    /// <param name="drops">The amount of XRP to escrow in drops.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowCreateBuilder WithAmountInDrops(string drops)
    {
        Transaction.Amount = drops;
        return this;
    }
    
    /// <summary>
    /// Sets the time when the escrow can be finished.
    /// </summary>
    /// <param name="finishAfter">The time when the escrow can be finished.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowCreateBuilder WithFinishAfter(DateTime finishAfter)
    {
        var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan timeSpan = finishAfter.ToUniversalTime() - rippleEpoch;
        uint finishAfterSeconds = (uint)timeSpan.TotalSeconds;
        
        Transaction.FinishAfter = finishAfterSeconds;
        return this;
    }
    
    /// <summary>
    /// Sets the time when the escrow can be finished in Ripple epoch seconds.
    /// </summary>
    /// <param name="finishAfterSeconds">The time when the escrow can be finished in Ripple epoch seconds.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowCreateBuilder WithFinishAfterEpoch(uint finishAfterSeconds)
    {
        Transaction.FinishAfter = finishAfterSeconds;
        return this;
    }
    
    /// <summary>
    /// Sets the time after which the escrow can be canceled.
    /// </summary>
    /// <param name="cancelAfter">The time after which the escrow can be canceled.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowCreateBuilder WithCancelAfter(DateTime cancelAfter)
    {
        var rippleEpoch = new DateTime(2000, 1, 1, 0, 0, 0, DateTimeKind.Utc);
        TimeSpan timeSpan = cancelAfter.ToUniversalTime() - rippleEpoch;
        uint cancelAfterSeconds = (uint)timeSpan.TotalSeconds;
        
        Transaction.CancelAfter = cancelAfterSeconds;
        return this;
    }
    
    /// <summary>
    /// Sets the time after which the escrow can be canceled in Ripple epoch seconds.
    /// </summary>
    /// <param name="cancelAfterSeconds">The time after which the escrow can be canceled in Ripple epoch seconds.</param>
    /// <returns>The builder for method chaining.</returns>
    public EscrowCreateBuilder WithCancelAfterEpoch(uint cancelAfterSeconds)
    {
        Transaction.CancelAfter = cancelAfterSeconds;
        return this;
    }
    
    /// <summary>
    /// Sets the cryptographic condition for finishing the escrow.
    /// </summary>
    /// <param name="condition">The cryptographic condition in hexadecimal format.</param>
    /// <returns>The builder for method chaining.</returns>
    /// <remarks>
    /// The condition must be in the format defined by crypto-conditions specification (PREIMAGE-SHA-256).
    /// </remarks>
    public EscrowCreateBuilder WithCondition(string condition)
    {
        Transaction.Condition = condition;
        return this;
    }
    
    /// <summary>
    /// Validates the escrow create transaction.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the transaction is invalid.</exception>
    protected override void Validate()
    {
        base.Validate();
        
        if (string.IsNullOrEmpty(Transaction.Destination))
        {
            throw new InvalidOperationException("Destination is required for an escrow create transaction.");
        }
        
        if (string.IsNullOrEmpty(Transaction.Amount))
        {
            throw new InvalidOperationException("Amount is required for an escrow create transaction.");
        }
        
        if (Transaction.FinishAfter == null && Transaction.CancelAfter == null && string.IsNullOrEmpty(Transaction.Condition))
        {
            throw new InvalidOperationException("At least one of FinishAfter, CancelAfter, or Condition is required for an escrow create transaction.");
        }
        
        if (Transaction.FinishAfter != null && Transaction.CancelAfter != null && Transaction.FinishAfter >= Transaction.CancelAfter)
        {
            throw new InvalidOperationException("FinishAfter must be earlier than CancelAfter.");
        }
    }
}