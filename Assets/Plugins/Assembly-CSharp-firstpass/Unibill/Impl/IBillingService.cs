namespace Unibill.Impl
{
	public interface IBillingService
	{
		void initialise(IBillingServiceCallback biller);

		void purchase(string item, string developerPayload);

		void finishTransaction(PurchasableItem item, string transactionId);

		void restoreTransactions();
	}
}
