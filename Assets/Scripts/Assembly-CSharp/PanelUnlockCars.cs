using System.Collections.Generic;
using UnityEngine;

public class PanelUnlockCars : MonoBehaviour
{
	public GameObject carEntrySource;

	public UIGrid carEntryGrid;

	public GameObject[] carEntryArray;

	public GameObject collectablesPanel;

	public GameObject collectablesTab;

	public GameObject challengesPanel;

	public GameObject challengesTab;

	public GameObject patherPanel;

	public UIScrollView carsScroll;

	private int carEntryNumber;

	private void Start()
	{
	}

	public void Init(List<CarEntryInfo> carList)
	{
		int count = carList.Count;
		carEntryArray = new GameObject[count];
		CarEntryInfo carInfo;
		for (int i = 1; i < count; i++)
		{
			carEntryArray[i] = Object.Instantiate(carEntrySource) as GameObject;
			carEntryArray[i].transform.parent = carEntryGrid.transform;
			carEntryArray[i].transform.localScale = Vector3.one;
			carInfo = carList[i];
			carEntryArray[i].GetComponent<CarEntryBehaviour>().Init(carInfo);
		}
		carInfo = carList[0];
		carEntryArray[0] = carEntrySource;
		carEntryArray[0].GetComponent<CarEntryBehaviour>().Init(carInfo);
		carEntryGrid.repositionNow = true;
	}

	public void ClearAll()
	{
		GameObject gameObject = carEntryArray[0];
		int num = carEntryArray.Length;
		for (int i = 1; i < num; i++)
		{
			Object.DestroyImmediate(carEntryArray[i].gameObject, true);
		}
		carEntryArray = null;
		gameObject.GetComponent<CarEntryBehaviour>().SetToDefault();
		carEntrySource = gameObject;
	}

	public void GoToCollectables()
	{
		patherPanel.GetComponent<UIAchievementsBehaviour>().OnTabClicked(collectablesPanel);
		patherPanel.GetComponent<UIAchievementsBehaviour>().OnButtonTabClicked(collectablesTab);
	}

	public void GoToChallenges()
	{
		patherPanel.GetComponent<UIAchievementsBehaviour>().OnTabClicked(challengesPanel);
		patherPanel.GetComponent<UIAchievementsBehaviour>().OnButtonTabClicked(challengesTab);
	}

	public void GoToGarage()
	{
		MySingleton<Manager>.Instance.carUnlockedId = MySingleton<Manager>.Instance.indexCurrentCar;
		if (LegacySceneManager.LoadedLevelName != "1Menu")
		{
			MySingleton<Manager>.Instance.guiGameLogic.BackToMenu();
			return;
		}
		MySingleton<Manager>.Instance.guiMenuLogic.ActiveProgressPanel(false);
		MySingleton<Manager>.Instance.guiMenuLogic.ShowSelectCar(true, true);
	}

	public void HidePanel()
	{
		patherPanel.gameObject.SetActive(false);
	}
}
