namespace Unibill.Impl
{
	public interface IRawGooglePlayInterface
	{
		void initialise(GooglePlayBillingService callback, string publicKey, string[] productIds);

		void purchase(string json);

		void finishTransaction(string transactionId);

		void restoreTransactions();
	}
}
