using UnityEngine;

public class GameBillingManagerExample : MonoBehaviour
{
	public const string COINS_ITEM = "small_coins_bag";

	public const string COINS_BOOST = "coins_bonus";

	public const string base64EncodedPublicKey = "REPLACE_WITH_YOUR_PUBLIC_KEY";

	private static bool _isInited;

	private static bool ListnersAdded;

	public static bool isInited
	{
		get
		{
			return _isInited;
		}
	}

	public static void init()
	{
		if (!ListnersAdded)
		{
			Singleton<AndroidInAppPurchaseManager>.instance.addProduct("small_coins_bag");
			Singleton<AndroidInAppPurchaseManager>.instance.addProduct("coins_bonus");
			Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_product_purchased", OnProductPurchased);
			Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_product_consumed", OnProductConsumed);
			Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_billing_setup_finished", OnBillingConnected);
			Singleton<AndroidInAppPurchaseManager>.instance.loadStore("REPLACE_WITH_YOUR_PUBLIC_KEY");
			ListnersAdded = true;
		}
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
		switch (purchase.SKU)
		{
		case "small_coins_bag":
			consume("small_coins_bag");
			break;
		case "coins_bonus":
			GameDataExample.EnableCoinsBoost();
			break;
		}
	}

	private static void OnProcessingConsumeProduct(GooglePurchaseTemplate purchase)
	{
		switch (purchase.SKU)
		{
		case "small_coins_bag":
			GameDataExample.AddCoins(100);
			break;
		}
	}

	private static void OnProductPurchased(CEvent e)
	{
		BillingResult billingResult = e.data as BillingResult;
		if (billingResult.isSuccess)
		{
			OnProcessingPurchasedProduct(billingResult.purchase);
		}
		else
		{
			AndroidMessage.Create("Product Purchase Failed", billingResult.response + " " + billingResult.message);
		}
		Debug.Log("Purchased Responce: " + billingResult.response + " " + billingResult.message);
	}

	private static void OnProductConsumed(CEvent e)
	{
		BillingResult billingResult = e.data as BillingResult;
		if (billingResult.isSuccess)
		{
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
			Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_retrieve_produc_finished", OnRetriveProductsFinised);
			Singleton<AndroidInAppPurchaseManager>.instance.retrieveProducDetails();
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
			UpdateStoreData();
			_isInited = true;
		}
		else
		{
			AndroidMessage.Create("Connection Responce", billingResult.response + " " + billingResult.message);
		}
	}

	private static void UpdateStoreData()
	{
		if (Singleton<AndroidInAppPurchaseManager>.instance.inventory.IsProductPurchased("small_coins_bag"))
		{
			consume("small_coins_bag");
		}
		if (Singleton<AndroidInAppPurchaseManager>.instance.inventory.IsProductPurchased("coins_bonus"))
		{
			GameDataExample.EnableCoinsBoost();
		}
	}
}
