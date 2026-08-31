using System.Collections.Generic;
using AxesInMotion.Utils.MiniJSON;
using Unibill;
using UnityEngine;

public class BillingManager : MonoBehaviour
{
	public enum InAppProduct
	{
		None = 0,
		MiniPack = 1,
		SmallPack = 2,
		MediumPack = 3,
		BigPack = 4,
		MegaPack = 5,
		RemoveAds = 6,
		RemoveAdsOffer = 7,
		StarterPack = 8,
		ExclusivePack = 9
	}

	public enum CarPackId
	{
		None = 0,
		LikeASir = 1,
		FunOnWheels = 2
	}

	public delegate void StarterPackBought();

	public const string CURRENCY_CREDITS = "credits";

	private const int NONE_INDEX = 0;

	private const int MINIPACK_INDEX = 1;

	private const int SMALLPACK_INDEX = 2;

	private const int MEDIUMPACK_INDEX = 3;

	private const int BIGPACK_INDEX = 4;

	private const int MEGAPACK_INDEX = 5;

	private const int REMOVEADS_INDEX = 6;

	private const int REMOVEADSOFFER_INDEX = 7;

	private const int EXCLUSIVEPACK_INDEX = 8;

	public const decimal MINI_PACK_AMOUNT = 1000m;

	public const decimal SMALL_PACK_AMOUNT = 5500m;

	public const decimal MEDIUM_PACK_AMOUNT = 11500m;

	public const decimal BIG_PACK_AMOUNT = 24000m;

	public const decimal MEGA_PACK_AMOUNT = 62500m;

	public const decimal EXCLUSIVE_PACK_AMOUNT = 200000m;

	public const decimal APPLY_PAINT_PRICE = 50m;

	public const int VIDEO_REWARD_CREDITS = 75;

	public const decimal SINGLE_CAR_PRICE = 3500m;

	public const decimal SINGLE_PACK_PRICE = 5500m;

	public const decimal STARTER_PACK_REWARD = 3500m;

	public const int STARTER_PACK_CAR_ID = 24;

	private const double REMOVE_ADS_OFFER_PRICE = 0.99;

	private const double INITIAL_PACK_PRICE = 0.99;

	private const double REMOVE_ADS_PRICE = 1.99;

	private const double STARTER_PACK_PRICE = 2.99;

	private const double CASUAL_PACK_PRICE = 4.99;

	private const double PROFESSIONAL_PACK_PRICE = 9.99;

	private const double EXTREME_PACK_PRICE = 19.99;

	private const double ULTIMATE_PACK_PRICE = 49.99;

	private const double EXCLUSIVE_PACK_PRICE = 99.99;

	public Dictionary<CarPackId, int[]> carPacks;

	public string removeAdsPurchaseId;

	public string removeAdsOfferPurchaseId;

	public string starterPackOfferId;

	public string miniPackId;

	public string smallPackId;

	public string mediumPackId;

	public string bigPackId;

	public string megaPackId;

	public string exclusivePackId;

	public Dictionary<string, object> data;

	public event StarterPackBought StarterPackBoughtEvent;

	private void PopulateCarPacks()
	{
		carPacks.Add(CarPackId.LikeASir, new int[3] { 1, 2, 3 });
		carPacks.Add(CarPackId.FunOnWheels, new int[3] { 4, 5, 6 });
	}

	private void Awake()
	{
		MySingleton<Manager>.Instance.billingManager = this;
	}

	private void Start()
	{
		Unibiller.onBillerReady += onBillerReady;
		Unibiller.onTransactionsRestored += onTransactionsRestored;
		Unibiller.onPurchaseCancelled += onCancelled;
		Unibiller.onPurchaseFailed += onFailed;
		Unibiller.Initialise();
	}

	private void onBillerReady(UnibillState state)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("onBillerReady:" + state);
		}
		ProcessNonConsumablePurchases(string.Empty);
		if (MySingleton<Manager>.Instance.guiCurrentLogic != null)
		{
			MySingleton<Manager>.Instance.guiCurrentLogic.BillerInitialized(state);
		}
	}

	private void onTransactionsRestored(bool success)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("Transactions restored: " + success);
		}
		ProcessNonConsumablePurchases(string.Empty);
		MySingleton<Manager>.Instance.guiCurrentLogic.PurchasesRestored(success);
		DisablePurchaseLoadingPanel();
	}

	private void onPurchased(PurchaseEvent e)
	{
		MonoBehaviour.print("COMPRANDO COSAS!!: " + e.PurchasedItem.name);
		if (Debug.isDebugBuild)
		{
			Debug.Log("Purchase OK: " + e.PurchasedItem.Id);
			Debug.Log("Receipt: " + e.Receipt);
			Debug.Log(string.Format("{0} has now been purchased {1} times.", e.PurchasedItem.Id, Unibiller.GetPurchaseCount(e.PurchasedItem)));
		}
		if (e.PurchasedItem.PurchaseType == PurchaseType.Consumable)
		{
			ProcessConsumablePurchase(GetPurchaseIdEnum(e.PurchasedItem.Id));
		}
		else if (e.PurchasedItem.PurchaseType == PurchaseType.NonConsumable)
		{
			ProcessNonConsumablePurchases(e.PurchasedItem.LocalId);
		}
		DisablePurchaseLoadingPanel();
		MySingleton<Manager>.Instance.guiCurrentLogic.PurchaseCompleted(e);
		Dictionary<string, object> dictionary = Json.Deserialize(e.Receipt) as Dictionary<string, object>;
		data = dictionary;
		string text = data["json"].ToString();
		string receiptSignature = data["signature"].ToString();
		Dictionary<string, object> dictionary2 = Json.Deserialize(text) as Dictionary<string, object>;
		string text2 = dictionary2["productId"].ToString();
		ManagerAnalytics.instance.FireIAPPurchased(text2, GetDollarPriceFromID(text2), text, receiptSignature);
	}

	private InAppProduct GetPurchaseIdEnum(string productID)
	{
		if (productID == miniPackId)
		{
			return InAppProduct.MiniPack;
		}
		if (productID == smallPackId)
		{
			return InAppProduct.SmallPack;
		}
		if (productID == mediumPackId)
		{
			return InAppProduct.MediumPack;
		}
		if (productID == bigPackId)
		{
			return InAppProduct.BigPack;
		}
		if (productID == megaPackId)
		{
			return InAppProduct.MegaPack;
		}
		if (productID == exclusivePackId)
		{
			return InAppProduct.ExclusivePack;
		}
		return InAppProduct.None;
	}

	private void ProcessConsumablePurchase(InAppProduct productEnumId)
	{
		switch (productEnumId)
		{
		case InAppProduct.MiniPack:
			Unibiller.CreditBalance("credits", 1000m);
			UpdateStoreGUI(1000m);
			break;
		case InAppProduct.SmallPack:
			Unibiller.CreditBalance("credits", 5500m);
			UpdateStoreGUI(5500m);
			break;
		case InAppProduct.MediumPack:
			Unibiller.CreditBalance("credits", 11500m);
			UpdateStoreGUI(11500m);
			break;
		case InAppProduct.BigPack:
			Unibiller.CreditBalance("credits", 24000m);
			UpdateStoreGUI(24000m);
			break;
		case InAppProduct.MegaPack:
			Unibiller.CreditBalance("credits", 62500m);
			UpdateStoreGUI(62500m);
			break;
		case InAppProduct.ExclusivePack:
			Unibiller.CreditBalance("credits", 200000m);
			UpdateStoreGUI(200000m);
			break;
		case InAppProduct.RemoveAds:
		case InAppProduct.RemoveAdsOffer:
		case InAppProduct.StarterPack:
			break;
		}
	}

	private void ProcessNonConsumablePurchases(string productId = "")
	{
		if (Unibiller.GetPurchaseCount(removeAdsPurchaseId) > 0 || Unibiller.GetPurchaseCount(removeAdsOfferPurchaseId) > 0 || Unibiller.GetPurchaseCount(starterPackOfferId) > 0)
		{
			MySingleton<Manager>.Instance.ads.DisableAllAds();
		}
		if (Unibiller.GetPurchaseCount(starterPackOfferId) > 0 && !SettingsManager.GetStarterPackBoughtOrEnded())
		{
			StarterPackLogic();
		}
	}

	private void StarterPackLogic()
	{
		SettingsManager.SetStarterPackBoughtOrEnded(true);
		int indexArrayPosFromId = MySingleton<Manager>.Instance.GetIndexArrayPosFromId(24);
		InfoCar infoCar = MySingleton<Manager>.Instance.carsInfoPrefabs[indexArrayPosFromId];
		infoCar.isBought = true;
		ManagerAnalytics.instance.FireCarUnlocked(infoCar.idName, "starter_pack");
		CreditBalance(3500m);
		MySingleton<Manager>.Instance.ads.DisableAllAds();
		LaunchStarterPackBoughtEvent();
	}

	private void UpdateStoreGUI(decimal amount)
	{
	}

	private void onCancelled(PurchasableItem item)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("Purchase cancelled: " + item.Id);
		}
		MySingleton<Manager>.Instance.guiCurrentLogic.PurchaseCanceled(item);
		DisablePurchaseLoadingPanel();
	}

	private void onFailed(PurchaseFailedEvent ev)
	{
		if (Debug.isDebugBuild)
		{
			Debug.Log("Purchase failed: " + ev.Reason);
		}
		if (ev.PurchasedItem.PurchaseType == PurchaseType.NonConsumable && Unibiller.GetPurchaseCount(ev.PurchasedItem.Id) > 0)
		{
			RestorePurchases();
			return;
		}
		MySingleton<Manager>.Instance.guiCurrentLogic.PurchaseFailed(ev.PurchasedItem);
		DisablePurchaseLoadingPanel();
	}

	private void DisablePurchaseLoadingPanel()
	{
		if (MySingleton<Manager>.Instance.currentLoadingPurchasePanel != null)
		{
			Debug.Log("Destruyendo panel de carga");
			Object.Destroy(MySingleton<Manager>.Instance.currentLoadingPurchasePanel.gameObject);
		}
	}

	public static decimal GetCreditsAmount(InAppProduct productEnumId)
	{
		switch (productEnumId)
		{
		case InAppProduct.MiniPack:
			return 1000m;
		case InAppProduct.SmallPack:
			return 5500m;
		case InAppProduct.MediumPack:
			return 11500m;
		case InAppProduct.BigPack:
			return 24000m;
		case InAppProduct.MegaPack:
			return 62500m;
		case InAppProduct.ExclusivePack:
			return 200000m;
		default:
			return -1m;
		}
	}

	public decimal GetCreditsAmount(string productId)
	{
		return GetCreditsAmount(GetPurchaseIdEnum(productId));
	}

	public string GetProductId(InAppProduct product)
	{
		switch (product)
		{
		case InAppProduct.MiniPack:
			return miniPackId;
		case InAppProduct.SmallPack:
			return smallPackId;
		case InAppProduct.MediumPack:
			return mediumPackId;
		case InAppProduct.BigPack:
			return bigPackId;
		case InAppProduct.MegaPack:
			return megaPackId;
		case InAppProduct.RemoveAds:
			return removeAdsPurchaseId;
		case InAppProduct.RemoveAdsOffer:
			return removeAdsOfferPurchaseId;
		case InAppProduct.StarterPack:
			return starterPackOfferId;
		case InAppProduct.ExclusivePack:
			return exclusivePackId;
		default:
			return null;
		}
	}

	public PurchasableItem GetUnibillItem(InAppProduct product)
	{
		string productId = GetProductId(product);
		return Unibiller.GetPurchasableItemById(productId);
	}

	public void InitiatePurchase(PurchasableItem purchasableItem)
	{
		SetActive_OnPurchaseCompleteEvent(true);
		Unibiller.initiatePurchase(purchasableItem, string.Empty);
	}

	public void InitiatePurchase(string purchasableItemName)
	{
		SetActive_OnPurchaseCompleteEvent(true);
		Unibiller.initiatePurchase(purchasableItemName, string.Empty);
	}

	public void RestorePurchases()
	{
		SetActive_OnPurchaseCompleteEvent();
		Unibiller.restoreTransactions();
	}

	private void SetActive_OnPurchaseCompleteEvent(bool enable = false)
	{
		if (enable)
		{
			Unibiller.onPurchaseCompleteEvent -= onPurchased;
			Unibiller.onPurchaseCompleteEvent += onPurchased;
		}
		else
		{
			Unibiller.onPurchaseCompleteEvent -= onPurchased;
		}
	}

	public static void CreditBalance(decimal amount)
	{
		Unibiller.CreditBalance("credits", amount);
	}

	private void LaunchStarterPackBoughtEvent()
	{
		if (StarterPackBoughtEvent != null)
		{
			StarterPackBoughtEvent();
		}
	}

	private double GetDollarPriceFromID(string id)
	{
		switch (id)
		{
		case "com.aim.racing.removeadsoffer":
			return 0.99;
		case "com.aim.racing.minipack":
			return 0.99;
		case "aimracing_remove_ads":
			return 1.99;
		case "com.aim.racing.starterpack":
			return 2.99;
		case "com.aim.racing.smallpack":
			return 4.99;
		case "com.aim.racing.mediumpack":
			return 9.99;
		case "com.aim.racing.bigpack":
			return 19.99;
		case "com.aim.racing.megapack":
			return 49.99;
		case "com.aim.racing.supermegapack":
			return 99.99;
		default:
			return 0.0;
		}
	}
}
