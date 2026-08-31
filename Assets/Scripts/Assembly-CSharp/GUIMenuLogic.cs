using System;
using System.Collections;
using System.Collections.Generic;
using AxesInMotion.Network;
using AxesInMotion.Network.Minigames;
using AxesInMotion.SocialPlugin;
using CodeStage.AntiCheat.ObscuredTypes;
using Unibill;
using UnityEngine;

public class GUIMenuLogic : GUILogic
{
	public const string DISTANCIA_RESTANTE_DESBLOQUEO = "distanceLeft";

	public const string CREDITS_LABEL = "car_credits";

	public const string COLECCIONABLES_RESTANTES_DESBLOQUEO = "collectablesLeft";

	public const string DISTANCIA_Y_COLECCIONABLES_RESTANTES = "distanceAndCollectablesLeft";

	public const string PROMO_1_ANDROID_ID = "com.aim.airplanefight";

	public const string RACING_IOS_ID = "TODO";

	public const string GAME_OBJECT_NAME = "GUIMenuLogic";

	public GameObject pnInfoReward;

	public UILabel lbAmountReward;

	public GameObject pnConnecting;

	public GameObject pnConnectionLost;

	public GameObject pnUpdateAvailable;

	public GameObject pnConnectingToMultiplayerMode;

	public MultiplayerPanel pnMultiplayer;

	public MultiplayerNotificationPanel pnMultiplayerNotification;

	public bool updateShowed;

	private List<string> EUcountries = new List<string>();

	public UIPanel pnPolicy;

	[HideInInspector]
	public string RATE_IOS_ID;

	[HideInInspector]
	public string RATE_GOOGLE_PLAY_ID;

	[HideInInspector]
	public string RATE_AMAZON_ID;

	[HideInInspector]
	public string RATE_WINDOWS_PHONE_ID;

	public UIWidget WgStarterPackButtons;

	public UIPanel pnStarterPack;

	public UIPanel pnPopup;

	public UIRoot StarterPackDoor;

	public UIPanel pnDoor;

	public UISprite spTitleFade;

	public UISprite spTitle;

	public UILabel lbGaraje;

	public UIButton btDrive;

	public UIButton btGaraje;

	public UIButton btStore;

	public UIButton btMultiplayer;

	public UIWidget btTaskCompleteMenu;

	public UISprite spNewCarUnlocked;

	public UIGrid UnlocksGrid;

	public GameObject spDistance;

	public UISprite spDistanceTick;

	public GameObject spDistanceOk;

	public GameObject spDistanceNo;

	public UILabel lbDistance;

	public GameObject spCollectables;

	public UISprite spCollectablesTick;

	public GameObject spCollectablesOk;

	public GameObject spCollectablesNo;

	public UISprite spMiniCar;

	public UILabel lbCollectables;

	public GameObject spFacebook;

	public UISprite spFacebookTick;

	public GameObject spFacebookOk;

	public GameObject spFacebookNo;

	public UISprite btPackAndChecks;

	public UIWidget pnDailyRewards;

	public UISprite spHidenCar;

	public UIWidget pnPurchase;

	public UILabel lbPurchase;

	public UIPanel pnPurchaseInfo;

	public UILabel lbPurchaseInfo;

	public UISprite spTaskBg;

	public UILabel lbCreditsValue;

	public UIWidget btPackage;

	public GameObject CarsGrid;

	public GameObject CarThumbnail;

	public UIPanel menuPanel;

	public UIPanel moreGamesPanel;

	public UIPanel ratePanel;

	public UIPanel exitPanel;

	public UIPanel gameModePanel;

	public UIPanel pnSelectCar;

	public GameObject pnScreenTouch;

	public UIWidget restorePurchases;

	public UIWidget moreGames;

	public GameObject rateButton;

	public UISprite btPreviousCar;

	public UISprite btNextCar;

	public UISprite btDriveIt;

	public GameObject lockedParent;

	public UISprite btFacebookLogin;

	public UISprite btFacebookInvite;

	public UILabel LbFacebookLoginLockContainer;

	public UISprite btFacebookLoginLock;

	public UISprite spFacebookLoginLockTick;

	public UISprite spCollectablesLeft;

	public GameObject newCarUnlocked;

	public Transform spawnMenu;

	public UILabel lbTotalDistanceMenu;

	public UILabel lbCreditsMenu;

	public GameObject currentCar;

	private int indexCurrentCarArrayPos;

	private Animator camAnimator;

	public Animator lightAnimator;

	private int toGarage = Animator.StringToHash("ToGarage");

	private int toShowroom = Animator.StringToHash("ToShowroom");

	public Camera mainCamera;

	private AudioListener audioListener;

	private int rateStatus;

	private int updateRequestStatus;

	public BackButtonMenu backButton;

	private bool putCarBusy;

	private bool showMenuSetMenuView = true;

	private InfoCar infoCar;

	public AudioClip carPurchasedSound;

	public UIWidget coChallangeChk;

	private AnimatorStateInfo currentStateCam;

	public GameObject btRemoveAds;

	public LocalizedPriceForElement lbPrice;

	public bool openingPackStoreFromGaraje;

	private float starterPackTime;

	private float savedStarterPackTime;

	public UILabel lbCounteroffer;

	public UILabel lbCounterofferPopup;

	public UISprite spColorStarterPack;

	public void ShowConnectingToMultiplayerModePanel()
	{
		HideAll();
		NGUITools.SetActive(pnConnectingToMultiplayerMode.gameObject, true);
	}

	public void ShowMultiplayerPanel()
	{
		HideAll();
		if (MySingleton<Manager>.Instance.connection.connected)
		{
			backButton.function = GUIMenuButton.MenuButtonFunction.ShowMenuPanel;
			NGUITools.SetActive(pnMultiplayer.gameObject, true);
			pnMultiplayer.RefreshGUI();
			pnMultiplayer.RefreshFriendsList();
		}
		else
		{
			MySingleton<Manager>.Instance.connection.connectionLost = true;
			MySingleton<Manager>.Instance.connection.connectionLostCause = DisconnectionCause.Exception;
			ShowMenu();
		}
		ShowAchievementsPanel(AchievementManager.AchievementCategory.MULTIPLAYER);
	}

	public void ShowMultiplayerNotification(MultiplayerNotificationPanel.NotificationType notification)
	{
		HideAll();
		NGUITools.SetActive(pnMultiplayerNotification.gameObject, true);
		pnMultiplayerNotification.SetNotification(notification);
	}

	private void ShowConnectionLost()
	{
		HideAll();
		if (MySingleton<Manager>.Instance.connection.connectionLostCause == DisconnectionCause.DisconnectBecauseOfRoomIsFull)
		{
			string message = Localization.Get("multiplayernotification_roomfull");
			MySingleton<Manager>.Instance.notificationManager.RequestNotificationOK(message);
		}
		else
		{
			string message = ((MySingleton<Manager>.Instance.connection.connectionLostCause != DisconnectionCause.Inactivity) ? Localization.Get("connection_lost_default") : Localization.Get("connection_lost_inactivity"));
			MySingleton<Manager>.Instance.notificationManager.RequestNotificationYesNo(message, Localization.Get("reconnect"), Localization.Get("back"), new EventDelegate(MySingleton<Manager>.Instance.connection, "Reconnect"), new EventDelegate(this, "ShowMenu"));
		}
	}

	public void GoToConnectingPanel()
	{
		HideAll();
		ConnectionTestPanel component = pnConnecting.GetComponent<ConnectionTestPanel>();
		NGUITools.SetActive(pnConnecting.gameObject, true);
		component.Show(true);
	}

	public void GoToConnectingPanel(MiniGameLogic.MiniGameType miniGameMode)
	{
		HideAll();
		ConnectionTestPanel component = pnConnecting.GetComponent<ConnectionTestPanel>();
		MySingleton<Manager>.Instance.networkGame.miniGameSelected = miniGameMode;
		NGUITools.SetActive(pnConnecting.gameObject, true);
		component.Show(true);
	}

	public void LoginFacebook()
	{
		pnMultiplayer.LoginFacebook();
	}

	public void LogoutFacebook()
	{
		pnMultiplayer.Logout();
	}

	public void RefreshFriendList()
	{
		pnMultiplayer.RefreshFriendsList();
	}

	public void ShowUpdateAvailablePanel()
	{
		HideAll();
		updateShowed = true;
		NGUITools.SetActive(pnUpdateAvailable.gameObject, true);
	}

	private void UpdateGame()
	{
		if (MySingleton<Manager>.Instance.platform == Manager.Platform.Amazon)
		{
			Application.OpenURL("amzn://apps/android?p=" + RATE_AMAZON_ID);
		}
		else if (MySingleton<Manager>.Instance.platform == Manager.Platform.GooglePlay)
		{
			Application.OpenURL("market://details?id=" + RATE_GOOGLE_PLAY_ID);
		}
	}

	private void Awake()
	{
		MySingleton<Manager>.Instance.guiMenuLogic = this;
		camAnimator = mainCamera.GetComponent<Animator>();
		indexCurrentCarArrayPos = MySingleton<Manager>.Instance.GetIndexArrayPosFromId(MySingleton<Manager>.Instance.indexCurrentCar);
		MySingleton<Manager>.Instance.currentCheckPoint = 0;
		if ((bool)btMultiplayer && !MySingleton<Manager>.Instance.isMultiplayerAvailable)
		{
			NGUITools.SetActive(btMultiplayer.gameObject, false);
		}
	}

	public void StarterPackWasBought()
	{
		HideStarterPack();
		MySingleton<Manager>.Instance.starterPackBought = true;
	}

	public void HideStarterPack()
	{
		NGUITools.SetActive(WgStarterPackButtons.gameObject, false);
		NGUITools.SetActive(pnStarterPack.gameObject, false);
		NGUITools.SetActive(StarterPackDoor.gameObject, false);
		NGUITools.SetActive(btStore.gameObject, true);
		MySingleton<Manager>.Instance.starterPackPanelIsActive = false;
		MySingleton<Manager>.Instance.starterPackDoorActive = false;
	}

	private IEnumerator Start()
	{
		Time.timeScale = 1f;
		if (backButton == null)
		{
			backButton = base.gameObject.AddComponent<BackButtonMenu>();
		}
		int indexNewCar = MySingleton<Manager>.Instance.carUnlockedId;
		if (indexNewCar < 0)
		{
			PutCar();
			while (putCarBusy)
			{
				yield return new WaitForSeconds(0.8f);
			}
		}
		else
		{
			LoadUnlockedCar();
		}
		HideRemoveAdsButtonIfBought();
		yield return null;
		InitStarterPackAndServices();
		Invoke("UpdateTaskSprite", 5f);
	}

	public void InitStarterPackAndServices()
	{
		if (ObscuredPrefs.GetBool(Manager.COPPA_ALREADY_SHOWED))
		{
			if (!SettingsManager.GetStarterPackBoughtOrEnded() && SettingsManager.GetStarterPackCount() == 1)
			{
				NGUITools.SetActive(pnStarterPack.gameObject, true);
				MySingleton<Manager>.Instance.starterPackDoorActive = true;
				NGUITools.SetActive(StarterPackDoor.gameObject, true);
				SettingsManager.SetStarterPackCount(SettingsManager.GetStarterPackCount() + 1);
				backButton.function = GUIMenuButton.MenuButtonFunction.None;
			}
			else
			{
				GameServicesLogin();
				SettingsManager.SetStarterPackCount(SettingsManager.GetStarterPackCount() + 1);
			}
		}
	}

	protected override void OnEnable()
	{
		base.OnEnable();
		UpdateCreditsLabels();
		UpdateTaskSprite();
		UpdateNewCarSprite();
		RefreshCarsButtons();
		UnhoverButtons();
		HideRemoveAdsButtonIfBought();
		if (camAnimator != null)
		{
			camAnimator.CrossFade(currentStateCam.fullPathHash, 0f);
		}
		if ((bool)currentCar)
		{
			UpdateUnlocksGUI(currentCar.GetComponent<InfoCar>());
		}
		MySingleton<Manager>.Instance.billingManager.StarterPackBoughtEvent += StarterPackWasBought;
		if (!SettingsManager.GetStarterPackBoughtOrEnded())
		{
			StartCoroutine(RefreshCrono());
		}
		else
		{
			StopCoroutine(RefreshCrono());
		}
		Invoke("EnableOrDisableStarterPackElements", 1f);
		lbTotalDistanceMenu.text = Converter.MetersToCurrentUnits(MySingleton<Manager>.Instance.stats.metersDrove, true, true);
	}

	private void EnableOrDisableStarterPackElements()
	{
		if (!SettingsManager.GetStarterPackBoughtOrEnded())
		{
			NGUITools.SetActive(WgStarterPackButtons.gameObject, true);
			NGUITools.SetActive(btStore.gameObject, false);
		}
		else
		{
			NGUITools.SetActive(WgStarterPackButtons.gameObject, false);
			NGUITools.SetActive(btStore.gameObject, true);
		}
	}

	protected void OnDisable()
	{
		MySingleton<Manager>.Instance.billingManager.StarterPackBoughtEvent -= StarterPackWasBought;
		if ((bool)camAnimator)
		{
			currentStateCam = camAnimator.GetCurrentAnimatorStateInfo(0);
		}
		StopCoroutine(RefreshCrono());
	}

	public override void UpdateLocalizeLabels()
	{
		UpdatePriceForInApps();
	}

	private void ShowRate()
	{
		rateStatus = SettingsManager.GetRate();
		if (rateStatus != -1 && rateStatus % 3 == 0)
		{
			ShowRateMe();
			SettingsManager.SetRate(1);
		}
		else if (rateStatus != -1)
		{
			SettingsManager.SetRate(rateStatus + 1);
		}
		else
		{
			NGUITools.SetActive(rateButton, false);
		}
	}

	public void LoadUnlockedCar()
	{
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		for (int i = 0; i < carsInfoPrefabs.Length; i++)
		{
			if (carsInfoPrefabs[i].id == MySingleton<Manager>.Instance.carUnlockedId)
			{
				indexCurrentCarArrayPos = i;
				break;
			}
		}
		PutCar();
		UpdateUnlocksGUI(currentCar.GetComponent<InfoCar>());
		ShowSelectCar(true, true);
		MySingleton<Manager>.Instance.carUnlockedId = -1;
	}

	public void GoToGame()
	{
		MySingleton<Manager>.Instance.gameMode = Manager.GameMode.Free;
		MySingleton<Manager>.Instance.needMoveToSpawnCarPosition = true;
		LoadSceneGame();
	}

	public void GoToCheckpointGame()
	{
		MySingleton<Manager>.Instance.gameMode = Manager.GameMode.Checkpoint;
		LoadSceneGame();
	}

	public void GoToTrafficGame()
	{
		MySingleton<Manager>.Instance.gameMode = Manager.GameMode.Traffic;
		LoadSceneGame();
	}

	private void LoadSceneGame()
	{
		ManagerAnalytics.instance.FireStartGame(currentCar.GetComponent<InfoCar>().idName, MySingleton<Manager>.Instance.gameMode.ToString());
		ShowLoading();
		MySingleton<Manager>.Instance.ads.CheckForTimeInterstitial();
		LegacySceneManager.LoadLevel(2);
	}

	public void ShowGameMode()
	{
		HideAll();
		backButton.function = GUIMenuButton.MenuButtonFunction.ShowMenuPanel;
		HideMenu();
		NGUITools.SetActive(gameModePanel.gameObject, true);
	}

	public void HideGameMode()
	{
		NGUITools.SetActive(gameModePanel.gameObject, false);
	}

	public void ShowMenu()
	{
		SetMenuView();
		if (showMenuSetMenuView)
		{
			if (SettingsManager.GetStarterPackCount() != 1)
			{
				ShowRate();
			}
		}
		else
		{
			showMenuSetMenuView = true;
		}
	}

	private void SetMenuView()
	{
		indexCurrentCarArrayPos = MySingleton<Manager>.Instance.GetIndexArrayPosFromId(SettingsManager.GetCurrentCarSelectedId());
		HideAll();
		lbTotalDistanceMenu.text = Converter.MetersToCurrentUnits(MySingleton<Manager>.Instance.stats.metersDrove, true, true);
		if ((bool)lbCreditsMenu)
		{
			lbCreditsMenu.text = Unibiller.GetCurrencyBalance("credits").ToString();
		}
		UpdateTaskSprite();
		NGUITools.SetActive(spTitleFade.gameObject, true);
		NGUITools.SetActive(menuPanel.gameObject, true);
		backButton.function = GUIMenuButton.MenuButtonFunction.ShowExit;
		UnhoverButtons();
	}

	public override void OnTaskFinished(Task finishedTask)
	{
		UpdateTaskSprite();
	}

	public void OpenCompleteTask()
	{
		UpdateTaskSprite();
		OpenStore();
	}

	public void GoMenuFromGameMode()
	{
		HideAll();
		NGUITools.SetActive(menuPanel.gameObject, true);
		backButton.function = GUIMenuButton.MenuButtonFunction.ShowExit;
		UnhoverButtons();
	}

	public void GoMenuFromGarage()
	{
		lbTotalDistanceMenu.text = Converter.MetersToCurrentUnits(MySingleton<Manager>.Instance.stats.metersDrove, true, true);
		if ((bool)lbCreditsMenu)
		{
			lbCreditsMenu.text = Unibiller.GetCurrencyBalance("credits").ToString();
		}
		UpdateTaskSprite();
		UpdateNewCarSprite();
		indexCurrentCarArrayPos = MySingleton<Manager>.Instance.GetIndexArrayPosFromId(SettingsManager.GetCurrentCarSelectedId());
		PutCar();
		PlayShowroomAnim();
		HideAll();
		NGUITools.SetActive(menuPanel.gameObject, true);
		HideRemoveRateButtinIfRated();
		backButton.function = GUIMenuButton.MenuButtonFunction.ShowExit;
		UnhoverButtons();
		NGUITools.SetActive(spTitle.gameObject, true);
	}

	public void UnhoverButtons()
	{
		btGaraje.state = UIButtonColor.State.Normal;
		btDrive.state = UIButtonColor.State.Normal;
		btStore.state = UIButtonColor.State.Normal;
	}

	private void HideMenu()
	{
		NGUITools.SetActive(menuPanel.gameObject, false);
		NGUITools.SetActive(gameModePanel.gameObject, false);
	}

	public void ShowLoading()
	{
		UnityEngine.Object.Instantiate(MySingleton<Manager>.Instance.UIRootLoading);
	}

	public void ShowMoreGames()
	{
		Application.OpenURL(MySingleton<Manager>.Instance.remoteConfiguration.crosspromoMoreGameslink);
	}

	public void HideMoreGames()
	{
		backButton.function = GUIMenuButton.MenuButtonFunction.ShowExit;
		NGUITools.SetActive(moreGamesPanel.gameObject, false);
	}

	public void ShowRateMe()
	{
		backButton.function = GUIMenuButton.MenuButtonFunction.ShowMenuPanel;
		NGUITools.SetActive(spTitleFade.gameObject, false);
		HideMenu();
		NGUITools.SetActive(ratePanel.gameObject, true);
	}

	public void HideRateMe()
	{
		backButton.function = GUIMenuButton.MenuButtonFunction.ShowExit;
		NGUITools.SetActive(ratePanel.gameObject, false);
	}

	public void RateYes()
	{
		SettingsManager.SetRate(-1);
		HideRemoveRateButtinIfRated();
		Application.OpenURL(MySingleton<Manager>.Instance.remoteConfiguration.rateUsLink);
		NGUITools.SetActive(ratePanel.gameObject, false);
		SetMenuView();
	}

	public void RateNo()
	{
		SetMenuView();
	}

	public void ExitYes()
	{
		Application.Quit();
	}

	public void ShowExit()
	{
		NGUITools.SetActive(spTitleFade.gameObject, false);
		NGUITools.SetActive(exitPanel.gameObject, true);
		HideMenu();
		backButton.function = GUIMenuButton.MenuButtonFunction.ShowMenuPanel;
	}

	public void HideExit()
	{
		NGUITools.SetActive(exitPanel.gameObject, false);
		backButton.function = GUIMenuButton.MenuButtonFunction.ShowExit;
	}

	private void UpdateCreditsLabels()
	{
		if ((bool)lbCreditsMenu)
		{
			lbCreditsMenu.text = Unibiller.GetCurrencyBalance("credits").ToString();
		}
	}

	private void UpdateTaskSprite()
	{
		MySingleton<Manager>.Instance.UpdateDailyReward();
		bool state = MySingleton<Manager>.Instance.taskManager.HasCompletedTasks();
		if ((bool)btTaskCompleteMenu)
		{
			NGUITools.SetActive(btTaskCompleteMenu.gameObject, state);
		}
		else
		{
			Debug.LogError("btTaskCompleteMenu NULL");
		}
	}

	private void UpdateNewCarSprite()
	{
		NGUITools.SetActive(spNewCarUnlocked.gameObject, LookForNewUnlockedCar());
	}

	public void RemoveAds()
	{
		ManagerAnalytics.instance.FireIAPClicked("remove_ads", "main_menu");
		MySingleton<Manager>.Instance.currentLoadingPurchasePanel = UnityEngine.Object.Instantiate(MySingleton<Manager>.Instance.pnLoadingPurchase) as GameObject;
		PurchasableItem purchasableItemById = Unibiller.GetPurchasableItemById(MySingleton<Manager>.Instance.billingManager.removeAdsPurchaseId);
		MySingleton<Manager>.Instance.unibill.InitiatePurchase(purchasableItemById);
		HideRemoveAdsButtonIfBought();
	}

	public void BuyStarterPack()
	{
		NGUITools.SetActive(pnStarterPack.gameObject, false);
		NGUITools.SetActive(StarterPackDoor.gameObject, false);
		MySingleton<Manager>.Instance.starterPackDoorActive = false;
		PurchasableItem purchasableItemById = Unibiller.GetPurchasableItemById(MySingleton<Manager>.Instance.billingManager.starterPackOfferId);
		MySingleton<Manager>.Instance.unibill.InitiatePurchase(purchasableItemById);
		MySingleton<Manager>.Instance.starterPackPanelIsActive = false;
	}

	public void CloseStarterPack()
	{
		if (!pnDoor.gameObject.activeSelf)
		{
			MySingleton<Manager>.Instance.starterPackDoorActive = false;
			NGUITools.SetActive(pnStarterPack.gameObject, false);
			NGUITools.SetActive(StarterPackDoor.gameObject, false);
			backButton.function = GUIMenuButton.MenuButtonFunction.ShowExit;
			MySingleton<Manager>.Instance.starterPackPanelIsActive = false;
		}
	}

	public void ShowSelectCar(bool state, bool showOnly = false)
	{
		RefreshCarsButtons();
		CenterScrollOnSelectedCar();
		MySingleton<Manager>.Instance.stats.RefreshChallengesCompleted();
		NGUITools.SetActive(newCarUnlocked, false);
		if (state && showOnly)
		{
			HideAll();
		}
		PlayGarageAnim();
		backButton.function = GUIMenuButton.MenuButtonFunction.GoToMenuFromGarage;
		NGUITools.SetActiveSelf(pnSelectCar.gameObject, state);
		NGUITools.SetActive(spTitle.gameObject, false);
		UpdateCar();
		UpdateUnlocksGUI(currentCar.GetComponent<InfoCar>());
		if (MySingleton<Manager>.Instance.carUnlockedId < 0)
		{
			MySingleton<Manager>.Instance.ads.timeFromLastPlay = Time.realtimeSinceStartup;
			MySingleton<Manager>.Instance.admobFromMenu = true;
			MySingleton<Manager>.Instance.adsFromPause = false;
			MySingleton<Manager>.Instance.ads.ShowAdmobOrHeyzap(false, MySingleton<Manager>.Instance.remoteConfiguration.admobAdplacementGarage, MySingleton<Manager>.Instance.remoteConfiguration.globalAdplacementGarage);
		}
	}

	public void RefreshCarsButtons()
	{
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		int num = 0;
		foreach (Transform item in CarsGrid.transform)
		{
			InfoCar infoCar = carsInfoPrefabs[num];
			item.gameObject.GetComponent<CarButtonInfo>().SetId(num);
			item.GetComponent<UISprite>().atlas = MySingleton<Manager>.Instance.atlasCar;
			item.GetComponent<UISprite>().spriteName = infoCar.iconName;
			if (infoCar.dailyReward && !infoCar.isBought)
			{
				item.GetComponent<UISprite>().color = Color.black;
			}
			num++;
		}
	}

	private void HideAll()
	{
		NGUITools.SetActiveSelf(menuPanel.gameObject, false);
		NGUITools.SetActiveSelf(pnSelectCar.gameObject, false);
		NGUITools.SetActiveSelf(exitPanel.gameObject, false);
		NGUITools.SetActiveSelf(ratePanel.gameObject, false);
		NGUITools.SetActiveSelf(gameModePanel.gameObject, false);
		NGUITools.SetActiveSelf(moreGamesPanel.gameObject, false);
		NGUITools.SetActive(pnPurchaseInfo.gameObject, false);
		NGUITools.SetActive(spHidenCar.gameObject, false);
	}

	public void ShowPanelInfo(string title, string info)
	{
		if (title != null && title != string.Empty)
		{
			MySingleton<Manager>.Instance.storeInitData.titleInfo = title;
		}
		if (info != null && info != string.Empty)
		{
			MySingleton<Manager>.Instance.storeInitData.textInfo = info;
		}
		MySingleton<Manager>.Instance.storeInitData.InfoStore = true;
		OpenStore();
	}

	public void HideRemoveAdsButtonIfBought()
	{
		if (Unibiller.GetPurchaseCount(MySingleton<Manager>.Instance.billingManager.removeAdsPurchaseId) > 0 || Unibiller.GetPurchaseCount(MySingleton<Manager>.Instance.billingManager.removeAdsOfferPurchaseId) > 0 || Unibiller.GetPurchaseCount(MySingleton<Manager>.Instance.billingManager.starterPackOfferId) > 0)
		{
			NGUITools.SetActive(btRemoveAds, false);
		}
	}

	public void HideRemoveRateButtinIfRated()
	{
		if (SettingsManager.GetRate() < 0)
		{
			NGUITools.SetActive(rateButton.gameObject, false);
		}
	}

	public void HidePurchaseInfoPanel()
	{
		NGUITools.SetActive(pnPurchaseInfo.gameObject, false);
	}

	public void OpenPackStore(InfoCar.PurchasablePack packID = InfoCar.PurchasablePack.none)
	{
		if (packID == InfoCar.PurchasablePack.none)
		{
			MySingleton<Manager>.Instance.storeInitData.packID = currentCar.GetComponent<InfoCar>().purchasablePack;
		}
		else
		{
			MySingleton<Manager>.Instance.storeInitData.packID = packID;
		}
		OpenStore();
	}

	public void OpenDailyRewards()
	{
		MySingleton<Manager>.Instance.storeInitData.storeDailyRewards = true;
		OpenStore();
	}

	public void SelectThisCar()
	{
		SelectThisCar(indexCurrentCarArrayPos);
	}

	public void SelectThisCar(int index)
	{
		indexCurrentCarArrayPos = index;
		SettingsManager.SetCurrentCarSelectedId(MySingleton<Manager>.Instance.carsInfoPrefabs[index].id);
		PlayShowroomAnim();
		ShowMenu();
		NGUITools.SetActive(spTitle.gameObject, true);
	}

	public void ShowConfirmationBuyCar(bool state, bool showOnly = false)
	{
		float price = currentCar.GetComponent<InfoCar>().price;
	}

	public void ShowPreviousCar()
	{
		ShowCar(indexCurrentCarArrayPos - 1);
		CenterScrollOnSelectedCar();
	}

	public void ShowNextCar()
	{
		ShowCar(indexCurrentCarArrayPos + 1);
		CenterScrollOnSelectedCar();
	}

	public void ShowCar(int value)
	{
		WasDrivedOrClicked(currentCar.GetComponent<InfoCar>());
		if (!putCarBusy && indexCurrentCarArrayPos != value)
		{
			NGUITools.SetActive(pnPurchaseInfo.gameObject, false);
			indexCurrentCarArrayPos = value;
			PutCar();
			UpdateUnlocksGUI(currentCar.GetComponent<InfoCar>());
			UpdateNewCarSprite();
		}
	}

	private void UpdatePriceForInApps()
	{
		if (!MySingleton<Manager>.Instance.isNewGUI)
		{
			lbPrice.LocalizeLabel();
		}
	}

	private void UpdateCar()
	{
		InfoCar component = currentCar.GetComponent<InfoCar>();
		bool state = indexCurrentCarArrayPos != 0;
		NGUITools.SetActiveSelf(btPreviousCar.gameObject, state);
		bool state2 = indexCurrentCarArrayPos != MySingleton<Manager>.Instance.carsInfoPrefabs.Length - 1;
		NGUITools.SetActiveSelf(btNextCar.gameObject, state2);
		bool isBought = component.isBought;
		NGUITools.SetActive(btDriveIt.gameObject, isBought);
		NGUITools.SetActive(lockedParent.gameObject, !isBought);
		float value = Mathf.Max(component.price - (float)MySingleton<Manager>.Instance.stats.metersDrove, 0f);
		string text = Converter.MetersToCurrentUnits(value, true, true);
		bool isCollectable = component.isCollectable;
		bool flag = MySingleton<Manager>.Instance.stats.AllCollectedForThisCar(component);
		currentCar.GetComponent<CarVisuals>().enabled = true;
	}

	public void UpdateUnlocksGUI(InfoCar infoCar)
	{
		if (infoCar.isBought)
		{
			NGUITools.SetActive(btDriveIt.gameObject, true);
			NGUITools.SetActive(lockedParent.gameObject, false);
			NGUITools.SetActive(spHidenCar.gameObject, false);
			return;
		}
		NGUITools.SetActive(lockedParent.gameObject, true);
		NGUITools.SetActive(spHidenCar.gameObject, infoCar.dailyReward);
		if (infoCar.purchasable && infoCar.purchasablePack == InfoCar.PurchasablePack.none && !infoCar.isChallangeReward)
		{
			NGUITools.SetActive(pnPurchase.gameObject, true);
			lbCreditsValue.text = 3500m.ToString();
		}
		else
		{
			NGUITools.SetActive(pnPurchase.gameObject, false);
		}
		NGUITools.SetActive(btPackage.gameObject, infoCar.purchasablePack != InfoCar.PurchasablePack.none && !infoCar.isChallangeReward);
		NGUITools.SetActive(btPackAndChecks.gameObject, infoCar.isChallangeReward && infoCar.purchasablePack != InfoCar.PurchasablePack.none);
		if (infoCar.price > 0f || infoCar.collectableType != Collectables.CollectableType.None || infoCar.isChallangeReward || infoCar.dailyReward)
		{
			NGUITools.SetActive(spTaskBg.gameObject, true);
			UpdateTaskBg(infoCar);
		}
		else
		{
			NGUITools.SetActive(spTaskBg.gameObject, false);
		}
	}

	private void UpdateTaskBg(InfoCar infoCar)
	{
		foreach (Transform item in UnlocksGrid.transform)
		{
			NGUITools.SetActive(item.gameObject, false);
		}
		if (infoCar.price > 0f)
		{
			float num = Mathf.Max(infoCar.price - (float)MySingleton<Manager>.Instance.stats.metersDrove, 0f);
			string arg = Converter.MetersToCurrentUnits(num, true, true);
			lbDistance.text = string.Format(Localization.Get("distanceLeft"), arg);
			if (num <= 0f)
			{
				if (!MySingleton<Manager>.Instance.isNewGUI)
				{
					spDistanceTick.GetComponent<UISprite>().spriteName = "checkbox_marked";
				}
				else
				{
					NGUITools.SetActive(spDistanceOk, true);
					NGUITools.SetActive(spDistanceNo, false);
				}
			}
			else if (!MySingleton<Manager>.Instance.isNewGUI)
			{
				spDistanceTick.GetComponent<UISprite>().spriteName = "checkbox_not_marked";
			}
			else
			{
				NGUITools.SetActive(spDistanceOk, false);
				NGUITools.SetActive(spDistanceNo, true);
			}
			NGUITools.SetActive(spDistance, true);
		}
		if (infoCar.collectableType != Collectables.CollectableType.None)
		{
			lbCollectables.text = string.Format(Localization.Get("collectablesLeft"), MySingleton<Manager>.Instance.stats.GetCollectablesRemainingForThisCar(infoCar));
			spMiniCar.atlas = MySingleton<Manager>.Instance.atlasCar;
			spMiniCar.spriteName = Collectables.GetIconNameFor(infoCar.collectableType);
			if (MySingleton<Manager>.Instance.stats.AllCollectedForThisCar(infoCar))
			{
				if (!MySingleton<Manager>.Instance.isNewGUI)
				{
					spCollectablesTick.GetComponent<UISprite>().spriteName = "checkbox_marked";
				}
				else
				{
					NGUITools.SetActive(spCollectablesOk, true);
					NGUITools.SetActive(spCollectablesNo, false);
				}
			}
			else if (!MySingleton<Manager>.Instance.isNewGUI)
			{
				spCollectablesTick.GetComponent<UISprite>().spriteName = "checkbox_not_marked";
			}
			else
			{
				NGUITools.SetActive(spCollectablesOk, false);
				NGUITools.SetActive(spCollectablesNo, true);
			}
			NGUITools.SetActive(spCollectables, true);
		}
		if (infoCar.isChallangeReward)
		{
			GUIMenuChallangeMode.MakeMenuItems(coChallangeChk, infoCar);
		}
		if (infoCar.dailyReward)
		{
			NGUITools.SetActive(pnDailyRewards.gameObject, true);
		}
		UnlocksGrid.Reposition();
	}

	public bool LookForNewUnlockedCar()
	{
		bool result = false;
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		foreach (InfoCar infoCar in carsInfoPrefabs)
		{
			bool flag = true;
			if (!infoCar.isBought && !infoCar.purchasable && !infoCar.dailyReward)
			{
				if (infoCar.purchasablePack != InfoCar.PurchasablePack.none)
				{
					flag = false;
				}
				if (infoCar.price > 0f)
				{
					float num = Mathf.Max(infoCar.price - (float)MySingleton<Manager>.Instance.stats.metersDrove, 0f);
					if (num > 0f)
					{
						flag = false;
					}
				}
				if (infoCar.collectableType != Collectables.CollectableType.None && !MySingleton<Manager>.Instance.stats.AllCollectedForThisCar(infoCar))
				{
					flag = false;
				}
				if (infoCar.isChallangeReward)
				{
					if (!GUIMenuChallangeMode.Completed(infoCar))
					{
						flag = false;
					}
					if (flag)
					{
						MonoBehaviour.print("COCHE OBTENIDO POR LAS PRUEBAS!!!: " + infoCar.name);
					}
				}
				if (flag)
				{
					infoCar.isBought = true;
				}
			}
			else if (!infoCar.isBought && infoCar.purchasable && infoCar.isChallangeReward)
			{
				if (!GUIMenuChallangeMode.Completed(infoCar))
				{
					flag = false;
				}
				if (flag)
				{
					infoCar.isBought = true;
				}
			}
			if (infoCar.isBought && !ObscuredPrefs.HasKey(infoCar.id + "keyDistanceDroveByCar"))
			{
				result = true;
			}
		}
		return result;
	}

	private void CenterScrollOnSelectedCar()
	{
		Transform child = CarsGrid.transform.GetChild(indexCurrentCarArrayPos);
		UIPanel uIPanel = NGUITools.FindInParents<UIPanel>(child.gameObject);
		if (uIPanel != null && uIPanel.clipping != UIDrawCall.Clipping.None)
		{
			UIScrollView component = uIPanel.GetComponent<UIScrollView>();
			Vector3 pos = -uIPanel.cachedTransform.InverseTransformPoint(child.position);
			if (!component.canMoveHorizontally)
			{
				pos.x = uIPanel.cachedTransform.localPosition.x;
			}
			if (!component.canMoveVertically)
			{
				pos.y = uIPanel.cachedTransform.localPosition.y;
			}
			SpringPanel.Begin(uIPanel.cachedGameObject, pos, 6f);
		}
	}

	public void PutCar()
	{
		putCarBusy = true;
		if ((bool)currentCar)
		{
			NGUITools.Destroy(currentCar);
		}
		if (indexCurrentCarArrayPos < 0 || indexCurrentCarArrayPos >= MySingleton<Manager>.Instance.carsInfoPrefabs.Length)
		{
			indexCurrentCarArrayPos = 0;
		}
		InfoCar infoCar = MySingleton<Manager>.Instance.carsInfoPrefabs[indexCurrentCarArrayPos];
		WasDrivedOrClicked(infoCar);
		GameObject original = Resources.Load(infoCar.idName) as GameObject;
		currentCar = UnityEngine.Object.Instantiate(original, spawnMenu.position, spawnMenu.rotation) as GameObject;
		InfoCar infoCar2 = currentCar.AddMissingComponent<InfoCar>();
		infoCar2.Initialize(infoCar);
		if (infoCar.dailyReward && !infoCar.isBought)
		{
			currentCar.SetActive(false);
		}
		Rigidbody previewBody = currentCar.GetComponent<Rigidbody>();
		previewBody.linearVelocity = Vector3.zero;
		previewBody.angularVelocity = Vector3.zero;
		previewBody.useGravity = false;
		previewBody.isKinematic = true;
		previewBody.constraints = RigidbodyConstraints.FreezeAll;
		Camera[] componentsInChildren = currentCar.GetComponentsInChildren<Camera>();
		Camera[] array = componentsInChildren;
		foreach (Camera camera in array)
		{
			camera.gameObject.SetActive(false);
		}
		UpdateCar();
		MySingleton<Manager>.Instance.indexLastCarInstantiate = infoCar.id;
		putCarBusy = false;
	}

	public void LoadFirstTime()
	{
		InfoCar component = currentCar.GetComponent<InfoCar>();
		SettingsManager.SetCarColor(component.id, component.defaultColor);
	}

	public void WasDrivedOrClicked(InfoCar info)
	{
		if (info.isBought)
		{
			MySingleton<Manager>.Instance.stats.carId = info.id;
			if (!ObscuredPrefs.HasKey(info.id + "keyDistanceDroveByCar"))
			{
				SettingsManager.SetDistanceDrove(info.id.ToString(), 0);
			}
			MySingleton<Manager>.Instance.stats.DistanceInitData();
		}
	}

	public void PlayShowroomAnim()
	{
		ResetAllTriggers();
		camAnimator.SetTrigger(toShowroom);
		lightAnimator.SetTrigger(toShowroom);
	}

	public void PlayGarageAnim()
	{
		ResetAllTriggers();
		camAnimator.SetTrigger(toGarage);
		lightAnimator.SetTrigger(toGarage);
	}

	public void ResetAllTriggers()
	{
		camAnimator.ResetTrigger(toShowroom);
		camAnimator.ResetTrigger(toGarage);
		lightAnimator.ResetTrigger(toShowroom);
		lightAnimator.ResetTrigger(toGarage);
	}

	private void GameServicesLogin()
	{
		try
		{
			if (MySingleton<Manager>.Instance.platform != Manager.Platform.GooglePlay || !(MySingleton<Manager>.Instance.gameServicesManager != null))
			{
				return;
			}
			MySingleton<Manager>.Instance.gameServicesManager.CheckAndLogin((ISocialEventArgs args) =>
			{
				if (MySingleton<Manager>.Instance.gameServicesManager.isLoggedIn)
				{
					MySingleton<Manager>.Instance.challangeManager.ReportAllScoresWhenLoginSucceed();
				}
			});
		}
		catch (Exception ex)
		{
			Debug.LogError("Error creating GPG object: " + ex.Message);
		}
	}

	public override void PurchasesRestored(bool success)
	{
		if (success)
		{
			string title = Localization.Get("billing_purchases_restored");
			ShowPanelInfo(title, " ");
		}
		else
		{
			string title2 = Localization.Get("billing_could_not_restore");
			string info = Localization.Get("check_internet");
			ShowPanelInfo(title2, info);
		}
		HideRemoveAdsButtonIfBought();
	}

	public override void PurchaseCompleted(PurchaseEvent purchase)
	{
		OpenStore(purchase);
		HideRemoveAdsButtonIfBought();
	}

	public override void PurchaseCanceled(PurchasableItem item)
	{
		string title = Localization.Get("billing_purchase_cancelled");
		string info = Localization.Get("billing_not_charged");
		ShowPanelInfo(title, info);
	}

	public override void PurchaseFailed(PurchasableItem item)
	{
		string title = Localization.Get("billing_purchase_failed");
		string info = Localization.Get("check_internet");
		ShowPanelInfo(title, info);
	}

	public override void BillerInitialized(UnibillState state)
	{
		UpdateLocalizeLabels();
	}

	public void PurchaseCar()
	{
		if (Unibiller.GetCurrencyBalance("credits") < 3500m)
		{
			ShowOffersPanel(true);
			return;
		}
		AudioSource.PlayClipAtPoint(carPurchasedSound, base.transform.position);
		Unibiller.DebitBalance("credits", 3500m);
		InfoCar component = currentCar.GetComponent<InfoCar>();
		component.isBought = true;
		ManagerAnalytics.instance.FireCarUnlocked(currentCar.GetComponent<InfoCar>().idName, "currency_car");
		WasDrivedOrClicked(component);
		SelectThisCar();
	}

	public void ShowOffersPanel(bool show)
	{
		if (show)
		{
			int num = (int)(3500m - Unibiller.GetCurrencyBalance("credits"));
			lbPurchaseInfo.text = string.Format(Localization.Get("need_more_credits_placeholder"), num.ToString());
		}
		NGUITools.SetActive(pnPurchaseInfo.gameObject, show);
	}

	private void RefreshGUIAfterPurchaseCar()
	{
		lbCreditsMenu.text = Unibiller.GetCurrencyBalance("credits").ToString();
		NGUITools.SetActive(btDriveIt.gameObject, true);
		NGUITools.SetActive(lockedParent.gameObject, false);
	}

	public void ShowScreenshotPanel(bool show)
	{
		if (show)
		{
			HideAll();
			NGUITools.SetActive(pnScreenTouch, true);
		}
		else
		{
			NGUITools.SetActive(pnScreenTouch.gameObject, false);
			NGUITools.SetActive(pnSelectCar.gameObject, true);
		}
	}

	public void ClosePolicy()
	{
	}

	public void OpenPolicy()
	{
		ClosePolicy();
		Application.OpenURL("https://sites.google.com/site/axesinmotion/politica-de-privacidad-privacy-policy");
	}

	private void loadUECountries()
	{
		EUcountries.Add("AT");
		EUcountries.Add("BE");
		EUcountries.Add("BG");
		EUcountries.Add("HR");
		EUcountries.Add("CY");
		EUcountries.Add("CZ");
		EUcountries.Add("DK");
		EUcountries.Add("EE");
		EUcountries.Add("FI");
		EUcountries.Add("FR");
		EUcountries.Add("GF");
		EUcountries.Add("PF");
		EUcountries.Add("TF");
		EUcountries.Add("DE");
		EUcountries.Add("GR");
		EUcountries.Add("HU");
		EUcountries.Add("IE");
		EUcountries.Add("IT");
		EUcountries.Add("LV");
		EUcountries.Add("LT");
		EUcountries.Add("LU");
		EUcountries.Add("MT");
		EUcountries.Add("NL");
		EUcountries.Add("PL");
		EUcountries.Add("PT");
		EUcountries.Add("RO");
		EUcountries.Add("SK");
		EUcountries.Add("SI");
		EUcountries.Add("ES");
		EUcountries.Add("SE");
		EUcountries.Add("ES");
		EUcountries.Add("GB");
	}

	private bool IsEUByNetworkCountryIso(out string country)
	{
		bool result = false;
		country = string.Empty;
		try
		{
			AndroidJavaObject androidJavaObject = new AndroidJavaObject("android.telephony.TelephonyManager");
			country = androidJavaObject.Call<string>("getNetworkCountryIso", new object[0]);
			if (EUcountries.Contains(country.ToUpper()))
			{
				result = true;
			}
			return result;
		}
		catch (Exception)
		{
			return result;
		}
	}

	private bool IsEuByTimeZone(out int timeZone)
	{
		bool result = false;
		int num = (timeZone = (int)TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now).TotalHours);
		if (num == 3 && !TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now))
		{
			result = false;
		}
		else if (num == 0 || num == 1 || num == 2 || num == 3)
		{
			result = true;
		}
		return result;
	}

	private void ApplyBannerChanges(string country, int timeZone)
	{
		PlayerPrefs.SetInt("ShowedBanner", 1);
		if (country == null || country == string.Empty)
		{
		}
		PlayerPrefs.Save();
	}

	public void ActiveAchievementPanel(bool active)
	{
		if (active)
		{
			pnAchievements.GetComponent<AchievementsPanel>().Init(AchievementManager.AchievementCategory.MULTIPLAYER);
		}
		NGUITools.SetActive(pnAchievements, active);
	}

	public void ActiveProgressPanel(bool active)
	{
		if (active)
		{
			if (pnProgressPanelReference != null)
			{
				UnityEngine.Object.DestroyImmediate(pnProgressPanelReference);
				pnProgressPanelReference = null;
			}
			pnProgressPanelReference = UnityEngine.Object.Instantiate(MySingleton<Manager>.Instance.pnProgresPanel) as GameObject;
			pnProgressPanelReference.transform.localScale = Vector3.one;
			pnProgressPanelReference.GetComponentInChildren<UIAchievementsBehaviour>().SetCamera(mainCamera.gameObject);
			backButton.function = GUIMenuButton.MenuButtonFunction.CloseProgressPanel;
		}
		else
		{
			if (pnProgressPanelReference != null)
			{
				UnityEngine.Object.DestroyImmediate(pnProgressPanelReference);
				pnProgressPanelReference = null;
			}
			backButton.function = GUIMenuButton.MenuButtonFunction.ShowMenuPanel;
		}
	}

	private IEnumerator RefreshCrono()
	{
		starterPackTime = MySingleton<Manager>.Instance.localChrono.GetRemainingUnescaled(MySingleton<Manager>.Instance.startingTime);
		while (starterPackTime > 0f)
		{
			UpdateColorSprite();
			if (starterPackTime > SettingsManager.STARTER_PACK_2ND_STEP)
			{
				lbCounterofferPopup.color = MySingleton<Manager>.Instance.cronoColors[0];
				lbCounteroffer.color = MySingleton<Manager>.Instance.cronoColors[0];
			}
			else if (starterPackTime > SettingsManager.STARTER_PACK_3ND_STEP && starterPackTime <= SettingsManager.STARTER_PACK_2ND_STEP)
			{
				lbCounterofferPopup.color = MySingleton<Manager>.Instance.cronoColors[1];
				lbCounteroffer.color = MySingleton<Manager>.Instance.cronoColors[1];
			}
			else
			{
				lbCounterofferPopup.color = MySingleton<Manager>.Instance.cronoColors[2];
				lbCounteroffer.color = MySingleton<Manager>.Instance.cronoColors[2];
			}
			lbCounterofferPopup.text = Converter.SecondsToHhMmSs(starterPackTime);
			lbCounteroffer.text = Converter.SecondsToHhMmSs(starterPackTime);
			starterPackTime = MySingleton<Manager>.Instance.localChrono.GetRemainingUnescaled(MySingleton<Manager>.Instance.startingTime);
			yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(1f));
		}
		SettingsManager.SetStarterPackBoughtOrEnded(true);
		Debug.Log("counter finish");
		HideStarterPack();
	}

	private void UpdateColorSprite()
	{
		if (starterPackTime > SettingsManager.STARTER_PACK_3ND_STEP && starterPackTime <= SettingsManager.STARTER_PACK_2ND_STEP)
		{
			spColorStarterPack.spriteName = "SP_OrangeIcon";
		}
		else if (starterPackTime < SettingsManager.STARTER_PACK_3ND_STEP)
		{
			spColorStarterPack.spriteName = "SP_RedIcon";
		}
	}
}
