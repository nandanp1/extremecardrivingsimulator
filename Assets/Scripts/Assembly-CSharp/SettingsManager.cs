using System;
using System.Collections.Generic;
using System.Text;
using AxesInMotion.Network;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public static class SettingsManager
{
	public enum Units
	{
		KMH = 0,
		MPH = 1
	}

	public enum Language
	{
		English = 0,
		Spanish = 1,
		Portuguese = 2
	}

	private const string KEY_CHALLANGE_CIRCUIT_BEST_PREFIX = "chgBest";

	private const string KEY_PUBLIC_NAME = "playername";

	public const string DEFAULT_PUBLIC_NAME = "New Player";

	private const string KEY_UPID = "upid";

	public const string DEFAULT_UPID = "none";

	private const string KEY_CURRENT_MULTIPLAYER_VERSION = "mpCurrentVersion";

	private const string KEY_CURRENT_MULTIPLAYER_DATE_VERSION = "mpCurrentDateVersion";

	private const int DEFAULT_CURRENT_MULTIPLAYER_VERSION = 0;

	private const string KEY_NEWEST_VERSION = "newestVersion";

	private const string KEY_NEWEST_DATE_VERSION = "newestDateVersion";

	private const string KEY_IS_OBSOLETE = "mpIsObsolete";

	private const int DEFAULT_IS_OBSOLETE = 0;

	private const string KEY_FRIENDS = "friends";

	private const string DEFAULT_FRIENDS = "none";

	private const string KEY_IGNORE_PLAYERS = "ignoredPlayers";

	private const string KEY_MUSIC_VOLUME = "keyMusicVolume";

	private const string CONTROL = "control";

	private const string KEY_UNITS = "keyUnits";

	private const string MUSIC = "music";

	public const string KEY_DISTANCE_DROVE_BY_CAR = "keyDistanceDroveByCar";

	public const string KEY_METERS_DROVE = "keyMetersDrove";

	public const string KEY_IS_FIRST_OFFER = "IsFirstOffer";

	public const string KEY_IS_SECOND_OFFER = "IsSecondOffer";

	public const string KEY_IS_OFFER = "IsOffer";

	public const string KEY_PICKED_COLLECTABLES = "keyPickedCollectables";

	public const string KEY_PICKED_COLLECTABLES_ID_ARRAY = "keyPickedCollectablesIdArray";

	private const string KEY_CURRENT_CAR_SELECTED_ID = "keyCurrentCarSelectedId";

	public const string KEY_CAR_BOUGHT = "keyCarBought";

	public const string KEY_CAR = "keyCar";

	private const string KEY_LANGUAGE = "language";

	private const string KEY_COLLECTABLES_STATUS = "collectables_status";

	public const string KEY_CARS_NEED_FIRST_PRECACHED = "keyCarsNeedFirstLoad";

	private const string KEY_RATE = "keyRate";

	private const string KEY_UPDATENEDED_COUNTER = "keyUpdateNededCounter";

	private const string KEY_VALUE_SEPARATOR = ">";

	private const string DICTIONARY_ENTRIES_SEPARATOR = ";";

	private const string LIST_ELEMENTS_SEPARATOR = ",";

	private const string SPLIT_TUPLE = ";";

	private const char SPLIT_KEY_VALUE = ',';

	private const string STARTER_PACK_KEY = "starterpack";

	private const string STARTER_PACK_LOCAL_TIME = "starterlocaltime";

	private const string STARTER_PACK_FLAG_KEY = "starterPackFlag";

	private const string KEY_POPUP_COUNT = "crosspromoTimesShowed";

	public static float STARTER_PACK_COUNTER = 259200f;

	public static float STARTER_PACK_2ND_STEP = 172800f;

	public static float STARTER_PACK_3ND_STEP = 86400f;

	public static int popupTimesShowed
	{
		get
		{
			return ObscuredPrefs.GetInt("crosspromoTimesShowed");
		}
		set
		{
			ObscuredPrefs.SetInt("crosspromoTimesShowed", value);
		}
	}

	public static float GetChallangeCircuitBestTime(string challangePlayerPrefKey, ChallangeSceneIndex sceneIndex)
	{
		return ObscuredPrefs.GetFloat(GetChallangeCircuitBestKey(challangePlayerPrefKey, sceneIndex), 0f);
	}

	public static void SetChallangeCircuitBestTime(string challangePlayerPrefKey, ChallangeSceneIndex sceneIndex, float value)
	{
		if (value == 0f)
		{
			ObscuredPrefs.DeleteKey(GetChallangeCircuitBestKey(challangePlayerPrefKey, sceneIndex));
		}
		ObscuredPrefs.SetFloat(GetChallangeCircuitBestKey(challangePlayerPrefKey, sceneIndex), value);
	}

	public static float GetChallangeRadarsBestSpeed(string challangePlayerPrefKey, ChallangeSceneIndex sceneIndex)
	{
		return ObscuredPrefs.GetFloat(GetChallangeCircuitBestKey(challangePlayerPrefKey, sceneIndex), 0f);
	}

	public static void SetChallangeRadarsBestSpeed(string challangePlayerPrefKey, ChallangeSceneIndex sceneIndex, float value)
	{
		ObscuredPrefs.DeleteKey(GetChallangeCircuitBestKey(challangePlayerPrefKey, sceneIndex));
		ObscuredPrefs.SetFloat(GetChallangeCircuitBestKey(challangePlayerPrefKey, sceneIndex), value);
	}

	public static int GetChallangeBestCrashables(string challangePlayerPrefKey, ChallangeSceneIndex sceneIndex)
	{
		return ObscuredPrefs.GetInt(GetChallangeCircuitBestKey(challangePlayerPrefKey, sceneIndex), 0);
	}

	public static void SetChallangeBestCrashables(string challangePlayerPrefKey, ChallangeSceneIndex sceneIndex, int value)
	{
		if (value == 0)
		{
			ObscuredPrefs.DeleteKey(GetChallangeCircuitBestKey(challangePlayerPrefKey, sceneIndex));
		}
		ObscuredPrefs.SetInt(GetChallangeCircuitBestKey(challangePlayerPrefKey, sceneIndex), value);
	}

	private static string GetChallangeCircuitBestKey(string challangePlayerPrefKey, ChallangeSceneIndex sceneIndex)
	{
		return "chgBest_" + challangePlayerPrefKey + "_" + sceneIndex.ToString("g");
	}

	public static string GetPlayername()
	{
		return PlayerPrefs.GetString("playername", "New Player");
	}

	public static void SetPlayername(string playername)
	{
		PlayerPrefs.SetString("playername", playername);
	}

	public static string GetUPID()
	{
		return PlayerPrefs.GetString("upid", "none");
	}

	public static void SetUPID(string upid)
	{
		PlayerPrefs.SetString("upid", upid);
	}

	public static Friend[] GetFriends()
	{
		return Friend.StringToFriends(PlayerPrefs.GetString("friends", "none"));
	}

	public static void SetFriends(Friend[] friends)
	{
		PlayerPrefs.SetString("friends", Friend.FriendsToString(friends));
	}

	public static int GetMultiplayerVersion()
	{
		return PlayerPrefs.GetInt("mpCurrentVersion", 0);
	}

	public static void SetMultiplayerVersion(int newVersion)
	{
		PlayerPrefs.SetInt("mpCurrentVersion", newVersion);
	}

	public static string GetMultiplayerDateVersion()
	{
		return PlayerPrefs.GetString("mpCurrentDateVersion", string.Empty);
	}

	public static void SetMultiplayerDateVersion(string date)
	{
		PlayerPrefs.SetString("mpCurrentDateVersion", date);
	}

	public static int GetNewestVersion()
	{
		return PlayerPrefs.GetInt("newestVersion", GetMultiplayerVersion());
	}

	public static void SetNewestVersion(int newNewestVersion)
	{
		PlayerPrefs.SetInt("newestVersion", newNewestVersion);
	}

	public static string GetNewestDateVersion()
	{
		return PlayerPrefs.GetString("newestDateVersion", string.Empty);
	}

	public static void SetNewestDateVersion(string date)
	{
		PlayerPrefs.SetString("newestDateVersion", date);
	}

	public static bool GetIsObsolete()
	{
		return PlayerPrefs.GetInt("mpIsObsolete", 0) == 1;
	}

	public static void SetIsObsolete(bool obsolete)
	{
		PlayerPrefs.SetInt("mpIsObsolete", obsolete ? 1 : 0);
	}

	public static string GetIgnorePlayers()
	{
		return PlayerPrefs.GetString("ignoredPlayers", string.Empty);
	}

	public static void SetIgnorePlayers(string ignorePlayers)
	{
		PlayerPrefs.SetString("ignoredPlayers", ignorePlayers);
	}

	public static void ToggleControl()
	{
		switch (MySingleton<Manager>.Instance.controls)
		{
		case Manager.Controls.Accelerometer:
			MySingleton<Manager>.Instance.controls = Manager.Controls.Buttons;
			break;
		case Manager.Controls.Buttons:
			MySingleton<Manager>.Instance.controls = Manager.Controls.Wheel;
			break;
		case Manager.Controls.Wheel:
			MySingleton<Manager>.Instance.controls = Manager.Controls.Accelerometer;
			break;
		}
		PlayerPrefs.SetInt("control", (int)MySingleton<Manager>.Instance.controls);
	}

	public static void SetControl(Manager.Controls newControl)
	{
		MySingleton<Manager>.Instance.controls = newControl;
		PlayerPrefs.SetInt("control", (int)MySingleton<Manager>.Instance.controls);
	}

	public static Manager.Controls GetControl()
	{
		Manager.Controls result = Manager.Controls.Wheel;
		switch (PlayerPrefs.GetInt("control", 2))
		{
		case 0:
			result = Manager.Controls.Wheel;
			break;
		case 1:
			result = Manager.Controls.Accelerometer;
			break;
		case 2:
			result = Manager.Controls.Buttons;
			break;
		}
		return result;
	}

	public static int GetMetersDrove()
	{
		return ObscuredPrefs.GetInt("keyMetersDrove", 0);
	}

	public static void SetMetersDrove(int value)
	{
		MySingleton<Manager>.Instance.stats.metersDrove = value;
		ObscuredPrefs.SetInt("keyMetersDrove", value);
	}

	public static int[] GetPickedCollectablesIdArray()
	{
		string text = PlayerPrefs.GetString("keyPickedCollectablesIdArray", string.Empty);
		string[] array = text.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
		int[] array2 = new int[array.Length];
		for (int i = 0; i != array.Length; i++)
		{
			string s = array[i];
			array2[i] = int.Parse(s);
		}
		return array2;
	}

	public static void DeleteOldCollectables()
	{
		PlayerPrefs.DeleteKey("keyPickedCollectablesIdArray");
	}

	public static int GetRate()
	{
		return PlayerPrefs.GetInt("keyRate", 0);
	}

	public static void SetRate(int rate)
	{
		PlayerPrefs.SetInt("keyRate", rate);
	}

	public static int GetUpdateNededShownCounter()
	{
		return PlayerPrefs.GetInt("keyUpdateNededCounter", 1);
	}

	public static void SetUpdateNededShownCounter(int counter)
	{
		PlayerPrefs.SetInt("keyUpdateNededCounter", counter);
	}

	public static float GetMusicVolume()
	{
		return PlayerPrefs.GetFloat("keyMusicVolume", 0.6f);
	}

	public static void SetMusicVolume(float volume)
	{
		PlayerPrefs.SetFloat("keyMusicVolume", volume);
	}

	public static Language GetLanguage()
	{
		return (Language)PlayerPrefs.GetInt("language", 0);
	}

	public static void SetLanguage(Language language)
	{
		MySingleton<Manager>.Instance.language = language;
		PlayerPrefs.SetInt("language", (int)language);
	}

	public static Units GetUnits()
	{
		return (Units)PlayerPrefs.GetInt("keyUnits", 0);
	}

	public static void SetUnits(Units units)
	{
		MySingleton<Manager>.Instance.units = units;
		PlayerPrefs.SetInt("keyUnits", (int)units);
	}

	public static int GetCurrentCarSelectedId()
	{
		return ObscuredPrefs.GetInt("keyCurrentCarSelectedId", 0);
	}

	public static void SetCurrentCarSelectedId(int id)
	{
		MySingleton<Manager>.Instance.indexCurrentCar = id;
		ObscuredPrefs.SetInt("keyCurrentCarSelectedId", id);
	}

	public static bool IsCarBought(string keyId)
	{
		return GetBool(keyId + "keyCarBought", false);
	}

	public static void SetCarBought(string keyId, bool val)
	{
		SetBool(keyId + "keyCarBought", val);
	}

	public static bool GetStarterPackBoughtOrEnded()
	{
		return GetBool("starterPackFlag", false);
	}

	public static void SetStarterPackBoughtOrEnded(bool val)
	{
		SetBool("starterPackFlag", val);
	}

	public static void SetShowStarterPack(bool val)
	{
		SetBool("starterpack", val);
	}

	public static float GetStarterPackLocalTime()
	{
		return ObscuredPrefs.GetFloat("starterlocaltime", 0f);
	}

	public static void SetStarterPackLocalTime(float val)
	{
		ObscuredPrefs.SetFloat("starterlocaltime", val);
	}

	public static int GetStarterPackCount()
	{
		return ObscuredPrefs.GetInt("starterPackCount", 0);
	}

	public static void SetStarterPackCount(int value)
	{
		ObscuredPrefs.SetInt("starterPackCount", value);
	}

	public static bool IsFirstRemoveAdsOffer()
	{
		return GetBool("IsFirstOffer", false);
	}

	public static void SetFirstRemoveAdsOffer(bool val)
	{
		SetBool("IsFirstOffer", val);
	}

	public static bool IsSecondRemoveAdsOffer()
	{
		return GetBool("IsSecondOffer", false);
	}

	public static void SetSecondRemoveAdsOffer(bool val)
	{
		SetBool("IsSecondOffer", val);
	}

	public static bool IsRemoveAdsOffer()
	{
		return GetBool("IsOffer", false);
	}

	public static void SetRemoveAdsOffer(bool val)
	{
		SetBool("IsOffer", val);
	}

	public static int GetDistanceDrove(string keyId)
	{
		return ObscuredPrefs.GetInt(keyId + "keyDistanceDroveByCar", 0);
	}

	public static void SetDistanceDrove(string keyId, int distance)
	{
		ObscuredPrefs.SetInt(keyId + "keyDistanceDroveByCar", distance);
	}

	public static float GetRewardValue(string keyId)
	{
		return ObscuredPrefs.GetFloat(keyId);
	}

	public static void SetTaskValue(Task task)
	{
		ObscuredPrefs.SetFloat(task.taskId, task.progress);
	}

	public static void SetCollectablesStatus(Dictionary<Collectables.CollectableType, List<int>> collectablesStatus)
	{
		StringBuilder stringBuilder = new StringBuilder();
		List<Collectables.CollectableType> list = new List<Collectables.CollectableType>(collectablesStatus.Keys);
		for (int i = 0; i < collectablesStatus.Count; i++)
		{
			Collectables.CollectableType collectableType = list[i];
			List<int> list2 = collectablesStatus[collectableType];
			if (list2 != null && list2.Count > 0)
			{
				stringBuilder.Append(collectableType);
				stringBuilder.Append(">");
				for (int j = 0; j < list2.Count; j++)
				{
					stringBuilder.Append(list2[j]);
					stringBuilder.Append(",");
				}
			}
			stringBuilder.Append(";");
		}
		ObscuredPrefs.SetString("collectables_status", stringBuilder.ToString());
	}

	public static Dictionary<Collectables.CollectableType, List<int>> GetCollectablesStatus()
	{
		Dictionary<Collectables.CollectableType, List<int>> dictionary = new Dictionary<Collectables.CollectableType, List<int>>();
		string text = ObscuredPrefs.GetString("collectables_status", string.Empty);
		if (!string.IsNullOrEmpty(text))
		{
			Collectables.CollectableType collectableType = Collectables.CollectableType.None;
			string[] array = text.Split(";".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
			string[] array2 = array;
			foreach (string text2 in array2)
			{
				List<int> list = new List<int>();
				string[] array3 = text2.Split(">".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				if (array3.Length != 2)
				{
					Debug.LogError("Serialized entry should have exactly 2 elements.");
					return null;
				}
				collectableType = (Collectables.CollectableType)(int)Enum.Parse(typeof(Collectables.CollectableType), array3[0]);
				string text3 = array3[1];
				string[] array4 = text3.Split(",".ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
				string[] array5 = array4;
				foreach (string s in array5)
				{
					list.Add(int.Parse(s));
				}
				dictionary.Add(collectableType, list);
			}
		}
		return dictionary;
	}

	public static DateTime GetTime(string keyTime)
	{
		return new DateTime(ObscuredPrefs.GetLong(keyTime, 0L));
	}

	public static void SetTime(string keyTime, DateTime time)
	{
		ObscuredPrefs.SetLong(keyTime, time.Ticks);
	}

	public static int GetDayForDailyReward()
	{
		return ObscuredPrefs.GetInt("keyDailyRewardDay", 0);
	}

	public static void SetDayForDailyReward(int day)
	{
		ObscuredPrefs.SetInt("keyDailyRewardDay", day);
	}

	public static bool IsFirstTimeLastDayReward(string keyId)
	{
		return GetBool(keyId, true);
	}

	public static void SetFirstTimeLastDayReward(string keyId, bool val)
	{
		SetBool(keyId, val);
	}

	public static Color GetCarColor(int id, Color defaultColor)
	{
		return GetColor("Car" + id + "Color", defaultColor);
	}

	public static void SetCarColor(int id, Color color)
	{
		SetColor("Car" + id + "Color", color);
	}

	public static void SetAchievement(string achievement)
	{
		ObscuredPrefs.SetInt(achievement, 1);
	}

	public static int GetAchievement(string achievement)
	{
		return ObscuredPrefs.GetInt(achievement, 0);
	}

	public static void SetAchievementCategory(string category)
	{
		ObscuredPrefs.SetInt(category, 1);
	}

	public static int GetAchievementCategory(string category)
	{
		return ObscuredPrefs.GetInt(category, 0);
	}

	public static void SetBool(string key, bool value)
	{
		ObscuredPrefs.SetInt(key, value ? 1 : 0);
	}

	public static bool GetBool(string key, bool defaultValue)
	{
		switch (ObscuredPrefs.GetInt(key, defaultValue ? 1 : 0))
		{
		case 0:
			return false;
		case 1:
			return true;
		default:
			Debug.LogError("Error retrieving bool " + key + " from PlayerPrefs.");
			return defaultValue;
		}
	}

	public static void SetColor(string key, Color color)
	{
		ObscuredPrefs.SetString(key, color.r + ";" + color.g + ";" + color.b + ";" + color.a);
	}

	public static Color GetColor(string key, Color defaultColor)
	{
		if (!ObscuredPrefs.HasKey(key))
		{
			return defaultColor;
		}
		string[] array = ObscuredPrefs.GetString(key).Split(";".ToCharArray());
		return new Color(float.Parse(array[0]), float.Parse(array[1]), float.Parse(array[2]), float.Parse(array[3]));
	}
}
