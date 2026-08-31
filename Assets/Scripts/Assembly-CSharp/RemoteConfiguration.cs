using System;
using System.Collections;
using System.Collections.Generic;
using AxesInMotion.Utils.MiniJSON;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class RemoteConfiguration : MonoBehaviour
{
	public delegate void RemoteConfigurationLoadCallBack();

	private const string CONFIGURATION_PREFIX = "CONFIG";

	private const string CONFIG_FILENAME = "gameconfig";

	private const string CONFIG_FILENAME_SUFFIX = "_ANDROID";

	private const string CONFIG_FILENAME_DEBUG_SUFFIX = "_DEV";

	private const string CONFIG_FILENAME_EXTENSION = ".json";

	private const string KEY_LAST_REFRESH_DATE = "lastRefresh";

	private const string KEY_AMAZON_REFRESH_RATE = "amazon_refresh_rate";

	private const string KEY_TIME_BETWEEN_ENTER_TO_GAME_ADS = "time_between_enter_to_game_ads";

	private const string KEY_MAX_TRYS_BETWEEN_CHALLENGE_ADS = "max_challenge_trys_between_before_ad";

	private const string KEY_TIME_BETWEEN_321_ADS = "time_between_321_ads";

	private const string KEY_TIME_BETWEEN_REGULAR_ADS = "time_between_regular_ads";

	private const string KEY_MAX_CONSECUTIVE_ADMOB_IN_REGULAR_ADS = "max_consecutive_admob_in_regular_ads";

	private const string KEY_MAX_CONSECUTIVE_HEYZAP_IN_REGULAR_ADS = "max_consecutive_heyzap_in_regular_ads";

	private const string KEY_START_REGULAR_ADS_WITH_ADMOB = "start_regular_ads_with_admob";

	private const string KEY_TIMES_PAUSED_BEFORE_PAUSE_AD = "times_pause_before_pause_ad";

	private const string KEY_ADPLACEMENT_GO_TO_GAME_ADMOB = "admob_adplacement_go_to_game";

	private const string KEY_ADPLACEMENT_FAIL_RETRY_CHALLENGES_ADMOB = "admob_adplacement_fail_or_retry_challenges";

	private const string KEY_ADPLACEMENT_START_END_CHALLENGES_ADMOB = "admob_adplacement_start_or_end_challenges";

	private const string KEY_ADPLACEMENT_PAUSE_ADMOB = "admob_adplacement_pause";

	private const string KEY_ADPLACEMENT_GARAGE_ADMOB = "admob_adplacement_garage";

	private const string KEY_ADPLACEMENT_GO_TO_GAME = "global_adplacement_go_to_game";

	private const string KEY_ADPLACEMENT_FAIL_RETRY_CHALLENGES = "global_adplacement_fail_or_retry_challenges";

	private const string KEY_ADPLACEMENT_START_END_CHALLENGES = "global_adplacement_start_or_end_challenges";

	private const string KEY_ADPLACEMENT_PAUSE = "global_adplacement_pause";

	private const string KEY_ADPLACEMENT_GARAGE = "global_adplacement_garage";

	private const string KEY_ADPLACEMENT_321 = "global_adplacement_321";

	private const string KEY_BANNER = "global_banner";

	private const string KEY_CROSSPROMO_MOREGAMES_LINK = "link_more_games";

	private const string KEY_RATE_US_LINK = "rate_us_link";

	private const string KEY_ENABLE_RATE_US = "enable_rate_us";

	public const string KEY_CROSSPROMO_PAUSE = "pause_promo";

	private const string KEY_PAUSE_CROSSPROMO_MAX = "pause_max_clicked_times";

	private const string KEY_PAUSE_CROSSPROMO_CLICKED_TIMES = "pause_clicked";

	private const string KEY_CROSSPROMO_PAUSE_CLICKED = "pause_promo_clicked";

	private const string KEY_MENU_CROSSPROMO_MAX = "menu_max_clicked_times";

	private const string KEY_MENU_CROSSPROMO_CLICKED_TIMES = "menu_clicked";

	private const string KEY_CROSSPROMO_MENU_CLICKED = "menu_promo_clicked";

	private const string KEY_MENU_POPUP_CROSSPROMO_MAX = "menu_popup_max_clicked_times";

	private const string KEY_MENU_POPUP_CROSSPROMO_CLICKED_TIMES = "menu_popup_clicked";

	private const string KEY_CROSSPROMO_MENU_POPUP_CLICKED = "menu_popup_promo_clicked";

	private const string KEY_POPUP_FIRST_TIME = "popup_first_time";

	private const string KEY_POPUP_SHOW_TIMES = "popup_show_times";

	private const string KEY_POPUP_FREQUENCY = "popup_frequency";

	public const string KEY_CROSSPROMO_MENU_POPUP = "menu_promo_popup";

	public const string KEY_CROSSPROMO_MENU = "menu_promo";

	public const string KEY_CROSSPROMO_PAUSE_PROMOTED = "pause_promo_promoted";

	public const string KEY_TIME_SERVERS = "servers";

	private const int fallbackAmazonRefreshRate = 0;

	private const int fallbackTimeBetween321Ads = 165;

	private const int fallbackTimeBetweenRegularAds = 0;

	private const int fallbackMaxChallengeTrysAds = 6;

	private const int fallbackTimeBetweenGoToGameAds = 1800;

	private const int fallbackConsecutiveHeyzapRegularAds = 1;

	private const int fallbackConsecutiveAdmobRegularAds = 4;

	private const bool fallbackStartRegularWithAdmob = true;

	private const bool fallbackAdmobAdplacementGoToGame = true;

	private const bool fallbackAdmobAdplacementFailRetryChallenges = true;

	private const bool fallbackAdmobAdplacementStartEndChallenges = true;

	private const bool fallbackAdmobAdplacementPause = true;

	private const bool fallbackAdmobAdplacementGarage = true;

	private const int fallbackTimesPausedBeforePauseAd = 1;

	private const bool fallbackGlobalAdplacementGoToGame = true;

	private const bool fallbackGlobalAdplacementFailRetryChallenges = true;

	private const bool fallbackGlobalAdplacementStartEndChallenges = true;

	private const bool fallbackGlobalAdplacementPause = true;

	private const bool fallbackGlobalAdplacementGarage = true;

	private const bool fallbackGlobalAdplacement321 = true;

	private const bool fallbackGlobalBanner = true;

	private const string fallbackCorsspromoMoreGameslink = "market://search?q=pub:AxesInMotion Racing";

	private const string fallbackRateUsLink = "market://details?id=com.aim.racing";

	private const bool fallbackEnableRateUs = true;

	private const int fallbackMenuMaxClickedTimes = -1;

	private const int fallbackPauseMaxClickedTimes = -1;

	private const int fallbackPopupFirstTime = 100000;

	private const int fallbackPopupFrequency = 100000;

	private const int fallbackPopupMaxClickedTimes = -1;

	public string BUCKET_NAME;

	public Dictionary<string, object> data;

	public TextAsset textAsset;

	private string fallbackCrossPromoPause = "{\"pause_promo\":[{\"icon\":\"DEFAULT\",\"link\":\"market://search?q=pub:AxesInMotion Racing\",\"title\":\"\"}]}";

	private string fallbackCrossPromoMenu = "{\"menu_promo\":{\"icon\":\"DEFAULT\",\"link\":\"\",\"title\":\"\",\"promoted\":false}}";

	private string fallbackCrossPromoMenuPopup = "{\"menu_promo_popup\":{\"icon\":\"DEFAULT\",\"link\":\"\",\"title\":\"\",\"promoted\":false}}";

	private string fallbackCrossPromoPausePromoted = string.Empty;

	private string fallbackTimeServers = "{\"servers\":[\"time.google.com\",\"time.nist.gov\",\"time.windows.com\"]}";

	private bool alreadyInit;

	private bool amazonJsonSuccess;

	private bool secondAttempt;

	public int amazonRefreshRate
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_amazon_refresh_rate", 0);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_amazon_refresh_rate", value);
		}
	}

	public DateTime lastRefresh
	{
		get
		{
			return DateTime.FromBinary(long.Parse(ObscuredPrefs.GetString("CONFIG_lastRefresh", "0")));
		}
		set
		{
			ObscuredPrefs.SetString("CONFIG_lastRefresh", value.ToBinary().ToString());
		}
	}

	public int timeBetween321Ads
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_time_between_321_ads", 165);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_time_between_321_ads", value);
		}
	}

	public int timeBetweenRegularAds
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_time_between_regular_ads", 0);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_time_between_regular_ads", value);
		}
	}

	public int maxChallengeTrysAds
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_max_challenge_trys_between_before_ad", 6);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_max_challenge_trys_between_before_ad", value);
		}
	}

	public int timeBetweenGoToGameAds
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_time_between_enter_to_game_ads", 1800);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_time_between_enter_to_game_ads", value);
		}
	}

	public int consecutiveHeyzapRegularAds
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_max_consecutive_heyzap_in_regular_ads", 1);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_max_consecutive_heyzap_in_regular_ads", value);
		}
	}

	public int consecutiveAdmobRegularAds
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_max_consecutive_admob_in_regular_ads", 4);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_max_consecutive_admob_in_regular_ads", value);
		}
	}

	public bool startRegularWithAdmob
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_start_regular_ads_with_admob", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_start_regular_ads_with_admob", value);
		}
	}

	public bool admobAdplacementGoToGame
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_admob_adplacement_go_to_game", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_admob_adplacement_go_to_game", value);
		}
	}

	public bool admobAdplacementFailRetryChallenges
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_admob_adplacement_fail_or_retry_challenges", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_admob_adplacement_fail_or_retry_challenges", value);
		}
	}

	public bool admobAdplacementStartEndChallenges
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_admob_adplacement_start_or_end_challenges", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_admob_adplacement_start_or_end_challenges", value);
		}
	}

	public bool admobAdplacementPause
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_admob_adplacement_pause", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_admob_adplacement_pause", value);
		}
	}

	public bool admobAdplacementGarage
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_admob_adplacement_garage", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_admob_adplacement_garage", value);
		}
	}

	public int timesPausedBeforePauseAd
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_times_pause_before_pause_ad", 1);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_times_pause_before_pause_ad", value);
		}
	}

	public bool globalAdplacementGoToGame
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_global_adplacement_go_to_game", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_global_adplacement_go_to_game", value);
		}
	}

	public bool globalAdplacementFailRetryChallenges
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_global_adplacement_fail_or_retry_challenges", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_global_adplacement_fail_or_retry_challenges", value);
		}
	}

	public bool globalAdplacementStartEndChallenges
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_global_adplacement_start_or_end_challenges", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_global_adplacement_start_or_end_challenges", value);
		}
	}

	public bool globalAdplacementPause
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_global_adplacement_pause", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_global_adplacement_pause", value);
		}
	}

	public bool globalAdplacementGarage
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_global_adplacement_garage", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_global_adplacement_garage", value);
		}
	}

	public bool globalAdplacement321
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_global_adplacement_321", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_global_adplacement_321", value);
		}
	}

	public bool globalBanner
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_global_banner", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_global_banner", value);
		}
	}

	public string crossPromoPause
	{
		get
		{
			return ObscuredPrefs.GetString("CONFIG_pause_promo", fallbackCrossPromoPause);
		}
		set
		{
			ObscuredPrefs.SetString("CONFIG_pause_promo", value);
		}
	}

	public string crossPromoMenu
	{
		get
		{
			return ObscuredPrefs.GetString("CONFIG_menu_promo", fallbackCrossPromoMenu);
		}
		set
		{
			ObscuredPrefs.SetString("CONFIG_menu_promo", value);
		}
	}

	public string crossPromoMenuPopup
	{
		get
		{
			return ObscuredPrefs.GetString("CONFIG_menu_promo_popup", fallbackCrossPromoMenuPopup);
		}
		set
		{
			ObscuredPrefs.SetString("CONFIG_menu_promo_popup", value);
		}
	}

	public string crossPromoPausePromoted
	{
		get
		{
			return ObscuredPrefs.GetString("CONFIG_pause_promo_promoted", fallbackCrossPromoPausePromoted);
		}
		set
		{
			ObscuredPrefs.SetString("CONFIG_pause_promo_promoted", value);
		}
	}

	public string timeServers
	{
		get
		{
			return ObscuredPrefs.GetString("CONFIG_servers", fallbackTimeServers);
		}
		set
		{
			ObscuredPrefs.SetString("CONFIG_servers", value);
		}
	}

	public string crosspromoMoreGameslink
	{
		get
		{
			return ObscuredPrefs.GetString("CONFIG_link_more_games", "market://search?q=pub:AxesInMotion Racing");
		}
		set
		{
			ObscuredPrefs.SetString("CONFIG_link_more_games", value);
		}
	}

	public string rateUsLink
	{
		get
		{
			return ObscuredPrefs.GetString("CONFIG_rate_us_link", "market://details?id=com.aim.racing");
		}
		set
		{
			ObscuredPrefs.SetString("CONFIG_rate_us_link", value);
		}
	}

	public bool enableRateUs
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_enable_rate_us", true);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_enable_rate_us", value);
		}
	}

	public int menuMaxClickedTimes
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_menu_max_clicked_times", -1);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_menu_max_clicked_times", value);
		}
	}

	public int menuClickedTimes
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_menu_clicked", 0);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_menu_clicked", value);
		}
	}

	public bool crossPromoMenuClicked
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_menu_promo_clicked", false);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_menu_promo_clicked", value);
		}
	}

	public int pauseMaxClickedTimes
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_pause_max_clicked_times", -1);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_pause_max_clicked_times", value);
		}
	}

	public int pauseClickedTimes
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_pause_clicked", 0);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_pause_clicked", value);
		}
	}

	public bool crossPromoPauseClicked
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_pause_promo_clicked", false);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_pause_promo_clicked", value);
		}
	}

	public int menuPopupShowTimes
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_popup_show_times", 0);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_popup_show_times", value);
		}
	}

	public int menuPopupFirstTime
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_popup_first_time", 100000);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_popup_first_time", value);
		}
	}

	public int menuPopupFrequency
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_popup_frequency", 100000);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_popup_frequency", value);
		}
	}

	public int menuPopupMaxClickedTimes
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_menu_popup_max_clicked_times", -1);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_menu_popup_max_clicked_times", value);
		}
	}

	public int menuPopupClickedTimes
	{
		get
		{
			return ObscuredPrefs.GetInt("CONFIG_menu_popup_clicked", 0);
		}
		set
		{
			ObscuredPrefs.SetInt("CONFIG_menu_popup_clicked", value);
		}
	}

	public bool crossPromoMenuPopupClicked
	{
		get
		{
			return ObscuredPrefs.GetBool("CONFIG_menu_popup_promo_clicked", false);
		}
		set
		{
			ObscuredPrefs.SetBool("CONFIG_menu_popup_promo_clicked", value);
		}
	}

	public static event RemoteConfigurationLoadCallBack OnRemoteConfigurationLoad;

	private void Start()
	{
		InitializeOfflineConfiguration();
	}

	private void RequestConfigurationFile()
	{
		if (!alreadyInit)
		{
			InitializeOfflineConfiguration();
		}
	}

	private void InitializeOfflineConfiguration()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		dictionary["icon"] = "DEFAULT";
		dictionary["link"] = string.Empty;
		dictionary["title"] = string.Empty;
		dictionary["promoted"] = false;

		data = new Dictionary<string, object>();
		data["pause_promo"] = new List<object>();
		data["pause_promo_promoted"] = dictionary;
		data["menu_promo"] = dictionary;
		data["menu_promo_popup"] = dictionary;
		data["servers"] = new List<object>();

		amazonRefreshRate = 0;
		timeBetween321Ads = 165;
		timeBetweenRegularAds = 0;
		maxChallengeTrysAds = 6;
		timeBetweenGoToGameAds = 1800;
		consecutiveHeyzapRegularAds = 1;
		consecutiveAdmobRegularAds = 4;
		startRegularWithAdmob = true;
		admobAdplacementGoToGame = false;
		admobAdplacementFailRetryChallenges = false;
		admobAdplacementStartEndChallenges = false;
		admobAdplacementPause = false;
		admobAdplacementGarage = false;
		globalAdplacementGoToGame = false;
		globalAdplacementFailRetryChallenges = false;
		globalAdplacementStartEndChallenges = false;
		globalAdplacementPause = false;
		globalAdplacementGarage = false;
		globalAdplacement321 = false;
		globalBanner = false;
		timesPausedBeforePauseAd = 2147483647;
		crossPromoPause = "{\"pause_promo\":[]}";
		crossPromoMenu = fallbackCrossPromoMenu;
		crossPromoMenuPopup = fallbackCrossPromoMenuPopup;
		crossPromoPausePromoted = string.Empty;
		timeServers = "{\"servers\":[]}";
		crosspromoMoreGameslink = string.Empty;
		rateUsLink = string.Empty;
		enableRateUs = false;
		menuMaxClickedTimes = 0;
		pauseMaxClickedTimes = 0;
		menuPopupMaxClickedTimes = 0;
		menuPopupFirstTime = 100000;
		menuPopupFrequency = 100000;
		crossPromoMenuClicked = true;
		crossPromoPauseClicked = true;
		crossPromoMenuPopupClicked = true;
		lastRefresh = DateTime.UtcNow;
		amazonJsonSuccess = true;
		alreadyInit = true;

		if (MySingleton<Manager>.Instance.ads != null)
		{
			MySingleton<Manager>.Instance.ads.disabledAds = true;
		}
		if (OnRemoteConfigurationLoad != null)
		{
			OnRemoteConfigurationLoad();
		}
	}

	private string OnConfigurationFileReceived(string file)
	{
		try
		{
			SetVariablesInMemory(data = Json.Deserialize(file) as Dictionary<string, object>);
			amazonJsonSuccess = true;
			Debug.Log("Archivo de configuración remoto de amazon actualizado y cargado correctamente");
		}
		catch (Exception exception)
		{
			Debug.LogError("Error al parsear archivo de configuración de amazon.");
			Debug.LogException(exception, this);
		}
		lastRefresh = DateTime.UtcNow;
		return file;
	}

	private IEnumerator WaitAndCheckOneMoreTime()
	{
		yield return new WaitForSeconds(60f);
		if (!amazonJsonSuccess)
		{
			secondAttempt = true;
			RequestConfigurationFile();
		}
	}

	private bool IsTimeToRefresh()
	{
		if (MySingleton<Manager>.Instance.timeManager.HasTimeFromInternet())
		{
			DateTime dateTime = lastRefresh;
			DateTime timeFromInternet = MySingleton<Manager>.Instance.timeManager.GetTimeFromInternet();
			TimeSpan timeSpan = timeFromInternet - dateTime;
			Debug.Log("RemoteConfiguration. Hora de Internet. Tenian que pasar como minimo " + amazonRefreshRate + " horas y han pasado " + timeSpan.TotalHours + " horas.");
			return timeSpan.TotalHours > (double)amazonRefreshRate;
		}
		DateTime dateTime2 = lastRefresh;
		DateTime utcNow = DateTime.UtcNow;
		TimeSpan timeSpan2 = utcNow - dateTime2;
		Debug.Log("RemoteConfiguration. Hora local. Tenian que pasar como minimo " + amazonRefreshRate + " horas y han pasado " + timeSpan2.TotalHours + " horas.");
		return timeSpan2.TotalHours > (double)amazonRefreshRate;
	}

	private void SetVariablesInMemory(Dictionary<string, object> json)
	{
		timeBetween321Ads = ParseTimeBetween321Ads(json);
		timeBetweenRegularAds = ParseTimeBetweenRegularAds(json);
		maxChallengeTrysAds = ParseChallengeMaxTry(json);
		timeBetweenGoToGameAds = ParseGoToGameAd(json);
		consecutiveHeyzapRegularAds = ParseConsecutiveHeyzapRegular(json);
		consecutiveAdmobRegularAds = ParseConsecutiveAdmobRegular(json);
		startRegularWithAdmob = ParseFirstRegularAdIsAdmob(json);
		admobAdplacementGoToGame = ParseAdmobAdplacementGoToGame(json);
		admobAdplacementFailRetryChallenges = ParseAdmobAdplacementFailRetryChallenges(json);
		admobAdplacementStartEndChallenges = ParseAdmobAdplacementStartEndChallenges(json);
		admobAdplacementPause = ParseAdmobAdplacementGoToPause(json);
		admobAdplacementGarage = ParseAdmobAdplacementGoToGarage(json);
		timesPausedBeforePauseAd = ParseTimesPaused(json);
		globalAdplacementGoToGame = ParseGlobalAdplacementGoToGame(json);
		globalAdplacementFailRetryChallenges = ParseGlobalAdplacementFailRetryChallenges(json);
		globalAdplacementStartEndChallenges = ParseGlobalAdplacementStartEndChallenges(json);
		globalAdplacementPause = ParseGlobalAdplacementGoToPause(json);
		globalAdplacementGarage = ParseGlobalAdplacementGoToGarage(json);
		globalAdplacement321 = ParseGlobalAdplacement321(json);
		globalBanner = ParseGlobalBanner(json);
		crosspromoMoreGameslink = ParseCrosspromoMoreGamesLink(json);
		rateUsLink = ParseRateUsLink(json);
		enableRateUs = ParseEnableRateUs(json);
		amazonRefreshRate = ParseRefreshPeriod(json);
		pauseMaxClickedTimes = ParsePauseMaxClickedTimes(json);
		menuMaxClickedTimes = ParseMenuMaxClickedTimes(json);
		menuPopupMaxClickedTimes = ParseMenuPopupMaxClickedTimes(json);
		menuPopupFirstTime = ParseMenuPopupFirstTime(json);
		menuPopupFrequency = ParseMenuPopupFrequency(json);
		crossPromoPause = ParseCrossPromoPause(json);
		crossPromoMenu = ParseMenuCrossPromo(json);
		crossPromoMenuPopup = ParseMenuCrossPromoPopup(json);
		crossPromoPausePromoted = ParsePauseCrossPromoPromoted(json);
		timeServers = ParseTimeServers(json);
		if (!enableRateUs && MySingleton<Manager>.Instance.guiMenuLogic != null)
		{
			NGUITools.SetActive(MySingleton<Manager>.Instance.guiMenuLogic.rateButton.gameObject, false);
		}
		MySingleton<Manager>.Instance.ads.admobAdActive = MySingleton<Manager>.Instance.remoteConfiguration.startRegularWithAdmob;
		if (OnRemoteConfigurationLoad != null)
		{
			OnRemoteConfigurationLoad();
		}
	}

	private int ParseRefreshPeriod(Dictionary<string, object> json)
	{
		Debug.Log("amazon_refresh_rate" + json["amazon_refresh_rate"]);
		return (int)(long)json["amazon_refresh_rate"];
	}

	private int ParseTimeBetween321Ads(Dictionary<string, object> json)
	{
		Debug.Log("time_between_321_ads" + json["time_between_321_ads"]);
		return (int)(long)json["time_between_321_ads"];
	}

	private int ParseTimeBetweenRegularAds(Dictionary<string, object> json)
	{
		Debug.Log("time_between_regular_ads" + json["time_between_regular_ads"]);
		return (int)(long)json["time_between_regular_ads"];
	}

	private int ParseChallengeMaxTry(Dictionary<string, object> json)
	{
		Debug.Log("max_challenge_trys_between_before_ad" + json["max_challenge_trys_between_before_ad"]);
		return (int)(long)json["max_challenge_trys_between_before_ad"];
	}

	private int ParseGoToGameAd(Dictionary<string, object> json)
	{
		Debug.Log("time_between_enter_to_game_ads" + json["time_between_enter_to_game_ads"]);
		return (int)(long)json["time_between_enter_to_game_ads"];
	}

	private int ParseConsecutiveHeyzapRegular(Dictionary<string, object> json)
	{
		Debug.Log("max_consecutive_heyzap_in_regular_ads" + json["max_consecutive_heyzap_in_regular_ads"]);
		return (int)(long)json["max_consecutive_heyzap_in_regular_ads"];
	}

	private int ParseConsecutiveAdmobRegular(Dictionary<string, object> json)
	{
		Debug.Log("max_consecutive_admob_in_regular_ads" + json["max_consecutive_admob_in_regular_ads"]);
		return (int)(long)json["max_consecutive_admob_in_regular_ads"];
	}

	private bool ParseFirstRegularAdIsAdmob(Dictionary<string, object> json)
	{
		Debug.Log("start_regular_ads_with_admob" + json["start_regular_ads_with_admob"]);
		return (bool)json["start_regular_ads_with_admob"];
	}

	private bool ParseAdmobAdplacementGoToGame(Dictionary<string, object> json)
	{
		Debug.Log("admob_adplacement_go_to_game" + json["admob_adplacement_go_to_game"]);
		return (bool)json["admob_adplacement_go_to_game"];
	}

	private bool ParseAdmobAdplacementFailRetryChallenges(Dictionary<string, object> json)
	{
		Debug.Log("admob_adplacement_fail_or_retry_challenges" + json["admob_adplacement_fail_or_retry_challenges"]);
		return (bool)json["admob_adplacement_fail_or_retry_challenges"];
	}

	private bool ParseAdmobAdplacementStartEndChallenges(Dictionary<string, object> json)
	{
		Debug.Log("admob_adplacement_start_or_end_challenges" + json["admob_adplacement_start_or_end_challenges"]);
		return (bool)json["admob_adplacement_start_or_end_challenges"];
	}

	private bool ParseAdmobAdplacementGoToPause(Dictionary<string, object> json)
	{
		Debug.Log("admob_adplacement_pause" + json["admob_adplacement_pause"]);
		return (bool)json["admob_adplacement_pause"];
	}

	private bool ParseAdmobAdplacementGoToGarage(Dictionary<string, object> json)
	{
		Debug.Log("admob_adplacement_garage" + json["admob_adplacement_garage"]);
		return (bool)json["admob_adplacement_garage"];
	}

	private bool ParseGlobalAdplacementGoToGame(Dictionary<string, object> json)
	{
		Debug.Log("global_adplacement_go_to_game" + json["global_adplacement_go_to_game"]);
		return (bool)json["global_adplacement_go_to_game"];
	}

	private bool ParseGlobalAdplacementFailRetryChallenges(Dictionary<string, object> json)
	{
		Debug.Log("global_adplacement_fail_or_retry_challenges" + json["global_adplacement_fail_or_retry_challenges"]);
		return (bool)json["global_adplacement_fail_or_retry_challenges"];
	}

	private bool ParseGlobalAdplacementStartEndChallenges(Dictionary<string, object> json)
	{
		Debug.Log("global_adplacement_start_or_end_challenges" + json["global_adplacement_start_or_end_challenges"]);
		return (bool)json["global_adplacement_start_or_end_challenges"];
	}

	private bool ParseGlobalAdplacementGoToPause(Dictionary<string, object> json)
	{
		Debug.Log("global_adplacement_pause" + json["global_adplacement_pause"]);
		return (bool)json["global_adplacement_pause"];
	}

	private bool ParseGlobalAdplacementGoToGarage(Dictionary<string, object> json)
	{
		Debug.Log("global_adplacement_garage" + json["global_adplacement_garage"]);
		return (bool)json["global_adplacement_garage"];
	}

	private bool ParseGlobalAdplacement321(Dictionary<string, object> json)
	{
		Debug.Log("global_adplacement_321" + json["global_adplacement_321"]);
		return (bool)json["global_adplacement_321"];
	}

	private bool ParseGlobalBanner(Dictionary<string, object> json)
	{
		Debug.Log("global_banner" + json["global_banner"]);
		return (bool)json["global_banner"];
	}

	private int ParseTimesPaused(Dictionary<string, object> json)
	{
		Debug.Log("times_pause_before_pause_ad" + json["times_pause_before_pause_ad"]);
		return (int)(long)json["times_pause_before_pause_ad"];
	}

	private string ParseCrosspromoMoreGamesLink(Dictionary<string, object> json)
	{
		Debug.Log("link_more_games" + json["link_more_games"]);
		if (json.ContainsKey("link_more_games"))
		{
			return (string)json["link_more_games"];
		}
		Debug.Log("variable no definida en json: link_more_games");
		return "market://search?q=pub:AxesInMotion Racing";
	}

	private string ParseRateUsLink(Dictionary<string, object> json)
	{
		Debug.Log("rate_us_link" + json["rate_us_link"]);
		if (json.ContainsKey("rate_us_link"))
		{
			return (string)json["rate_us_link"];
		}
		Debug.Log("variable no definida en json: rate_us_link");
		return "market://details?id=com.aim.racing";
	}

	private bool ParseEnableRateUs(Dictionary<string, object> json)
	{
		Debug.Log("enable_rate_us" + json["enable_rate_us"]);
		return (bool)json["enable_rate_us"];
	}

	private int ParseMenuPopupMaxClickedTimes(Dictionary<string, object> json)
	{
		Debug.Log("menu_popup_max_clicked_times" + json["menu_popup_max_clicked_times"]);
		return (int)(long)json["menu_popup_max_clicked_times"];
	}

	private int ParseMenuPopupFirstTime(Dictionary<string, object> json)
	{
		Debug.Log("popup_first_time" + json["popup_first_time"]);
		return (int)(long)json["popup_first_time"];
	}

	private int ParseMenuPopupFrequency(Dictionary<string, object> json)
	{
		Debug.Log("popup_frequency" + json["popup_frequency"]);
		return (int)(long)json["popup_frequency"];
	}

	private int ParseMenuMaxClickedTimes(Dictionary<string, object> json)
	{
		Debug.Log("menu_max_clicked_times" + json["menu_max_clicked_times"]);
		return (int)(long)json["menu_max_clicked_times"];
	}

	private int ParsePauseMaxClickedTimes(Dictionary<string, object> json)
	{
		Debug.Log("pause_max_clicked_times" + json["pause_max_clicked_times"]);
		return (int)(long)json["pause_max_clicked_times"];
	}

	private string ParseCrossPromoPause(Dictionary<string, object> json)
	{
		string empty = string.Empty;
		if (json.ContainsKey("pause_promo"))
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			List<object> value = MySingleton<Manager>.Instance.remoteConfiguration.data["pause_promo"] as List<object>;
			dictionary["pause_promo"] = value;
			return Json.Serialize(dictionary);
		}
		return fallbackCrossPromoPause;
	}

	private string ParseMenuCrossPromo(Dictionary<string, object> json)
	{
		string empty = string.Empty;
		if (json.ContainsKey("menu_promo"))
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			Dictionary<string, object> dictionary2 = json["menu_promo"] as Dictionary<string, object>;
			object value = dictionary2;
			dictionary["menu_promo"] = value;
			return Json.Serialize(dictionary);
		}
		return fallbackCrossPromoMenu;
	}

	private string ParsePauseCrossPromoPromoted(Dictionary<string, object> json)
	{
		string empty = string.Empty;
		if (json.ContainsKey("pause_promo_promoted"))
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			Dictionary<string, object> dictionary2 = json["pause_promo_promoted"] as Dictionary<string, object>;
			object value = dictionary2;
			dictionary["pause_promo_promoted"] = value;
			return Json.Serialize(dictionary);
		}
		return fallbackCrossPromoPausePromoted;
	}

	private string ParseMenuCrossPromoPopup(Dictionary<string, object> json)
	{
		string empty = string.Empty;
		if (json.ContainsKey("menu_promo_popup"))
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			Dictionary<string, object> dictionary2 = json["menu_promo_popup"] as Dictionary<string, object>;
			object value = dictionary2;
			dictionary["menu_promo_popup"] = value;
			return Json.Serialize(dictionary);
		}
		return fallbackCrossPromoMenuPopup;
	}

	private string ParseTimeServers(Dictionary<string, object> json)
	{
		string empty = string.Empty;
		if (json.ContainsKey("servers"))
		{
			Dictionary<string, object> dictionary = new Dictionary<string, object>();
			List<object> value = MySingleton<Manager>.Instance.remoteConfiguration.data["servers"] as List<object>;
			dictionary["servers"] = value;
			return Json.Serialize(dictionary);
		}
		return fallbackCrossPromoPause;
	}
}
