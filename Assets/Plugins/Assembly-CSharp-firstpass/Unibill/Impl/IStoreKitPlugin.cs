using Uniject;

namespace Unibill.Impl
{
	public interface IStoreKitPlugin
	{
		void initialise(AppleAppStoreBillingService callback, IUtil util);

		bool unibillPaymentsAvailable();

		void unibillRequestProductData(string payload);

		void unibillPurchaseProduct(string productId);

		void finishTransaction(string transactionIdentifier);

		void unibillRestoreTransactions();

		void addTransactionObserver();

		void refreshAppReceipt();
	}
}
