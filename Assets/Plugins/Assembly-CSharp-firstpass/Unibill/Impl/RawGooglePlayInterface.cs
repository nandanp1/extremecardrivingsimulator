using UnityEngine;

namespace Unibill.Impl
{
	public class RawGooglePlayInterface : IRawGooglePlayInterface
	{
		private AndroidJavaObject plugin;

		public void initialise(GooglePlayBillingService callback, string publicKey, string[] productIds)
		{
			new GameObject().AddComponent<GooglePlayCallbackMonoBehaviour>().Initialise(callback);
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.outlinegames.unibill.UniBill"))
			{
				plugin = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
			plugin.Call("initialise", publicKey);
		}

		public void restoreTransactions()
		{
			plugin.Call("restoreTransactions");
		}

		public void purchase(string json)
		{
			plugin.Call("purchaseProduct", json);
		}

		public void pollForConsumables()
		{
			plugin.Call("pollForConsumables");
		}

		public void finishTransaction(string transactionId)
		{
			plugin.Call("finishTransaction", transactionId);
		}
	}
}
