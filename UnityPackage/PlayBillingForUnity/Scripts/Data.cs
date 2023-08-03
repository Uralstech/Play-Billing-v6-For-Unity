using System;

#nullable enable

namespace Uralstech.PlayBillingForUnity.Data
{
    #region Enums

    /// <summary>
    /// Enum representing the type of a product. It can be either an in-app product or a subscription product.
    /// </summary>
    public enum ProductType
    {
        /// <summary>
        /// Represents an in-app product.
        /// </summary>
        InApp,

        /// <summary>
        /// Represents a subscription product.
        /// </summary>
        Subscription
    }

    /// <summary>
    /// Enum representing the reason for the failure of a purchase.
    /// </summary>
    public enum PurchaseFailureReason
    {
        /// <summary>
        /// The product details have not been set in the plugin.
        /// </summary>
        ProductDetailsNotSet,
        /// <summary>
        /// Subscription offer details were given for the purchase of a non-subscription product.
        /// </summary>
        SubscriptionDetailsGivenForInAppProduct,
        /// <summary>
        /// Subscription offer details were not provided for the purchase of a subscription product.
        /// </summary>
        SubscriptionDetailsNotGiven,
        /// <summary>
        /// The offer/base plan given in the subscription offer details was not found.
        /// </summary>
        SubscriptionOfferOrPlanNotFound,
        /// <summary>
        /// The details of the product to be purchased were not found.
        /// </summary>
        ProductDetailsNotFound,
        /// <summary>
        /// The product to be purchased is not defined in the plugin.
        /// </summary>
        ProductNotDefined,
        /// <summary>
        /// The billing client is not yet ready to purchase a product.
        /// </summary>
        BilingClientNotReady,
        /// <summary>
        /// The requested feature is not supported by the Play Store on the current device.
        /// </summary>
        FeatureNotSupported,
        /// <summary>
        /// The app is not connected to the Play Store service via the Google Play Billing Library.
        /// </summary>
        ServiceDisconnected,
        /// <summary>
        /// The transaction was canceled by the user.
        /// </summary>
        UserCancelled,
        /// <summary>
        /// The service is currently unavailable.
        /// </summary>
        ServiceUnavailable,
        /// <summary>
        /// A user billing error occurred during processing.
        /// </summary>
        BillingUnavailable,
        /// <summary>
        /// The requested product is not available for purchase.
        /// </summary>
        ItemUnavailable,
        /// <summary>
        /// Error resulting from incorrect usage of the API. (This could be a fault of the plugin)
        /// </summary>
        DeveloperError,
        /// <summary>
        /// A fatal error occured during the API action.
        /// </summary>
        Error,
        /// <summary>
        /// The purchase failed because the item is already owned.
        /// </summary>
        ItemAlreadyOwned,
        /// <summary>
        /// The requested action on the item failed since it is not owned by the user.
        /// </summary>
        ItemNotOwned,
        /// <summary>
        /// A network error occurred during the operation.
        /// </summary>
        NetworkError
    }

    /// <summary>
    /// Enum representing the state of the billing client.
    /// </summary>
    public enum BillingClientStatus
    {
        /// <summary>
        /// The requested feature is not supported by the Play Store on the current device.
        /// </summary>
        FeatureNotSupported,
        /// <summary>
        /// The app is not connected to the Play Store service via the Google Play Billing Library.
        /// </summary>
        ServiceDisconnected,
        /// <summary>
        /// The app connected to the Play Store service.
        /// </summary>
        Connected,
        /// <summary>
        /// The service is currently unavailable.
        /// </summary>
        ServiceUnavailable,
        /// <summary>
        /// A user billing error occurred during processing.
        /// </summary>
        BillingUnavailable,
        /// <summary>
        /// Error resulting from incorrect usage of the API. (This could be a fault of the plugin)
        /// </summary>
        DeveloperError,
        /// <summary>
        /// A fatal error occured during the API action.
        /// </summary>
        Error,
        /// <summary>
        /// A network error occurred during the operation.
        /// </summary>
        NetworkError,
    }

    /// <summary>
    /// Enum representing the state of a purchase. It can be unspecified, purchased, or pending.
    /// </summary>
    public enum PurchaseState
    {
        /// <summary>
        /// Represents an unspecified purchase state.
        /// </summary>
        Unspecified,
        /// <summary>
        /// Represents a purchased purchase state.
        /// </summary>
        Purchased,
        /// <summary>
        /// Represents a pending purchase state.
        /// </summary>
        Pending
    }

    /// <summary>
    /// Enum representing the validity of a purchase. It can be valid, failed, or failed becuase the purchase validation key was not found.
    /// </summary>
    public enum PurchaseValidity
    {
        /// <summary>
        /// Represents a valid purchase.
        /// </summary>
        Valid,

        /// <summary>
        /// Represents a failed validation.
        /// </summary>
        Failed,

        /// <summary>
        /// Represents a failed validation because the purchase validation key was not found.
        /// </summary>
        KeyNotFound
    }

    /// <summary>
    /// Enum representing the recurrence mode of a pricing phase. It can be infinite, finite or none.
    /// </summary>
    public enum RecurrenceMode
    {
        /// <summary>
        /// Represents a billing plan payment which recurs for infinite billing periods unless cancelled.
        /// </summary>
        Infinite,
        /// <summary>
        /// Represents a billing plan payment which recurs for a fixed number of billing periods set in the billing cycle count.
        /// </summary>
        Finite,
        /// <summary>
        /// Represents a billing plan payment which is a one time charge that does not repeat.
        /// </summary>
        None
    }

    #endregion

    #region Main classes
    /// <summary>
    /// Struct representing basic product details. It includes the ID and type of the product.
    /// </summary>
    public struct Product
    {
        /// <summary>
        /// The ID of the product. It must be the same as the product/subscription ID set in Google Play Console.
        /// </summary>
        public string ID { get; private set; }

        /// <summary>
        /// The type of the product.
        /// </summary>
        public ProductType ProductType { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Product"/> object.
        /// </summary>
        /// <param name="ID">The ID of the product. It must be the same as the product/subscription ID set in Google Play Console.</param>
        /// <param name="productType">The type of the product.</param>
        public Product(string ID, ProductType productType)
        {
            this.ID = ID;
            ProductType = productType;
        }
    }

    /// <summary>
    /// Struct representing an in-app billing purchase.
    /// </summary>
    public struct Purchase
    {
        /// <summary>
        /// The account identifiers associated with the purchase.
        /// </summary>
        public AccountIdentifiers? AccountIdentifiers { get; private set; }

        /// <summary>
        /// The payload specified when the purchase was acknowledged or consumed.
        /// </summary>
        public string DeveloperPayload { get; private set; }

        /// <summary>
        /// The unique order identifier for the transaction.
        /// </summary>
        public string OrderID { get; private set; }

        /// <summary>
        /// The JSON string which contains details about the purchase order.
        /// </summary>
        public string OriginalJSON { get; private set; }

        /// <summary>
        /// The package name of the application from which the purchase originated.
        /// </summary>
        public string PackageName { get; private set; }

        /// <summary>
        /// The product IDs of the purchase.
        /// </summary>
        public string[] ProductIDs { get; private set; }

        /// <summary>
        /// The state of the purchase.
        /// </summary>
        public PurchaseState PurchaseState { get; private set; }

        /// <summary>
        /// The time of the purchase.
        /// </summary>
        public DateTime PurchaseTime { get; private set; }

        /// <summary>
        /// The token which uniquely identifies a purchase for a given item and user pair.
        /// </summary>
        public string PurchaseToken { get; private set; }

        /// <summary>
        /// The the quantity of the purchased product.
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// The signature of the purchase data that was signed with your private key.
        /// </summary>
        public string Signature { get; private set; }

        /// <summary>
        /// Indicates whether the purchase has been acknowledged.
        /// </summary>
        public bool IsAcknowledged { get; private set; }

        /// <summary>
        /// Indicates whether the subscription renews automatically. Only for purchases of subscription products!
        /// </summary>
        public bool IsAutoRenewing { get; private set; }

        /// <summary>
        /// Creates a new <see cref="Purchase"/> object from a <see cref="PurchaseRaw"/> object.
        /// </summary>
        /// <param name="rawPurchase">The <see cref="PurchaseRaw"/> object used to create a new <see cref="Purchase"/> object.</param>
        public Purchase(PurchaseRaw rawPurchase)
        {
            AccountIdentifiers = rawPurchase.AccountIdentifiers;
            DeveloperPayload = rawPurchase.DeveloperPayload;
            OrderID = rawPurchase.OrderID;
            OriginalJSON = rawPurchase.OriginalJSON;
            PackageName = rawPurchase.PackageName;
            ProductIDs = rawPurchase.ProductIDs;

            switch (rawPurchase.PurchaseState)
            {
                case 1:
                    PurchaseState = PurchaseState.Purchased;
                    break;
                case 2:
                    PurchaseState = PurchaseState.Pending;
                    break;
                default:
                    PurchaseState = PurchaseState.Unspecified;
                    break;
            }

            PurchaseTime = new DateTime(1970, 1, 1).AddMilliseconds(double.Parse(rawPurchase.PurchaseTime));
            PurchaseToken = rawPurchase.PurchaseToken;
            Quantity = rawPurchase.Quantity;
            Signature = rawPurchase.Signature;
            IsAcknowledged = rawPurchase.IsAcknowledged;
            IsAutoRenewing = rawPurchase.IsAutoRenewing;
        }
    }

    /// <summary>
    /// Struct representing an in-app billing purchase history record.
    /// </summary>
    public struct PurchaseHistoryRecord
    {
        /// <summary>
        /// The payload specified when the purchase was acknowledged or consumed.
        /// </summary>
        public string DeveloperPayload { get; private set; }

        /// <summary>
        /// The JSON string which contains details about the purchase order.
        /// </summary>
        public string OriginalJSON { get; private set; }

        /// <summary>
        /// The product IDs of the purchase.
        /// </summary>
        public string[] ProductIDs { get; private set; }

        /// <summary>
        /// The time the product was purchased.
        /// </summary>
        public DateTime PurchaseTime { get; private set; }

        /// <summary>
        /// The token which uniquely identifies a purchase for a given item and user pair.
        /// </summary>
        public string PurchaseToken { get; private set; }

        /// <summary>
        /// The quantity of the purchased product.
        /// </summary>
        public int Quantity { get; private set; }

        /// <summary>
        /// The signature of the purchase data that was signed with your private key.
        /// </summary>
        public string Signature { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PurchaseHistoryRecord"/> object from a <see cref="PurchaseHistoryRecordRaw"/> object.
        /// </summary>
        /// <param name="rawPurchaseHistoryRecord">The <see cref="PurchaseHistoryRecordRaw"/> object used to create a new <see cref="PurchaseHistoryRecord"/> object.</param>
        public PurchaseHistoryRecord(PurchaseHistoryRecordRaw rawPurchaseHistoryRecord)
        {
            DeveloperPayload = rawPurchaseHistoryRecord.DeveloperPayload;
            OriginalJSON = rawPurchaseHistoryRecord.OriginalJSON;
            ProductIDs = rawPurchaseHistoryRecord.ProductIDs;
            PurchaseTime = new DateTime(1970, 1, 1).AddMilliseconds(double.Parse(rawPurchaseHistoryRecord.PurchaseTime));
            PurchaseToken = rawPurchaseHistoryRecord.PurchaseToken;
            Quantity = rawPurchaseHistoryRecord.Quantity;
            Signature = rawPurchaseHistoryRecord.Signature;
        }
    }

    /// <summary>
    /// Struct representing the details of a product, as defined in Google Play Console.
    /// </summary
    public struct ProductDetails
    {
        /// <summary>
        /// The description of the product.
        /// </summary>
        public string Description { get; private set; }

        /// <summary>
        /// The name of the product. Similar to <see cref="Title"/> but does not include the name of the app which owns the product.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The offer details of a one-time purchase for a non-subscription product. <see langword="null"/> for subscription products.
        /// </summary>
        public OneTimePurchaseOfferDetails? OneTimePurchaseOfferDetails { get; private set; }

        /// <summary>
        /// The ID of the product.
        /// </summary>
        public string ProductID { get; private set; }

        /// <summary>
        /// The type of the product.
        /// </summary>
        public ProductType ProductType { get; private set; }

        /// <summary>
        /// The list containing all available offers and base plans to purchase a subscription product. <see langword="null"/> for non-subscription products.
        /// </summary>
        public SubscriptionOfferDetails[]? SubscriptionOfferDetails { get; private set; }

        /// <summary>
        /// The name of the product, including the name of the app which owns the product.
        /// </summary>
        public string Title { get; private set; }

        /// <summary>
        /// Creates a new <see cref="ProductDetails"/> object from a <see cref="ProductDetailsRaw"/> object.
        /// </summary>
        /// <param name="rawProductDetails">The <see cref="ProductDetailsRaw"/> object used to create a new <see cref="ProductDetails"/> object.</param>
        public ProductDetails(ProductDetailsRaw rawProductDetails)
        {
            Description = rawProductDetails.Description;
            Name = rawProductDetails.Name;
            OneTimePurchaseOfferDetails = rawProductDetails.OneTimePurchaseOfferDetails.HasValue ? new OneTimePurchaseOfferDetails(rawProductDetails.OneTimePurchaseOfferDetails.Value) : null;
            ProductID = rawProductDetails.ProductID;
            ProductType = rawProductDetails.ProductType == Utility.InAppProductTypeString ? ProductType.InApp : ProductType.Subscription;

            if (rawProductDetails.SubscriptionOfferDetails is not null)
            {
                SubscriptionOfferDetails = new SubscriptionOfferDetails[rawProductDetails.SubscriptionOfferDetails.Length];
                for (int i = 0; i < rawProductDetails.SubscriptionOfferDetails.Length; i++)
                    SubscriptionOfferDetails[i] = new SubscriptionOfferDetails(rawProductDetails.SubscriptionOfferDetails[i]);
            }
            else
                SubscriptionOfferDetails = null;

            Title = rawProductDetails.Title;
        }
    }

    /// <summary>
    /// Struct representing details to buy a one-time purchase product.
    /// </summary>
    public struct OneTimePurchaseOfferDetails
    {
        /// <summary>
        /// The formatted price for the payment, including it's currency sign.
        /// </summary>
        public string FormattedPrice { get; private set; }

        /// <summary>
        /// The price for the payment in micro-units, where 1,000,000 micro-units equal one unit of the currency.
        /// </summary>
        public long PriceAmountMicros { get; private set; }

        /// <summary>
        /// The ISO 4217 currency code for the price.
        /// </summary>
        public string PriceCurrencyCode { get; private set; }

        /// <summary>
        /// Creates a new <see cref="OneTimePurchaseOfferDetails"/> object from a <see cref="OneTimePurchaseOfferDetailsRaw"/> object.
        /// </summary>
        /// <param name="rawOneTimePurchaseOfferDetails">The <see cref="OneTimePurchaseOfferDetailsRaw"/> object used to create a new <see cref="OneTimePurchaseOfferDetails"/> object.</param>
        public OneTimePurchaseOfferDetails(OneTimePurchaseOfferDetailsRaw rawOneTimePurchaseOfferDetails)
        {
            FormattedPrice = rawOneTimePurchaseOfferDetails.FormattedPrice;
            PriceAmountMicros = long.Parse(rawOneTimePurchaseOfferDetails.PriceAmountMicros);
            PriceCurrencyCode = rawOneTimePurchaseOfferDetails.PriceCurrencyCode;
        }
    }

    /// <summary>
    /// Struct representing the available offers/base plans to buy a subscription product.
    /// </summary>
    public struct SubscriptionOfferDetails
    {
        /// <summary>
        /// The base plan ID associated with the subscription product.
        /// </summary>
        public string BasePlanID { get; private set; }

        /// <summary>
        /// The offer ID associated with the subscription product. <see langword="null"/> for a regular base plan.
        /// </summary>
        public string OfferID { get; private set; }

        /// <summary>
        /// The tags associated with the offer.
        /// </summary>
        public string[] OfferTags { get; private set; }

        /// <summary>
        /// The token used to purchase the subscription product with the offer/base plan.
        /// </summary>
        public string OfferToken { get; private set; }

        /// <summary>
        /// The pricing phases for the offer/base plan.
        /// </summary>
        public PricingPhase[] PricingPhases { get; private set; }

        /// <summary>
        /// Creates a new <see cref="SubscriptionOfferDetails"/> object from a <see cref="SubscriptionOfferDetailsRaw"/> object.
        /// </summary>
        /// <param name="rawSubscriptionOfferDetails">The <see cref="SubscriptionOfferDetailsRaw"/> object used to create a new <see cref="SubscriptionOfferDetails"/> object.</param>
        public SubscriptionOfferDetails(SubscriptionOfferDetailsRaw rawSubscriptionOfferDetails)
        {
            BasePlanID = rawSubscriptionOfferDetails.BasePlanID;
            OfferID = rawSubscriptionOfferDetails.OfferID;
            OfferTags = rawSubscriptionOfferDetails.OfferTags;
            OfferToken = rawSubscriptionOfferDetails.OfferToken;

            PricingPhases = new PricingPhase[rawSubscriptionOfferDetails.PricingPhases.Length];
            for (int i = 0; i < rawSubscriptionOfferDetails.PricingPhases.Length; i++)
                PricingPhases[i] = new PricingPhase(rawSubscriptionOfferDetails.PricingPhases[i]);
        }
    }

    /// <summary>
    /// Struct representing the user's account identifiers that are specified when a purchase is made.
    /// </summary>
    [Serializable]
    public struct AccountIdentifiers
    {
        /// <summary>
        /// The user's obfuscated account ID. Used for Google Play's fraud detection system. Make sure to obfuscate the ID by, for example, hashing it.
        /// </summary>
        public string ObfuscatedAccountID;

        /// <summary>
        /// The user's obfuscated profile ID. Used for Google Play's fraud detection system. Make sure to obfuscate the ID by, for example, hashing it.
        /// </summary>
        public string ObfuscatedProfileID;

        /// <summary>
        /// Creates a new <see cref="AccountIdentifiers"/> object.
        /// </summary>
        /// <param name="obfuscatedAccountID">The user's obfuscated account ID. Used for Google Play's fraud detection system. Make sure to obfuscate the ID by, for example, hashing it.</param>
        /// <param name="obfuscatedProfileID">The user's obfuscated profile ID. Used for Google Play's fraud detection system. Make sure to obfuscate the ID by, for example, hashing it.</param>
        public AccountIdentifiers(string obfuscatedAccountID, string obfuscatedProfileID)
        {
            ObfuscatedAccountID = obfuscatedAccountID;
            ObfuscatedProfileID = obfuscatedProfileID;
        }
    }

    /// <summary>
    /// Struct representing a pricing phase, describing how the user pays at a point in time.
    /// </summary>
    public struct PricingPhase
    {
        /// <summary>
        /// The number of cycles for which the billing period is applied.
        /// </summary>
        public int BillingCycleCount { get; private set; }

        /// <summary>
        /// The billing period for which the given price applies, specified in ISO 8601 format.
        /// </summary>
        public string BillingPeriod { get; private set; }

        /// <summary>
        /// The formatted price for the payment cycle, including it's currency sign.
        /// </summary>
        public string FormattedPrice { get; private set; }

        /// <summary>
        /// The price for the payment cycle in micro-units, where 1,000,000 micro-units equal one unit of the currency.
        /// </summary>
        public long PriceAmountMicros { get; private set; }

        /// <summary>
        /// The ISO 4217 currency code for the price.
        /// </summary>
        public string PriceCurrencyCode { get; private set; }

        /// <summary>
        /// The recurrence mode of the pricing phase.
        /// </summary>
        public RecurrenceMode RecurrenceMode { get; private set; }

        /// <summary>
        /// Creates a new <see cref="PricingPhase"/> object from a <see cref="PricingPhaseRaw"/> object.
        /// </summary>
        /// <param name="rawPricingPhase">The <see cref="PricingPhaseRaw"/> object used to create a new <see cref="PricingPhase"/> object.</param>
        public PricingPhase(PricingPhaseRaw rawPricingPhase)
        {
            BillingCycleCount = rawPricingPhase.BillingCycleCount;
            BillingPeriod = rawPricingPhase.BillingPeriod;
            FormattedPrice = rawPricingPhase.FormattedPrice;
            PriceAmountMicros = long.Parse(rawPricingPhase.PriceAmountMicros);
            PriceCurrencyCode = rawPricingPhase.PriceCurrencyCode;

            switch (rawPricingPhase.RecurrenceMode)
            {
                case 1:
                    RecurrenceMode = RecurrenceMode.Infinite;
                    break;
                case 2:
                    RecurrenceMode = RecurrenceMode.Finite;
                    break;
                default:
                    RecurrenceMode = RecurrenceMode.None;
                    break;
            }
        }
    }

    /// <summary>
    /// Struct representing the response to a consume purchase call.
    /// </summary>
    public struct ConsumePurchaseResponse
    {
        /// <summary>
        /// The purchase token of the response.
        /// </summary>
        public string PurchaseToken;

        /// <summary>
        /// The status code of the response.
        /// </summary>
        public int StatusCode;

        /// <summary>
        /// Whether the operation was successful or not.
        /// </summary>
        public bool IsSuccessful;

        /// <summary>
        /// Creates a new <see cref="ConsumePurchaseResponse"/> object from a <see cref="ConsumePurchaseResponseRaw"/> object.
        /// </summary>
        /// <param name="rawConsumePurchaseResponse">The <see cref="ConsumePurchaseResponseRaw"/> object used to create a new <see cref="ConsumePurchaseResponse"/> object.</param>
        public ConsumePurchaseResponse(ConsumePurchaseResponseRaw rawConsumePurchaseResponse)
        {
            PurchaseToken = rawConsumePurchaseResponse.PurchaseToken;
            StatusCode = rawConsumePurchaseResponse.ResponseCode;
            IsSuccessful = rawConsumePurchaseResponse.ResponseCode == 0;
        }
    }

    /// <summary>
    /// Struct representing the response to an acknowledge purchase call.
    /// </summary>
    public struct AcknowledgePurchaseResponse
    {
        /// <summary>
        /// The status code of the response.
        /// </summary>
        public int StatusCode;

        /// <summary>
        /// Whether the operation was successful or not.
        /// </summary>
        public bool IsSuccessful;

        /// <summary>
        /// Creates a new <see cref="AcknowledgePurchaseResponse"/> object from an <see langword="int"/> status code.
        /// </summary>
        /// <param name="statusCode">The <see langword="int"/> status code used to create a new <see cref="AcknowledgePurchaseResponse"/> object.</param>
        public AcknowledgePurchaseResponse(int statusCode)
        {
            StatusCode = statusCode;
            IsSuccessful = statusCode == 0;
        }
    }

    #endregion

    #region Raw data classes

    /// <summary>
    /// Struct representing an in-app billing purchase, converted as-is from JSON.
    /// </summary>
    [Serializable]
    public struct PurchaseRaw
    {
        /// <summary>
        /// The account identifiers associated with the purchase.
        /// </summary>
        public AccountIdentifiers? AccountIdentifiers;

        /// <summary>
        /// The payload specified when the purchase was acknowledged or consumed.
        /// </summary>
        public string DeveloperPayload;

        /// <summary>
        /// The unique order identifier for the transaction.
        /// </summary>
        public string OrderID;

        /// <summary>
        /// The JSON string which contains details about the purchase order.
        /// </summary>
        public string OriginalJSON;

        /// <summary>
        /// The application package from which the purchase originated.
        /// </summary>
        public string PackageName;

        /// <summary>
        /// The product IDs of the purchase.
        /// </summary>
        public string[] ProductIDs;

        /// <summary>
        /// The state of the purchase.
        /// </summary>
        /// <remarks>
        /// These are the integer status codes:<br/>
        /// 0 -> UNSPECIFIED_STATE<br/>
        /// 1 -> PURCHASED<br/>
        /// 2 -> PENDING<br/>
        /// </remarks>
        public int PurchaseState;

        /// <summary>
        /// The time the product was purchased, in milliseconds since the epoch (Jan 1, 1970).
        /// </summary>
        public string PurchaseTime;

        /// <summary>
        /// The token which uniquely identifies a purchase for a given item and user pair.
        /// </summary>
        public string PurchaseToken;

        /// <summary>
        /// The the quantity of the purchased product.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// The signature of the purchase data that was signed with your private key.
        /// </summary>
        public string Signature;

        /// <summary>
        /// Indicates whether the purchase has been acknowledged (by you).
        /// </summary>
        public bool IsAcknowledged;

        /// <summary>
        /// Indicates whether the subscription renews automatically. Only for purchases of subscription products!
        /// </summary>
        public bool IsAutoRenewing;
    }

    /// <summary>
    /// Struct representing an in-app billing purchase history record, converted as-is from JSON.
    /// </summary>
    [Serializable]
    public struct PurchaseHistoryRecordRaw
    {
        /// <summary>
        /// The payload specified when the purchase was acknowledged or consumed.
        /// </summary>
        public string DeveloperPayload;

        /// <summary>
        /// The JSON string which contains details about the purchase order.
        /// </summary>
        public string OriginalJSON;

        /// <summary>
        /// The product IDs of the purchase.
        /// </summary>
        public string[] ProductIDs;

        /// <summary>
        /// The time the product was purchased, in milliseconds since the epoch (Jan 1, 1970).
        /// </summary>
        public string PurchaseTime;

        /// <summary>
        /// The token which uniquely identifies a purchase for a given item and user pair.
        /// </summary>
        public string PurchaseToken;

        /// <summary>
        /// The quantity of the purchased product.
        /// </summary>
        public int Quantity;

        /// <summary>
        /// The signature of the purchase data that was signed with your private key.
        /// </summary>
        public string Signature;
    }

    /// <summary>
    /// Struct representing offer details to buy a one-time purchase product, converted as-is from JSON.
    /// </summary>
    [Serializable]
    public struct OneTimePurchaseOfferDetailsRaw
    {
        /// <summary>
        /// The formatted price for the payment, including its currency sign.
        /// </summary>
        public string FormattedPrice;

        /// <summary>
        /// The price for the payment in micro-units, where 1,000,000 micro-units equal one unit of the currency.
        /// </summary>
        public string PriceAmountMicros;

        /// <summary>
        /// The ISO 4217 currency code for price.
        /// </summary>
        public string PriceCurrencyCode;
    }

    /// <summary>
    /// Struct representing a pricing phase, describing how a user pays at a point in time, converted as-is from JSON.
    /// </summary>
    [Serializable]
    public struct PricingPhaseRaw
    {
        /// <summary>
        /// The number of cycles for which the billing period is applied.
        /// </summary>
        public int BillingCycleCount;

        /// <summary>
        /// The billing period for which the given price applies, specified in ISO 8601 format.
        /// </summary>
        public string BillingPeriod;

        /// <summary>
        /// The formatted price for the payment cycle, including its currency sign.
        /// </summary>
        public string FormattedPrice;

        /// <summary>
        /// The price for the payment cycle in micro-units, where 1,000,000 micro-units equal one unit of the currency.
        /// </summary>
        public string PriceAmountMicros;

        /// <summary>
        /// The ISO 4217 currency code for price.
        /// </summary>
        public string PriceCurrencyCode;

        /// <summary>
        /// The recurrence mode of the pricing phase.
        /// </summary>
        /// <remarks>
        /// These are the integer codes for each recurrence mode:<br/>
        /// 1 -> INFINITE_RECURRING<br/>
        /// 2 -> FINITE_RECURRING<br/>
        /// 3 -> NON_RECURRING<br/>
        /// </remarks>
        public int RecurrenceMode;
    }

    /// <summary>
    /// Struct representing the available purchase plans to buy a subscription product, converted as-is from JSON.
    /// </summary>
    [Serializable]
    public struct SubscriptionOfferDetailsRaw
    {
        /// <summary>
        /// The base plan id associated with the subscription product.
        /// </summary>
        public string BasePlanID;

        /// <summary>
        /// The offer id associated with the subscription product. <see langword="null"/> for a regular base plan.
        /// </summary>
        public string OfferID;

        /// <summary>
        /// The offer tags associated with this Subscription Offer.
        /// </summary>
        public string[] OfferTags;

        /// <summary>
        /// The offer token used to purchase the subscription product with these pricing phases.
        /// </summary>
        public string OfferToken;

        /// <summary>
        /// The pricing phases for the subscription product.
        /// </summary>
        public PricingPhaseRaw[] PricingPhases;
    }

    /// <summary>
    /// Struct representing the details of a product, as defined in Google Play Console, converted as-is from JSON.
    /// </summary>
    [Serializable]
    public struct ProductDetailsRaw
    {
        /// <summary>
        /// The description of the product.
        /// </summary>
        public string Description;

        /// <summary>
        /// The name of the product. Similar to <see cref="Title"/> but does not include the name of the app which owns the product.
        /// </summary>
        public string Name;

        /// <summary>
        /// The offer details of a one-time purchase for a non-subscription product. <see langword="null"/> for subscription products.
        /// </summary>
        public OneTimePurchaseOfferDetailsRaw? OneTimePurchaseOfferDetails;

        /// <summary>
        /// The product's ID.
        /// </summary>
        public string ProductID;

        /// <summary>
        /// The product type of the product.
        /// </summary>
        public string ProductType;

        /// <summary>
        /// The list containing all available offers to purchase a subscription product. <see langword="null"/> for non-subscription products.
        /// </summary>
        public SubscriptionOfferDetailsRaw[]? SubscriptionOfferDetails;

        /// <summary>
        /// The name of the product, including the name of the app which owns the product.
        /// </summary>
        public string Title;
    }

    /// <summary>
    /// Struct representing consume purchase response, converted as-is from JSON.
    /// </summary>
    [Serializable]
    public struct ConsumePurchaseResponseRaw
    {
        /// <summary>
        /// The response code of the consume purchase response.
        /// </summary>
        public int ResponseCode;

        /// <summary>
        /// The purchase token of the consume purchase response.
        /// </summary>
        public string PurchaseToken;
    }

    #endregion

    #region Utility class

    /// <summary>
    /// Static class for extension methods and utility constants.
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Constant value for a subscription product type.
        /// </summary>
        public const string SubscriptionProductTypeString = "subs";

        /// <summary>
        /// Constant value for an in-app/non-subscription product type.
        /// </summary>
        public const string InAppProductTypeString = "inapp";

        /// <summary>
        /// Converts a ProductType enum to it's string representation.
        /// </summary>
        /// <param name="productType">The <see cref="ProductType"/> to convert.</param>
        /// <returns>The string representation of the <see cref="ProductType"/>.</returns>
        public static string ConvertToString(this ProductType productType) => (productType == ProductType.InApp) ? InAppProductTypeString : SubscriptionProductTypeString;
    }

    #endregion
}
