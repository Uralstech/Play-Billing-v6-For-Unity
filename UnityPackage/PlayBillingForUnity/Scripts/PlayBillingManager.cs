using System.Linq;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace Uralstech.PlayBillingForUnity
{
    public enum ProductType
    {
        InAppProduct,
        Subscription
    }

    public class PlayBillingManager : MonoBehaviour
    {
        public static PlayBillingManager Instance { get; private set; }

        public UnityEvent OnBillingSetupSuccessCallback { get; private set; } = new UnityEvent();
        public UnityEvent<string> OnBillingSetupFailedCallback { get; private set; } = new UnityEvent<string>();
        public UnityEvent OnBillingServiceDisconnectedCallback { get; private set; } = new UnityEvent();
        public UnityEvent HandlePurchaseCallback { get; private set; } = new UnityEvent();
        public UnityEvent<string> OnPurchaseFailedCallback { get; private set; } = new UnityEvent<string>();
        public UnityEvent<string> OnGotProductDetailsCallback { get; private set; } = new UnityEvent<string>();
        public UnityEvent<string> OnGotSubscriptionDetailsCallback { get; private set; } = new UnityEvent<string>();

        private Dictionary<string, ProductType> _products;
        private AndroidJavaObject _plugin;

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else if (Instance != this)
                Destroy(gameObject);

            Debug.Log("PlayBillingManager.Awake()");

            _plugin = new AndroidJavaClass("com.uralstech.playbillingforunity.PlayBilling").CallStatic<AndroidJavaObject>("getInstance", new AndroidJavaClass("com.unity3d.player.UnityPlayer").GetStatic<AndroidJavaObject>("currentActivity"));
            _plugin.Call("setCallbackReceiver", gameObject.name);
        }

        private string ProductTypeToString(ProductType productType)
        {
            return (productType == ProductType.InAppProduct) ? "inapp" : "subs";
        }

        public void SetupBillingClient(Dictionary<string, ProductType> products)
        {
            Debug.Log("SetupBillingClient()");

            _products = products;
            ProductType[] productsTypes = products.Values.ToArray();
            string[] productsTypesAsString = new string[productsTypes.Length];
            for (int i = 0; i < productsTypes.Length; i++)
                productsTypesAsString[i] = ProductTypeToString(productsTypes[i]);

            _plugin.Call("setupBillingClient", products.Keys.ToArray(), productsTypesAsString);
        }

        public void PurchaseProduct(string productId)
        {
            Debug.Log("PurchaseProduct()");
            if (_products == null || _products.Count == 0)
                throw new System.Exception("SetupBillingClient() must be called before PurchaseProduct()");
            else if (!_products.ContainsKey(productId))
                throw new System.Exception("Product with id '" + productId + "' was not found in PlayBillingManager's dictionary");

            _plugin.Call("purchaseProduct", productId, ProductTypeToString(_products[productId]));
        }

        public void SetupGooglePlayFraudDetection(string obfuscatedAccountID, string obfuscatedProfileID)
        {
            Debug.Log("SetupGooglePlayFraudDetection()");
            _plugin.Call("setupGooglePlayFraudDetection", obfuscatedAccountID, obfuscatedProfileID);
        }

        public void QueryProductDetails()
        {
            Debug.Log("QueryProductDetails()");
            _plugin.Call("queryProductDetails");
        }

        public void QuerySubscriptionDetails()
        {
            Debug.Log("QuerySubscriptionDetails()");
            _plugin.Call("querySubscriptionDetails");
        }

        // Callback methods

        public void OnBillingSetupSuccess(string _)
        {
            Debug.Log("Billing setup successful");
            OnBillingSetupSuccessCallback?.Invoke();
        }

        public void OnBillingSetupFailed(string message)
        {
            Debug.Log("Billing setup failed: " + message);
            OnBillingSetupFailedCallback?.Invoke(message);
        }

        public void OnBillingServiceDisconnected(string _)
        {
            Debug.Log("Billing service disconnected");
            OnBillingServiceDisconnectedCallback?.Invoke();
        }

        public void OnPurchaseFailed(string message)
        {
            Debug.Log("Purchase failed: " + message);
            OnPurchaseFailedCallback?.Invoke(message);
        }

        public void HandlePurchase(string message)
        {
            Debug.Log("Handling purchase: " + message);
            HandlePurchaseCallback?.Invoke();
        }

        public void OnGotProductDetails(string message)
        {
            Debug.Log("Got product details: " + message);
            OnGotProductDetailsCallback?.Invoke(message);
        }

        public void OnGotSubscriptionDetails(string message)
        {
            Debug.Log("Got subscription details: " + message);
            OnGotSubscriptionDetailsCallback?.Invoke(message);
        }
    }
}