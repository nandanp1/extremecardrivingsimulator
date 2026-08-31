namespace Unibill
{
	public class PurchaseEvent
	{
		public PurchasableItem PurchasedItem { get; private set; }

		public bool IsNewPurchase { get; private set; }

		public string Receipt { get; private set; }

		public string TransactionId { get; private set; }

		internal PurchaseEvent(PurchasableItem purchasedItem, bool isNewPurchase, string receipt, string transactionId)
		{
			PurchasedItem = purchasedItem;
			IsNewPurchase = isNewPurchase;
			Receipt = receipt;
			TransactionId = transactionId;
		}
	}
}
