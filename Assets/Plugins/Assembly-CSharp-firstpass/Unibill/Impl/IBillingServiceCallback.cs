using System.Collections.Generic;

namespace Unibill.Impl
{
	public interface IBillingServiceCallback
	{
		void onSetupComplete(bool successful);

		void onSetupComplete(List<ProductDescription> products);

		void onPurchaseSucceeded(string platformSpecificId, string receipt, string transactionIdentifier);

		void onPurchaseRefundedEvent(string item);

		void onPurchaseFailedEvent(PurchaseFailureDescription desc);

		void onPurchaseDeferredEvent(string item);

		void onTransactionsRestoredSuccess();

		void onTransactionsRestoredFail(string error);

		void onPurchaseReceiptRetrieved(string productId, string receipt);

		void setAppReceipt(string receipt);

		void logError(UnibillError error, params object[] args);

		void logError(UnibillError error);
	}
}
