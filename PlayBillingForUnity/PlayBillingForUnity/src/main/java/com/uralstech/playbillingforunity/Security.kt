package com.uralstech.playbillingforunity

import android.text.TextUtils
import android.util.Base64
import java.security.*
import java.security.spec.InvalidKeySpecException
import java.security.spec.X509EncodedKeySpec

object Security {
    private const val KEY_FACTORY_ALGORITHM = "RSA"
    private const val SIGNATURE_ALGORITHM = "SHA1withRSA"

    /**
     * Verifies that the data was signed with the given signature, and returns
     * the result. Note: this method is slow and should be called asynchronously.
     * @param publicKey the public key in base64-encoded form
     * @param signedData the signed data
     * @param signature the signature
     */
    fun verifyPurchase(publicKey: String, signedData: String, signature: String): Boolean {
        if (TextUtils.isEmpty(signedData) || TextUtils.isEmpty(publicKey) || TextUtils.isEmpty(signature)) {
            return false
        }
        val key = generatePublicKey(publicKey)
        return verify(key, signedData, signature)
    }

    /**
     * Generates a PublicKey instance from a string containing the
     * Base64-encoded public key.
     *
     * @param encodedPublicKey Base64-encoded public key
     * @throws IllegalArgumentException if encodedPublicKey is invalid
     */
    private fun generatePublicKey(encodedPublicKey: String): PublicKey {
        try {
            val decodedKey = Base64.decode(encodedPublicKey, Base64.DEFAULT)
            val keyFactory = KeyFactory.getInstance(KEY_FACTORY_ALGORITHM)
            return keyFactory.generatePublic(X509EncodedKeySpec(decodedKey))
        } catch (e: NoSuchAlgorithmException) {
            throw RuntimeException(e)
        } catch (e: InvalidKeySpecException) {
            throw IllegalArgumentException("Invalid key specification.")
        } catch (e: IllegalArgumentException) {
            throw IllegalArgumentException("Base64 decoding failed.")
        }
    }

    /**
     * Verifies that the signature from the server matches the computed
     * signature on the data. Returns true if the data is correctly signed.
     *
     * @param publicKey public key associated with the developer account
     * @param signedData signed data from server
     * @param signature server signature
     * @return true if the data and signature match
     */
    private fun verify(publicKey: PublicKey, signedData: String, signature: String): Boolean {
        val sig: Signature
        try {
            sig = Signature.getInstance(SIGNATURE_ALGORITHM)
            sig.initVerify(publicKey)
            sig.update(signedData.toByteArray())
            if (!sig.verify(Base64.decode(signature, Base64.DEFAULT))) {
                return false
            }
            return true
        } catch (e: NoSuchAlgorithmException) {
            // "RSA" is guaranteed to be available.
            throw RuntimeException(e)
        } catch (e: InvalidKeyException) {
            throw IllegalArgumentException(e)
        } catch (e: SignatureException) {
            throw IllegalArgumentException(e)
        } catch (e: IllegalArgumentException) {
            throw IllegalArgumentException("Base64 decoding failed.")
        }
    }
}