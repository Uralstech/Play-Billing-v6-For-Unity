using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Events;
using Newtonsoft.Json;

#nullable enable

namespace Uralstech.PlayBillingForUnity
{
    using Data;

    /// <summary>
    /// Class to interface with the PlayBillingForUnity Android native Kotlin plugin.
    /// </summary>
    public class PlayBillingManager : MonoBehaviour
    {
        private enum UpdateStatus { Empty, Success, Fail, Waiting }

        /// <summary>
        /// The current active instance of <see cref="PlayBillingManager"/>.
        /// </summary>
        public static PlayBillingManager Instance { get; private set; }

        /// <summary>
        /// Event for when the status of the billing client is updated, including the status.
        /// </summary>
        public UnityEvent<BillingClientStatus> OnBillingClientStatusUpdate = new UnityEvent<BillingClientStatus>();

        /// <summary>
        /// Event for when a purchase fails, including the reason.
        /// </summary>
        public UnityEvent<PurchaseFailureReason> OnPurchaseFailed = new UnityEvent<PurchaseFailureReason>();

        /// <summary>
        /// Event for when a purchase needs to be handled, including the <see cref="Purchase"/> object.
        /// </summary>
        public UnityEvent<Purchase> HandlePurchase = new UnityEvent<Purchase>();

        /// <summary>
        /// Event for when the result of a <see cref="ConsumePurchase"/> call has been received, including the <see cref="ConsumePurchaseResponse"/> object.
        /// </summary>
        public UnityEvent<ConsumePurchaseResponse> OnConsumePurchaseResult = new UnityEvent<ConsumePurchaseResponse>();

        /// <summary>
        /// Event for when the result of a <see cref="AcknowledgePurchase"/> call has been received, including the <see cref="AcknowledgePurchaseResponse"/> object.
        /// </summary>
        public UnityEvent<AcknowledgePurchaseResponse> OnAcknowledgePurchaseResult = new UnityEvent<AcknowledgePurchaseResponse>();

        private bool _cacheInitialized = false;

        private AndroidJavaObject _playBilling;

        private (ProductDetails[], UpdateStatus) _productDetails = new (new ProductDetails[0], UpdateStatus.Empty);
        private (Purchase[], UpdateStatus) _purchases = new (new Purchase[0], UpdateStatus.Empty);
        private (PurchaseHistoryRecord[], UpdateStatus) _purchaseHistory = new (new PurchaseHistoryRecord[0], UpdateStatus.Empty);
        private bool _calledGetProductDetails = false;
        private bool _calledGetUserPurchases = false;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            DontDestroyOnLoad(gameObject);
            using (var unityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
            {
                AndroidJavaObject activity = unityPlayer.GetStatic<AndroidJavaObject>("currentActivity");
                _playBilling = new AndroidJavaClass("com.uralstech.playbillingforunity.PlayBilling").CallStatic<AndroidJavaObject>("getInstance", activity);
            }

            _playBilling.Call("changeCallbackReceiver", gameObject.name);
        }

        /// <summary>
        /// Sets up the billing client with the provided products and (optional) purchase validation key.
        /// </summary>
        /// <param name="products">The products to be bought by the user.</param>
        /// <param name="applicationBase64PublicKey">The base64 purchase validation key. Optional, but required for purchase validation.</param>
        public void SetupBillingClient(Product[] products, string? applicationBase64PublicKey = null)
        {
            string[] productIDs = new string[products.Length];
            string[] productTypesAsString = new string[products.Length];
            for (int i = 0; i < products.Length; i++)
            {
                productIDs[i] = products[i].ID;
                productTypesAsString[i] = products[i].ProductType.ConvertToString();
            }

            _playBilling.Call("setupBillingClient", productIDs, productTypesAsString, applicationBase64PublicKey);
        }

        /// <summary>
        /// Sets up Google Play fraud detection with the provided user account identifiers. Google Play can use the identifiers to detect irregular activity, such<br/>
        /// as many devices making purchases on the same account in a short period of time. Do not include any Personally Identifiable Information (PII)<br/>
        /// such as emails in cleartext. Attempting to include PII will result in purchases being blocked. Google Play recommends that you use either encryption<br/>
        /// or a one-way hash to generate an obfuscated identifier to send to Google Play.
        /// </summary>
        /// <param name="identifiers">The obfuscated account identifiers of the user.</param>
        public void SetupGooglePlayFraudDetection(AccountIdentifiers identifiers) => _playBilling.Call("setupGooglePlayFraudDetection", identifiers.ObfuscatedAccountID, identifiers.ObfuscatedProfileID);

        /// <summary>
        /// Gets all products' details asynchronously.
        /// </summary>
        /// <returns>The retrieved product details (as an array of <see cref="ProductDetails"/> objects) and whether the operation was successful (as a <see langword="bool"/>).</returns>
        public async Task<(ProductDetails[], bool)> GetProductDetailsAsync()
        {
            if (!_cacheInitialized)
            {
                while (!_cacheInitialized)
                    await Task.Yield();
            }

            Debug.Log("GetProductDetailsAsync called.");
            if (_productDetails.Item2 == UpdateStatus.Success)
                return (_productDetails.Item1, true);
            else
            {
                string[] result = _playBilling.Call<string[]>("getProductDetails");
                string[] productDetailsJSON = result[1..];
                string status = result[0];

                if (status == "SUCCESS")
                {
                    _productDetails.Item1 = new ProductDetails[productDetailsJSON.Length];
                    for (int i = 0; i < productDetailsJSON.Length; i++)
                        _productDetails.Item1[i] = new ProductDetails(JsonConvert.DeserializeObject<ProductDetailsRaw>(productDetailsJSON[i]));

                    _productDetails.Item2 = UpdateStatus.Success;
                    return (_productDetails.Item1, true);
                }
                else
                {
                    _productDetails.Item2 = UpdateStatus.Empty;
                    while (_productDetails.Item2 == UpdateStatus.Empty)
                        await Task.Yield();

                    if (_productDetails.Item2 == UpdateStatus.Waiting)
                        return await GetProductDetailsAsync();
                    return (new ProductDetails[0], false);
                }
            }
        }

        /// <summary>
        /// Gets the purchase details for currently owned items bought within your app, asynchronously. Only active subscriptions and non-consumed one-time purchases are returned.
        /// </summary>
        /// <param name="refresh">Whether to refresh the purchase list.</param>
        /// <returns>The retrieved purchases (as an array of <see cref="Purchase"/> objects) and whether the operation was successful (as a <see langword="bool"/>).</returns>
        public async Task<(Purchase[], bool)> GetUserPurchasesAsync(bool refresh = false)
        {
            if (!_cacheInitialized)
            {
                while (!_cacheInitialized)
                    await Task.Yield();
            }

            Debug.Log("GetUserPurchasesAsync called!");
            if (refresh)
                _purchases.Item2 = UpdateStatus.Empty;

            if (_purchases.Item2 == UpdateStatus.Success)
                return (_purchases.Item1, true);
            else
            {
                string[] result = _playBilling.Call<string[]>("getUserPurchases", refresh);
                string[] purchasesJSON = result[1..];
                string status = result[0];

                if (status == "SUCCESS")
                {
                    _purchases.Item1 = new Purchase[purchasesJSON.Length];
                    for (int i = 0; i < purchasesJSON.Length; i++)
                        _purchases.Item1[i] = new Purchase(JsonConvert.DeserializeObject<PurchaseRaw>(purchasesJSON[i]));

                    _purchases.Item2 = UpdateStatus.Success;
                    return (_purchases.Item1, true);
                }
                else
                {
                    _purchases.Item2 = UpdateStatus.Empty;
                    while (_purchases.Item2 == UpdateStatus.Empty)
                        await Task.Yield();

                    if (_purchases.Item2 == UpdateStatus.Waiting)
                        return await GetUserPurchasesAsync();
                    return (new Purchase[0], false);
                }
            }
        }

        /// <summary>
        /// Gets the most recent purchase made by the user for each product, even if that purchase is expired, canceled, or consumed, asynchronously.
        /// </summary>
        /// <param name="refresh">Whether to refresh the purchase list.</param>
        /// <returns>The retrieved purchases (as an array of <see cref="PurchaseHistoryRecord"/> objects) and whether the operation was successful (as a <see langword="bool"/>).</returns>
        public async Task<(PurchaseHistoryRecord[], bool)> GetUserPurchaseHistoryAsync(bool refresh = false)
        {
            Debug.Log("GetUserPurchaseHistoryAsync called!");

            if (refresh)
                _purchaseHistory.Item2 = UpdateStatus.Empty;

            if (_purchaseHistory.Item2 == UpdateStatus.Success)
                return (_purchaseHistory.Item1, true);
            else
            {
                string[] result = _playBilling.Call<string[]>("getUserPurchaseHistory", refresh);
                string[] purchaseHistoryJSON = result[1..];
                string status = result[0];

                if (status == "SUCCESS")
                {
                    _purchaseHistory.Item1 = new PurchaseHistoryRecord[purchaseHistoryJSON.Length];
                    for (int i = 0; i < purchaseHistoryJSON.Length; i++)
                        _purchaseHistory.Item1[i] = new PurchaseHistoryRecord(JsonConvert.DeserializeObject<PurchaseHistoryRecordRaw>(purchaseHistoryJSON[i]));

                    _purchaseHistory.Item2 = UpdateStatus.Success;
                    return (_purchaseHistory.Item1, true);
                }
                else
                {
                    _purchaseHistory.Item2 = UpdateStatus.Empty;
                    while (_purchaseHistory.Item2 == UpdateStatus.Empty)
                        await Task.Yield();

                    if (_purchaseHistory.Item2 == UpdateStatus.Waiting)
                        return await GetUserPurchaseHistoryAsync();
                    return (new PurchaseHistoryRecord[0], false);
                }
            }
        }

        /// <summary>
        /// Initiates the purchase of a product.
        /// </summary>
        /// <param name="product">The product to purchase.</param>
        /// <param name="subscriptionDetails">The additional details for the product (only for subscription type products).</param>
        public void PurchaseProduct(Product product, SubscriptionOfferDetails? subscriptionDetails = null) => _playBilling.Call("purchaseProduct", product.ID, subscriptionDetails?.BasePlanID, subscriptionDetails?.OfferID);

        /// <summary>
        /// Initiates the purchase of a product.
        /// </summary>
        /// <param name="product">The product to purchase.</param>
        /// <param name="subscriptionDetails">The additional details for the product (only for subscription type products).</param>
        public void PurchaseProduct(ProductDetails product, SubscriptionOfferDetails? subscriptionDetails = null) => _playBilling.Call("purchaseProduct", product.ProductID, subscriptionDetails?.BasePlanID, subscriptionDetails?.OfferID);

        /// <summary>
        /// Checks the validity of a purchase.
        /// </summary>
        /// <param name="purchase">The purchase to check the validity of.</param>
        /// <returns>The validity of the purchase.</returns>
        public PurchaseValidity CheckPurchaseValidity(Purchase purchase)
        {
            switch (_playBilling.Call<string>("checkPurchaseValidity", purchase.OriginalJSON, purchase.Signature))
            {
                case "VALID":
                    return PurchaseValidity.Valid;
                case "KEY_NOT_FOUND":
                    return PurchaseValidity.KeyNotFound;
                default:
                    return PurchaseValidity.Failed;
            }
        }

        /// <summary>
        /// Checks the validity of a purchase.
        /// </summary>
        /// <param name="purchase">The purchase to check the validity of.</param>
        /// <returns>The validity of the purchase.</returns>
        public PurchaseValidity CheckPurchaseValidity(PurchaseHistoryRecord purchase)
        {
            switch (_playBilling.Call<string>("checkPurchaseValidity", purchase.OriginalJSON, purchase.Signature))
            {
                case "VALID":
                    return PurchaseValidity.Valid;
                case "KEY_NOT_FOUND":
                    return PurchaseValidity.KeyNotFound;
                default:
                    return PurchaseValidity.Failed;
            }
        }

        /// <summary>
        /// Acknowledges a purchase.
        /// </summary>
        /// <param name="purchase">The purchase to acknowledge.</param>
        public void AcknowledgePurchase(Purchase purchase) => _playBilling.Call("acknowledgePurchase", purchase.PurchaseToken);

        /// <summary>
        /// Acknowledges a purchase.
        /// </summary>
        /// <param name="purchase">The purchase to acknowledge.</param>
        public void AcknowledgePurchase(PurchaseHistoryRecord purchase) => _playBilling.Call("acknowledgePurchase", purchase.PurchaseToken);

        /// <summary>
        /// Consumes the purchase of a consumable product.
        /// </summary>
        /// <param name="purchase">The purchase to consume.</param>
        public void ConsumePurchase(Purchase purchase) => _playBilling.Call("consumePurchase", purchase.PurchaseToken);

        /// <summary>
        /// Consumes the purchase of a consumable product.
        /// </summary>
        /// <param name="purchase">The purchase to consume.</param>
        public void ConsumePurchase(PurchaseHistoryRecord purchase) => _playBilling.Call("consumePurchase", purchase.PurchaseToken);

        // Callbacks from Kotlin
        private void UpdateCacheInitialized() => _cacheInitialized = _calledGetProductDetails && _calledGetUserPurchases;

        public void _OnBillingClientStatusUpdate(string status)
        {
            BillingClientStatus billingClientStatus = BillingClientStatus.Error;
            switch (status)
            {
                case "CONNECTED":
                    billingClientStatus = BillingClientStatus.Connected;
                    break;
                case "-2":
                    billingClientStatus = BillingClientStatus.FeatureNotSupported;
                    break;
                case "-1":
                case "DISCONNECTED":
                    billingClientStatus = BillingClientStatus.ServiceDisconnected;
                    break;
                case "2":
                    billingClientStatus = BillingClientStatus.ServiceUnavailable;
                    break;
                case "3":
                    billingClientStatus = BillingClientStatus.BillingUnavailable;
                    break;
                case "5":
                    billingClientStatus = BillingClientStatus.DeveloperError;
                    break;
                case "12":
                    billingClientStatus = BillingClientStatus.NetworkError;
                    break;
            }

            Debug.Log($"Billing setup status update: {billingClientStatus}");
            OnBillingClientStatusUpdate?.Invoke(billingClientStatus);
        }

        public void _OnGetProductDetailsStatusUpdate(string status)
        {
            Debug.Log($"Got product details status: {status}");
            if (status == "SUCCESS")
            {
                _calledGetProductDetails = true;
                _productDetails.Item2 = UpdateStatus.Waiting;
                UpdateCacheInitialized();
            }
            else if (status != "SUCCESS_INAPP" && status != "SUCCESS_SUBS")
            {
                _calledGetProductDetails = true;
                _productDetails.Item2 = UpdateStatus.Fail;
                UpdateCacheInitialized();
            }
        }

        public void _OnGetUserPurchasesStatusUpdate(string status)
        {
            Debug.Log($"Got user purchases status: {status}");
            UpdateCacheInitialized();

            if (status == "SUCCESS")
            {
                _calledGetUserPurchases = true;
                _purchases.Item2 = UpdateStatus.Waiting;
                UpdateCacheInitialized();
            }
            else if (status != "SUCCESS_INAPP" && status != "SUCCESS_SUBS")
            {
                _calledGetUserPurchases = true;
                _purchases.Item2 = UpdateStatus.Fail;
                UpdateCacheInitialized();
            }
        }

        public void _OnGetUserPurchaseHistoryStatusUpdate(string status)
        {
            Debug.Log($"Got user purchase history status: {status}");
            if (status == "SUCCESS")
                _purchaseHistory.Item2 = UpdateStatus.Waiting;
            else if (status != "SUCCESS_INAPP" && status != "SUCCESS_SUBS")
                _purchaseHistory.Item2 = UpdateStatus.Fail;
        }

        public void _HandlePurchase(string purchaseJSON)
        {
            Debug.Log("Handling successful purchase.");
            HandlePurchase?.Invoke(new Purchase(JsonConvert.DeserializeObject<PurchaseRaw>(purchaseJSON)));
        }

        public void _OnGotConsumePurchaseResult(string responseJSON)
        {
            Debug.Log("Got consume purchase response!");
            OnConsumePurchaseResult?.Invoke(new ConsumePurchaseResponse(JsonConvert.DeserializeObject<ConsumePurchaseResponseRaw>(responseJSON)));
        }

        public void _OnGotAcknowledgePurchaseResult(string status)
        {
            Debug.Log($"Got acknowledge purchase response: {status}");
            OnAcknowledgePurchaseResult?.Invoke(new AcknowledgePurchaseResponse(int.Parse(status)));
        }

        public void _OnPurchaseFailed(string reason)
        {
            PurchaseFailureReason purchaseFailureReason = PurchaseFailureReason.Error;
            switch (reason)
            {
                case "PRODUCT_DETAILS_NOT_SET":
                    purchaseFailureReason = PurchaseFailureReason.ProductDetailsNotSet;
                    break;
                case "SUBS_PLAN_FOR_INAPP":
                    purchaseFailureReason = PurchaseFailureReason.SubscriptionDetailsGivenForInAppProduct;
                    break;
                case "SUBS_PLAN_NULL":
                    purchaseFailureReason = PurchaseFailureReason.SubscriptionDetailsNotGiven;
                    break;
                case "OFFER_OR_PLAN_NOT_FOUND":
                    purchaseFailureReason = PurchaseFailureReason.SubscriptionOfferOrPlanNotFound;
                    break;
                case "PRODUCT_DETAILS_NOT_FOUND":
                    purchaseFailureReason = PurchaseFailureReason.ProductDetailsNotFound;
                    break;
                case "PRODUCT_NOT_DEFINED":
                    purchaseFailureReason = PurchaseFailureReason.ProductNotDefined;
                    break;
                case "BILLING_NOT_READY":
                    purchaseFailureReason = PurchaseFailureReason.BilingClientNotReady;
                    break;
                case "-2":
                    purchaseFailureReason = PurchaseFailureReason.FeatureNotSupported;
                    break;
                case "-1":
                    purchaseFailureReason = PurchaseFailureReason.ServiceDisconnected;
                    break;
                case "1":
                    purchaseFailureReason = PurchaseFailureReason.UserCancelled;
                    break;
                case "2":
                    purchaseFailureReason = PurchaseFailureReason.ServiceUnavailable;
                    break;
                case "3":
                    purchaseFailureReason = PurchaseFailureReason.BillingUnavailable;
                    break;
                case "4":
                    purchaseFailureReason = PurchaseFailureReason.ItemUnavailable;
                    break;
                case "5":
                    purchaseFailureReason = PurchaseFailureReason.DeveloperError;
                    break;
                case "7":
                    purchaseFailureReason = PurchaseFailureReason.ItemAlreadyOwned;
                    break;
                case "8":
                    purchaseFailureReason = PurchaseFailureReason.ItemNotOwned;
                    break;
                case "12":
                    purchaseFailureReason = PurchaseFailureReason.NetworkError;
                    break;
            }

            Debug.Log($"Purchase failed: {purchaseFailureReason}");
            OnPurchaseFailed?.Invoke(purchaseFailureReason);
        }
    }
}