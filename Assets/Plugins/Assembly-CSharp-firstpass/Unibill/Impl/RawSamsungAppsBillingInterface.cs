using UnityEngine;

namespace Unibill.Impl
{
	public class RawSamsungAppsBillingInterface : IRawSamsungAppsBillingService
	{
		private AndroidJavaObject samsung;

		public RawSamsungAppsBillingInterface()
		{
			using (AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.outlinegames.unibill.samsung.Unibill"))
			{
				samsung = androidJavaClass.CallStatic<AndroidJavaObject>("instance", new object[0]);
			}
		}

		public void initialise(SamsungAppsBillingService samsung)
		{
			new GameObject().AddComponent<SamsungAppsCallbackMonoBehaviour>().initialise(samsung);
		}

		public void getProductList(string json)
		{
			samsung.Call("initialise", json);
		}

		public void initiatePurchaseRequest(string productId)
		{
			samsung.Call("initiatePurchaseRequest", productId);
		}

		public void restoreTransactions()
		{
			samsung.Call("restoreTransactions");
		}
	}
}
