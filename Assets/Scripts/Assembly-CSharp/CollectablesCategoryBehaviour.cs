using System.Collections.Generic;
using UnityEngine;

public class CollectablesCategoryBehaviour : MonoBehaviour
{
	public GameObject collectableEntrySource;

	public GameObject[] collectableEntryArray;

	public UIGrid collectableEntryGrid;

	public UILabel collectableCategoryNameLabel;

	public UIProgressBar collectableCategoryProgressBar;

	public UILabel progressLabel;

	public UISprite collectableCategorySprite;

	private int collectableNumber;

	private int collectableCompleted;

	public void Init(List<CollectablesEntryInfo> entryList, string categorySpriteName)
	{
		if (collectableCategorySprite != null)
		{
			collectableCategorySprite.spriteName = categorySpriteName;
		}
		CollectablesEntryInfo collectablesEntryInfo = entryList[0];
		collectableCategoryNameLabel.text = collectablesEntryInfo.categoryName;
		collectableEntryArray = new GameObject[entryList.Count];
		for (int i = 1; i < entryList.Count; i++)
		{
			collectableEntryArray[i] = Object.Instantiate(collectableEntrySource) as GameObject;
			collectableEntryArray[i].GetComponent<CollectableEntryBehaviour>().Init(entryList[i]);
			collectableEntryArray[i].transform.parent = collectableEntryGrid.transform;
			collectableEntryArray[i].transform.localScale = Vector3.one;
			collectableNumber += entryList[i].collectablesTotal;
			collectableCompleted += entryList[i].collectablesGot;
		}
		collectableEntryArray[0] = collectableEntrySource;
		collectableEntryArray[0].GetComponent<CollectableEntryBehaviour>().Init(entryList[0]);
		collectableNumber += entryList[0].collectablesTotal;
		collectableCompleted += entryList[0].collectablesGot;
		progressLabel.text = string.Empty + collectableCompleted + "/" + collectableNumber;
		float value = (float)collectableCompleted / (float)collectableNumber;
		collectableCategoryProgressBar.value = value;
	}

	private void Start()
	{
	}

	private void Update()
	{
	}
}
