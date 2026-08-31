using Unibill.Impl;
using UnityEngine;

[AddComponentMenu("")]
public class AmazonAppStoreCallbackMonoBehaviour : MonoBehaviour
{
	private AmazonAppStoreBillingService amazon;

	public void Start()
	{
		base.gameObject.name = GetType().ToString();
		Object.DontDestroyOnLoad(this);
	}

	public void initialise(AmazonAppStoreBillingService amazon)
	{
		this.amazon = amazon;
	}

	public void onSDKAvailable(string isSandboxEnvironment)
	{
		amazon.onSDKAvailable(isSandboxEnvironment);
	}

	public void onGetItemDataFailed(string empty)
	{
		amazon.onGetItemDataFailed();
	}

	public void onPurchaseFailed(string item)
	{
		amazon.onPurchaseFailed(item);
	}

	public void onPurchaseSucceeded(string item)
	{
		amazon.onPurchaseSucceeded(item);
	}

	public void onPurchaseUpdateFailed(string empty)
	{
		amazon.onPurchaseUpdateFailed();
	}

	public void onPurchaseUpdateSuccess(string data)
	{
		amazon.onPurchaseUpdateSuccess(data);
	}
}
