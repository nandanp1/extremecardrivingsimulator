using System;

public class GooglePurchaseTemplate
{
	public string orderId;

	public string packageName;

	public string SKU;

	public string developerPayload;

	public string signature;

	public string token;

	public long time;

	public string originalJson;

	public GooglePurchaseState state;

	public void SetState(string code)
	{
		switch (Convert.ToInt32(code))
		{
		case 0:
			state = GooglePurchaseState.PURCHASED;
			break;
		case 1:
			state = GooglePurchaseState.CANCELED;
			break;
		case 2:
			state = GooglePurchaseState.REFUNDED;
			break;
		}
	}
}
