namespace Unibill
{
	public class PurchaseFailedEvent
	{
		public PurchasableItem PurchasedItem { get; private set; }

		public PurchaseFailureReason Reason { get; private set; }

		public string Message { get; private set; }

		internal PurchaseFailedEvent(PurchasableItem purchasedItem, PurchaseFailureReason reason, string message)
		{
			PurchasedItem = purchasedItem;
			Reason = reason;
			Message = message;
		}
	}
}
