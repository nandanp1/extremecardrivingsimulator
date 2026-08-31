using System;
using System.Collections.Generic;
using Unibill;
using Unibill.Impl;

namespace Tests
{
	public class FakeBillingService : IAppleExtensions, IBillingService
	{
		private IBillingServiceCallback biller;

		private List<string> purchasedItems = new List<string>();

		private ProductIdRemapper remapper;

		public bool purchaseCalled;

		public bool restoreCalled;

		public event Action<string> onAppReceiptRefreshed;

		public event Action onAppReceiptRefreshFailed;

		public FakeBillingService(ProductIdRemapper remapper)
		{
			this.remapper = remapper;
		}

		public void initialise(IBillingServiceCallback biller)
		{
			this.biller = biller;
			List<ProductDescription> list = new List<ProductDescription>();
			string[] allPlatformSpecificProductIds = remapper.getAllPlatformSpecificProductIds();
			foreach (string id in allPlatformSpecificProductIds)
			{
				list.Add(new ProductDescription(id, "$123.45", "Fake title", "Fake description", "USD", 123.45m));
			}
			biller.onSetupComplete(list);
		}

		public void purchase(string item, string developerPayload)
		{
			purchaseCalled = true;
			if (remapper.getPurchasableItemFromPlatformSpecificId(item).PurchaseType == PurchaseType.NonConsumable)
			{
				purchasedItems.Add(item);
			}
			biller.onPurchaseReceiptRetrieved(item, "fake receipt");
			biller.onPurchaseSucceeded(item, "{ \"this\" : \"is a fake receipt\" }", Guid.NewGuid().ToString());
		}

		public void restoreTransactions()
		{
			restoreCalled = true;
			foreach (string purchasedItem in purchasedItems)
			{
				biller.onPurchaseSucceeded(purchasedItem, "{ \"this\" : \"is a fake receipt\" }", "1");
			}
			biller.onTransactionsRestoredSuccess();
		}

		public void finishTransaction(PurchasableItem item, string transactionId)
		{
		}

		public void registerPurchaseForRestore(string productId)
		{
			purchasedItems.Add(productId);
		}

		public void refreshAppReceipt()
		{
			if (onAppReceiptRefreshed != null)
			{
				onAppReceiptRefreshed("fake!");
			}
		}
	}
}
