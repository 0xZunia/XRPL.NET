namespace XRPL.NET.Core.Constants;

/// <summary>
/// Defines all transaction types supported by the XRP Ledger.
/// Each transaction type has a unique name and code used in the binary serialization format.
/// </summary>
/// <remarks>
/// This class serves as the single source of truth for transaction types in the SDK.
/// All other components should reference these constants rather than hardcoding values.
/// </remarks>
public static class TransactionTypes
{
    // Core Account Operations
    public const string Payment = "Payment";
    public const string AccountSet = "AccountSet";
    public const string SetRegularKey = "SetRegularKey";
    public const string AccountDelete = "AccountDelete";
    
    // Trust and Authorization
    public const string TrustSet = "TrustSet";
    public const string DepositPreauth = "DepositPreauth";
    
    // Decentralized Exchange
    public const string OfferCreate = "OfferCreate";
    public const string OfferCancel = "OfferCancel";
    
    // Multi-signature
    public const string SignerListSet = "SignerListSet";
    
    // Escrow Operations
    public const string EscrowCreate = "EscrowCreate";
    public const string EscrowFinish = "EscrowFinish";
    public const string EscrowCancel = "EscrowCancel";
    
    // Payment Channels
    public const string PaymentChannelCreate = "PaymentChannelCreate";
    public const string PaymentChannelFund = "PaymentChannelFund";
    public const string PaymentChannelClaim = "PaymentChannelClaim";
    
    // Checks
    public const string CheckCreate = "CheckCreate";
    public const string CheckCash = "CheckCash";
    public const string CheckCancel = "CheckCancel";
    
    // NFT Operations
    public const string NFTokenMint = "NFTokenMint";
    public const string NFTokenBurn = "NFTokenBurn";
    public const string NFTokenCreateOffer = "NFTokenCreateOffer";
    public const string NFTokenCancelOffer = "NFTokenCancelOffer";
    public const string NFTokenAcceptOffer = "NFTokenAcceptOffer";
    
    // Smart Contracts
    public const string SetHook = "SetHook";
    
    // Governance
    public const string EnableAmendment = "EnableAmendment";
    public const string UNLModify = "UNLModify";
    
    // Transaction Management
    public const string Ticket = "Ticket";
    
    // Decentralized Identifiers
    public const string DIDSet = "DIDSet";
    public const string DIDDelete = "DIDDelete";
    
    // Cross-Chain Operations
    public const string XChainCreateClaimID = "XChainCreateClaimID";
    public const string XChainCommit = "XChainCommit";
    public const string XChainClaim = "XChainClaim";
    public const string XChainCreateBridge = "XChainCreateBridge";
    public const string XChainModifyBridge = "XChainModifyBridge";
    
    // Automated Market Maker
    public const string AMMCreate = "AMMCreate";
    public const string AMMDeposit = "AMMDeposit";
    public const string AMMWithdraw = "AMMWithdraw";
    public const string AMMVote = "AMMVote";
    
    /// <summary>
    /// Gets the transaction type code used in serialization from the transaction type name.
    /// </summary>
    /// <param name="transactionType">The transaction type name.</param>
    /// <returns>The integer code used in binary serialization for this transaction type.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid transaction type is provided.</exception>
    public static int GetTransactionTypeCode(string transactionType)
    {
        return transactionType switch
        {
            Payment => 0,
            AccountSet => 3,
            SetRegularKey => 5,
            OfferCreate => 7,
            OfferCancel => 8,
            TrustSet => 20,
            AccountDelete => 4,
            CheckCreate => 16,
            CheckCash => 17,
            CheckCancel => 18,
            DepositPreauth => 19,
            EscrowCreate => 9,
            EscrowFinish => 10,
            EscrowCancel => 11,
            PaymentChannelCreate => 12,
            PaymentChannelFund => 13,
            PaymentChannelClaim => 14,
            SignerListSet => 15,
            NFTokenMint => 25,
            NFTokenBurn => 26,
            NFTokenCreateOffer => 27,
            NFTokenCancelOffer => 28,
            NFTokenAcceptOffer => 29,
            SetHook => 22,
            Ticket => 21,
            DIDSet => 30,
            DIDDelete => 31,
            XChainCreateClaimID => 32,
            XChainCommit => 33,
            XChainClaim => 34,
            XChainCreateBridge => 35,
            XChainModifyBridge => 36,
            AMMCreate => 37,
            AMMDeposit => 38,
            AMMWithdraw => 39,
            AMMVote => 40,
            EnableAmendment => 100,
            UNLModify => 101,
            _ => throw new ArgumentException($"Unknown transaction type: {transactionType}", nameof(transactionType))
        };
    }
    
    /// <summary>
    /// Gets the transaction type name from the transaction type code used in serialization.
    /// </summary>
    /// <param name="typeCode">The integer code used in binary serialization.</param>
    /// <returns>The transaction type name.</returns>
    /// <exception cref="ArgumentException">Thrown when an invalid transaction type code is provided.</exception>
    public static string GetTransactionTypeName(int typeCode)
    {
        return typeCode switch
        {
            0 => Payment,
            3 => AccountSet,
            5 => SetRegularKey,
            7 => OfferCreate,
            8 => OfferCancel,
            20 => TrustSet,
            4 => AccountDelete,
            16 => CheckCreate,
            17 => CheckCash,
            18 => CheckCancel,
            19 => DepositPreauth,
            9 => EscrowCreate,
            10 => EscrowFinish,
            11 => EscrowCancel,
            12 => PaymentChannelCreate,
            13 => PaymentChannelFund,
            14 => PaymentChannelClaim,
            15 => SignerListSet,
            25 => NFTokenMint,
            26 => NFTokenBurn,
            27 => NFTokenCreateOffer,
            28 => NFTokenCancelOffer,
            29 => NFTokenAcceptOffer,
            22 => SetHook,
            21 => Ticket,
            30 => DIDSet,
            31 => DIDDelete,
            32 => XChainCreateClaimID,
            33 => XChainCommit,
            34 => XChainClaim,
            35 => XChainCreateBridge,
            36 => XChainModifyBridge,
            37 => AMMCreate,
            38 => AMMDeposit,
            39 => AMMWithdraw,
            40 => AMMVote,
            100 => EnableAmendment,
            101 => UNLModify,
            _ => throw new ArgumentException($"Unknown transaction type code: {typeCode}", nameof(typeCode))
        };
    }
}