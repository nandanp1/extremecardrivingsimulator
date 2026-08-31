using System;
using System.Collections.Generic;
using Unibill.Impl;
using Uniject;

public class TransactionDatabase
{
	private const string TRANSACTION_SET_KEY = "com.outlinegames.unibill.transactionlog";

	private IStorage storage;

	private ILogger logger;

	private List<object> recentTransactionIdentifiers;

	public string UserId { get; set; }

	public TransactionDatabase(IStorage storage, ILogger logger)
	{
		this.storage = storage;
		this.logger = logger;
		UserId = "default";
		string json = storage.GetString("com.outlinegames.unibill.transactionlog", "[]");
		recentTransactionIdentifiers = json.arrayListFromJson();
	}

	public int getPurchaseHistory(PurchasableItem item)
	{
		return storage.GetInt(getKey(item.Id), 0);
	}

	public bool recordPurchase(PurchasableItem item, string transactionId)
	{
		int purchaseHistory = getPurchaseHistory(item);
		if (item.PurchaseType == PurchaseType.NonConsumable && purchaseHistory != 0)
		{
			logger.LogWarning("Apparently multi purchased a non consumable:{0}", item.Id);
			return false;
		}
		if (item.PurchaseType == PurchaseType.Consumable && transactionId != null)
		{
			if (recentTransactionIdentifiers.Contains(transactionId))
			{
				logger.Log("Transaction {0} already recorded.", transactionId);
				return false;
			}
			if (recentTransactionIdentifiers.Count > 20)
			{
				recentTransactionIdentifiers.RemoveAt(0);
			}
			recentTransactionIdentifiers.Add(transactionId);
			storage.SetString("com.outlinegames.unibill.transactionlog", MiniJSON.jsonEncode(recentTransactionIdentifiers));
		}
		storage.SetInt(getKey(item.Id), purchaseHistory + 1);
		return true;
	}

	public void clearPurchases(PurchasableItem item)
	{
		storage.SetInt(getKey(item.Id), 0);
	}

	public void onRefunded(PurchasableItem item)
	{
		int purchaseHistory = getPurchaseHistory(item);
		purchaseHistory = Math.Max(0, purchaseHistory - 1);
		storage.SetInt(getKey(item.Id), purchaseHistory);
	}

	private string getKey(string fragment)
	{
		return string.Format("{0}.{1}", UserId, fragment);
	}
}
