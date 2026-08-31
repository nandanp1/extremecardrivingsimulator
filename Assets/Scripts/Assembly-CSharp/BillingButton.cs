using UnityEngine;

public class BillingButton : MonoBehaviour
{
	public BillingManager.InAppProduct product;

	public void OnClick()
	{
		string productId = MySingleton<Manager>.Instance.billingManager.GetProductId(product);
		if (product == BillingManager.InAppProduct.StarterPack)
		{
			string empty = string.Empty;
			empty = (MySingleton<Manager>.Instance.starterPackPanelIsActive ? "store_popup" : ((!MySingleton<Manager>.Instance.starterPackDoorActive) ? "store_scroll" : "door_animation"));
			ManagerAnalytics.instance.FireIAPClicked("starter_pack", empty);
		}
		InitPurchase(productId);
	}

	private void InitPurchase(string productId)
	{
		MySingleton<Manager>.Instance.currentLoadingPurchasePanel = Object.Instantiate(MySingleton<Manager>.Instance.pnLoadingPurchase) as GameObject;
		MySingleton<Manager>.Instance.unibill.InitiatePurchase(productId);
	}
}
