using UnityEngine;

public class CarPacksShowcase : MonoBehaviour
{
	public void Start()
	{
		if (MySingleton<Manager>.Instance.selectedGame == Manager.SelectedGame.SanFrancisco)
		{
			if (MySingleton<Manager>.Instance.storeInitData.packID != InfoCar.PurchasablePack.none)
			{
				MySingleton<Manager>.Instance.guiStoreLogic.PutCarSpinnerNew(MySingleton<Manager>.Instance.storeInitData.packID);
			}
		}
		else
		{
			MySingleton<Manager>.Instance.guiStoreLogic.PutCarSpinner(MySingleton<Manager>.Instance.indexLastCarInstantiate);
		}
	}

	public void OnPackSelected(BillingManager.CarPackId pack)
	{
	}

	public void OnCarSelected(int carId)
	{
		MySingleton<Manager>.Instance.guiStoreLogic.PutCarSpinner(carId);
	}
}
