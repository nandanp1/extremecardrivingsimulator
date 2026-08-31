using System;
using System.Collections.Generic;
using UnityEngine;

public class UIAchievementsBehaviour : MonoBehaviour
{
	private const string UI_ROOT_NAME_GAME_OBJECT = "UI Root";

	public PanelUnlockCars panelCars;

	public PanelChallengeScores panelScores;

	public PanelCollectables panelCollectables;

	public PanelAchievements panelAchievements;

	public GameObject[] TabsButtons;

	public GameObject[] TabsPanels;

	public UIGrid tabsGrid;

	public int tabButtonSelectedHeight = 70;

	private int CurrentTabIndex;

	private string CurrentTabName = string.Empty;

	private int tabButtonStandarHeight;

	private GameObject currentUIRootDisabled;

	public GameObject MainCameraGame2;

	private void Start()
	{
		currentUIRootDisabled = GameObject.Find("UI Root");
		HideOtherUIRoot(true);
		tabButtonStandarHeight = TabsButtons[0].GetComponent<UISprite>().height;
		TabsButtons[0].GetComponent<UISprite>().height = tabButtonStandarHeight + tabButtonSelectedHeight;
		panelCars.gameObject.SetActive(true);
		panelScores.gameObject.SetActive(false);
		panelCollectables.gameObject.SetActive(false);
		panelAchievements.gameObject.SetActive(false);
	}

	private void Update()
	{
	}

	public void Init(List<CarEntryInfo> carList, List<List<ChallengeEntryInfo>> challengeList, List<List<CollectablesEntryInfo>> collectionsList, List<List<AchievementsEntryInfo>> achievementsList)
	{
		panelCars.Init(carList);
		panelScores.Init(challengeList);
		panelCollectables.Init(collectionsList);
		panelAchievements.Init(achievementsList);
	}

	public void ClearAll()
	{
		panelCars.ClearAll();
		panelScores.ClearPanel();
	}

	public void OnTabClicked(GameObject origen)
	{
		if (CurrentTabName != origen.name)
		{
		}
		CurrentTabName = origen.name;
		string[] array = CurrentTabName.Split('-');
		CurrentTabIndex = Convert.ToInt32(array[1]) - 1;
		GameObject[] tabsPanels = TabsPanels;
		foreach (GameObject gameObject in tabsPanels)
		{
			if (gameObject != origen)
			{
				gameObject.SetActive(false);
			}
			origen.gameObject.SetActive(true);
		}
	}

	public void OnButtonTabClicked(GameObject Button)
	{
		GameObject[] tabsButtons = TabsButtons;
		foreach (GameObject gameObject in tabsButtons)
		{
			gameObject.GetComponent<UISprite>().height = tabButtonStandarHeight;
		}
		Button.GetComponent<UISprite>().height = tabButtonStandarHeight + tabButtonSelectedHeight;
	}

	public void OnExitClicked()
	{
		HideOtherUIRoot(false);
		if ((bool)MySingleton<Manager>.Instance.guiMenuLogic)
		{
			MySingleton<Manager>.Instance.guiMenuLogic.ActiveProgressPanel(false);
		}
		else if ((bool)MySingleton<Manager>.Instance.guiGameLogic)
		{
			MySingleton<Manager>.Instance.guiGameLogic.ActiveProgressPanel(false);
		}
	}

	private void HideOtherUIRoot(bool active)
	{
		if (!currentUIRootDisabled)
		{
			Debug.LogError("Revisar que el UI Root de la escena se llama como indica la constante UI_ROOT_NAME_GAME_OBJECT");
		}
		NGUITools.SetActive(currentUIRootDisabled.gameObject, !active);
		NGUITools.SetActive(MainCameraGame2, !active);
	}

	public void SetCamera(GameObject camera)
	{
		MainCameraGame2 = camera;
	}
}
