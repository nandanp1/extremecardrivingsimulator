using System.Collections.Generic;
using UnityEngine;

public class AchievementsCategoryBehaviour : MonoBehaviour
{
	public GameObject achievementsEntrySource;

	public GameObject[] achievementsEntryArray;

	public UIGrid achievementsEntryGrid;

	public UILabel achievementsCategoryNameLabel;

	public void Init(List<AchievementsEntryInfo> entryList)
	{
		AchievementsEntryInfo achievementsEntryInfo = entryList[0];
		achievementsCategoryNameLabel.text = achievementsEntryInfo.categoryName;
		achievementsEntryArray = new GameObject[entryList.Count];
		for (int i = 1; i < entryList.Count; i++)
		{
			achievementsEntryArray[i] = Object.Instantiate(achievementsEntrySource) as GameObject;
			achievementsEntryArray[i].GetComponent<AchievementsEntryBehaviour>().Init(entryList[i]);
			achievementsEntryArray[i].transform.parent = achievementsEntryGrid.transform;
			achievementsEntryArray[i].transform.localScale = Vector3.one;
		}
		achievementsEntryArray[0] = achievementsEntrySource;
		achievementsEntryArray[0].GetComponent<AchievementsEntryBehaviour>().Init(entryList[0]);
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
