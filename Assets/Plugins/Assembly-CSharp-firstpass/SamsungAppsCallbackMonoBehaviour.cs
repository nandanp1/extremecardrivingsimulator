using Unibill.Impl;
using UnityEngine;

[AddComponentMenu("")]
public class SamsungAppsCallbackMonoBehaviour : MonoBehaviour
{
	private SamsungAppsBillingService samsung;

	public void Start()
	{
		base.gameObject.name = GetType().ToString();
		Object.DontDestroyOnLoad(this);
	}

	public void initialise(SamsungAppsBillingService samsung)
	{
		this.samsung = samsung;
	}

	public void onProductListReceived(string productCSVString)
	{
		samsung.onProductListReceived(productCSVString);
	}

	public void onPurchaseFailed(string item)
	{
		samsung.onPurchaseFailed(item);
	}

	public void onPurchaseSucceeded(string item)
	{
		samsung.onPurchaseSucceeded(item);
	}

	public void onPurchaseCancelled(string item)
	{
		samsung.onPurchaseCancelled(item);
	}

	public void onTransactionsRestored(string success)
	{
		samsung.onTransactionsRestored(success);
	}

	public void onInitFail()
	{
		samsung.onInitFail();
	}
}
