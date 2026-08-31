using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class CarButtonInfo : MonoBehaviour
{
	private int _arrayPos;

	public GameObject newCarUnlocked;

	public int arrayPos
	{
		get
		{
			return _arrayPos;
		}
	}

	public void SetId(int id)
	{
		_arrayPos = id;
		CheckIfisNew();
	}

	private void CheckIfisNew()
	{
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		InfoCar infoCar = carsInfoPrefabs[_arrayPos];
		if (ObscuredPrefs.HasKey(infoCar.id + "keyDistanceDroveByCar") || !infoCar.isBought)
		{
			newCarUnlocked.SetActive(false);
		}
		else
		{
			newCarUnlocked.SetActive(true);
		}
	}
}
