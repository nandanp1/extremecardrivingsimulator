using System;
using System.Collections;
using System.Collections.Generic;
using AxesInMotion.Network;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class Manager : MySingleton<Manager>
{
	public enum SelectedGame
	{
		ExtremeCarDriving = 0,
		ExtremeSUVDriving = 1,
		MultiplayerDriving = 2,
		SanFrancisco = 3,
		NewYork = 4
	}

	public enum Platform
	{
		GooglePlay = 0,
		Amazon = 1
	}

	public enum SO
	{
		Android = 0,
		IOS = 1,
		WP8 = 2
	}

	public enum TrafficSystem
	{
		Old = 0,
		ITStraffic = 1
	}

	public enum Controls
	{
		Wheel = 0,
		Accelerometer = 1,
		Buttons = 2
	}

	public enum GameMode
	{
		None = -1,
		Free = 0,
		Checkpoint = 1,
		Traffic = 2,
		Multiplayer = 3
	}

	public const string TAG_REMOTE_PLAYER = "RemotePlayer";

	public const int SCENE_LOADING = 0;

	public const int SCENE_MENU = 1;

	public const int SCENE_GAME = 2;

	public const int SCENE_WORKSHOP = 3;

	public const int SCENE_STORE = 4;

	public const string SCENE_STORE_NAME_FOR_DESTROY = "4Store";

	public const string LAYER_TRAFFIC = "Traffic";

	public const string LAYER_WHEEL = "Wheel";

	public const string LAYER_CRASHABLE_ACTIVE = "CrashableActive";

	public const string LAYER_INTERIOR = "Interior";

	public const string LAYER_STORE = "Store";

	public const int LAYER_CRASHABLE_ACTIVE_INT = 17;

	public const int LAYER_IGNORE_RAYCAST_INT = 2;

	public const string TAG_CAR = "Car";

	public const string TAG_INSTANTIATE = "InstantiateAI";

	public const string TAG_FRONTLIGHT = "FrontLight";

	public const string TAG_PLAYER_CAR = "Player";

	public const string TAG_TRIAL_CAMERA = "TrialCamera";

	public const string POLICY_URL = "https://sites.google.com/site/axesinmotion/politica-de-privacidad-privacy-policy";

	private const string CHALLENGE_TRIAL_COMPLETED_FLAG_KEY_PLAYERPREFS = "AllTrialsCompleted";

	private const string CHALLENGE_RADAR_COMPLETED_FLAG_KEY_PLAYERPREFS = "AllRadarsCompleted";

	private const string CHALLENGE_CRASHABLE_COMPLETED_FLAG_KEY_PLAYERPREFS = "AllCrashablesCompleted";

	private const string ALL_CHALLENGES_COMPLETED_FLAG_KEY_PLAYERPREFS = "AllChallengesCompleted";

	public NetworkManager networkManager;

	public Connection connection;

	public NetworkRooms networkRooms;

	public NetworkGame networkGame;

	public NetworkFriends networkFriends;

	public Color[] cronoColors;

	public SelectedGame selectedGame;

	public Platform platform;

	public TrafficSystem trafficSystem;

	public string bundleIdentifier;

	public string bundleversion;

	public bool isChallangeAvailable;

	public bool isFacebookAvailable;

	public bool isGooglePlayServicesAvailable;

	public bool isLeaderboardsAvailable;

	public bool isMultiplayerAvailable;

	public bool isAnalyticsAvailable;

	public bool starterPackBought;

	public bool starterPackDoorActive;

	public bool isNewGUI;

	public Ads ads;

	public bool adsFromPause;

	public bool admobFromPause;

	public bool admobFromMenu;

	public bool starterPopUpAlreadySeen;

	public static string COPPA_ALREADY_SHOWED = "COPPA_KEY";

	public UIAtlas atlasCar;

	public BillingManager unibill;

	public GameObject currentUIRoot;

	public GUILogic guiCurrentLogic;

	public GUIMenuLogic guiMenuLogic;

	public GUIGameLogic guiGameLogic;

	public GUIWorkshopLogic guiWorkshopLogic;

	public GUIStoreLogic guiStoreLogic;

	public GUIChallangeMode guiChallangeModeLogic;

	public GameLogic gameLogic;

	public BillingManager billingManager;

	public string[] packNameKeys;

	public GameObject UIRootLoading;

	public GameObject instanceUIRootLoading;

	public GameObject pnLoadingPurchase;

	public GameObject currentLoadingPurchasePanel;

	public bool isRollingScreenshotMode;

	public int currentWorkshopId;

	public string currentLevelZoneName;

	public bool returningFromWorkshop;

	public int currentCheckPoint;

	public LevelZone levelZone;

	public LevelZone startChallangeLevelZone;

	public GameObject checkPointPrefab;

	public bool paused;

	public InfoCar[] carsInfoPrefabs;

	public GameObject pnProgresPanel;

	public List<CollectablesInfoList> collectablesCollection;

	public List<CollectablesInfoList> collectablesZone1;

	public List<CollectablesInfoList> collectablesZone2;

	public List<CollectablesInfoList> collectablesZone3;

	public int indexCurrentCar;

	public int indexLastCarInstantiate;

	public InfoCar.PurchasablePack indexLastPackInstantiate;

	public bool alreadyLoaded;

	[HideInInspector]
	public int carUnlockedId = -1;

	public SettingsManager.Language language;

	public List<LanguageToggleButton> allLanguages = new List<LanguageToggleButton>();

	[HideInInspector]
	public StatsManager stats;

	[HideInInspector]
	public TaskManager taskManager;

	[HideInInspector]
	public bool needMoveToSpawnCarPosition = true;

	[HideInInspector]
	public ChallangeManager challangeManager;

	[HideInInspector]
	public ChallangeModeManager challangeModeManager;

	[HideInInspector]
	public FacebookManager facebookManager;

	[HideInInspector]
	public GameServicesManager gameServicesManager;

	[HideInInspector]
	public NotificationManager notificationManager;

	[HideInInspector]
	public RemoteConfiguration remoteConfiguration;

	[HideInInspector]
	public GameMode gameMode;

	[HideInInspector]
	public Controls controls;

	[HideInInspector]
	public TimeManager timeManager;

	public SettingsManager.Units units;

	public UISprite spLoading;

	public StoreInitData storeInitData;

	public CollectablesManagerInfo collectablesManagerAnalytics;

	private float refreshTaskHours = 1f;

	public LayerMask carWheelLayerMask;

	public ChronometerEvent chronoEvents;

	public ChronometerClass localChrono;

	private DateTime currentDate;

	public float startingTime;

	[HideInInspector]
	public bool starterPackPanelIsActive;

	public bool popupActive = true;

	public bool isDriftInstalled;

	public bool isRunnerCarsInstalled;

	public bool isRunnerBikesInstalled;

	public bool restorePurchases;

	public bool multiplayerShowedAchievements { get; set; }

	public bool generalShowedAchievements { get; set; }

	public bool admobInterstitialIsReady
	{
		get
		{
			return ads.admobUniversal.IsAdmobAdLoaded();
		}
	}

	private void CheckVersion()
	{
		GameObject gameObject = GameObject.FindGameObjectWithTag("MPVersion");
		NetworkVersion.LoadVersion(gameObject.name);
		NetworkVersion.CheckVersion();
		UnityEngine.Object.Destroy(gameObject);
	}

	private void Awake()
	{
		Application.targetFrameRate = 60;
		AndroidNative.enableImmersiveMode();
		generalShowedAchievements = true;
		multiplayerShowedAchievements = true;
		isFacebookAvailable = false;
		isGooglePlayServicesAvailable = false;
		isLeaderboardsAvailable = false;
		isMultiplayerAvailable = false;
		isAnalyticsAvailable = false;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		Screen.sleepTimeout = -1;
		LoadSettings();
		UnityEngine.Object.Instantiate(UIRootLoading);
		timeManager = GetComponent<TimeManager>();
		stats = GetComponent<StatsManager>();
		taskManager = GetComponent<TaskManager>();
		challangeManager = GetComponent<ChallangeManager>();
		notificationManager = GetComponent<NotificationManager>();
		remoteConfiguration = GetComponent<RemoteConfiguration>();
		SetNewGUILogic();
		if (!SettingsManager.GetStarterPackBoughtOrEnded())
		{
			StartCrono();
		}
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		if (hasFocus)
		{
			AndroidNative.enableImmersiveMode();
		}
	}

	private void Start()
	{
		if (ObscuredPrefs.GetBool(COPPA_ALREADY_SHOWED, false))
		{
		}
		StartCoroutine("WaitOneSecondForHourlyReward");
		StartCoroutine(GoToGame());
		CheckChallengesCompleted();
		if (collectablesManagerAnalytics != null)
		{
			collectablesManagerAnalytics.CheckCollectables();
		}
	}

	private IEnumerator WaitOneSecondForHourlyReward()
	{
		while (true)
		{
			UpdateTaskHours();
			yield return StartCoroutine("Wait", refreshTaskHours);
		}
	}

	public IEnumerator Wait(float time)
	{
		float start = Time.realtimeSinceStartup;
		while (Time.realtimeSinceStartup < start + time)
		{
			yield return null;
		}
	}

	private void LoadSettings()
	{
		controls = SettingsManager.GetControl();
		units = SettingsManager.GetUnits();
		indexCurrentCar = SettingsManager.GetCurrentCarSelectedId();
		language = SettingsManager.GetLanguage();
		Localization.language = language.ToString();
	}

	public int GetIndexArrayPosFromId(int id)
	{
		for (int i = 0; i < MySingleton<Manager>.Instance.carsInfoPrefabs.Length; i++)
		{
			if (MySingleton<Manager>.Instance.carsInfoPrefabs[i].id == id)
			{
				return i;
			}
		}
		return 0;
	}

	public string GetPackNameKey(InfoCar.PurchasablePack pack)
	{
		int num = (int)(pack - 1);
		return packNameKeys[num];
	}

	private void SetNewGUILogic()
	{
		SelectedGame selectedGame = this.selectedGame;
		if (selectedGame == SelectedGame.SanFrancisco)
		{
			isNewGUI = true;
		}
		else
		{
			isNewGUI = false;
		}
	}

	public void MoreCars()
	{
		Debug.Log("MoreCars pressed");
		if (platform == Platform.GooglePlay)
		{
			Application.OpenURL("market://search?q=pub:AxesInMotion Racing");
		}
		if (platform == Platform.Amazon)
		{
			Application.OpenURL("amzn://apps/android?p=com.aim.racing&showAll=1");
		}
	}

	private IEnumerator GoToGame()
	{
		yield return new WaitForSeconds(0.3f);
		float total = carsInfoPrefabs.Length;
		float actual = 0f;
		if (SettingsManager.GetBool("keyCarsNeedFirstLoad", true))
		{
			InfoCar[] array = carsInfoPrefabs;
			foreach (InfoCar carInfo in array)
			{
				actual++;
				float percent = actual / total;
				spLoading.fillAmount = percent;
				yield return null;
				ResourceRequest request = Resources.LoadAsync(carInfo.idName);
				while (!request.isDone)
				{
					yield return null;
				}
			}
			SettingsManager.SetBool("keyCarsNeedFirstLoad", false);
		}
		LegacySceneManager.LoadLevelAsync(1);
	}

	public void AdsStart()
	{
		AudioListener.volume = 0f;
		Time.timeScale = 0f;
	}

	public void AdsEnded()
	{
		ads.lastTimeAds = Time.realtimeSinceStartup;
		if (MySingleton<Manager>.Instance.paused)
		{
			AudioListener.volume = 0f;
			Time.timeScale = 0f;
		}
		else
		{
			AudioListener.volume = 1f;
			Time.timeScale = 1f;
		}
	}

	public void SetStoreReward(int newReward)
	{
		storeInitData.storeFlag = newReward;
	}

	public int ConsumeStoreFlag()
	{
		int storeFlag = storeInitData.storeFlag;
		storeInitData.storeFlag = -1;
		return storeFlag;
	}

	public void UpdateDailyReward()
	{
		if (MySingleton<Manager>.Instance.timeManager.HasTimeFromInternet())
		{
			DateTime time = SettingsManager.GetTime("keyLastDailyRewardClaimed");
			DateTime timeFromInternetPlusLocalTimenZone = MySingleton<Manager>.Instance.timeManager.GetTimeFromInternetPlusLocalTimenZone();
			double totalHours = timeFromInternetPlusLocalTimenZone.Date.Subtract(time.Date).TotalHours;
			if ((time.Ticks > 1 && totalHours > 24.0) || time.DayOfYear != timeFromInternetPlusLocalTimenZone.DayOfYear)
			{
				taskManager.taskDaily.AddProgress(1f);
			}
		}
	}

	private void UpdateTaskHours()
	{
		MySingleton<Manager>.Instance.taskManager.taskHour.AddProgress(refreshTaskHours);
	}

	public void CheckChallengesCompleted()
	{
		if (!ObscuredPrefs.HasKey("AllTrialsCompleted"))
		{
			int num = 1;
			bool flag = true;
			while (flag && num <= 4)
			{
				if (!ObscuredPrefs.HasKey("chgBest_Trial__2Game_Trial_0" + num))
				{
					flag = false;
				}
				num++;
			}
			if (flag)
			{
				ObscuredPrefs.SetInt("AllTrialsCompleted", 1);
			}
		}
		if (!ObscuredPrefs.HasKey("AllRadarsCompleted"))
		{
			int num2 = 1;
			bool flag2 = true;
			while (flag2 && num2 <= 4)
			{
				if (!ObscuredPrefs.HasKey("chgBest_Radar__2Game_Radar_0" + num2))
				{
					flag2 = false;
				}
				num2++;
			}
			if (flag2)
			{
				ObscuredPrefs.SetInt("AllRadarsCompleted", 1);
			}
		}
		if (!ObscuredPrefs.HasKey("AllCrashablesCompleted"))
		{
			int num3 = 1;
			bool flag3 = true;
			while (flag3 && num3 <= 4)
			{
				if (!ObscuredPrefs.HasKey("chgBest_Crashable__2Game_Crashable_0" + num3))
				{
					flag3 = false;
				}
				else
				{
					int num4 = ObscuredPrefs.GetInt("chgBest_Crashable__2Game_Crashable_0" + num3);
					if (num4 < CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE)
					{
						flag3 = false;
					}
				}
				num3++;
			}
			if (flag3)
			{
				ObscuredPrefs.SetInt("AllCrashablesCompleted", 1);
			}
		}
		if (ObscuredPrefs.HasKey("AllTrialsCompleted") && ObscuredPrefs.HasKey("AllRadarsCompleted") && ObscuredPrefs.HasKey("AllCrashablesCompleted") && !ObscuredPrefs.HasKey("AllChallengesCompleted"))
		{
			ObscuredPrefs.SetInt("AllChallengesCompleted", 1);
		}
	}

	private void StartCrono()
	{
		if (!ObscuredPrefs.HasKey("starterlocaltime"))
		{
			PlayerPrefs.SetString("starterlocaltime", DateTime.Now.ToBinary().ToString());
		}
		long dateData = Convert.ToInt64(PlayerPrefs.GetString("starterlocaltime"));
		DateTime value = DateTime.FromBinary(dateData);
		currentDate = DateTime.Now;
		TimeSpan timeSpan = currentDate.Subtract(value);
		startingTime = SettingsManager.STARTER_PACK_COUNTER - (float)timeSpan.TotalSeconds;
		localChrono = new ChronometerClass();
		chronoEvents = new ChronometerEvent();
		localChrono.Start();
	}

	private bool IsAppInstalled(string bundleID)
	{
		if (Application.platform == RuntimePlatform.Android)
		{
			AndroidJavaObject androidJavaObject = null;
			try
			{
				AndroidJavaClass androidJavaClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
				AndroidJavaObject androidJavaObject2 = androidJavaClass.GetStatic<AndroidJavaObject>("currentActivity");
				AndroidJavaObject androidJavaObject3 = androidJavaObject2.Call<AndroidJavaObject>("getPackageManager", new object[0]);
				androidJavaObject = androidJavaObject3.Call<AndroidJavaObject>("getLaunchIntentForPackage", new object[1] { bundleID });
			}
			catch (Exception ex)
			{
				Debug.Log("exception" + ex.Message);
				return false;
			}
			if (androidJavaObject == null)
			{
				return false;
			}
			return true;
		}
		return false;
	}

	public string GetCarNameIDFromID(int id)
	{
		InfoCar[] array = carsInfoPrefabs;
		foreach (InfoCar infoCar in array)
		{
			if (infoCar.id == id)
			{
				return infoCar.idName;
			}
		}
		Debug.LogError("Ningun coche encontrado con ese ID");
		return "NONE";
	}
}
