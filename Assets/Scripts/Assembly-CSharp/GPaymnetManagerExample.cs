using UnityEngine;

public class GPaymnetManagerExample : MonoBehaviour
{
	public const string ANDROID_TEST_PURCHASED = "android.test.purchased";

	public const string ANDROID_TEST_CANCELED = "android.test.canceled";

	public const string ANDROID_TEST_REFUNDED = "android.test.refunded";

	public const string ANDROID_TEST_ITEM_UNAVALIABLE = "android.test.item_unavailable";

	public const string base64EncodedPublicKey = "REPLACE_WITH_YOUR_PUBLIC_KEY";

	private static bool _isInited;

	public static bool isInited
	{
		get
		{
			return _isInited;
		}
	}

	public static void init()
	{
		Singleton<AndroidInAppPurchaseManager>.instance.addProduct("android.test.purchased");
		Singleton<AndroidInAppPurchaseManager>.instance.addProduct("android.test.canceled");
		Singleton<AndroidInAppPurchaseManager>.instance.addProduct("android.test.refunded");
		Singleton<AndroidInAppPurchaseManager>.instance.addProduct("android.test.item_unavailable");
		Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_product_purchased", OnProductPurchased);
		Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_product_consumed", OnProductConsumed);
		Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_billing_setup_finished", OnBillingConnected);
		Singleton<AndroidInAppPurchaseManager>.instance.loadStore("REPLACE_WITH_YOUR_PUBLIC_KEY");
	}

	public static void purchase(string SKU)
	{
		Singleton<AndroidInAppPurchaseManager>.instance.purchase(SKU);
	}

	public static void consume(string SKU)
	{
		Singleton<AndroidInAppPurchaseManager>.instance.consume(SKU);
	}

	private static void OnProcessingPurchasedProduct(GooglePurchaseTemplate purchase)
	{
	}

	private static void OnProcessingConsumeProduct(GooglePurchaseTemplate purchase)
	{
	}

	private static void OnProductPurchased(CEvent e)
	{
		BillingResult billingResult = e.data as BillingResult;
		if (billingResult.isSuccess)
		{
			AndroidMessage.Create("Product Purchased", billingResult.purchase.SKU);
			OnProcessingPurchasedProduct(billingResult.purchase);
		}
		else
		{
			AndroidMessage.Create("Product Purchase Failed", billingResult.response + " " + billingResult.message);
		}
		Debug.Log("Purchased Responce: " + billingResult.response + " " + billingResult.message);
		Debug.Log(billingResult.purchase.originalJson);
	}

	private static void OnProductConsumed(CEvent e)
	{
		BillingResult billingResult = e.data as BillingResult;
		if (billingResult.isSuccess)
		{
			AndroidMessage.Create("Product Consumed", billingResult.purchase.SKU);
			OnProcessingConsumeProduct(billingResult.purchase);
		}
		else
		{
			AndroidMessage.Create("Product Cousume Failed", billingResult.response + " " + billingResult.message);
		}
		Debug.Log("Cousume Responce: " + billingResult.response + " " + billingResult.message);
	}

	private static void OnBillingConnected(CEvent e)
	{
		BillingResult billingResult = e.data as BillingResult;
		Singleton<AndroidInAppPurchaseManager>.instance.removeEventListener("on_billing_setup_finished", OnBillingConnected);
		if (billingResult.isSuccess)
		{
			Singleton<AndroidInAppPurchaseManager>.instance.retrieveProducDetails();
			Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_retrieve_produc_finished", OnRetriveProductsFinised);
		}
		AndroidMessage.Create("Connection Responce", billingResult.response + " " + billingResult.message);
		Debug.Log("Connection Responce: " + billingResult.response + " " + billingResult.message);
	}

	private static void OnRetriveProductsFinised(CEvent e)
	{
		BillingResult billingResult = e.data as BillingResult;
		Singleton<AndroidInAppPurchaseManager>.instance.removeEventListener("on_retrieve_produc_finished", OnRetriveProductsFinised);
		if (billingResult.isSuccess)
		{
			_isInited = true;
			AndroidMessage.Create("Success", "Billing init complete inventory contains: " + Singleton<AndroidInAppPurchaseManager>.instance.inventory.purchases.Count + " products");
		}
		else
		{
			AndroidMessage.Create("Connection Responce", billingResult.response + " " + billingResult.message);
		}
		Debug.Log("Connection Responce: " + billingResult.response + " " + billingResult.message);
	}
}
