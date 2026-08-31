using System.Collections.Generic;
using ANMiniJSON;
using AxesInMotion.Utils.MiniJSON;
using UnityEngine;

public class CrossPromoMenuNEW : MonoBehaviour
{
	public CrossPromoNEW element;

	public GameObject standardMoreGame;

	private Dictionary<string, object> menuPromoDict;

	private void Awake()
	{
		menuPromoDict = new Dictionary<string, object>();
	}

	private void OnEnable()
	{
		LoadData();
		RemoteConfiguration.OnRemoteConfigurationLoad += LoadData;
	}

	private void OnDisable()
	{
		RemoteConfiguration.OnRemoteConfigurationLoad -= LoadData;
		SaveData();
	}

	public void Init(Dictionary<string, object> data)
	{
		bool flag = data["icon"].ToString().Equals("DEFAULT") || MySingleton<Manager>.Instance.remoteConfiguration.crossPromoMenuClicked;
		standardMoreGame.SetActive(!flag);
		element.gameObject.SetActive(!flag);
		// Refresh the serialized data even while hidden: OnDisable persists it.
		element.Init(data);
	}

	public void OpenMoreGames()
	{
		Application.OpenURL(MySingleton<Manager>.Instance.remoteConfiguration.crosspromoMoreGameslink);
	}

	private void SaveData()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["icon"] = element.crossPromoData.icon;
		dictionary["link"] = element.crossPromoData.link;
		dictionary["title"] = element.crossPromoData.title;
		dictionary["promoted"] = element.crossPromoData.promoted;
		object value = dictionary;
		menuPromoDict["menu_promo"] = value;
		MySingleton<Manager>.Instance.remoteConfiguration.crossPromoMenu = ANMiniJSON.Json.Serialize(menuPromoDict);
	}

	private void LoadData()
	{
		string crossPromoMenu = MySingleton<Manager>.Instance.remoteConfiguration.crossPromoMenu;
		Dictionary<string, object> dictionary = (Dictionary<string, object>)AxesInMotion.Utils.MiniJSON.Json.Deserialize(crossPromoMenu);
		Dictionary<string, object> data = (Dictionary<string, object>)dictionary["menu_promo"];
		Init(data);
	}
}
