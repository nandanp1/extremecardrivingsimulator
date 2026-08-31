using System;
using System.Collections.Generic;
using UnityEngine;

public class AndroidInAppPurchaseManager : Singleton<AndroidInAppPurchaseManager>
{
	public const string ON_PRODUCT_PURCHASED = "on_product_purchased";

	public const string ON_PRODUCT_CONSUMED = "on_product_consumed";

	public const string ON_BILLING_SETUP_FINISHED = "on_billing_setup_finished";

	public const string ON_RETRIEVE_PRODUC_FINISHED = "on_retrieve_produc_finished";

	private List<string> _productsIds = new List<string>();

	private AndroidInventory _inventory;

	private bool _IsConnectingToServiceInProcess;

	private bool _IsProductRetrievingInProcess;

	private bool _IsConnectd;

	private bool _IsInventoryLoaded;

	public AndroidInventory inventory
	{
		get
		{
			return _inventory;
		}
	}

	public bool IsConnectingToServiceInProcess
	{
		get
		{
			return _IsConnectingToServiceInProcess;
		}
	}

	public bool IsProductRetrievingInProcess
	{
		get
		{
			return _IsProductRetrievingInProcess;
		}
	}

	public bool IsConnectd
	{
		get
		{
			return _IsConnectd;
		}
	}

	public bool IsInventoryLoaded
	{
		get
		{
			return _IsInventoryLoaded;
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		_inventory = new AndroidInventory();
	}

	public void addProduct(string SKU)
	{
		if (!_productsIds.Contains(SKU))
		{
			_productsIds.Add(SKU);
		}
	}

	public void retrieveProducDetails()
	{
		_IsProductRetrievingInProcess = true;
		AndroidNative.retrieveProducDetails();
	}

	public void purchase(string SKU)
	{
		AndroidNative.purchase(SKU);
	}

	public void consume(string SKU)
	{
		AndroidNative.consume(SKU);
	}

	public void loadStore()
	{
		if (AndroidNativeSettings.Instance.IsBase64KeyWasReplaced)
		{
			loadStore(AndroidNativeSettings.Instance.base64EncodedPublicKey);
			_IsConnectingToServiceInProcess = true;
		}
		else
		{
			Debug.LogError("Replace base64EncodedPublicKey in Androdi Native Setting menu");
		}
	}

	public void loadStore(string base64EncodedPublicKey)
	{
		foreach (string inAppProduct in AndroidNativeSettings.Instance.InAppProducts)
		{
			addProduct(inAppProduct);
		}
		string text = string.Empty;
		int count = _productsIds.Count;
		for (int i = 0; i < count; i++)
		{
			if (i != 0)
			{
				text += ",";
			}
			text += _productsIds[i];
		}
		AndroidNative.connectToBilling(text, base64EncodedPublicKey);
	}

	public void OnPurchaseFinishedCallback(string data)
	{
		string[] array = data.Split("|"[0]);
		int num = Convert.ToInt32(array[0]);
		GooglePurchaseTemplate googlePurchaseTemplate = null;
		if (num == 0)
		{
			googlePurchaseTemplate = new GooglePurchaseTemplate();
			googlePurchaseTemplate.SKU = array[2];
			googlePurchaseTemplate.packageName = array[3];
			googlePurchaseTemplate.developerPayload = array[4];
			googlePurchaseTemplate.orderId = array[5];
			googlePurchaseTemplate.SetState(array[6]);
			googlePurchaseTemplate.token = array[7];
			googlePurchaseTemplate.signature = array[8];
			googlePurchaseTemplate.time = Convert.ToInt64(array[9]);
			googlePurchaseTemplate.originalJson = array[10];
			if (_inventory != null)
			{
				_inventory.addPurchase(googlePurchaseTemplate);
			}
		}
		BillingResult data2 = new BillingResult(num, array[1], googlePurchaseTemplate);
		dispatch("on_product_purchased", data2);
	}

	public void OnConsumeFinishedCallBack(string data)
	{
		string[] array = data.Split("|"[0]);
		int num = Convert.ToInt32(array[0]);
		GooglePurchaseTemplate googlePurchaseTemplate = null;
		if (num == 0)
		{
			googlePurchaseTemplate = new GooglePurchaseTemplate();
			googlePurchaseTemplate.SKU = array[2];
			googlePurchaseTemplate.packageName = array[3];
			googlePurchaseTemplate.developerPayload = array[4];
			googlePurchaseTemplate.orderId = array[5];
			googlePurchaseTemplate.SetState(array[6]);
			googlePurchaseTemplate.token = array[7];
			googlePurchaseTemplate.signature = array[8];
			if (_inventory != null)
			{
				_inventory.removePurchase(googlePurchaseTemplate);
			}
		}
		BillingResult data2 = new BillingResult(num, array[1], googlePurchaseTemplate);
		dispatch("on_product_consumed", data2);
	}

	public void OnBillingSetupFinishedCallback(string data)
	{
		string[] array = data.Split("|"[0]);
		int code = Convert.ToInt32(array[0]);
		_IsConnectd = true;
		_IsConnectingToServiceInProcess = false;
		BillingResult data2 = new BillingResult(code, array[1]);
		dispatch("on_billing_setup_finished", data2);
	}

	public void OnQueryInventoryFinishedCallBack(string data)
	{
		string[] array = data.Split("|"[0]);
		int code = Convert.ToInt32(array[0]);
		BillingResult data2 = new BillingResult(code, array[1]);
		_IsInventoryLoaded = true;
		_IsProductRetrievingInProcess = false;
		dispatch("on_retrieve_produc_finished", data2);
	}

	public void OnPurchasesRecive(string data)
	{
		if (data.Equals(string.Empty))
		{
			Debug.Log("InAppPurchaseManager, no purchases avaiable");
			return;
		}
		string[] array = data.Split("|"[0]);
		for (int i = 0; i < array.Length; i += 7)
		{
			GooglePurchaseTemplate googlePurchaseTemplate = new GooglePurchaseTemplate();
			googlePurchaseTemplate.SKU = array[i];
			googlePurchaseTemplate.packageName = array[i + 1];
			googlePurchaseTemplate.developerPayload = array[i + 2];
			googlePurchaseTemplate.orderId = array[i + 3];
			googlePurchaseTemplate.SetState(array[i + 4]);
			googlePurchaseTemplate.token = array[i + 5];
			googlePurchaseTemplate.signature = array[i + 6];
			_inventory.addPurchase(googlePurchaseTemplate);
		}
		Debug.Log("InAppPurchaseManager, tottal purchases loaded: " + _inventory.purchases.Count);
	}

	public void OnProducttDetailsRecive(string data)
	{
		if (data.Equals(string.Empty))
		{
			Debug.Log("InAppPurchaseManager, no products avaiable");
			return;
		}
		string[] array = data.Split("|"[0]);
		for (int i = 0; i < array.Length; i += 6)
		{
			GoogleProductTemplate googleProductTemplate = new GoogleProductTemplate();
			googleProductTemplate.SKU = array[i];
			googleProductTemplate.price = array[i + 1];
			googleProductTemplate.title = array[i + 2];
			googleProductTemplate.description = array[i + 3];
			googleProductTemplate.priceAmountMicros = array[i + 4];
			googleProductTemplate.priceCurrencyCode = array[i + 5];
			_inventory.addProduct(googleProductTemplate);
		}
		Debug.Log("InAppPurchaseManager, tottal products loaded: " + _inventory.products.Count);
	}
}
