using System;
using System.Collections.Generic;
using Uniject;

namespace Unibill.Impl
{
	public class AppleAppStoreBillingService : IAppleExtensions, IBillingService
	{
		private IBillingServiceCallback biller;

		private ProductIdRemapper remapper;

		private ILogger logger;

		private bool restoreInProgress;

		public IStoreKitPlugin storekit { get; private set; }

		public event Action<string> onAppReceiptRefreshed;

		public event Action onAppReceiptRefreshFailed;

		public AppleAppStoreBillingService(ProductIdRemapper mapper, IStoreKitPlugin storekit, IUtil util, ILogger logger)
		{
			this.storekit = storekit;
			remapper = mapper;
			this.logger = logger;
			storekit.initialise(this, util);
		}

		public void initialise(IBillingServiceCallback biller)
		{
			this.biller = biller;
			if (storekit.unibillPaymentsAvailable())
			{
				storekit.unibillRequestProductData(remapper.getAllPlatformSpecificProductIds().toJson());
				return;
			}
			biller.logError(UnibillError.STOREKIT_BILLING_UNAVAILABLE);
			biller.onSetupComplete(false);
		}

		public void purchase(string item, string developerPayload)
		{
			storekit.unibillPurchaseProduct(item);
		}

		public void restoreTransactions()
		{
			restoreInProgress = true;
			storekit.unibillRestoreTransactions();
		}

		public void refreshAppReceipt()
		{
			storekit.refreshAppReceipt();
		}

		public void onProductListReceived(string productListString)
		{
			if (productListString.Length == 0)
			{
				biller.logError(UnibillError.STOREKIT_RETURNED_NO_PRODUCTS);
				biller.onSetupComplete(false);
				return;
			}
			Dictionary<string, object> dic = (Dictionary<string, object>)MiniJSON.jsonDecode(productListString);
			Dictionary<string, object> hash = dic.getHash("products");
			List<ProductDescription> products = Util.DeserialiseProductList(hash);
			storekit.addTransactionObserver();
			string receipt = dic.getString("appReceipt", string.Empty);
			onAppReceiptRetrieved(receipt);
			biller.onSetupComplete(products);
		}

		public void onPurchaseSucceeded(string data, string receipt)
		{
			Dictionary<string, object> dictionary = (Dictionary<string, object>)MiniJSON.jsonDecode(data);
			string text = (string)dictionary["productId"];
			string transactionIdentifier = dictionary.getString("transactionIdentifier", string.Empty);
			if (restoreInProgress && remapper.canMapProductSpecificId(text) && remapper.getPurchasableItemFromPlatformSpecificId(text).PurchaseType == PurchaseType.Consumable)
			{
				logger.Log("Ignoring restore of consumable: " + text);
				storekit.finishTransaction(transactionIdentifier);
			}
			else
			{
				biller.onPurchaseSucceeded(text, receipt, transactionIdentifier);
			}
		}

		public void onPurchaseFailed(string jsonHash)
		{
			biller.onPurchaseFailedEvent(new PurchaseFailureDescription(jsonHash));
		}

		public void onPurchaseDeferred(string productId)
		{
			biller.onPurchaseDeferredEvent(productId);
		}

		public void onTransactionsRestoredSuccess()
		{
			restoreInProgress = false;
			biller.onTransactionsRestoredSuccess();
		}

		public void onTransactionsRestoredFail(string error)
		{
			restoreInProgress = false;
			biller.onTransactionsRestoredFail(error);
		}

		public void onFailedToRetrieveProductList()
		{
			biller.logError(UnibillError.STOREKIT_FAILED_TO_RETRIEVE_PRODUCT_DATA);
			biller.onSetupComplete(true);
		}

		public void finishTransaction(PurchasableItem item, string transactionId)
		{
			logger.Log("Finishing transaction " + transactionId);
			storekit.finishTransaction(transactionId);
		}

		public void onAppReceiptRetrieved(string receipt)
		{
			if (receipt != null)
			{
				biller.setAppReceipt(receipt);
				if (onAppReceiptRefreshed != null)
				{
					onAppReceiptRefreshed(receipt);
				}
			}
		}

		public void onAppReceiptRefreshedFailed()
		{
			if (onAppReceiptRefreshFailed != null)
			{
				onAppReceiptRefreshFailed();
			}
		}
	}
}
