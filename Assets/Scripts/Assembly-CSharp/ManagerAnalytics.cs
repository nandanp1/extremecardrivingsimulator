using System.Collections;
using System.Collections.Generic;
using AxesInMotion.Utils.MiniJSON;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class ManagerAnalytics : MonoBehaviour
{
	public enum EventType
	{
		IAP_purchase = 0,
		start_app = 1,
		start_game = 2,
		car_unlocked = 3,
		blueprint_found = 4,
		challenge_start = 5,
		challenge_ends = 6,
		back_to_menu = 7,
		zone_changed = 8,
		photo_mode = 9,
		customization = 10,
		retention_claim = 11,
		revenue = 12,
		rewarded_video = 13,
		IAP_clicked = 14,
		countryIP = 15
	}

	public enum DataIds
	{
		game_mode = 0,
		car_id = 1,
		unlock_type = 2,
		blueprint_id = 3,
		zone = 4,
		jump_button = 5,
		challenge_id = 6,
		challenge_type = 7,
		time = 8,
		result = 9,
		zone_to = 10,
		zone_from = 11,
		location = 12,
		adplacement = 13,
		currency_type = 14,
		type = 15,
		value = 16,
		placement = 17,
		item = 18,
		server_result = 19,
		server = 20
	}

	public enum AnalyticsPlatform
	{
		AMPLITUDE = 0,
		GAME_ANALYTICS = 1
	}

	[HideInInspector]
	public string userCountry = string.Empty;

	public bool tier1User;

	private static ManagerAnalytics _instance;

	public GameObject gameAnalyticsObject;

	private string[] urls = new string[4] { "https://extreme-ip-lookup.com/json", "http://geoip.nekudo.com/api", "https://ip.nf/me.json", "http://ip-api.com/json" };

	private Dictionary<string, object> data;

	private int urlIndex;

	public static ManagerAnalytics instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindFirstObjectByType<ManagerAnalytics>();
			}
			return _instance;
		}
	}

	private void Awake()
	{
		_instance = this;
		tier1User = false;
	}

	private void Start()
	{
	}

	public void CoppaRequest()
	{
	}

	public void Init()
	{
		tier1User = false;
	}

	public void FireStartApp()
	{
		EventType eventType = EventType.start_app;
		FireEvent(eventType, true);
	}

	public void FireStartGame(string carID, string gameMode)
	{
		EventType eventType = EventType.start_game;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.game_mode.ToString()] = gameMode;
		dictionary[DataIds.car_id.ToString()] = carID;
		FireEvent(eventType, dictionary, true);
		FireGAEvent(eventType, dictionary);
	}

	public void FireCarUnlocked(string carID, string unlockType)
	{
		EventType eventType = EventType.car_unlocked;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.car_id.ToString()] = carID;
		dictionary[DataIds.unlock_type.ToString()] = unlockType;
		FireEvent(eventType, dictionary, true);
		FireGAEvent(eventType, dictionary);
	}

	public void FireBlueprintFound(string blueprintID, string carID, string zone, string gameMode, string jumpButton)
	{
		EventType eventType = EventType.blueprint_found;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.blueprint_id.ToString()] = blueprintID;
		dictionary[DataIds.car_id.ToString()] = carID;
		dictionary[DataIds.zone.ToString()] = zone;
		dictionary[DataIds.game_mode.ToString()] = gameMode;
		dictionary[DataIds.jump_button.ToString()] = jumpButton;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2[DataIds.blueprint_id.ToString()] = blueprintID;
		dictionary2[DataIds.car_id.ToString()] = carID;
		FireEvent(eventType, dictionary, true);
		FireGAEvent(eventType, dictionary2);
	}

	public void FireChallengeStart(string challengeID, string challengeType, string carId, string zone)
	{
		EventType eventType = EventType.challenge_start;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.challenge_id.ToString()] = challengeID;
		dictionary[DataIds.challenge_type.ToString()] = challengeType;
		dictionary[DataIds.car_id.ToString()] = carId;
		dictionary[DataIds.zone.ToString()] = zone;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2[DataIds.challenge_id.ToString()] = challengeType + "_" + challengeID;
		FireEvent(eventType, dictionary, true);
		FireGAEvent(eventType, dictionary2);
	}

	public void FireChallengeEnds(string challengeID, string time, string challengeType, string carId, string zone, string result)
	{
		EventType eventType = EventType.challenge_ends;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.challenge_id.ToString()] = challengeID;
		dictionary[DataIds.time.ToString()] = time;
		dictionary[DataIds.challenge_type.ToString()] = challengeType;
		dictionary[DataIds.car_id.ToString()] = carId;
		dictionary[DataIds.zone.ToString()] = zone;
		dictionary[DataIds.result.ToString()] = result;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2[DataIds.challenge_id.ToString()] = challengeType + "_" + challengeID;
		dictionary2[DataIds.result.ToString()] = result;
		FireEvent(eventType, dictionary, true);
		FireGAEvent(eventType, dictionary2);
	}

	public void FireIAPPurchased(string productID, double realPrice, string receipt, string receiptSignature)
	{
		EventType eventType = EventType.IAP_purchase;
		Dictionary<string, object> dictData = new Dictionary<string, object>();
		FireRevenueEvent(productID, realPrice, receipt, receiptSignature, dictData, true);
	}

	public void FireZoneChanged(string zoneTo, string zoneFrom)
	{
		EventType eventType = EventType.zone_changed;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.zone_from.ToString()] = zoneFrom;
		dictionary[DataIds.zone_to.ToString()] = zoneTo;
		FireEvent(eventType, dictionary, true);
		FireGAEvent(eventType, dictionary);
	}

	public void FireRetentionClaim(string type, string value)
	{
		EventType eventType = EventType.retention_claim;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.type.ToString()] = type;
		dictionary[DataIds.value.ToString()] = value;
		FireEvent(eventType, dictionary, true);
		if (type == "daily")
		{
			Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
			dictionary2["Aux1"] = "daily_reward";
			dictionary2[DataIds.value.ToString()] = value;
			dictionary2["Aux2"] = "claimed";
			FireGAEvent(eventType, dictionary2);
		}
	}

	public void FireBackToMenu(string gameMode, string carID, string zone)
	{
		EventType eventType = EventType.back_to_menu;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.game_mode.ToString()] = gameMode;
		dictionary[DataIds.car_id.ToString()] = carID;
		dictionary[DataIds.zone.ToString()] = zone;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2[DataIds.game_mode.ToString()] = gameMode;
		dictionary2[DataIds.car_id.ToString()] = carID;
		FireEvent(eventType, dictionary, false);
		FireGAEvent(eventType, dictionary2);
	}

	public void FirePhotoMode(string carID, string location, string zone)
	{
		EventType eventType = EventType.photo_mode;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.car_id.ToString()] = carID;
		dictionary[DataIds.location.ToString()] = location;
		dictionary[DataIds.zone.ToString()] = zone;
		Dictionary<string, object> dictionary2 = new Dictionary<string, object>();
		dictionary2[DataIds.zone.ToString()] = zone;
		dictionary2[DataIds.car_id.ToString()] = carID;
		FireEvent(eventType, dictionary, false);
		FireGAEvent(eventType, dictionary2);
	}

	public void FireCustomization(string carID, string currencyType)
	{
		EventType eventType = EventType.customization;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.car_id.ToString()] = carID;
		dictionary[DataIds.currency_type.ToString()] = currencyType;
		FireEvent(eventType, dictionary, false);
		FireGAEvent(eventType, dictionary);
	}

	public void FireRewardedVideo(string adplacement)
	{
		EventType eventType = EventType.rewarded_video;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.adplacement.ToString()] = adplacement;
		FireEvent(eventType, dictionary, true);
		FireGAEvent(eventType, dictionary);
	}

	public void FireIAPClicked(string item, string placement)
	{
		EventType eventType = EventType.IAP_clicked;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.item.ToString()] = item;
		dictionary[DataIds.placement.ToString()] = placement;
		FireEvent(eventType, dictionary, true);
		FireGAEvent(eventType, dictionary);
	}

	public void FireGetIpCountry(string result, string server)
	{
		EventType eventType = EventType.countryIP;
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary[DataIds.server_result.ToString()] = result;
		dictionary[DataIds.server.ToString()] = server;
		FireGAEvent(eventType, dictionary);
	}

	private void FireEvent(EventType eventType, bool amplitude)
	{
	}

	private void FireEvent(EventType eventType, Dictionary<string, object> dictData, bool amplitude)
	{
	}

	private void FireGAEvent(EventType eventType, Dictionary<string, object> dictData)
	{
	}

	private void FireRevenueEvent(string productID, double realPrice, string receipt, string receiptSignature, Dictionary<string, object> dictData, bool amplitude)
	{
	}

	private void Request()
	{
	}

	private IEnumerator CheckUrl(string url)
	{
		WWW www = new WWW(url);
		yield return www;
		if (www.error != null)
		{
			Debug.LogError("There was an error getting the data: " + www.error + " from: " + url);
			urlIndex++;
			Request();
			FireGetIpCountry("url_failed", url);
		}
		else
		{
			string json = www.text;
			ParseIPJson(json);
		}
	}

	private void ParseIPJson(string json)
	{
		Dictionary<string, object> dictionary = Json.Deserialize(json) as Dictionary<string, object>;
		data = dictionary;
		switch (urlIndex)
		{
		case 0:
			userCountry = data["country"].ToString();
			break;
		case 1:
		{
			Dictionary<string, object> dictionary3 = data["country"] as Dictionary<string, object>;
			userCountry = dictionary3["name"].ToString();
			break;
		}
		case 2:
		{
			Dictionary<string, object> dictionary2 = data["ip"] as Dictionary<string, object>;
			userCountry = dictionary2["country"].ToString();
			break;
		}
		case 3:
			userCountry = data["country"].ToString();
			break;
		default:
			userCountry = string.Empty;
			break;
		}
		if (userCountry != string.Empty)
		{
			CheckCountry();
			return;
		}
		FireGetIpCountry("parse_failed", "Try index: " + urlIndex);
		urlIndex++;
		Request();
	}

	private void CheckCountry()
	{
		if (Debug.isDebugBuild)
		{
			if (userCountry == "Spain")
			{
				tier1User = true;
				Debug.LogError("Country Spain - Tier 1");
			}
		}
		else if (userCountry == "United States")
		{
			FireGetIpCountry("success", urls[urlIndex]);
			tier1User = true;
		}
		Init();
	}
}
