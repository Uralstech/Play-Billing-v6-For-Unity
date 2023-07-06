package com.uralstech.playbillingforunity

import android.app.Activity
import android.util.Log
import com.android.billingclient.api.*
import com.unity3d.player.UnityPlayer
import java.lang.ref.WeakReference

// TODO: Verify purchases
// TODO: Void purchases
// TODO: Purchase history
// TODO: New subscription stuff

class PlayBilling(private val activity: Activity) : PurchasesUpdatedListener {
    companion object {
        private const val LOGGING_TAG = "PlayBillingForUnity"

        private var UNITY_RECEIVER = "PlayBillingManager"
        private var INSTANCE: WeakReference<PlayBilling> = WeakReference(null)

        @JvmStatic
        fun getInstance(activity: Activity) : PlayBilling {
            val instance = INSTANCE.get()
            return if (instance != null) {
                instance
            } else {
                val newInstance = PlayBilling(activity)

                INSTANCE.clear()
                INSTANCE = WeakReference(newInstance)
                newInstance
            }
        }
    }

    private val billingClient: BillingClient = BillingClient.newBuilder(activity)
        .setListener(this)
        .enablePendingPurchases()
        .build()

    private val inAppProducts: MutableMap<String,QueryProductDetailsParams.Product> = mutableMapOf()
    private val subscriptionProducts: MutableMap<String,QueryProductDetailsParams.Product> = mutableMapOf()
    private val allProducts : MutableList<String> = mutableListOf()

    private var allInAppProductsDetails: MutableList<ProductDetails> = mutableListOf()
    private var allSubscriptionProductsDetails: MutableList<ProductDetails> = mutableListOf()
    private var hasProductDetails: Boolean = false

    private var allSubscriptionsDetails: MutableList<Purchase> = mutableListOf()

    private var obfuscatedAccountID: String = ""
    private var obfuscatedProfileID: String = ""

    fun setCallbackReceiver(receiver: String) {
        UNITY_RECEIVER = receiver
        Log.i(LOGGING_TAG, "Set callback receiver.")
    }

    fun setupGooglePlayFraudDetection(obfuscatedAccountID: String, obfuscatedProfileID: String) {
        this.obfuscatedAccountID = obfuscatedAccountID
        this.obfuscatedProfileID = obfuscatedProfileID
        Log.i(LOGGING_TAG, "Setup Google Play Fraud Detection.")
    }

    fun setupBillingClient(productsIDs: Array<String>, productsTypes: Array<String>) {
        Log.i(LOGGING_TAG, "Setting up Billing Client.")
        for (index in 0 until productsIDs.count()){
            val properProduct = QueryProductDetailsParams.Product.newBuilder()
                                .setProductId(productsIDs[index])
                                .setProductType(productsTypes[index])
                                .build()

            allProducts.add(productsIDs[index])
            if (productsTypes[index] == BillingClient.ProductType.SUBS) {
                subscriptionProducts[productsIDs[index]] = properProduct
            } else {
                inAppProducts[productsIDs[index]] = properProduct
            }
        }

        startBillingClientConnection()
    }

    private fun startBillingClientConnection() {
        Log.i(LOGGING_TAG, "Starting Billing Client connection.")

        billingClient.startConnection(object : BillingClientStateListener {
            override fun onBillingSetupFinished(billingResult: BillingResult) {
                if (billingResult.responseCode == BillingClient.BillingResponseCode.OK) {
                    Log.i(LOGGING_TAG, "Successfully finished Billing Client setup!")
                    UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBillingSetupSuccess", "")
                    queryProductDetails(BillingClient.ProductType.SUBS)
                    queryProductDetails(BillingClient.ProductType.INAPP)
                    querySubscriptionDetails()
                } else {
                    Log.e(LOGGING_TAG, "Failed setting up Billing Client: ${billingResult.responseCode}")
                    UnityPlayer.UnitySendMessage(
                        UNITY_RECEIVER,
                        "OnBillingSetupFailed",
                        billingResult.responseCode.toString()
                    )
                }
            }

            override fun onBillingServiceDisconnected() {
                Log.e(LOGGING_TAG, "Disconnected from Billing Service!")
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnBillingServiceDisconnected", "")
            }
        })
    }

    override fun onPurchasesUpdated(billingResult: BillingResult, purchases: List<Purchase>?) {
        Log.i(LOGGING_TAG, "Updating purchases.")
        if (billingResult.responseCode == BillingClient.BillingResponseCode.OK && purchases != null) {
            for (purchase in purchases) {
                handlePurchase(purchase)
            }
        } else {
            Log.e(LOGGING_TAG, "Purchase failed: ${billingResult.responseCode}")
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnPurchaseFailed", billingResult.responseCode.toString())
        }
    }

    fun purchaseProduct(productId: String, productType: String) {
        Log.i(LOGGING_TAG, "Purchasing product of ID: $productId")

        if (billingClient.isReady) {
            if (allProducts.contains(productId)) {
                if (!hasProductDetails) {
                    Log.e(LOGGING_TAG, "Purchase failed: Product details not found!")
                    UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnPurchaseFailed", "NO PRODUCT DETAILS FOUND")
                    queryProductDetails(productType)
                }
                else {
                    var foundProductDetails = false
                    for (details in if (productType == BillingClient.ProductType.SUBS) allSubscriptionProductsDetails else allInAppProductsDetails) {
                        if (details.productId == productId) {
                            foundProductDetails = true
                            Log.i(LOGGING_TAG, "Found product!")
                            startPurchase(details)
                            break
                        }
                    }

                    if (!foundProductDetails) {
                        Log.e(LOGGING_TAG, "Purchase failed: Product not found!")
                        UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnPurchaseFailed", "PRODUCT NOT FOUND")
                    }
                }
            } else {
                Log.e(LOGGING_TAG, "Purchase failed: Product not defined in products!")
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnPurchaseFailed", "PRODUCT NOT DEFINED AT INITIALIZATION")
            }
        } else {
            Log.e(LOGGING_TAG, "Purchase failed: Billing Client not ready for purchasing products!")
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnPurchaseFailed", "BILLING CLIENT NOT READY")
        }
    }

    private fun startPurchase(productDetails: ProductDetails) {
        Log.i(LOGGING_TAG, "Starting purchase flow.")
        val billingFlowParams = BillingFlowParams.newBuilder()
            .setObfuscatedAccountId(obfuscatedAccountID)
            .setObfuscatedProfileId(obfuscatedProfileID)
            .setProductDetailsParamsList(listOf(
                    BillingFlowParams.ProductDetailsParams.newBuilder()
                        .setProductDetails(productDetails)
                        .build()
                ))
            .build()
        billingClient.launchBillingFlow(activity, billingFlowParams)
    }

    private fun handlePurchase(purchase: Purchase) {
        Log.i(LOGGING_TAG, "Handling purchase!")
        if (purchase.purchaseState == Purchase.PurchaseState.PURCHASED) {
            // Grant entitlement to the user.
            if (!purchase.isAcknowledged) {
                val purchaseToken = purchase.purchaseToken
                val acknowledgePurchaseParams = AcknowledgePurchaseParams.newBuilder()
                    .setPurchaseToken(purchaseToken)
                    .build()
                billingClient.acknowledgePurchase(acknowledgePurchaseParams) { billingResult ->
                    if (billingResult.responseCode == BillingClient.BillingResponseCode.OK) {
                        Log.i(LOGGING_TAG, "Product has been purchased!")
                        UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "HandlePurchase", "OK")
                    } else {
                        Log.e(LOGGING_TAG, "Purchase failed: ${billingResult.responseCode}")
                        UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnPurchaseFailed", billingResult.responseCode.toString())
                    }
                }
            }
        } else if (purchase.purchaseState == Purchase.PurchaseState.PENDING) {
            Log.i(LOGGING_TAG, "Purchase is currently pending.")
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "HandlePurchase", "PENDING")
        }
    }

    fun queryProductDetails(productType: String) {
        Log.i(LOGGING_TAG, "Querying product details.")

        if (billingClient.isReady && ((productType == BillingClient.ProductType.SUBS && subscriptionProducts.isNotEmpty()) || (productType == BillingClient.ProductType.INAPP && inAppProducts.isNotEmpty()))) {
            val productDetailsQueryParamsBuilder = QueryProductDetailsParams.newBuilder()
            if (productType == BillingClient.ProductType.SUBS) {
                productDetailsQueryParamsBuilder.setProductList(subscriptionProducts.values.toList())
            } else {
                productDetailsQueryParamsBuilder.setProductList(inAppProducts.values.toList())
            }

            billingClient.queryProductDetailsAsync(productDetailsQueryParamsBuilder.build()) { billingResult, productDetailsList ->
                if (billingResult.responseCode == BillingClient.BillingResponseCode.OK) {
                    if (productDetailsList.isNotEmpty()) {
                        if (productType == BillingClient.ProductType.SUBS) {
                            allSubscriptionProductsDetails = productDetailsList
                        } else {
                            allInAppProductsDetails = productDetailsList
                        }

                        Log.i(LOGGING_TAG, "Got product details!")
                        UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnGotProductDetails", "")
                    } else {
                        Log.e(LOGGING_TAG, "Failed to get product details: No products found!")
                        UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnGetProductDetailsFailed", "NO PRODUCTS FOUND")
                    }
                } else {
                    Log.e(LOGGING_TAG, "Failed to get product details: ${billingResult.responseCode}")
                    UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnGetProductDetailsFailed", billingResult.responseCode.toString())
                }
            }
        } else {
            Log.e(LOGGING_TAG, "Failed to get product details: Billing Client not ready to query product details!")
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnGetProductDetailsFailed", "BILLING CLIENT NOT READY")
        }
    }

    fun querySubscriptionDetails() {
        Log.i(LOGGING_TAG, "Querying subscription details.")

        if (billingClient.isReady) {
            val subscriptionParams = QueryPurchasesParams.newBuilder()
                .setProductType(BillingClient.ProductType.SUBS)
                .build()
            billingClient.queryPurchasesAsync(subscriptionParams) { billingResult, subscriptionsDetailsList ->
                if (billingResult.responseCode == BillingClient.BillingResponseCode.OK) {
                    if (subscriptionsDetailsList.isNotEmpty()) {
                        allSubscriptionsDetails = subscriptionsDetailsList
                        hasProductDetails = true

                        Log.i(LOGGING_TAG, "Got subscription details!")
                        UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnGotSubscriptionDetails", "")
                    } else {
                        Log.e(LOGGING_TAG, "Failed to get subscription details: No products found!")
                        UnityPlayer.UnitySendMessage(
                            UNITY_RECEIVER,
                            "OnGetSubscriptionDetailsFailed",
                            "NO SUBSCRIPTIONS FOUND"
                        )
                    }
                } else {
                    Log.e(LOGGING_TAG, "Failed to get subscription details: ${billingResult.responseCode}")
                    UnityPlayer.UnitySendMessage(
                        UNITY_RECEIVER,
                        "OnGetSubscriptionDetailsFailed",
                        billingResult.responseCode.toString()
                    )
                }
            }
        } else {
            Log.e(LOGGING_TAG, "Failed to get subscription details: Billing Client not ready to query subscription details!")
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, "OnGetSubscriptionDetailsFailed", "BILLING CLIENT NOT READY")
        }
    }
}