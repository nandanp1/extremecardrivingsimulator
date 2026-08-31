using System.Collections.Generic;
using UnityEngine;

public class PanelAchievements : MonoBehaviour
{
	public UIGrid achievementsCategoryGrid;

	public GameObject achievementsCategorySource;

	public GameObject[] achievementsCategoryArray;

	private List<List<AchievementsEntryInfo>> achievementsList;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void Init(List<List<AchievementsEntryInfo>> achievementsList)
	{
		int count = achievementsList.Count;
		achievementsCategoryArray = new GameObject[count];
		List<AchievementsEntryInfo> list;
		for (int i = 1; i < count; i++)
		{
			achievementsCategoryArray[i] = Object.Instantiate(achievementsCategorySource) as GameObject;
			achievementsCategoryArray[i].transform.parent = achievementsCategoryGrid.transform;
			achievementsCategoryArray[i].transform.localScale = Vector3.one;
			list = achievementsList[i];
			achievementsCategoryArray[i].GetComponent<AchievementsCategoryBehaviour>().Init(achievementsList[i]);
		}
		list = achievementsList[0];
		achievementsCategoryArray[0] = achievementsCategorySource;
		achievementsCategoryArray[0].GetComponent<AchievementsCategoryBehaviour>().Init(achievementsList[0]);
		achievementsCategoryGrid.repositionNow = true;
	}
}
