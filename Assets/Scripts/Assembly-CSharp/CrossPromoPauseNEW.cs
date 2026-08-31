using System.Collections.Generic;
using ANMiniJSON;
using AxesInMotion.Utils.MiniJSON;
using UnityEngine;

public class CrossPromoPauseNEW : MonoBehaviour
{
	public CrossPromoNEW[] elements;

	public CrossPromoNEW elementPromoted;

	private Dictionary<string, List<object>> jsonDataDict;

	private bool initialized;

	private void Awake()
	{
		jsonDataDict = new Dictionary<string, List<object>>();
	}

	private void OnEnable()
	{
		LoadData();
		RemoteConfiguration.OnRemoteConfigurationLoad += LoadData;
	}

	private void OnDisable()
	{
		SaveData();
		RemoteConfiguration.OnRemoteConfigurationLoad -= LoadData;
	}

	private void SaveData()
	{
		SaveArray();
		SavePausePromoted();
	}

	private void SaveArray()
	{
		List<object> list = new List<object>();
		for (int i = 0; i < elements.Length && elements[i].gameObject.activeSelf; i++)
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			dictionary["icon"] = elements[i].crossPromoData.icon;
			dictionary["link"] = elements[i].crossPromoData.link;
			dictionary["title"] = elements[i].crossPromoData.title;
			dictionary["promoted"] = elements[i].crossPromoData.promoted.ToString();
			list.Add(dictionary);
		}
		jsonDataDict["pause_promo"] = list;
		MySingleton<Manager>.Instance.remoteConfiguration.crossPromoPause = ANMiniJSON.Json.Serialize(jsonDataDict);
	}

	private void SavePausePromoted()
	{
		if (!string.IsNullOrEmpty(MySingleton<Manager>.Instance.remoteConfiguration.crossPromoPausePromoted))
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2["icon"] = elementPromoted.crossPromoData.icon;
			dictionary2["link"] = elementPromoted.crossPromoData.link;
			dictionary2["title"] = elementPromoted.crossPromoData.title;
			dictionary2["promoted"] = elementPromoted.crossPromoData.promoted.ToString();
			object value = dictionary2;
			dictionary["pause_promo_promoted"] = value;
			MySingleton<Manager>.Instance.remoteConfiguration.crossPromoPausePromoted = ANMiniJSON.Json.Serialize(dictionary);
		}
	}

	private void LoadData()
	{
		string crossPromoPause = MySingleton<Manager>.Instance.remoteConfiguration.crossPromoPause;
		Dictionary<string, object> dictionary = AxesInMotion.Utils.MiniJSON.Json.Deserialize(crossPromoPause) as Dictionary<string, object>;
		Init((List<object>)dictionary["pause_promo"]);
	}

	public void Init(List<object> promos)
	{
		for (int i = 0; i < elements.Length; i++)
		{
			elements[i].gameObject.SetActive(false);
		}
		if (MySingleton<Manager>.Instance.remoteConfiguration.crossPromoPauseClicked || string.IsNullOrEmpty(MySingleton<Manager>.Instance.remoteConfiguration.crossPromoPausePromoted))
		{
			elementPromoted.gameObject.SetActive(false);
		}
		else
		{
			InitPausePromoPomoted();
			elementPromoted.gameObject.SetActive(true);
		}
		for (int j = 0; j < promos.Count && j < elements.Length; j++)
		{
			elements[j].gameObject.SetActive(true);
			elements[j].Init((Dictionary<string, object>)promos[j]);
		}
	}

	private void InitPausePromoPomoted()
	{
		Dictionary<string, object> data = MySingleton<Manager>.Instance.remoteConfiguration.data["pause_promo_promoted"] as Dictionary<string, object>;
		elementPromoted.Init(data);
	}

	public void Init()
	{
		List<object> promos = MySingleton<Manager>.Instance.remoteConfiguration.data["pause_promo"] as List<object>;
		Init(promos);
	}
}
