using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CollectablesInfoList : MonoBehaviour
{
	[Serializable]
	public enum ListKind
	{
		NONE = 0,
		BYCAR = 1,
		BYZONE = 2
	}

	public string categoryName;

	public string categoryIconName;

	public ListKind listKind;

	public CollectableZone zone;

	public Collectables.CollectableType collectableType;

	public int collectionID;

	public int rewardID;

	public int totalCollectables;

	public int numberCollected;

	public List<CollectablesInfo> collectablesList;

	public void OnEnable()
	{
		countdata();
	}

	public void countdata()
	{
		totalCollectables = collectablesList.Count;
		numberCollected = 0;
		foreach (CollectablesInfo collectables in collectablesList)
		{
			if (collectables.collected)
			{
				numberCollected++;
			}
		}
	}
}
