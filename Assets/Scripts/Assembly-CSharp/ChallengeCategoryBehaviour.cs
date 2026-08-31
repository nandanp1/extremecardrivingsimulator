using System.Collections.Generic;
using UnityEngine;

public class ChallengeCategoryBehaviour : MonoBehaviour
{
	public const string TRIAL_NAME = "challange_trial_name";

	public const string RADAR_NAME = "challange_radar_name";

	public const string CRASH_NAME = "challange_crashable_name";

	public GameObject challengeEntrySource;

	public GameObject[] challengeEntryArray;

	public UIGrid challengeEntryGrid;

	public UILabel challengeCategoryNameLabel;

	public UIProgressBar challengeCategoryProgressBar;

	public UILabel progressLabel;

	public UISprite challengeCategorySprite;

	public UISprite carRewardSprite;

	private int challengesNumber;

	private int challengesCompleted;

	public bool carButtonEnable;

	public void Init(List<ChallengeEntryInfo> entryList, string categorySpriteName, bool carButton, InfoCar infoCarReward)
	{
		carButtonEnable = carButton;
		challengeCategorySprite.spriteName = categorySpriteName;
		ChallengeEntryInfo challengeEntryInfo = entryList[0];
		if (carButton)
		{
			InitCarButton(infoCarReward);
		}
		switch (challengeEntryInfo.challengeCategory)
		{
		case ChallengeKind.Crash:
			challengeCategoryNameLabel.text = Localization.Get("challange_crashable_name");
			break;
		case ChallengeKind.Radar:
			challengeCategoryNameLabel.text = Localization.Get("challange_radar_name");
			break;
		case ChallengeKind.Trial:
			challengeCategoryNameLabel.text = Localization.Get("challange_trial_name");
			break;
		}
		challengeEntryArray = new GameObject[entryList.Count];
		challengesNumber = entryList.Count;
		for (int i = 1; i < entryList.Count; i++)
		{
			challengeEntryArray[i] = Object.Instantiate(challengeEntrySource) as GameObject;
			challengeEntryArray[i].GetComponent<ChallengeEntryBehaviour>().Init(entryList[i]);
			challengeEntryArray[i].transform.parent = challengeEntryGrid.transform;
			challengeEntryArray[i].transform.localScale = Vector3.one;
			if (entryList[i].challengeCompleted)
			{
				challengesCompleted++;
			}
		}
		challengeEntryArray[0] = challengeEntrySource;
		challengeEntryArray[0].GetComponent<ChallengeEntryBehaviour>().Init(entryList[0]);
		if (entryList[0].challengeCompleted)
		{
			challengesCompleted++;
		}
		progressLabel.text = string.Empty + challengesCompleted + "/" + challengesNumber;
		float value = (float)challengesCompleted / (float)challengesNumber;
		challengeCategoryProgressBar.value = value;
	}

	public void SetToDefault()
	{
		carButtonEnable = false;
		challengeCategorySprite.spriteName = string.Empty;
		carRewardSprite.spriteName = string.Empty;
		carRewardSprite.MakePixelPerfect();
		challengeCategoryNameLabel.text = string.Empty;
		GameObject gameObject = challengeEntryArray[0];
		int num = challengeEntryArray.Length;
		for (int i = 1; i < num; i++)
		{
			Object.DestroyImmediate(challengeEntryArray[i], true);
		}
		challengeEntryArray = null;
		gameObject.GetComponent<ChallengeEntryBehaviour>().SetToDefault();
		challengeEntrySource = gameObject;
		challengeCategoryProgressBar.value = 0f;
		challengesCompleted = 0;
		challengesNumber = 0;
		progressLabel.text = "0/0";
	}

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void GoToGarage()
	{
		Debug.Log("INTRODUCIR METODO QUE NOS LLEVE AL GARAGE");
	}

	private void InitCarButton(InfoCar info)
	{
		carRewardSprite.spriteName = info.iconName;
		carRewardSprite.MakePixelPerfect();
	}
}
