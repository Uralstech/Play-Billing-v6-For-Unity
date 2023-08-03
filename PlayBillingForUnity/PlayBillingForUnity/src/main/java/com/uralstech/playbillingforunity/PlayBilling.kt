package com.uralstech.playbillingforunity

import android.app.Activity
import android.util.Log
import com.android.billingclient.api.*
import com.unity3d.player.UnityPlayer
import java.lang.ref.WeakReference

class PlayBilling(private val activity: Activity) : PurchasesUpdatedListener {
    companion object {
        private const val LOGGING_TAG = "PlayBillingForUnity"

        private const val CB_ON_SETUP_UPDATE = "_OnBillingClientStatusUpdate"
        private const val CB_ON_PRODUCT_DETAILS_UPDATE = "_OnGetProductDetailsStatusUpdate"
        private const val CB_ON_PURCHASE_UPDATE = "_OnGetUserPurchasesStatusUpdate"
        private const val CB_ON_PURCHASE_HISTORY_UPDATE = "_OnGetUserPurchaseHistoryStatusUpdate"
        private const val CB_HANDLE_PURCHASE = "_HandlePurchase"
        private const val CB_PURCHASE_FAIL = "_OnPurchaseFailed"
        private const val CB_ACKNOWLEDGE_RESULT = "_OnGotAcknowledgePurchaseResult"
        private const val CB_CONSUME_RESULT = "_OnGotConsumePurchaseResult"

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

    private val inAppProducts: MutableMap<String, QueryProductDetailsParams.Product> = mutableMapOf()
    private val subscriptionProducts: MutableMap<String, QueryProductDetailsParams.Product> = mutableMapOf()
    private val allProducts : MutableMap<String, String> = mutableMapOf()

    private var allInAppProductsDetails: List<ProductDetails> = listOf()
    private var allSubscriptionProductsDetails: List<ProductDetails> = listOf()
    private var setAllInAppProductDetails: Boolean = false
    private var setAllSubscriptionProductDetails: Boolean = false

    private var userInAppPurchases: List<Purchase> = listOf()
    private var userSubscriptionPurchases: List<Purchase> = listOf()
    private var setUserInAppPurchases: Boolean = false
    private var setUserSubscriptionPurchases: Boolean = false

    private var userInAppPurchaseHistory: List<PurchaseHistoryRecord>? = listOf()
    private var userSubscriptionPurchaseHistory: List<PurchaseHistoryRecord>? = listOf()
    private var setUserInAppPurchaseHistory: Boolean = false
    private var setUserSubscriptionPurchaseHistory: Boolean = false

    private var obfuscatedAccountID: String? = null
    private var obfuscatedProfileID: String? = null

    private var applicationBase64PublicKey: String? = null

    fun changeCallbackReceiver(receiver: String) {
        UNITY_RECEIVER = receiver
        Log.i(LOGGING_TAG, "Set callback receiver.")
    }

    fun setupGooglePlayFraudDetection(obfuscatedAccountID: String?, obfuscatedProfileID: String?) {
        this.obfuscatedAccountID = obfuscatedAccountID
        this.obfuscatedProfileID = obfuscatedProfileID
        Log.i(LOGGING_TAG, "Setup Google Play Fraud Detection.")
    }

    fun setupBillingClient(productsIDs: Array<String>, productsTypes: Array<String>, applicationBase64PublicKey: String? = null) {
        Log.i(LOGGING_TAG, "Setting up Billing Client.")
        this.applicationBase64PublicKey = applicationBase64PublicKey

        for (index in 0 until productsIDs.count()){
            allProducts[productsIDs[index]] = productsTypes[index]
            if (productsTypes[index] == BillingClient.ProductType.SUBS) {
                subscriptionProducts[productsIDs[index]] = QueryProductDetailsParams.Product.newBuilder()
                    .setProductId(productsIDs[index])
                    .setProductType(productsTypes[index])
                    .build()
            } else {
                inAppProducts[productsIDs[index]] = QueryProductDetailsParams.Product.newBuilder()
                    .setProductId(productsIDs[index])
                    .setProductType(productsTypes[index])
                    .build()
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
                    UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ON_SETUP_UPDATE, "CONNECTED")
                    queryProductDetails(BillingClient.ProductType.INAPP)
                    queryProductDetails(BillingClient.ProductType.SUBS)
                    queryUserPurchases(BillingClient.ProductType.INAPP)
                    queryUserPurchases(BillingClient.ProductType.SUBS)
                } else {
                    Log.e(LOGGING_TAG, "Failed setting up Billing Client: ${billingResult.responseCode}")
                    UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ON_SETUP_UPDATE, billingResult.responseCode.toString())
                }
            }

            override fun onBillingServiceDisconnected() {
                Log.e(LOGGING_TAG, "Disconnected from Billing Service!")
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ON_SETUP_UPDATE, "DISCONNECTED")
            }
        })
    }

    fun getProductDetails(): Array<String> {
        return if (setAllInAppProductDetails && setAllSubscriptionProductDetails) {
            Log.i(LOGGING_TAG, "getProductDetails: SUCCESS")
            arrayOf("SUCCESS") + Conversions.convertProductDetailsListToStringList((allInAppProductsDetails + allSubscriptionProductsDetails))
        }
        else {
            if (!setAllInAppProductDetails) {
                queryProductDetails(BillingClient.ProductType.INAPP)
            }
            if (!setAllSubscriptionProductDetails) {
                queryProductDetails(BillingClient.ProductType.SUBS)
            }

            arrayOf("CALLED")
        }
    }

    fun getUserPurchases(refresh: Boolean = false): Array<String> {
        if (refresh) {
            setUserInAppPurchases = false
            setUserSubscriptionPurchases = false
        }

        return if (setUserInAppPurchases && setUserSubscriptionPurchases) {
            arrayOf("SUCCESS") + Conversions.convertPurchasesListToStringList((userInAppPurchases + userSubscriptionPurchases))
        }
        else {
            if (!setUserInAppPurchases) {
                queryUserPurchases(BillingClient.ProductType.INAPP)
            }
            if (!setUserSubscriptionPurchases) {
                queryUserPurchases(BillingClient.ProductType.SUBS)
            }

            arrayOf("CALLED")
        }
    }

    fun getUserPurchaseHistory(refresh: Boolean): Array<String> {
        val userInAppPurchaseHistory = this.userInAppPurchaseHistory
        val userSubscriptionPurchaseHistory = this.userSubscriptionPurchaseHistory

        if (refresh) {
            setUserInAppPurchaseHistory = false
            setUserSubscriptionPurchaseHistory = false
        }

        return if (setUserInAppPurchaseHistory && setUserSubscriptionPurchaseHistory) {
            if (userInAppPurchaseHistory != null && userSubscriptionPurchaseHistory == null) {
                arrayOf("SUCCESS") + Conversions.convertPurchaseHistoryRecordsListToStringList(userInAppPurchaseHistory)
            } else if (userInAppPurchaseHistory == null && userSubscriptionPurchaseHistory != null) {
                arrayOf("SUCCESS") + Conversions.convertPurchaseHistoryRecordsListToStringList(userSubscriptionPurchaseHistory)
            } else if (userInAppPurchaseHistory != null && userSubscriptionPurchaseHistory != null) {
                arrayOf("SUCCESS") + Conversions.convertPurchaseHistoryRecordsListToStringList((userInAppPurchaseHistory + userSubscriptionPurchaseHistory))
            } else {
                arrayOf("SUCCESS")
            }
        } else {
            if (!setUserInAppPurchaseHistory) {
                queryUserPurchaseHistory(BillingClient.ProductType.INAPP)
            }
            if (!setUserSubscriptionPurchaseHistory) {
                queryUserPurchaseHistory(BillingClient.ProductType.SUBS)
            }

            arrayOf("CALLED")
        }
    }

    fun purchaseProduct(productId: String, subscriptionPlan: String? = null, subscriptionOffer: String? = null) {
        Log.i(LOGGING_TAG, "Purchasing product of ID: $productId")

        if (billingClient.isReady) {
            if (allProducts.containsKey(productId)) {
                val productType = allProducts[productId]
                if ((productType == BillingClient.ProductType.INAPP && !setAllInAppProductDetails) || (productType == BillingClient.ProductType.SUBS && !setAllSubscriptionProductDetails)) {
                    Log.e(LOGGING_TAG, "Purchase failed: Product details not found!")
                    UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_PURCHASE_FAIL, "PRODUCT_DETAILS_NOT_SET")
                    queryProductDetails(productType)
                }
                else {
                    var foundProductDetails = false
                    for (details in if (productType == BillingClient.ProductType.SUBS) allSubscriptionProductsDetails else allInAppProductsDetails) {
                        if (details.productId == productId) {
                            foundProductDetails = true
                            Log.i(LOGGING_TAG, "Found product!")

                            var offerToken: String? = null
                            if ((!subscriptionPlan.isNullOrEmpty() || !subscriptionOffer.isNullOrEmpty()) && productType == BillingClient.ProductType.INAPP) {
                                Log.e(LOGGING_TAG, "Purchase failed: subscriptionPlan or subscriptionOffer given for productType INAPP!")
                                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_PURCHASE_FAIL, "SUBS_PLAN_FOR_INAPP")
                                return
                            } else if (subscriptionPlan.isNullOrEmpty() && productType == BillingClient.ProductType.SUBS) {
                                Log.e(LOGGING_TAG, "Purchase failed: subscriptionPlan is null for productType SUBS!")
                                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_PURCHASE_FAIL, "SUBS_PLAN_NULL")
                                return
                            } else if (productType == BillingClient.ProductType.SUBS) {
                                val subscriptionOfferDetailsList = details.subscriptionOfferDetails

                                if (subscriptionOfferDetailsList != null) {
                                    for (subscriptionOfferDetails in subscriptionOfferDetailsList) {
                                        if (subscriptionOfferDetails.basePlanId == subscriptionPlan && (subscriptionOffer.isNullOrEmpty() || subscriptionOfferDetails.offerId == subscriptionOffer)) {
                                            offerToken = subscriptionOfferDetails.offerToken
                                            break
                                        }
                                    }
                                }

                                if (offerToken.isNullOrEmpty()) {
                                    Log.e(LOGGING_TAG, "Purchase failed: Subscription plan not found!")
                                    UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_PURCHASE_FAIL, "OFFER_OR_PLAN_NOT_FOUND")
                                    return
                                }
                            }

                            startPurchase(details, offerToken)
                            break
                        }
                    }

                    if (!foundProductDetails) {
                        Log.e(LOGGING_TAG, "Purchase failed: Product not found!")
                        UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_PURCHASE_FAIL, "PRODUCT_DETAILS_NOT_FOUND")
                    }
                }
            } else {
                Log.e(LOGGING_TAG, "Purchase failed: Product not defined in products!")
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_PURCHASE_FAIL, "PRODUCT_NOT_DEFINED")
            }
        } else {
            Log.e(LOGGING_TAG, "Purchase failed: Billing Client not ready for purchasing products!")
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_PURCHASE_FAIL, "BILLING_NOT_READY")
        }
    }

    private fun startPurchase(productDetails: ProductDetails, offerToken: String?) {
        Log.i(LOGGING_TAG, "Starting purchase flow.")
        val productDetailsParams = BillingFlowParams.ProductDetailsParams.newBuilder()
            .setProductDetails(productDetails)

        if (!offerToken.isNullOrEmpty()) {
            productDetailsParams.setOfferToken(offerToken)
        }

        val billingFlowParamsBuilder = BillingFlowParams.newBuilder()
            .setProductDetailsParamsList(listOf(productDetailsParams.build()))

        val obfuscatedAccountID = this.obfuscatedAccountID
        val obfuscatedProfileID = this.obfuscatedProfileID

        if (obfuscatedAccountID != null)
            billingFlowParamsBuilder.setObfuscatedAccountId(obfuscatedAccountID)
        if (obfuscatedProfileID != null)
            billingFlowParamsBuilder.setObfuscatedProfileId(obfuscatedProfileID)
        billingClient.launchBillingFlow(activity, billingFlowParamsBuilder.build())
    }

    fun checkPurchaseValidity(purchaseJSON: String, signature: String): String {
        val applicationBase64PublicKey = this.applicationBase64PublicKey
        return if (applicationBase64PublicKey != null) {
            val isValid = Security.verifyPurchase(applicationBase64PublicKey, purchaseJSON, signature)
            if (isValid) {
                "VALID"
            } else {
                Log.e(LOGGING_TAG, "Purchase validation failed")
                "FAILED"
            }
        } else {
            "KEY_NOT_FOUND"
        }
    }

    fun acknowledgePurchase(purchaseToken: String) {
        val acknowledgePurchaseParams = AcknowledgePurchaseParams.newBuilder()
            .setPurchaseToken(purchaseToken)
            .build()

        billingClient.acknowledgePurchase(acknowledgePurchaseParams) { billingResult ->
            if (billingResult.responseCode == BillingClient.BillingResponseCode.OK) {
                Log.i(LOGGING_TAG, "Purchase acknowledged!")
            } else {
                Log.e(LOGGING_TAG, "Failed acknowledging purchase: ${billingResult.responseCode}")
            }

            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ACKNOWLEDGE_RESULT, billingResult.responseCode.toString())
        }
    }

    fun consumePurchase(purchaseToken: String) {
        val consumePurchaseParams = ConsumeParams.newBuilder()
            .setPurchaseToken(purchaseToken)
            .build()

        billingClient.consumeAsync(consumePurchaseParams) { billingResult, purchaseToken_ ->
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_CONSUME_RESULT, """
                {
                    "ResponseCode":${billingResult.responseCode},
                    "PurchaseToken":"$purchaseToken_"
                }
            """.trimIndent())
        }
    }

    override fun onPurchasesUpdated(billingResult: BillingResult, purchases: List<Purchase>?) {
        Log.i(LOGGING_TAG, "Updating purchases.")
        if (billingResult.responseCode == BillingClient.BillingResponseCode.OK && purchases != null) {
            for (purchase in purchases) {
                if (purchase.purchaseState == Purchase.PurchaseState.PURCHASED) {
                    UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_HANDLE_PURCHASE, Conversions.convertPurchaseToJSON(purchase))
                }
            }
        } else {
            Log.e(LOGGING_TAG, "Purchase failed: ${billingResult.responseCode}")
            UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_PURCHASE_FAIL, billingResult.responseCode.toString())
        }
    }

    private fun queryProductDetails(productType: String) {
        Log.i(LOGGING_TAG, "Querying product details.")
        val productList = if (productType == BillingClient.ProductType.SUBS) subscriptionProducts.values.toList() else inAppProducts.values.toList()
        val params = QueryProductDetailsParams.newBuilder()
            .setProductList(productList)
            .build()

        billingClient.queryProductDetailsAsync(params) { billingResult, productDetailsList ->
            if (billingResult.responseCode == BillingClient.BillingResponseCode.OK) {
                Log.i(LOGGING_TAG, "Successfully got product details! ($productType)")

                if (productType == BillingClient.ProductType.SUBS) {
                    allSubscriptionProductsDetails = productDetailsList
                    setAllSubscriptionProductDetails = true
                } else {
                    allInAppProductsDetails = productDetailsList
                    setAllInAppProductDetails = true
                }

                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ON_PRODUCT_DETAILS_UPDATE, "SUCCESS${if (!setAllInAppProductDetails || !setAllSubscriptionProductDetails) "_${productType.uppercase()}" else ""}")
            } else {
                Log.e(LOGGING_TAG, "Failed getting product details: ${billingResult.responseCode}")
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ON_PRODUCT_DETAILS_UPDATE, "FAIL_${billingResult.responseCode}")
            }
        }
    }

    private fun queryUserPurchases(productType: String) {
        Log.i(LOGGING_TAG, "Querying user purchase details.")
        val queryPurchasesParams = QueryPurchasesParams.newBuilder()
            .setProductType(productType)
            .build()

        billingClient.queryPurchasesAsync(queryPurchasesParams) { billingResult, purchasesList ->
            if (billingResult.responseCode == BillingClient.BillingResponseCode.OK) {
                Log.i(LOGGING_TAG, "Successfully got user purchase details!")

                if (productType == BillingClient.ProductType.INAPP) {
                    userInAppPurchases = purchasesList
                    setUserInAppPurchases = true
                } else {
                    userSubscriptionPurchases = purchasesList
                    setUserSubscriptionPurchases = true
                }

                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ON_PURCHASE_UPDATE, "SUCCESS${if (!setUserInAppPurchases || !setUserSubscriptionPurchases) "_${productType.uppercase()}" else ""}")
            } else {
                Log.e(LOGGING_TAG, "Failed getting user purchase details: ${billingResult.responseCode}")
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ON_PURCHASE_UPDATE, "FAIL_${billingResult.responseCode}")
            }
        }
    }

    private fun queryUserPurchaseHistory(productType: String) {
        Log.i(LOGGING_TAG, "Querying user purchase history.")
        val queryPurchasesParams = QueryPurchaseHistoryParams.newBuilder()
            .setProductType(productType)
            .build()

        billingClient.queryPurchaseHistoryAsync(queryPurchasesParams) { billingResult, purchasesList ->
            if (billingResult.responseCode == BillingClient.BillingResponseCode.OK) {
                Log.i(LOGGING_TAG, "Successfully got user purchase history!")

                if (productType == BillingClient.ProductType.INAPP) {
                    userInAppPurchaseHistory = purchasesList
                    setUserInAppPurchaseHistory = true
                } else {
                    userSubscriptionPurchaseHistory = purchasesList
                    setUserSubscriptionPurchaseHistory = true
                }

                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ON_PURCHASE_HISTORY_UPDATE, "SUCCESS${if (!setUserInAppPurchaseHistory || !setUserSubscriptionPurchaseHistory) "_${productType.uppercase()}" else ""}")
            } else {
                Log.e(LOGGING_TAG, "Failed getting user purchase history: ${billingResult.responseCode}")
                UnityPlayer.UnitySendMessage(UNITY_RECEIVER, CB_ON_PURCHASE_HISTORY_UPDATE, "FAIL_${billingResult.responseCode}")
            }
        }
    }
}
