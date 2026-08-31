using System.Collections.Generic;

namespace Unibill.Impl
{
	public class SamsungAppsBillingService : IBillingService
	{
		private IBillingServiceCallback callback;

		private UnibillConfiguration config;

		private IRawSamsungAppsBillingService rawSamsung;

		private HashSet<string> unknownSamsungProducts = new HashSet<string>();

		public SamsungAppsBillingService(UnibillConfiguration config, IRawSamsungAppsBillingService rawSamsung)
		{
			this.config = config;
			this.rawSamsung = rawSamsung;
		}

		public void initialise(IBillingServiceCallback biller)
		{
			callback = biller;
			rawSamsung.initialise(this);
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary.Add("mode", config.SamsungAppsMode.ToString());
			dictionary.Add("itemGroupId", config.SamsungItemGroupId);
			rawSamsung.getProductList(dictionary.toJson());
		}

		public void purchase(string item, string developerPayload)
		{
			if (unknownSamsungProducts.Contains(item))
			{
				callback.logError(UnibillError.SAMSUNG_APPS_ATTEMPTING_TO_PURCHASE_PRODUCT_NOT_RETURNED_BY_SAMSUNG, item);
				callback.onPurchaseFailedEvent(new PurchaseFailureDescription(item, PurchaseFailureReason.ITEM_UNAVAILABLE, null));
			}
			else
			{
				rawSamsung.initiatePurchaseRequest(item);
			}
		}

		public void restoreTransactions()
		{
			rawSamsung.restoreTransactions();
		}

		public void onProductListReceived(string productListString)
		{
			callback.onSetupComplete(Util.DeserialiseProductList(productListString));
		}

		public void onPurchaseFailed(string item)
		{
			callback.onPurchaseFailedEvent(new PurchaseFailureDescription(item, PurchaseFailureReason.UNKNOWN, null));
		}

		public void onPurchaseCancelled(string item)
		{
			callback.onPurchaseFailedEvent(new PurchaseFailureDescription(item, PurchaseFailureReason.USER_CANCELLED, null));
		}

		public void onPurchaseSucceeded(string json)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			callback.onPurchaseSucceeded((string)dictionary["productId"], (string)dictionary["signature"], null);
		}

		public void onTransactionsRestored(string success)
		{
			if (bool.Parse(success))
			{
				callback.onTransactionsRestoredSuccess();
			}
			else
			{
				callback.onTransactionsRestoredFail(string.Empty);
			}
		}

		public void onInitFail()
		{
			callback.onSetupComplete(false);
		}

		public void finishTransaction(PurchasableItem item, string transactionId)
		{
		}
	}
}
