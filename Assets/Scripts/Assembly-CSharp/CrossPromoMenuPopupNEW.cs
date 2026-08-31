using System.Collections.Generic;
using ANMiniJSON;
using AxesInMotion.Utils.MiniJSON;
using UnityEngine;

public class CrossPromoMenuPopupNEW : MonoBehaviour
{
	public CrossPromoNEW element;

	public GameObject standardMoreGame;

	private Dictionary<string, object> menuPromoDict;

	private bool disabled;

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
		if (data["icon"].ToString().Equals("DEFAULT"))
		{
			disabled = true;
		}
		bool flag = MustShow();
		standardMoreGame.SetActive(flag);
		element.gameObject.SetActive(flag);
		// Refresh the serialized data even while hidden: OnDisable persists it.
		element.Init(data);
	}

	public void Close()
	{
		standardMoreGame.SetActive(false);
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
		menuPromoDict["menu_promo_popup"] = value;
		MySingleton<Manager>.Instance.remoteConfiguration.crossPromoMenuPopup = ANMiniJSON.Json.Serialize(menuPromoDict);
	}

	private void LoadData()
	{
		string crossPromoMenuPopup = MySingleton<Manager>.Instance.remoteConfiguration.crossPromoMenuPopup;
		Dictionary<string, object> dictionary = (Dictionary<string, object>)AxesInMotion.Utils.MiniJSON.Json.Deserialize(crossPromoMenuPopup);
		Dictionary<string, object> data = (Dictionary<string, object>)dictionary["menu_promo_popup"];
		Init(data);
	}

	private bool MustShow()
	{
		if (MySingleton<Manager>.Instance.remoteConfiguration.crossPromoMenuPopupClicked || disabled)
		{
			return false;
		}
		MySingleton<Manager>.Instance.remoteConfiguration.menuPopupShowTimes++;
		if (MySingleton<Manager>.Instance.remoteConfiguration.menuPopupShowTimes == MySingleton<Manager>.Instance.remoteConfiguration.menuPopupFirstTime)
		{
			return true;
		}
		if (MySingleton<Manager>.Instance.remoteConfiguration.menuPopupShowTimes > MySingleton<Manager>.Instance.remoteConfiguration.menuPopupFirstTime)
		{
			if ((MySingleton<Manager>.Instance.remoteConfiguration.menuPopupShowTimes + MySingleton<Manager>.Instance.remoteConfiguration.menuPopupFirstTime) % MySingleton<Manager>.Instance.remoteConfiguration.menuPopupFrequency == 0)
			{
				return true;
			}
			return false;
		}
		return false;
	}
}
