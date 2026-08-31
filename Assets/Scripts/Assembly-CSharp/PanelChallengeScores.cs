using System.Collections.Generic;
using UnityEngine;

public class PanelChallengeScores : MonoBehaviour
{
	public UIGrid challengeCategoryGrid;

	public GameObject challengeCategorySource;

	public GameObject[] challengeCategoryArray;

	private List<List<ChallengeEntryInfo>> challengesList;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Init(List<List<ChallengeEntryInfo>> challengesList)
	{
		int count = challengesList.Count;
		challengeCategoryArray = new GameObject[count];
		bool flag = false;
		List<ChallengeEntryInfo> list;
		ChallengeEntryInfo challengeEntryInfo;
		for (int i = 1; i < count; i++)
		{
			challengeCategoryArray[i] = Object.Instantiate(challengeCategorySource) as GameObject;
			challengeCategoryArray[i].transform.parent = challengeCategoryGrid.transform;
			challengeCategoryArray[i].transform.localScale = Vector3.one;
			list = challengesList[i];
			challengeEntryInfo = list[i];
			flag = !(challengeEntryInfo.carRewardInfo == null) && !challengeEntryInfo.carRewardInfo.isBought;
			challengeCategoryArray[i].GetComponent<ChallengeCategoryBehaviour>().Init(challengesList[i], list[0].challengeCategoryIconName, flag, challengeEntryInfo.carRewardInfo);
		}
		list = challengesList[0];
		challengeEntryInfo = list[0];
		flag = !(challengeEntryInfo.carRewardInfo == null);
		challengeCategoryArray[0] = challengeCategorySource;
		challengeCategoryArray[0].GetComponent<ChallengeCategoryBehaviour>().Init(challengesList[0], list[0].challengeCategoryIconName, flag, challengeEntryInfo.carRewardInfo);
		challengeCategoryGrid.sorting = UIGrid.Sorting.Alphabetic;
		challengeCategoryGrid.repositionNow = true;
	}

	public void ClearPanel()
	{
		int num = challengeCategoryArray.Length;
		GameObject gameObject = challengeCategoryArray[0];
		for (int i = 1; i < num; i++)
		{
			Object.DestroyImmediate(challengeCategoryArray[i], true);
		}
		challengeCategoryArray = null;
		gameObject.GetComponent<ChallengeCategoryBehaviour>().SetToDefault();
		gameObject.GetComponent<ChallengeCategoryBehaviour>().carButtonEnable = false;
		challengeCategorySource = gameObject;
		challengeCategoryGrid.repositionNow = true;
		challengesList = null;
	}

	public void GoToLeaderboardsGeneral()
	{
		MySingleton<Manager>.Instance.gameServicesManager.ShowAllLeaderboardOrLogin();
	}
}
