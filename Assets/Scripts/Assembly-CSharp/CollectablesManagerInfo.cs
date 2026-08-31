using System;
using System.Collections.Generic;
using UnityEngine;

public class CollectablesManagerInfo : MonoBehaviour
{
	private const string ALL_COLLECTABLES_COMPLETED_FLAG_KEY_PLAYERPREFS = "AllCollectablesGot";

	private const string ALL_COLLECTABLES_ZONE1_COMPLETED_FLAG_KEY_PLAYERPREFS = "AllCollectablesZone1Got";

	private const string ALL_COLLECTABLES_ZONE2_COMPLETED_FLAG_KEY_PLAYERPREFS = "AllCollectablesZone2Got";

	private const string ALL_COLLECTABLES_ZONE3_COMPLETED_FLAG_KEY_PLAYERPREFS = "AllCollectablesZone3Got";

	private void Start()
	{
	}

	private void Update()
	{
	}

	private void OnEnable()
	{
	}

	public void CheckCollectables()
	{
		int num = 0;
		int num2 = 0;
		foreach (CollectablesInfoList item in MySingleton<Manager>.Instance.collectablesCollection)
		{
			List<int> value = new List<int>();
			MySingleton<Manager>.Instance.stats.collectablesCounter.TryGetValue(item.collectablesList[0].collectableType, out value);
			foreach (CollectablesInfo collectables in item.collectablesList)
			{
				if (value != null && value.Contains(collectables.collectableID))
				{
					collectables.collected = true;
				}
			}
			item.countdata();
			num += item.totalCollectables;
			num2 += item.numberCollected;
		}
		if (num2 < num || !PlayerPrefs.HasKey("AllCollectablesGot"))
		{
		}
		int num3 = 0;
		int num4 = 0;
		if (MySingleton<Manager>.Instance.collectablesZone1.Count != 0)
		{
			string text = "ZONE-1";
			foreach (CollectablesInfoList item2 in MySingleton<Manager>.Instance.collectablesZone1)
			{
				List<int> value2 = new List<int>();
				MySingleton<Manager>.Instance.stats.collectablesCounter.TryGetValue(item2.collectablesList[0].collectableType, out value2);
				foreach (CollectablesInfo collectables2 in item2.collectablesList)
				{
					if (value2 != null && value2.Contains(collectables2.collectableID))
					{
						collectables2.collected = true;
					}
				}
				item2.countdata();
				num3 += item2.totalCollectables;
				num4 += item2.numberCollected;
				text = Enum.GetName(typeof(CollectableZone), item2.collectablesList[0].zone);
			}
			if (num4 >= num3 && PlayerPrefs.HasKey("AllCollectablesZone1Got"))
			{
			}
		}
		int num5 = 0;
		int num6 = 0;
		if (MySingleton<Manager>.Instance.collectablesZone2.Count != 0)
		{
			string text2 = "ZONE-2";
			foreach (CollectablesInfoList item3 in MySingleton<Manager>.Instance.collectablesZone2)
			{
				List<int> value3 = new List<int>();
				MySingleton<Manager>.Instance.stats.collectablesCounter.TryGetValue(item3.collectablesList[0].collectableType, out value3);
				foreach (CollectablesInfo collectables3 in item3.collectablesList)
				{
					if (value3 != null && value3.Contains(collectables3.collectableID))
					{
						collectables3.collected = true;
					}
				}
				item3.countdata();
				num5 += item3.totalCollectables;
				num6 += item3.numberCollected;
				text2 = Enum.GetName(typeof(CollectableZone), item3.collectablesList[0].zone);
			}
			if (num6 >= num5 && PlayerPrefs.HasKey("AllCollectablesZone2Got"))
			{
			}
		}
		int num7 = 0;
		int num8 = 0;
		if (MySingleton<Manager>.Instance.collectablesZone3.Count == 0)
		{
			return;
		}
		string text3 = "ZONE-3";
		foreach (CollectablesInfoList item4 in MySingleton<Manager>.Instance.collectablesZone3)
		{
			List<int> value4 = new List<int>();
			MySingleton<Manager>.Instance.stats.collectablesCounter.TryGetValue(item4.collectablesList[0].collectableType, out value4);
			foreach (CollectablesInfo collectables4 in item4.collectablesList)
			{
				if (value4 != null && value4.Contains(collectables4.collectableID))
				{
					collectables4.collected = true;
				}
			}
			item4.countdata();
			num7 += item4.totalCollectables;
			num8 += item4.numberCollected;
			text3 = Enum.GetName(typeof(CollectableZone), item4.collectablesList[0].zone);
		}
		if (num8 >= num7 && PlayerPrefs.HasKey("AllCollectablesZone3Got"))
		{
		}
	}
}
