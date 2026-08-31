using System.Collections.Generic;
using Uniject;

namespace Unibill.Impl
{
	public class AmazonAppStoreBillingService : IBillingService
	{
		private IBillingServiceCallback callback;

		private ProductIdRemapper remapper;

		private ILogger logger;

		private IRawAmazonAppStoreBillingInterface amazon;

		private TransactionDatabase tDb;

		private bool finishedSetup;

		public AmazonAppStoreBillingService(IRawAmazonAppStoreBillingInterface amazon, ProductIdRemapper remapper, TransactionDatabase tDb, ILogger logger)
		{
			this.remapper = remapper;
			this.logger = logger;
			logger.prefix = "UnibillAmazonBillingService";
			this.amazon = amazon;
			this.tDb = tDb;
		}

		public void initialise(IBillingServiceCallback biller)
		{
			callback = biller;
			amazon.initialise(this);
			amazon.initiateItemDataRequest(remapper.getAllPlatformSpecificProductIds());
		}

		public void purchase(string item, string developerPayload)
		{
			amazon.initiatePurchaseRequest(item);
		}

		public void restoreTransactions()
		{
			amazon.restoreTransactions();
		}

		public void onSDKAvailable(string isSandbox)
		{
			bool flag = bool.Parse(isSandbox);
			logger.Log("Running against {0} Amazon environment", (!flag) ? "PRODUCTION" : "SANDBOX");
		}

		public void onGetItemDataFailed()
		{
			callback.logError(UnibillError.AMAZONAPPSTORE_GETITEMDATAREQUEST_FAILED);
			callback.onSetupComplete(false);
		}

		private void onUserIdRetrieved(string userId)
		{
			tDb.UserId = userId;
		}

		public void onPurchaseFailed(string json)
		{
			callback.onPurchaseFailedEvent(new PurchaseFailureDescription(json));
		}

		public void onPurchaseSucceeded(string json)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			string platformSpecificId = (string)dictionary["productId"];
			string receipt = (string)dictionary["purchaseToken"];
			string transactionIdentifier = (string)dictionary["transactionId"];
			callback.onPurchaseSucceeded(platformSpecificId, receipt, transactionIdentifier);
		}

		public void onPurchaseUpdateFailed()
		{
			logger.LogWarning("AmazonAppStoreBillingService: onPurchaseUpdate() failed.");
		}

		public void onPurchaseUpdateSuccess(string json)
		{
			Dictionary<string, object> dic = (Dictionary<string, object>)MiniJSON.jsonDecode(json);
			onUserIdRetrieved(dic.getString("userId", string.Empty));
			if (!finishedSetup)
			{
				List<ProductDescription> products = Util.DeserialiseProductList(dic.getHash("products"));
				callback.onSetupComplete(products);
				finishedSetup = true;
			}
			else
			{
				callback.onTransactionsRestoredSuccess();
			}
		}

		public void finishTransaction(PurchasableItem item, string transactionId)
		{
			amazon.finishTransaction(transactionId);
		}
	}
}
