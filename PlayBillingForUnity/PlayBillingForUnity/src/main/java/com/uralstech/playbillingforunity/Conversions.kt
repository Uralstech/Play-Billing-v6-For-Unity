package com.uralstech.playbillingforunity

import com.android.billingclient.api.*
import com.android.billingclient.api.ProductDetails.SubscriptionOfferDetails

object Conversions {
    fun convertPurchaseToJSON(purchase: Purchase): String {
        return """{ "AccountIdentifiers":${if (purchase.accountIdentifiers != null) """{ "ObfuscatedAccountID":"${purchase.accountIdentifiers?.obfuscatedAccountId}", "ObfuscatedProfileID":"${purchase.accountIdentifiers?.obfuscatedProfileId}" }""" else null}, "DeveloperPayload":"${purchase.developerPayload}", "OrderID":"${purchase.orderId}", "OriginalJSON":"${purchase.originalJson.replace("\"", "\\\"")}", "PackageName":"${purchase.packageName}", "ProductIDs":[ ${purchase.products.joinToString("\",\"", "\"", "\"")} ], "PurchaseState":${purchase.purchaseState}, "PurchaseTime":"${purchase.purchaseTime}", "PurchaseToken":"${purchase.purchaseToken}", "Quantity":${purchase.quantity}, "Signature":"${purchase.signature}", "IsAcknowledged":${purchase.isAcknowledged}, "IsAutoRenewing":${purchase.isAutoRenewing} }"""
    }

    private fun convertPurchaseHistoryRecordToJSON(purchaseHistoryRecord: PurchaseHistoryRecord): String {
        return """{ "DeveloperPayload":"${purchaseHistoryRecord.developerPayload}", "OriginalJSON":"${purchaseHistoryRecord.originalJson.replace("\"", "\\\"")}", "ProductIDs":[ ${purchaseHistoryRecord.products.joinToString("\",\"", "\"", "\"")} ], "PurchaseTime":"${purchaseHistoryRecord.purchaseTime}", "PurchaseToken":"${purchaseHistoryRecord.purchaseToken}", "Quantity":${purchaseHistoryRecord.quantity}, "Signature":"${purchaseHistoryRecord.signature}" }"""
    }

    private fun convertProductDetailsToJSON(productDetails: ProductDetails): String {
        val oneTimePurchaseOfferDetails = productDetails.oneTimePurchaseOfferDetails
        val subscriptionOfferDetails = productDetails.subscriptionOfferDetails

        return """{ "Description":"${productDetails.description}", "Name":"${productDetails.name}", "OneTimePurchaseOfferDetails":${if (oneTimePurchaseOfferDetails != null) convertOneTimePurchaseOfferDetailsToJSON(oneTimePurchaseOfferDetails) else null}, "ProductID":"${productDetails.productId}", "ProductType":"${productDetails.productType}", "SubscriptionOfferDetails":${if (subscriptionOfferDetails != null) """[ ${convertSubscriptionOfferDetailsListToJSON(subscriptionOfferDetails)} ]""" else null}, "Title":"${productDetails.title}" }"""
    }

    private fun convertOneTimePurchaseOfferDetailsToJSON(oneTimePurchaseOfferDetails: ProductDetails.OneTimePurchaseOfferDetails): String {
        return """{ "FormattedPrice":"${oneTimePurchaseOfferDetails.formattedPrice}", "PriceAmountMicros":"${oneTimePurchaseOfferDetails.priceAmountMicros}", "PriceCurrencyCode":"${oneTimePurchaseOfferDetails.priceCurrencyCode}" }"""
    }

    private fun convertSubscriptionOfferDetailsToJSON(subscriptionOfferDetails: SubscriptionOfferDetails): String {
        return """{ "BasePlanID":"${subscriptionOfferDetails.basePlanId}", "OfferID":${if (subscriptionOfferDetails.offerId != null) """"${subscriptionOfferDetails.offerId}"""" else null}, "OfferTags": [ ${subscriptionOfferDetails.offerTags.joinToString("\",\"", "\"", "\"")} ], "OfferToken":"${subscriptionOfferDetails.offerToken}", "PricingPhases": [ ${convertPricingPhasesToJSON(subscriptionOfferDetails.pricingPhases)} ] }"""
    }

    private fun convertPricingPhaseToJSON(pricingPhase: ProductDetails.PricingPhase): String {
        return """{ "BillingCycleCount":${pricingPhase.billingCycleCount}, "BillingPeriod":"${pricingPhase.billingPeriod}", "FormattedPrice":"${pricingPhase.formattedPrice}", "PriceAmountMicros":"${pricingPhase.priceAmountMicros}", "PriceCurrencyCode":"${pricingPhase.priceCurrencyCode}", "RecurrenceMode":${pricingPhase.recurrenceMode} }"""
    }

    fun convertPurchasesListToStringList(purchases: List<Purchase>): List<String> {
        val purchasesAsStringList: MutableList<String> = mutableListOf()
        for (purchase in purchases) {
            purchasesAsStringList.add(convertPurchaseToJSON(purchase))
        }

        return purchasesAsStringList
    }

    fun convertPurchaseHistoryRecordsListToStringList(purchaseHistoryRecordsList: List<PurchaseHistoryRecord>): List<String> {
        val purchaseHistoryRecordsListAsStringList: MutableList<String> = mutableListOf()
        for (purchaseHistoryRecord in purchaseHistoryRecordsList) {
            purchaseHistoryRecordsListAsStringList.add(convertPurchaseHistoryRecordToJSON(purchaseHistoryRecord))
        }

        return purchaseHistoryRecordsListAsStringList
    }

    fun convertProductDetailsListToStringList(productDetailsList: List<ProductDetails>): List<String> {
        val productDetailsAsStringList: MutableList<String> = mutableListOf()
        for (productDetails in productDetailsList) {
            productDetailsAsStringList.add(convertProductDetailsToJSON(productDetails))
        }

        return productDetailsAsStringList
    }

    private fun convertSubscriptionOfferDetailsListToJSON(subscriptionOfferDetailsList: List<SubscriptionOfferDetails>): String {
        val subscriptionOfferDetailsJSONList: MutableList<String> = mutableListOf()
        for (subscriptionOfferDetails in subscriptionOfferDetailsList) {
            subscriptionOfferDetailsJSONList.add(convertSubscriptionOfferDetailsToJSON(subscriptionOfferDetails))
        }

        return subscriptionOfferDetailsJSONList.joinToString()
    }

    private fun convertPricingPhasesToJSON(pricingPhases: ProductDetails.PricingPhases): String {
        val pricingPhasesJSONList: MutableList<String> = mutableListOf()
        for (pricingPhase in pricingPhases.pricingPhaseList) {
            pricingPhasesJSONList.add(convertPricingPhaseToJSON(pricingPhase))
        }

        return pricingPhasesJSONList.joinToString()
    }
}