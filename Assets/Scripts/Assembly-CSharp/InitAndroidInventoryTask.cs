using UnityEngine;

public class InitAndroidInventoryTask : EventDispatcher
{
	public static InitAndroidInventoryTask Create()
	{
		return new GameObject("InitAndroidInventoryTask").AddComponent<InitAndroidInventoryTask>();
	}

	public void Run()
	{
		Debug.Log("InitAndroidInventoryTask task started");
		if (Singleton<AndroidInAppPurchaseManager>.instance.IsConnectd)
		{
			OnBillingConnected(null);
			return;
		}
		Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_billing_setup_finished", OnBillingConnected);
		if (!Singleton<AndroidInAppPurchaseManager>.instance.IsConnectingToServiceInProcess)
		{
			Singleton<AndroidInAppPurchaseManager>.instance.loadStore();
		}
	}

	private void OnBillingConnected(CEvent e)
	{
		Debug.Log("OnBillingConnected");
		if (e == null)
		{
			OnBillingConnectFinished();
			return;
		}
		BillingResult billingResult = e.data as BillingResult;
		Singleton<AndroidInAppPurchaseManager>.instance.removeEventListener("on_billing_setup_finished", OnBillingConnected);
		if (billingResult.isSuccess)
		{
			OnBillingConnectFinished();
			return;
		}
		Debug.Log("OnBillingConnected Failed");
		dispatch("failed");
	}

	private void OnBillingConnectFinished()
	{
		Debug.Log("OnBillingConnected COMPLETE");
		if (Singleton<AndroidInAppPurchaseManager>.instance.IsInventoryLoaded)
		{
			Debug.Log("IsInventoryLoaded COMPLETE");
			dispatch("complete");
			return;
		}
		Singleton<AndroidInAppPurchaseManager>.instance.addEventListener("on_retrieve_produc_finished", OnRetriveProductsFinised);
		if (!Singleton<AndroidInAppPurchaseManager>.instance.IsProductRetrievingInProcess)
		{
			Singleton<AndroidInAppPurchaseManager>.instance.retrieveProducDetails();
		}
	}

	private void OnRetriveProductsFinised(CEvent e)
	{
		Debug.Log("OnRetriveProductsFinised");
		BillingResult billingResult = e.data as BillingResult;
		Singleton<AndroidInAppPurchaseManager>.instance.removeEventListener("on_retrieve_produc_finished", OnRetriveProductsFinised);
		if (billingResult.isSuccess)
		{
			Debug.Log("OnRetriveProductsFinised COMPLETE");
			dispatch("complete");
		}
		else
		{
			Debug.Log("OnRetriveProductsFinised FAILED");
			dispatch("failed");
		}
	}
}
