using UnityEngine;

public class BillingExample : MonoBehaviour
{
	public void init()
	{
		GPaymnetManagerExample.init();
	}

	public void SuccsesPurchase()
	{
		if (GPaymnetManagerExample.isInited)
		{
			Singleton<AndroidInAppPurchaseManager>.instance.purchase("android.test.purchased");
		}
		else
		{
			AndroidMessage.Create("Error", "PaymnetManagerExample not yet inited");
		}
	}

	public void FailPurchase()
	{
		if (GPaymnetManagerExample.isInited)
		{
			Singleton<AndroidInAppPurchaseManager>.instance.purchase("android.test.item_unavailable");
		}
		else
		{
			AndroidMessage.Create("Error", "PaymnetManagerExample not yet inited");
		}
	}

	public void ConsumeProduct()
	{
		if (GPaymnetManagerExample.isInited)
		{
			if (Singleton<AndroidInAppPurchaseManager>.instance.inventory.IsProductPurchased("android.test.purchased"))
			{
				GPaymnetManagerExample.consume("android.test.purchased");
			}
			else
			{
				AndroidMessage.Create("Error", "You do not own product to consume it");
			}
		}
		else
		{
			AndroidMessage.Create("Error", "PaymnetManagerExample not yet inited");
		}
	}
}
