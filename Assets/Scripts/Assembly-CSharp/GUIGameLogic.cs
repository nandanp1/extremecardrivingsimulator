using System.Collections;
using System.Collections.Generic;
using AxesInMotion.Network;
using AxesInMotion.Network.Minigames;
using Unibill;
using UnityEngine;

public class GUIGameLogic : GUILogic, GUIFacebookScreenshotModeExtension
{
	private const float WIDE_GAMEPLAY_HUD_ASPECT = 2f;

	private const float WIDE_GAMEPLAY_HUD_SCALE = 0.68f;

	private bool wideGameplayHudBaselineCaptured;

	private Vector3 wideGameplayHudPanelBaselineScale;

	private readonly Dictionary<Transform, Vector3> wideGameplayHudChildBaselinePositions = new Dictionary<Transform, Vector3>();

	private Coroutine wideGameplayHudLayoutCoroutine;

	public const string GAME_OBJECT_NAME = "GUIGameLogic";

	public const string ISLAND_ID = "com.aim.racingisland";

	public const string EXTREME_CAR_ID_GOOGLEPLAY = "com.aim.racing&referrer=utm_source%3Dcom.aim.rallyhum%26utm_campaign%3DInhouse";

	public const string EXTREME_CAR_ID_AMAZON = "com.aim.racing";

	public const string EXTREME_SUV_ID_GOOGLEPLAY = "com.aim.rallyhum&referrer=utm_source%3Dcom.aim.racing%26utm_campaign%3DInhouse";

	public const string EXTREME_SUV_ID_AMAZON = "com.aim.rallyhum";

	public const string EXTREME_CAR_ID_IOS = "959498315";

	public const string EXTREME_SUV_ID_IOS = "999788251";

	public const string TO_UNLOCK = "to_unlock";

	public const string OTHER_GAME = "other_game";

	public const string UNITS_KMH_SPRITE_NAME = "kmh";

	public const string UNITS_MPH_SPRITE_NAME = "mph";

	public GameObject ui3DRoot;

	public GameObject pnNetworkRadar;

	public UIButton btPlayers;

	public PlayersPanel pnPlayers;

	public GameObject pnChat;

	public Chat playerChat;

	public GameObject pnAddPlayerConfirmation;

	public Transform miniGamesParent;

	public GUIRacingMiniGameLogic racingGUILogic;

	public PlayerEntry playerEntrySelected;

	private Camera _lastCamera;

	private Vector3 _relativeTargetToGround;

	private Vector3 _relativeCameraToTarget;

	private bool _clampPitch;

	private float _clampPitchThreshold = 0.25f;

	private Vector3 _groundYPosition = new Vector3(0f, 20f, 0f);

	public static readonly Color COLOR_SELECTED = Color.white;

	public static readonly Color COLOR_UNSELECTED = new Color(1f, 1f, 1f, 0.5f);

	public static readonly Color COLOR_DARK_GRAY = new Color(0.6f, 0.6f, 0.6f);

	public static readonly Color COLOR_MAGENTA = new Color(1f, 0f, 0.35f);

	public Transform arrowTransform;

	public Transform cameraPointerTransform;

	public UIWidget btTaskCompleteHUD;

	public UIWidget btTaskCompletePause;

	public UIPanel pnPause;

	public UILabel lbTotalDistance;

	public UIPanel pnHud;

	public UIPanel pnChangeZone;

	public UIPanel pnCantPaint;

	public UISprite spAbsStatus;

	public UISprite spTcStatus;

	public UISprite spEspStatus;

	public UIButton controlsWheelButton;

	public UIButton controlsAccelButton;

	public UIButton controlsArrowsButton;

	public UILabel lbGear;

	public UILabel lbSpeed;

	public UISprite spRevs;

	public UISprite spUnits;

	public UILabel lbControl;

	public UILabel lbUnits;

	public UILabel lbMusic;

	public UISlider btSliderVolume;

	public UILabel lbCredits;

	public GameObject arrows;

	public GameObject steeringWheel;

	[HideInInspector]
	public BackButtonGame backButton;

	private GameLogic logic;

	private Ads ads;

	public UIPanel pnAds;

	public UILabel lbTimeAd;

	public UISprite spSpeedUnit;

	public UILabel lbOdometer;

	public UILabel lbUnitsOdo;

	public UISprite btUnits;

	public UIPanel pnCarUnlocked;

	public UISprite spUnlockedCar;

	public UIPanel pnCollectablesLeft;

	public UILabel lbCollectablesLeft;

	public UISprite spCollectableLeft;

	public UIPanel pnLoadingBg;

	public UISprite spMiniLoading;

	public UIPanel pnLevelLoading;

	public InfoCar nextCarByDistance;

	public int carUnlocked;

	public Font normalFont;

	public Font boldFont;

	public UILabel lbToUnlock;

	public UISprite spIsland;

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

	public UISprite spNextCar;

	public UIPanel pnChallengeMode;

	public UIWidget btNextToUnlock;

	public UISprite btResetCar;

	public UISprite btRestartChallangeCircuit;

	public UISprite btLanguage;

	public UISprite btBackToGame;

	public UISprite btBackToGarage;

	public UISprite btLights;

	public UILabel lbl_facebookTitleGame;

	public GameObject[] collectablesAllZones;

	public GameObject MainCameraGame2;

	public GUIChallangeMode guiChallangeMode;

	public UIButton btInvite;

	public bool facebookScreenshotModeAvaialable;

	public GameObject exclusiveNetworkElements;

	public GameObject exclusiveFreeElements;

	private int pauseAdsCounter;

	public GameObject btRemoveAds;

	public GameObject btRemoveAdsOffer;

	public LocalizedPriceForElement lbPrice;

	public UIWidget btLeaderboardPause;

	public UIWidget btLeaderboardActivationZone;

	public UIWidget btLeaderboardPreview;

	public UIWidget btLeaderboardFinish;

	public UIWidget btVolume;

	public UIPanel pnStarterPack;

	public GameObject starterPackElement;

	public UIGrid packsGrid;

	public UISprite spStoreStarterPack;

	public UISprite spStore;

	private float starterPackTime;

	private float savedStarterPackTime;

	public UILabel lbCounterofferPopup;

	public UISprite spColorStarterPack;

	public UILabel lbCronoStarterPackPacks;

	private IEnumerator CheckPing()
	{
		int badPingThreshold = 150;
		int badPingTimes = 0;
		int maxBadPingAllowed = 10;
		int timeBetweenCheking = 1;
		while (MySingleton<Manager>.Instance.networkManager.connected)
		{
			int currentPing = PhotonNetwork.GetPing();
			if (currentPing > badPingThreshold)
			{
				badPingTimes++;
				Debug.LogWarning("Bad ping. Times: " + badPingTimes);
			}
			else
			{
				badPingTimes = 0;
			}
			if (badPingTimes >= maxBadPingAllowed)
			{
				MySingleton<Manager>.Instance.connection.connectionLost = true;
				MySingleton<Manager>.Instance.connection.lostInGame = MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Multiplayer;
				MySingleton<Manager>.Instance.connection.connectionLostCause = DisconnectionCause.LatencyTooHigh;
				PhotonNetwork.Disconnect();
				BackToMenu();
			}
			yield return new WaitForSeconds(timeBetweenCheking);
		}
	}

	public void ShowPlayersList()
	{
		NGUITools.SetActive(pnPause.gameObject, false);
		NGUITools.SetActive(pnPlayers.gameObject, true);
		backButton.function = GUIGameButton.GameButtonFunction.Pause;
		pnPlayers.RefreshPlayerList();
	}

	public void HidePlayerList()
	{
		NGUITools.SetActive(pnPause.gameObject, true);
		NGUITools.SetActive(pnPlayers.gameObject, false);
		backButton.function = GUIGameButton.GameButtonFunction.Resume;
	}

	public void ShowAddFriendConfirmation()
	{
		NGUITools.SetActive(pnPause.gameObject, false);
		NGUITools.SetActive(pnPlayers.gameObject, false);
		NGUITools.SetActive(pnAddPlayerConfirmation.gameObject, true);
		backButton.function = GUIGameButton.GameButtonFunction.Pause;
	}

	public void HideAddFriendConfirmation()
	{
		NGUITools.SetActive(pnAddPlayerConfirmation.gameObject, false);
		NGUITools.SetActive(pnPlayers.gameObject, true);
		playerEntrySelected = null;
		backButton.function = GUIGameButton.GameButtonFunction.Resume;
	}

	public void ConfirmateAddFriend()
	{
		playerEntrySelected.AddThisPlayerAsFriend();
		HideAddFriendConfirmation();
	}

	public void ActivePauseMenu(bool active)
	{
		NGUITools.SetActive(pnPause.gameObject, active);
	}

	private void AwakeFacebookScreenshotMode()
	{
		GUIFacebookScreenshotMode.Instance().guiExtension = this;
	}

	public void SetBackButtonFunctionToNone()
	{
		backButton.function = GUIGameButton.GameButtonFunction.None;
	}

	public void SetBackButtonFunctionToMinimizeResumeScreenshot()
	{
		backButton.function = GUIGameButton.GameButtonFunction.MinimizeResumeScreenshot;
	}

	public void SetBackButtonFunctionToCancelScreenshot()
	{
		backButton.function = GUIGameButton.GameButtonFunction.CancelScreenshot;
	}

	public void SetBackButtonFunctionToLast()
	{
		backButton.function = GUIGameButton.GameButtonFunction.Resume;
	}

	public void SetBackButtonFunctionToResumeScreenshot()
	{
		backButton.function = GUIGameButton.GameButtonFunction.ResumeScreenshot;
	}

	public void PreInitMode()
	{
		GUIFacebookScreenshotMode.Instance().SetTarget(MySingleton<Manager>.Instance.gameLogic.currentCar);
		MySingleton<Manager>.Instance.ads.DisableAdsFromChallange();
		_lastCamera = MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarCameras>().Current();
		_lastCamera.gameObject.SetActive(false);
		if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Checkpoint)
		{
			CheckpointLogic checkpointLogic = Object.FindFirstObjectByType<CheckpointLogic>();
			if (checkpointLogic != null)
			{
				checkpointLogic.gameObject.SetActive(false);
			}
		}
		GUIFacebookScreenshotMode.Instance().SetGroundLimit(_groundYPosition);
		NGUITools.SetActive(pnPause.gameObject, false);
	}

	public void PostFinalizeMode()
	{
		_lastCamera.gameObject.SetActive(true);
		_lastCamera = null;
		MySingleton<Manager>.Instance.ads.EnableAds();
		if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Checkpoint)
		{
			CheckpointLogic checkpointLogic = Object.FindFirstObjectByType<CheckpointLogic>();
			if (checkpointLogic != null)
			{
				checkpointLogic.gameObject.SetActive(true);
			}
		}
		NGUITools.SetActive(pnPause.gameObject, true);
	}

	public void BeforePerformCameraRotation()
	{
	}

	public Camera GetCurrentCamera()
	{
		return MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarCameras>().FixedCameras[0];
	}

	private void Awake()
	{
		MySingleton<Manager>.Instance.guiGameLogic = this;
		if (facebookScreenshotModeAvaialable)
		{
			AwakeFacebookScreenshotMode();
		}
	}

	private void Start()
	{
		MainCameraGame2 = GameObject.Find("MainCamera");
		// The reference 4.17.6 HUD uses the fourth left-column slot for repair.
		// BtLights occupied the same slot in the recovered scene and was being
		// forcibly enabled here, producing a duplicate control that is not in the
		// reference gameplay.
		NGUITools.SetActive(btLights.gameObject, false);
		logic = MySingleton<Manager>.Instance.gameLogic;
		logic.steer = steeringWheel.GetComponent<CarSteeringWheel>();
		SetActiveCollectables();
		logic.guiLogic = this;
		HideRemoveAdsButtonIfBought();
		ads = MySingleton<Manager>.Instance.ads;
		ads.PnAdsComing = pnAds;
		ads.LbTime = lbTimeAd;
		MySingleton<Manager>.Instance.carUnlockedId = -1;
		logic.musicAudioSource.volume = SettingsManager.GetMusicVolume();
		backButton = base.gameObject.AddMissingComponent<BackButtonGame>();
		NextCarToUnlock();
		UpdateGUI();
		ScheduleWideGameplayHudLayout();
		ads.EnableAds();
	}

	private void ScheduleWideGameplayHudLayout()
	{
		if (wideGameplayHudLayoutCoroutine != null)
		{
			StopCoroutine(wideGameplayHudLayoutCoroutine);
		}
		wideGameplayHudLayoutCoroutine = StartCoroutine(AdaptGameplayHudForWideScreensAfterLayout());
	}

	private IEnumerator AdaptGameplayHudForWideScreensAfterLayout()
	{
		// NGUI's recovered widgets use AnchorUpdate.OnEnable. Their first Update
		// happens after MonoBehaviour.Start and would otherwise overwrite the
		// counter-scaled child positions, collapsing some of the left-side rows.
		yield return null;
		AdaptGameplayHudForWideScreens();
		wideGameplayHudLayoutCoroutine = null;
	}

	private void AdaptGameplayHudForWideScreens()
	{
		if (pnHud == null || Screen.height <= 0 || (float)Screen.width / (float)Screen.height < WIDE_GAMEPLAY_HUD_ASPECT)
		{
			return;
		}
		// Scale the panel so button press tweens retain their original local
		// scales. Counter-scale each top-level position to keep its screen anchor
		// unchanged instead of pulling the whole HUD toward screen center.
		if (!wideGameplayHudBaselineCaptured)
		{
			wideGameplayHudPanelBaselineScale = pnHud.transform.localScale;
			wideGameplayHudChildBaselinePositions.Clear();
			foreach (Transform child in pnHud.transform)
			{
				wideGameplayHudChildBaselinePositions[child] = child.localPosition;
			}
			wideGameplayHudBaselineCaptured = true;
		}

		Vector3 panelScale = wideGameplayHudPanelBaselineScale;
		panelScale.x *= WIDE_GAMEPLAY_HUD_SCALE;
		panelScale.y *= WIDE_GAMEPLAY_HUD_SCALE;
		pnHud.transform.localScale = panelScale;
		foreach (Transform child in pnHud.transform)
		{
			Vector3 localPosition;
			if (!wideGameplayHudChildBaselinePositions.TryGetValue(child, out localPosition))
			{
				localPosition = child.localPosition;
				wideGameplayHudChildBaselinePositions[child] = localPosition;
			}
			localPosition.x /= WIDE_GAMEPLAY_HUD_SCALE;
			localPosition.y /= WIDE_GAMEPLAY_HUD_SCALE;
			child.localPosition = localPosition;
		}
	}

	private void OnDisable()
	{
		MySingleton<Manager>.Instance.billingManager.StarterPackBoughtEvent -= StarterPackWasBought;
		CancelInvoke("CheckForNewCarAvailable");
		StopCoroutine(RefreshCrono());
	}

	protected override void OnEnable()
	{
		MySingleton<Manager>.Instance.billingManager.StarterPackBoughtEvent += StarterPackWasBought;
		HideRemoveAdsButtonIfBought();
		base.OnEnable();
		UpdateCredits();
		UpdateTasksSprites();
		InvokeRepeating("CheckForNewCarAvailable", 0f, 0.5f);
		StarterPackEnable();
		NextCarToUnlock();
		if (wideGameplayHudBaselineCaptured)
		{
			ScheduleWideGameplayHudLayout();
		}
	}

	private void SetActiveCollectables()
	{
		GameObject[] array = collectablesAllZones;
		foreach (GameObject gameObject in array)
		{
			gameObject.SetActive(true);
		}
	}

	private void UpdatePriceForInApps()
	{
		lbPrice.LocalizeLabel();
	}

	public override void UpdateLocalizeLabels()
	{
		UpdateUnlockNextCar();
	}

	private void LateUpdate()
	{
		UpdateHUD();
	}

	private void UpdateHUD()
	{
		lbGear.text = logic.GetGear();
		lbSpeed.text = logic.GetSpeed().ToString();
		spRevs.fillAmount = logic.GetRevs01Range();
		lbOdometer.text = Converter.MetersToCurrentUnits(MySingleton<Manager>.Instance.stats.metersDrove, false, true);
		if ((bool)arrowTransform && MySingleton<Manager>.Instance.gameLogic.challangeInProgress && MySingleton<Manager>.Instance.challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Radar)
		{
			arrowTransform.localRotation = cameraPointerTransform.localRotation;
		}
	}

	private void UpdateGUI()
	{
		UpdateCredits();
		RefreshUnits();
		UpdateUnlockNextCar();
		ReloadSettings();
		UpdateTasksSprites();
		HideRemoveAdsButtonIfBought();
	}

	public void UpdateCredits()
	{
		lbTotalDistance.text = Converter.MetersToCurrentUnits(MySingleton<Manager>.Instance.stats.metersDrove, true, true);
		lbCredits.text = Unibiller.GetCurrencyBalance("credits").ToString();
	}

	private void UpdateTasksSprites()
	{
		// Completed tasks remain available from the pause UI. The reference
		// normal-driving HUD does not display the promotional gift shortcut.
		NGUITools.SetActive(btTaskCompleteHUD.gameObject, false);
		NGUITools.SetActive(btTaskCompletePause.gameObject, MySingleton<Manager>.Instance.taskManager.HasCompletedTasks());
	}

	private void NextCarToUnlock()
	{
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		for (int i = 0; i != carsInfoPrefabs.Length; i++)
		{
			InfoCar infoCar = carsInfoPrefabs[i];
			if (infoCar.id != 1 && !infoCar.isBought && !infoCar.purchasable && infoCar.price > (float)MySingleton<Manager>.Instance.stats.metersDrove)
			{
				nextCarByDistance = infoCar;
				carUnlocked = i;
				return;
			}
		}
		nextCarByDistance = null;
		carUnlocked++;
	}

	public void CheckForNewCarAvailable()
	{
		if (nextCarByDistance == null)
		{
			CancelInvoke("CheckForNewCarAvailable");
			return;
		}
		UnlockThisCar(nextCarByDistance);
		NextCarToUnlock();
	}

	public void UnlockTheseCars(List<InfoCar> infoCar)
	{
		foreach (InfoCar item in infoCar)
		{
			UnlockThisCar(item);
		}
	}

	public void UnlockThisCar(InfoCar infoCar)
	{
		if ((float)MySingleton<Manager>.Instance.stats.metersDrove >= infoCar.price && infoCar.price > 0f)
		{
			if (!infoCar.isCollectable || (infoCar.isCollectable && MySingleton<Manager>.Instance.stats.AllCollectedForThisCar(infoCar)))
			{
				infoCar.isBought = true;
				MySingleton<Manager>.Instance.carUnlockedId = infoCar.id;
				if (!infoCar.isCollectable)
				{
					ManagerAnalytics.instance.FireCarUnlocked(infoCar.idName, "km");
				}
				else if (infoCar.isCollectable && MySingleton<Manager>.Instance.stats.AllCollectedForThisCar(infoCar))
				{
					ManagerAnalytics.instance.FireCarUnlocked(infoCar.idName, "km_and_blueprints");
				}
				spUnlockedCar.GetComponent<UISprite>().atlas = MySingleton<Manager>.Instance.atlasCar;
				spUnlockedCar.GetComponent<UISprite>().spriteName = infoCar.iconName;
				NGUITools.SetActive(pnCarUnlocked.gameObject, true);
				Invoke("DisableNewCarAvailable", 10f);
			}
		}
		else if (infoCar.isCollectable && !MySingleton<Manager>.Instance.stats.AllCollectedForThisCar(infoCar))
		{
		}
	}

	public void ShowCollectablePanel(Collectables.CollectableType collectableType)
	{
		int count = MySingleton<Manager>.Instance.stats.collectablesCounter[collectableType].Count;
		int count2 = MySingleton<Manager>.Instance.stats.maxCollectables[collectableType].Count;
		lbCollectablesLeft.text = string.Format("{0}/{1}", count, count2);
		spCollectableLeft.atlas = MySingleton<Manager>.Instance.atlasCar;
		spCollectableLeft.spriteName = Collectables.GetIconNameFor(collectableType);
		NGUITools.SetActiveSelf(pnCollectablesLeft.gameObject, true);
		Invoke("HideCollectablesPanel", 3f);
	}

	public void HideCollectablesPanel()
	{
		NGUITools.SetActiveSelf(pnCollectablesLeft.gameObject, false);
	}

	public void ShowCantPaintPanel()
	{
		NGUITools.SetActive(pnCantPaint.gameObject, true);
		Invoke("HideCantPaintPanel", 3f);
	}

	public void HideCantPaintPanel()
	{
		NGUITools.SetActive(pnCantPaint.gameObject, false);
	}

	public void ShowMiniLoading()
	{
		NGUITools.SetActive(pnLoadingBg.gameObject, true);
		spMiniLoading.alpha = 0f;
		TweenAlpha component = spMiniLoading.GetComponent<TweenAlpha>();
		component.ResetToBeginning();
		component.PlayForward();
		NGUITools.SetActive(spMiniLoading.gameObject, true);
	}

	public void HideMiniLoading()
	{
		spMiniLoading.alpha = 0f;
		NGUITools.SetActive(spMiniLoading.gameObject, false);
		NGUITools.SetActive(pnLoadingBg.gameObject, false);
	}

	public void DisableNewCarAvailable()
	{
		MySingleton<Manager>.Instance.carUnlockedId = -1;
		NGUITools.SetActive(pnCarUnlocked.gameObject, false);
	}

	public void ChangeAbs()
	{
		bool flag = logic.ChangeAbs();
		spAbsStatus.spriteName = ((!flag) ? "off" : "on");
	}

	public void ChangeEsp()
	{
		bool flag = logic.ChangeEsp();
		spEspStatus.spriteName = ((!flag) ? "off" : "on");
	}

	public void ChangeTc()
	{
		bool flag = logic.ChangeTc();
		spTcStatus.spriteName = ((!flag) ? "off" : "on");
	}

	public void NextCamera()
	{
		logic.NextCamera();
	}

	public void Gas(bool isPressed)
	{
		logic.Gas(isPressed);
	}

	public void Brake(bool isPressed)
	{
		logic.Brake(isPressed);
	}

	public void HandBrake(bool isPressed)
	{
		logic.Handbrake(isPressed);
	}

	public void DirLeft(bool isPressed)
	{
		logic.DirLeft(isPressed);
	}

	public void DirRight(bool isPressed)
	{
		logic.DirRight(isPressed);
	}

	public void Repair()
	{
		logic.Repair();
	}

	public void PauseMode()
	{
		PauseWithNoAds();
		if (pauseAdsCounter % MySingleton<Manager>.Instance.remoteConfiguration.timesPausedBeforePauseAd == 0)
		{
			PutAd();
		}
		pauseAdsCounter++;
	}

	public void PauseWithNoAds()
	{
		MySingleton<Manager>.Instance.stats.SaveDataIntoPlayerPrefs();
		MySingleton<Manager>.Instance.carUnlockedId = -1;
		MySingleton<Manager>.Instance.paused = true;
		if (MySingleton<Manager>.Instance.starterPackPanelIsActive)
		{
			backButton.function = GUIGameButton.GameButtonFunction.CloseStartingPackPopup;
		}
		else
		{
			backButton.function = GUIGameButton.GameButtonFunction.Resume;
		}
		ShowPause();
		UpdateOfferPanel();
		AudioListener.volume = 0f;
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
		{
			Time.timeScale = 0f;
		}
	}

	private void ShowPause()
	{
		UpdateGUI();
		NGUITools.SetActive(pnPause.gameObject, true);
		NGUITools.SetActive(pnHud.gameObject, false);
		if (MySingleton<Manager>.Instance.gameLogic.challangeInProgress)
		{
			guiChallangeMode.ShowPause();
		}
	}

	public void Resume()
	{
		MySingleton<Manager>.Instance.starterPopUpAlreadySeen = false;
		MySingleton<Manager>.Instance.paused = false;
		backButton.function = GUIGameButton.GameButtonFunction.Pause;
		ShowHUD();
		AudioListener.volume = 1f;
		SettingsManager.SetMusicVolume(btSliderVolume.value);
		logic.musicAudioSource.volume = SettingsManager.GetMusicVolume();
		if (MySingleton<Manager>.Instance.gameLogic.challangeInProgress)
		{
			guiChallangeMode.ResumePause();
			if (MySingleton<Manager>.Instance.challangeModeManager.currentChallange.type == ChallangeType.Crashable)
			{
				MySingleton<Manager>.Instance.guiChallangeModeLogic.crashableChallengeManager.ResumeChronometer();
			}
		}
		logic.ResetCarInput();
		Time.timeScale = 1f;
	}

	public void Restart()
	{
		ShowLoading();
		Time.timeScale = 1f;
		LegacySceneManager.LoadLevel(2);
	}

	public void ShowHUD()
	{
		NGUITools.SetActive(pnHud.gameObject, true);
		NGUITools.SetActive(pnPause.gameObject, false);
		NGUITools.SetActive(pnChangeZone.gameObject, false);
		ScheduleWideGameplayHudLayout();
	}

	public void HideHUD()
	{
		NGUITools.SetActive(pnHud.gameObject, false);
	}

	public void ShowChangeZonePanel()
	{
		NGUITools.SetActive(pnChangeZone.gameObject, true);
		NGUITools.SetActive(pnHud.gameObject, false);
	}

	public void ShowLoading()
	{
		Object.Instantiate(MySingleton<Manager>.Instance.UIRootLoading);
	}

	public void BackToMenu()
	{
		if (MySingleton<Manager>.Instance.gameLogic.challangeInProgress)
		{
			string[] array = MySingleton<Manager>.Instance.challangeModeManager.currentChallange.GetCurrentCircuit().sceneIndex.ToString().Split('_');
			ManagerAnalytics.instance.FireChallengeEnds(array[3], Chronometer.GetElapsed().ToString(), MySingleton<Manager>.Instance.challangeModeManager.currentChallange.key.ToString(), MySingleton<Manager>.Instance.gameLogic.instantiatedCar.GetComponent<InfoCar>().idName, MySingleton<Manager>.Instance.levelZone.gameObject.name, "car_unlocked_leave");
		}
		else
		{
			ManagerAnalytics.instance.FireBackToMenu(MySingleton<Manager>.Instance.gameMode.ToString(), MySingleton<Manager>.Instance.gameLogic.instantiatedCar.GetComponent<InfoCar>().idName, MySingleton<Manager>.Instance.levelZone.gameObject.name);
		}
		Time.timeScale = 1f;
		AudioListener.volume = 1f;
		MySingleton<Manager>.Instance.paused = false;
		ShowLoading();
		MySingleton<Manager>.Instance.starterPopUpAlreadySeen = false;
		LegacySceneManager.LoadLevel(1);
	}

	public void BackToMenuWithAd()
	{
		MySingleton<Manager>.Instance.ads.ShowAdmobOrHeyzap(false, MySingleton<Manager>.Instance.remoteConfiguration.admobAdplacementGarage, MySingleton<Manager>.Instance.remoteConfiguration.globalAdplacementGarage);
		BackToMenu();
	}

	public void OpenCompleteTask()
	{
		PauseWithNoAds();
		OpenStore();
	}

	public void Reset()
	{
		logic.Reset();
	}

	public void ReloadSettings()
	{
		controlsWheelButton.defaultColor = COLOR_DARK_GRAY;
		controlsAccelButton.defaultColor = COLOR_DARK_GRAY;
		controlsArrowsButton.defaultColor = COLOR_DARK_GRAY;
		switch (MySingleton<Manager>.Instance.controls)
		{
		case Manager.Controls.Accelerometer:
			controlsAccelButton.defaultColor = COLOR_SELECTED;
			NGUITools.SetActive(arrows, false);
			NGUITools.SetActive(steeringWheel, false);
			logic.steer = steeringWheel.GetComponent<CarSteeringWheel>();
			break;
		case Manager.Controls.Buttons:
			controlsArrowsButton.defaultColor = COLOR_SELECTED;
			NGUITools.SetActive(arrows, true);
			NGUITools.SetActive(steeringWheel, false);
			break;
		default:
			controlsWheelButton.defaultColor = COLOR_SELECTED;
			NGUITools.SetActive(arrows, false);
			NGUITools.SetActive(steeringWheel, true);
			break;
		}
		btSliderVolume.value = SettingsManager.GetMusicVolume();
		CarSettings component = logic.currentCar.GetComponent<CarSettings>();
		spAbsStatus.spriteName = ((!component.abs) ? "off" : "on");
		spTcStatus.spriteName = ((!component.tc) ? "off" : "on");
		spEspStatus.spriteName = ((!component.esp) ? "off" : "on");
	}

	public void HideRemoveAdsButtonIfBought()
	{
		if (Unibiller.GetPurchaseCount(MySingleton<Manager>.Instance.billingManager.removeAdsPurchaseId) > 0 || Unibiller.GetPurchaseCount(MySingleton<Manager>.Instance.billingManager.removeAdsOfferPurchaseId) > 0 || Unibiller.GetPurchaseCount(MySingleton<Manager>.Instance.billingManager.starterPackOfferId) > 0)
		{
			NGUITools.SetActive(btRemoveAds, false);
			NGUITools.SetActive(btRemoveAdsOffer, false);
		}
	}

	public void UpdateOfferPanel()
	{
		if (SettingsManager.IsRemoveAdsOffer())
		{
			NGUITools.SetActive(btRemoveAdsOffer, true);
			backButton.function = GUIGameButton.GameButtonFunction.CloseOfferPanel;
			SettingsManager.SetRemoveAdsOffer(false);
			return;
		}
		NGUITools.SetActive(btRemoveAdsOffer, false);
		if (MySingleton<Manager>.Instance.starterPackPanelIsActive)
		{
			backButton.function = GUIGameButton.GameButtonFunction.CloseStartingPackPopup;
		}
		else
		{
			backButton.function = GUIGameButton.GameButtonFunction.Resume;
		}
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

	public void HidePanelInfo()
	{
		NGUITools.SetActive(pnPause.gameObject, true);
		backButton.function = GUIGameButton.GameButtonFunction.Resume;
	}

	public void RemoveAds()
	{
		ManagerAnalytics.instance.FireIAPClicked("remove_ads", "pause");
		MySingleton<Manager>.Instance.currentLoadingPurchasePanel = Object.Instantiate(MySingleton<Manager>.Instance.pnLoadingPurchase) as GameObject;
		PurchasableItem purchasableItemById = Unibiller.GetPurchasableItemById(MySingleton<Manager>.Instance.billingManager.removeAdsPurchaseId);
		MySingleton<Manager>.Instance.unibill.InitiatePurchase(purchasableItemById);
	}

	public void RemoveAdsOffer()
	{
		MySingleton<Manager>.Instance.currentLoadingPurchasePanel = Object.Instantiate(MySingleton<Manager>.Instance.pnLoadingPurchase) as GameObject;
		PurchasableItem purchasableItemById = Unibiller.GetPurchasableItemById(MySingleton<Manager>.Instance.billingManager.removeAdsOfferPurchaseId);
		MySingleton<Manager>.Instance.unibill.InitiatePurchase(purchasableItemById);
		backButton.function = GUIGameButton.GameButtonFunction.Resume;
	}

	public override void OnTaskFinished(Task finishedTask)
	{
		UpdateTasksSprites();
	}

	public void GoToGarage()
	{
		MySingleton<Manager>.Instance.carUnlockedId = nextCarByDistance.id;
		BackToMenu();
	}

	private void UpdateUnlockNextCar()
	{
		if ((bool)nextCarByDistance)
		{
			spNextCar.atlas = MySingleton<Manager>.Instance.atlasCar;
			spNextCar.spriteName = nextCarByDistance.iconName;
			lbToUnlock.text = Localization.Get("to_unlock");
			foreach (Transform item in UnlocksGrid.transform)
			{
				NGUITools.SetActive(item.gameObject, false);
			}
			NGUITools.SetActive(spIsland.gameObject, false);
			NGUITools.SetActive(spNextCar.gameObject, true);
			btNextToUnlock.GetComponent<GUIGameButton>().function = GUIGameButton.GameButtonFunction.NewCar;
			bool flag = true;
			if (nextCarByDistance.price > 0f)
			{
				float num = Mathf.Max(nextCarByDistance.price - (float)MySingleton<Manager>.Instance.stats.metersDrove, 0f);
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
					flag = false;
				}
				else
				{
					NGUITools.SetActive(spDistanceOk, false);
					NGUITools.SetActive(spDistanceNo, true);
				}
				NGUITools.SetActive(spDistance, true);
			}
			if (nextCarByDistance.collectableType != Collectables.CollectableType.None)
			{
				lbCollectables.text = string.Format(Localization.Get("collectablesLeft"), MySingleton<Manager>.Instance.stats.GetCollectablesRemainingForThisCar(nextCarByDistance));
				spMiniCar.spriteName = Collectables.GetIconNameFor(nextCarByDistance.collectableType);
				if (MySingleton<Manager>.Instance.stats.AllCollectedForThisCar(nextCarByDistance))
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
					flag = false;
				}
				else
				{
					NGUITools.SetActive(spCollectablesOk, false);
					NGUITools.SetActive(spCollectablesNo, true);
				}
				NGUITools.SetActive(spCollectables, true);
			}
			UnlocksGrid.Reposition();
			return;
		}
		foreach (Transform item2 in UnlocksGrid.transform)
		{
			NGUITools.SetActive(item2.gameObject, false);
		}
		NGUITools.SetActive(spNextCar.gameObject, false);
		NGUITools.SetActive(spIsland.gameObject, true);
		lbToUnlock.text = Localization.Get("other_game");
		btNextToUnlock.GetComponent<GUIGameButton>().function = GUIGameButton.GameButtonFunction.GoToIsland;
	}

	public void GoToIsland()
	{
		if (MySingleton<Manager>.Instance.platform == Manager.Platform.GooglePlay)
		{
			Application.OpenURL("market://details?id=com.aim.racingisland");
		}
		else if (MySingleton<Manager>.Instance.platform == Manager.Platform.Amazon)
		{
			Application.OpenURL("amzn://apps/android?p=com.aim.racingisland");
		}
	}

	public void GoToExtremeCarDriving()
	{
		Debug.Log("GoToExtremeCarDriving");
		if (MySingleton<Manager>.Instance.platform == Manager.Platform.GooglePlay)
		{
			Application.OpenURL("market://details?id=com.aim.racing&referrer=utm_source%3Dcom.aim.rallyhum%26utm_campaign%3DInhouse");
		}
		else if (MySingleton<Manager>.Instance.platform == Manager.Platform.Amazon)
		{
			Application.OpenURL("amzn://apps/android?p=com.aim.racing");
		}
	}

	public void GoToExtremeSUVDriving()
	{
		Debug.Log("GoToExtremeSUVDriving");
		if (MySingleton<Manager>.Instance.platform == Manager.Platform.GooglePlay)
		{
			Application.OpenURL("market://details?id=com.aim.rallyhum&referrer=utm_source%3Dcom.aim.racing%26utm_campaign%3DInhouse");
		}
		else if (MySingleton<Manager>.Instance.platform == Manager.Platform.Amazon)
		{
			Application.OpenURL("amzn://apps/android?p=com.aim.rallyhum");
		}
	}

	private void PutAd()
	{
		MySingleton<Manager>.Instance.adsFromPause = true;
		MySingleton<Manager>.Instance.ads.RestartCounter();
		MySingleton<Manager>.Instance.ads.CancelPnMsg();
		MySingleton<Manager>.Instance.ads.ShowAdmobOrHeyzap(false, MySingleton<Manager>.Instance.remoteConfiguration.admobAdplacementPause, MySingleton<Manager>.Instance.remoteConfiguration.globalAdplacementPause);
	}

	public void SetUnits(SettingsManager.Units newUnits)
	{
		logic.SetUnits(newUnits);
		RefreshUnits();
		RefreshGUI();
		if (MySingleton<Manager>.Instance.isChallangeAvailable)
		{
			MySingleton<Manager>.Instance.guiChallangeModeLogic.UpdateChallangeUnitLabels();
		}
	}

	private void RefreshGUI()
	{
		lbTotalDistance.text = Converter.MetersToCurrentUnits(MySingleton<Manager>.Instance.stats.metersDrove, true, true);
		if ((bool)nextCarByDistance)
		{
			float value = Mathf.Max(nextCarByDistance.price - (float)MySingleton<Manager>.Instance.stats.metersDrove, 0f);
			string arg = Converter.MetersToCurrentUnits(value, true, true);
			lbDistance.text = string.Format(Localization.Get("distanceLeft"), arg);
		}
	}

	private void RefreshUnits()
	{
		switch (MySingleton<Manager>.Instance.units)
		{
		case SettingsManager.Units.KMH:
			spSpeedUnit.spriteName = "kmh";
			lbUnitsOdo.text = "KM";
			lbUnits.text = "KM/H";
			btUnits.GetComponent<GUIGameButton>().function = GUIGameButton.GameButtonFunction.UnitsMPH;
			break;
		case SettingsManager.Units.MPH:
			spSpeedUnit.spriteName = "mph";
			lbUnitsOdo.text = "MI";
			lbUnits.text = "MPH";
			btUnits.GetComponent<GUIGameButton>().function = GUIGameButton.GameButtonFunction.UnitsKMH;
			break;
		}
	}

	public void VolumeChanged(float value)
	{
		logic.VolumeChanged(value);
	}

	public void PlayHorn(bool isPressed)
	{
		MySingleton<Manager>.Instance.gameLogic.PlayHorn(isPressed);
	}

	public void SwitchCarLights()
	{
		MySingleton<Manager>.Instance.gameLogic.SwitchCarLights();
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
		string title = Localization.Get("billing_purchase_success");
		ShowPanelInfo(title, " ");
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
	}

	public void PlayCircuit()
	{
		guiChallangeMode.EnterInCircuitInActivationZone();
	}

	public void Play()
	{
		guiChallangeMode.Play();
	}

	public void ShowLeaderboard()
	{
		guiChallangeMode.ShowLeaderboard();
	}

	public void BackToGame()
	{
		guiChallangeMode.BackToGame();
	}

	public void Retry()
	{
		guiChallangeMode.Restart();
	}

	public void RespawnLastCheckpoint()
	{
		guiChallangeMode.RespawnLastCheckpoint();
	}

	public void BreakRoute()
	{
		guiChallangeMode.BreakRoute();
	}

	public void GoToGarajeFromChallange()
	{
		guiChallangeMode.GoToGarajeFromChallange();
	}

	public void InviteFriends()
	{
		if (MySingleton<Manager>.Instance.facebookManager.isLoggedIn)
		{
			MySingleton<Manager>.Instance.facebookManager.InviteFriends();
		}
		else
		{
			MySingleton<Manager>.Instance.facebookManager.Login(OnLoginSuccess, OnLoginCancel, OnLoginFailed);
		}
	}

	private void OnLoginFailed()
	{
	}

	private void OnLoginCancel()
	{
	}

	private void OnLoginSuccess()
	{
		MySingleton<Manager>.Instance.facebookManager.InviteFriends();
	}

	public void ActiveAchievementPanel(bool active)
	{
		if (active)
		{
			pnAchievements.GetComponent<AchievementsPanel>().Init(AchievementManager.AchievementCategory.GENERAL);
		}
		NGUITools.SetActive(pnAchievements, active);
	}

	public void ActiveProgressPanel(bool active)
	{
		if (active)
		{
			if (pnProgressPanelReference != null)
			{
				Object.DestroyImmediate(pnProgressPanelReference);
				pnProgressPanelReference = null;
			}
			pnProgressPanelReference = Object.Instantiate(MySingleton<Manager>.Instance.pnProgresPanel) as GameObject;
			pnProgressPanelReference.transform.localScale = Vector3.one;
			pnProgressPanelReference.GetComponentInChildren<UIAchievementsBehaviour>().SetCamera(MainCameraGame2);
			backButton.function = GUIGameButton.GameButtonFunction.CloseProgressPanel;
		}
		else if (pnProgressPanelReference != null)
		{
			Object.DestroyImmediate(pnProgressPanelReference);
			pnProgressPanelReference = null;
		}
	}

	public void HideStarterPack()
	{
		NGUITools.SetActive(starterPackElement, false);
		NGUITools.SetActive(spStoreStarterPack.gameObject, false);
		NGUITools.SetActive(spStore.gameObject, true);
		NGUITools.SetActive(pnStarterPack.gameObject, false);
		MySingleton<Manager>.Instance.starterPackPanelIsActive = false;
		packsGrid.Reposition();
		StopCoroutine(RefreshCrono());
	}

	public void StarterPackWasBought()
	{
		HideStarterPack();
		MySingleton<Manager>.Instance.starterPackBought = true;
	}

	public void CloseStarterPack()
	{
		NGUITools.SetActive(pnStarterPack.gameObject, false);
		backButton.function = GUIGameButton.GameButtonFunction.Resume;
		MySingleton<Manager>.Instance.starterPackPanelIsActive = false;
	}

	private void StarterPackEnable()
	{
		if (!SettingsManager.GetStarterPackBoughtOrEnded())
		{
			NGUITools.SetActive(starterPackElement, true);
			NGUITools.SetActive(spStoreStarterPack.gameObject, true);
			NGUITools.SetActive(spStore.gameObject, false);
			packsGrid.Reposition();
			StartCoroutine(RefreshCrono());
		}
		else
		{
			HideStarterPack();
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
				lbCronoStarterPackPacks.color = MySingleton<Manager>.Instance.cronoColors[0];
			}
			else if (starterPackTime > SettingsManager.STARTER_PACK_3ND_STEP && starterPackTime <= SettingsManager.STARTER_PACK_2ND_STEP)
			{
				lbCounterofferPopup.color = MySingleton<Manager>.Instance.cronoColors[1];
				lbCronoStarterPackPacks.color = MySingleton<Manager>.Instance.cronoColors[1];
			}
			else
			{
				lbCounterofferPopup.color = MySingleton<Manager>.Instance.cronoColors[2];
				lbCronoStarterPackPacks.color = MySingleton<Manager>.Instance.cronoColors[2];
			}
			lbCounterofferPopup.text = Converter.SecondsToHhMmSs(starterPackTime);
			lbCronoStarterPackPacks.text = Converter.SecondsToHhMmSs(starterPackTime);
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
