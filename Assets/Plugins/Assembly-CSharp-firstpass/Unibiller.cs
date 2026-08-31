using System;
using System.Collections.Generic;
using Unibill;
using Unibill.Impl;
using Uniject.Impl;
using UnityEngine;

public class Unibiller
{
	private const string OFFLINE_CURRENCY_PREFIX = "com.outlinegames.unibill.currencies.";

	private const string OFFLINE_CURRENCY_SUFFIX = ".balance";

	private static readonly UnityPlayerPrefsStorage offlineStorage = new UnityPlayerPrefsStorage();

	private static readonly Dictionary<string, PurchasableItem> offlineItems = new Dictionary<string, PurchasableItem>();

	public static BillingPlatform BillingPlatform
	{
		get
		{
			return BillingPlatform.UnityEditor;
		}
	}

	public static bool Initialised
	{
		get
		{
			return true;
		}
	}

	public static UnibillError[] Errors
	{
		get
		{
			return new UnibillError[0];
		}
	}

	public static PurchasableItem[] AllPurchasableItems
	{
		get
		{
			return new List<PurchasableItem>(offlineItems.Values).ToArray();
		}
	}

	public static PurchasableItem[] AllNonConsumablePurchasableItems
	{
		get
		{
			return new PurchasableItem[0];
		}
	}

	public static PurchasableItem[] AllConsumablePurchasableItems
	{
		get
		{
			return new PurchasableItem[0];
		}
	}

	public static PurchasableItem[] AllSubscriptions
	{
		get
		{
			return new PurchasableItem[0];
		}
	}

	public static string[] AllCurrencies
	{
		get
		{
			return new string[1] { "credits" };
		}
	}

	public static event Action<UnibillState> onBillerReady;

	public static event Action<PurchasableItem> onPurchaseCancelled;

	public static event Action<PurchaseEvent> onPurchaseCompleteEvent;

	public static event Action<PurchasableItem> onPurchaseComplete;

	public static event Action<PurchaseFailedEvent> onPurchaseFailed;

	public static event Action<PurchasableItem> onPurchaseDeferred;

	public static event Action<PurchasableItem> onPurchaseRefunded;

	public static event Action<bool> onTransactionsRestored;

	public static void Initialise(List<ProductDefinition> runtimeProducts = null)
	{
		if (onBillerReady != null)
		{
			onBillerReady(UnibillState.SUCCESS);
		}
	}

	public static PurchasableItem GetPurchasableItemById(string unibillPurchasableId)
	{
		string text = unibillPurchasableId;
		if (string.IsNullOrEmpty(text))
		{
			text = "offline_unavailable_product";
		}
		PurchasableItem value;
		if (!offlineItems.TryGetValue(text, out value))
		{
			value = new PurchasableItem();
			value.Id = text;
			value.name = text;
			value.description = "Purchases are unavailable in the offline build.";
			value.localizedTitle = value.name;
			value.localizedDescription = value.description;
			value.localizedPrice = 0m;
			value.localizedPriceString = "Unavailable";
			value.priceInLocalCurrency = 0m;
			value.isoCurrencySymbol = string.Empty;
			value.AvailableToPurchase = false;
			offlineItems.Add(text, value);
		}
		return value;
	}

	public static void initiatePurchase(PurchasableItem purchasable, string developerPayload = "")
	{
		if (onPurchaseCancelled != null)
		{
			onPurchaseCancelled(purchasable ?? GetPurchasableItemById(null));
		}
	}

	public static void initiatePurchase(string purchasableId, string developerPayload = "")
	{
		initiatePurchase(GetPurchasableItemById(purchasableId), developerPayload);
	}

	public static int GetPurchaseCount(PurchasableItem item)
	{
		return 0;
	}

	public static int GetPurchaseCount(string purchasableId)
	{
		return 0;
	}

	public static decimal GetCurrencyBalance(string currencyIdentifier)
	{
		return offlineStorage.GetInt(GetOfflineCurrencyKey(currencyIdentifier), 0);
	}

	public static void CreditBalance(string currencyIdentifier, decimal amount)
	{
		if (amount <= 0m)
		{
			return;
		}
		decimal num = GetCurrencyBalance(currencyIdentifier) + amount;
		if (num > 2147483647m)
		{
			num = 2147483647m;
		}
		offlineStorage.SetInt(GetOfflineCurrencyKey(currencyIdentifier), (int)num);
		PlayerPrefs.Save();
	}

	public static bool DebitBalance(string currencyIdentifier, decimal amount)
	{
		decimal currencyBalance = GetCurrencyBalance(currencyIdentifier);
		if (amount >= 0m && currencyBalance >= amount)
		{
			offlineStorage.SetInt(GetOfflineCurrencyKey(currencyIdentifier), (int)(currencyBalance - amount));
			PlayerPrefs.Save();
			return true;
		}
		return false;
	}

	public static void restoreTransactions()
	{
		if (onTransactionsRestored != null)
		{
			onTransactionsRestored(false);
		}
	}

	public static void clearTransactions()
	{
	}

	public static IAppleExtensions getAppleExtensions()
	{
		return null;
	}

	private static string GetOfflineCurrencyKey(string currencyIdentifier)
	{
		return OFFLINE_CURRENCY_PREFIX + (currencyIdentifier ?? string.Empty) + OFFLINE_CURRENCY_SUFFIX;
	}

	public static void _internal_hook_events(Biller biller, BillerFactory factory)
	{
		biller.onBillerReady += (bool success) =>
		{
			if (onBillerReady != null)
			{
				if (success)
				{
					onBillerReady((biller.State != BillerState.INITIALISED) ? UnibillState.SUCCESS_WITH_ERRORS : UnibillState.SUCCESS);
				}
				else
				{
					onBillerReady(UnibillState.CRITICAL_ERROR);
				}
			}
		};
		biller.onPurchaseComplete += _onPurchaseComplete;
		biller.onPurchaseFailed += _onPurchaseFailed;
		biller.onPurchaseDeferred += _onPurchaseDeferred;
		biller.onPurchaseRefunded += _onPurchaseRefunded;
		biller.onTransactionsRestored += _onTransactionsRestored;
	}

	private static void _onPurchaseComplete(PurchaseEvent e)
	{
		if (onPurchaseComplete != null)
		{
			onPurchaseComplete(e.PurchasedItem);
		}
		if (onPurchaseCompleteEvent != null)
		{
			onPurchaseCompleteEvent(e);
		}
	}

	private static void _onPurchaseFailed(PurchaseFailedEvent e)
	{
		if (onPurchaseFailed != null)
		{
			onPurchaseFailed(e);
		}
	}

	private static void _onPurchaseDeferred(PurchasableItem item)
	{
		if (onPurchaseDeferred != null)
		{
			onPurchaseDeferred(item);
		}
	}

	private static void _onPurchaseRefunded(PurchasableItem item)
	{
		if (onPurchaseRefunded != null)
		{
			onPurchaseRefunded(item);
		}
	}

	private static void _onTransactionsRestored(bool success)
	{
		if (onTransactionsRestored != null)
		{
			onTransactionsRestored(success);
		}
	}
}
