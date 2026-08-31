using System;
using System.Collections;
using UnityEngine;

public class GUIChallangeMode : MonoBehaviour
{
	public ChallangeModeManager challangeModeManager;

	public UIButton btResetCar;

	public UIPanel pnChallangeMode;

	public UIPanel pnActivationZone;

	public UILabel lbActivationZoneBestTime;

	public UILabel lbActivationZoneName;

	public UIPanel pnPreview;

	public UILabel lbBestTime;

	public UILabel lbPreviewName;

	public UILabel lbResume;

	public GUIGameButton btPlay;

	public UIPanel pnPlaying;

	public UILabel lbElapsedTime;

	public UIPanel pnFinish;

	public UISprite spChallangeProgress;

	public UISprite spChallangeProgressBackground;

	public UISprite spNewCarUnlocked;

	public UISprite spReward;

	public UILabel lbReward;

	public UILabel lbFinishName;

	public UILabel lbFinishBestTime;

	public UILabel lbFinishElapsed;

	public UIPanel pnSpawner;

	public Camera cSpawnerCamera;

	public GUIGameLogic guiGameLogic;

	public float fillingAnimationPlusTime = 0.5f;

	public GameObject arrow;

	public UILabel lbActivationZoneBestTotalSpeed;

	public UILabel lbBestSpeed;

	public UISprite spRadarSpeeds;

	public UILabel lbTotalSpeed;

	public UILabel lbLastSpeed;

	public UILabel lbRadar;

	public UISprite spFlash;

	public AudioSource audioFlash;

	public UISprite spFinalRadarSpeeds;

	public UILabel lbFinalTotalSpeed;

	public UILabel lbFinishBestSpeed;

	public float[] radarScoresMPS;

	public float radarTotalSpeedMPS;

	public RadarManager radarManager;

	public UIGrid radarsGrid;

	public GameObject radarSpeedThumbnail;

	public CrashableChallangeManager crashableChallengeManager;

	public int totalCrashables;

	public UILabel lbActivationZoneBestTotalCrashables;

	public UILabel lbBestCrashables;

	public UISprite SpCrashablesCounter;

	public UILabel lbCrashablesCounter;

	public UILabel lbFinishBestCrashables;

	public UILabel lbFinalCrashables;

	public UISprite spCrashableResult;

	public UIWidget wgCrashableResult;

	public UIWidget wgCrashableResultBar;

	public UIWidget wdChallangeSuccess;

	public UIWidget wdChallangeFailed;

	private bool _insideActivationZone;

	private float _fillingAnimationDuration;

	private bool _fillingAnimate;

	private float _fillingAnimateStart;

	private float _fillingAnimateEnd;

	private float _fillingAnimateInvDuration;

	private float _fillingFrom;

	private float _fillingTo;

	private GUIGameButton.GameButtonFunction _lastDefaultBackButtonFunction;

	private UIPanel _activePanelWhenPaused;

	public GameObject signs;

	private void Awake()
	{
		MySingleton<Manager>.Instance.guiChallangeModeLogic = this;
	}

	private void Start()
	{
		InitializeChallangeMode();
		arrow = MySingleton<Manager>.Instance.guiGameLogic.arrowTransform.gameObject;
	}

	public void EnterInCircuitInActivationZone()
	{
		ChallangeModeInactivateAll();
		guiGameLogic.HideHUD();
		NGUITools.SetActive(guiGameLogic.pnLoadingBg.gameObject, true);
		NGUITools.SetActive(guiGameLogic.btLights.gameObject, false);
		SetEnableCollectablesParent(false);
		HideSignsInTrials();
		challangeModeManager.DisableCarControl();
		challangeModeManager.PlayCircuitInActivationZone();
		challangeModeManager.currentChallange.GetCurrentCircuit().OnEnteredEvent += OnCircuitEntered;
		challangeModeManager.currentChallange.GetCurrentCircuit().OnLeaveEvent += OnCircuitLeave;
		MySingleton<Manager>.Instance.ads.ResetOccurredEvents();
		MySingleton<Manager>.Instance.ads.ShowAdsFromChallange();
		MySingleton<Manager>.Instance.ads.DisableAdsFromChallange();
	}

	private void HideSignsInTrials()
	{
		if ((bool)signs)
		{
			NGUITools.SetActive(signs, false);
		}
	}

	private void ShowSignsInTrials()
	{
		if ((bool)signs)
		{
			NGUITools.SetActive(signs, true);
		}
	}

	public void Play()
	{
		string[] array = challangeModeManager.currentChallange.GetCurrentCircuit().sceneIndex.ToString().Split('_');
		ManagerAnalytics.instance.FireChallengeStart(array[3], challangeModeManager.currentChallange.key.ToString(), MySingleton<Manager>.Instance.gameLogic.instantiatedCar.GetComponent<InfoCar>().idName, MySingleton<Manager>.Instance.levelZone.gameObject.name);
		MySingleton<Manager>.Instance.gameLogic.challangeInProgress = true;
		MySingleton<Manager>.Instance.startChallangeLevelZone = MySingleton<Manager>.Instance.levelZone;
		guiGameLogic.ShowHUD();
		ChallangeModeInactivateAll();
		NGUITools.SetActive(pnPlaying.gameObject, true);
		guiGameLogic.backButton.function = GUIGameButton.GameButtonFunction.Pause;
		switch (challangeModeManager.GetChallangeInActivationZone().key)
		{
		case ChallangeKey.Radar:
			MySingleton<Manager>.Instance.gameLogic.mainArrowPointer.gameObject.SetActive(true);
			ResetRadarValues();
			NGUITools.SetActive(arrow.gameObject, true);
			SetRadarsGrid();
			break;
		case ChallangeKey.Crashable:
			crashableChallengeManager.Init();
			break;
		}
		challangeModeManager.EnableCarControl();
	}

	public void RespawnLastCheckpoint()
	{
		MySingleton<Manager>.Instance.ads.ShowAdmobOrHeyzapAboutOccurredEvents();
		challangeModeManager.RespawnLastCheckpoint();
	}

	public void ShowLeaderboard()
	{
		challangeModeManager.ShowLeaderboard();
	}

	public void BackToGame()
	{
		NGUITools.SetActive(guiGameLogic.btLights.gameObject, true);
		ShowSignsInTrials();
		challangeModeManager.currentChallange.LeaveCircuit();
		SetPauseMode();
	}

	public void GoToGarajeFromChallange()
	{
		MySingleton<Manager>.Instance.guiGameLogic.BackToMenu();
	}

	public void Restart()
	{
		guiGameLogic.Repair();
		RestartChallengesValues();
		NGUITools.SetActive(arrow.gameObject, false);
		MySingleton<Manager>.Instance.gameLogic.mainArrowPointer.gameObject.SetActive(false);
		Time.timeScale = 1f;
		guiGameLogic.HideHUD();
		SetLbBestTime();
		RealoadScenesIfNeccesary();
		OnCircuitEntered(challangeModeManager.currentChallange.GetCurrentCircuit());
	}

	public void RealoadScenesIfNeccesary()
	{
		if ((bool)MySingleton<Manager>.Instance.startChallangeLevelZone && MySingleton<Manager>.Instance.levelZone != MySingleton<Manager>.Instance.startChallangeLevelZone)
		{
			NGUITools.SetActive(MySingleton<Manager>.Instance.guiGameLogic.pnLevelLoading.gameObject, true);
			LevelZone.UnloadAllZones(MySingleton<Manager>.Instance.gameLogic.allZones);
			MySingleton<Manager>.Instance.startChallangeLevelZone.ForceLoadLevel();
		}
	}

	public void ResetRadarValues()
	{
		radarManager.nextRadar = radarManager.radars[0].target;
		radarManager.radars[0].aura.gameObject.SetActive(true);
		radarManager.radars[0].GetComponent<Collider>().enabled = true;
		radarManager.lastCheckpoint.gameObject.SetActive(false);
		radarTotalSpeedMPS = 0f;
		radarScoresMPS = new float[radarManager.radars.Length];
		radarManager.lastSpeedCheckedMPS = 0f;
		lbTotalSpeed.text = Converter.MSToCurrentUnits(0f, true);
		lbLastSpeed.text = Converter.MSToCurrentUnits(0f, true);
		lbRadar.text = "RADAR:";
	}

	public void ResetCrashablesValues()
	{
		totalCrashables = 0;
	}

	private void SetRadarsGrid()
	{
		int num = radarsGrid.transform.childCount - 1;
		for (int num2 = num; num2 >= 0; num2--)
		{
			UnityEngine.Object.Destroy(radarsGrid.transform.GetChild(num2).gameObject);
		}
		ChallangeRadarLogic[] radars = radarManager.radars;
		foreach (ChallangeRadarLogic challangeRadarLogic in radars)
		{
			NGUITools.AddChild(radarsGrid.gameObject, radarSpeedThumbnail);
		}
		radarsGrid.GetComponent<UIGrid>().Reposition();
	}

	public void UpdateChallangeUnitLabels()
	{
		if ((bool)challangeModeManager.GetChallangeInActivationZone() && challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Radar && MySingleton<Manager>.Instance.gameLogic.challangeInProgress)
		{
			lbTotalSpeed.text = Converter.MSToCurrentUnits(radarTotalSpeedMPS, true);
			lbLastSpeed.text = Converter.MSToCurrentUnits(radarManager.lastSpeedCheckedMPS, true);
		}
		if (lbActivationZoneBestTotalSpeed != null && challangeModeManager.GetCircuitInActivationZone() != null)
		{
			lbActivationZoneBestTotalSpeed.text = Converter.MSToCurrentUnits(challangeModeManager.GetCircuitInActivationZone().bestSpeed, true);
		}
	}

	public void RestartChallengesValues()
	{
		if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Radar)
		{
			ResetRadarValues();
		}
		if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Crashable)
		{
			crashableChallengeManager.ResetValues();
		}
	}

	public void BreakRoute(bool delay = false)
	{
		btPlay.function = GUIGameButton.GameButtonFunction.Play;
		challangeModeManager.circuitSplineMaster.Stop();
		MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarCameras>().showFixedCams = true;
		if (!delay)
		{
			DoBreakRoute();
		}
	}

	private IEnumerator DelayBreakRoute()
	{
		yield return new WaitForSeconds(1f);
		DoBreakRoute();
	}

	public void DoBreakRoute()
	{
		challangeModeManager.checkpointMaster.HideAllButCurrent();
		challangeModeManager.checkpointMaster.ShowLast();
		challangeModeManager.circuitSplineMaster.SetEnableAll(false);
		guiGameLogic.backButton.function = GUIGameButton.GameButtonFunction.None;
	}

	private void OnCircuitEntered(ChallangeCircuit circuit)
	{
		ChallangeModeInactivateAll();
		ForwardTransition(pnPreview.GetComponent<TweenAlpha>(), null);
		NGUITools.SetActive(guiGameLogic.pnLoadingBg.gameObject, false);
		if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Trial || challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Crashable)
		{
			NGUITools.SetActive(guiGameLogic.btResetCar.gameObject, false);
			NGUITools.SetActive(btResetCar.gameObject, true);
		}
		btPlay.function = GUIGameButton.GameButtonFunction.BreakRoute;
		_insideActivationZone = false;
		lbResume.text = challangeModeManager.currentChallange.GetResume();
		SetLbBestTime();
		if ((bool)challangeModeManager.currentChallange.infoCar)
		{
			spChallangeProgress.atlas = MySingleton<Manager>.Instance.atlasCar;
			spChallangeProgressBackground.atlas = MySingleton<Manager>.Instance.atlasCar;
			spChallangeProgress.spriteName = challangeModeManager.currentChallange.infoCar.iconName;
			spChallangeProgressBackground.spriteName = challangeModeManager.currentChallange.infoCar.iconName;
		}
		_lastDefaultBackButtonFunction = guiGameLogic.backButton.function;
		guiGameLogic.backButton.function = GUIGameButton.GameButtonFunction.BreakRoute;
		challangeModeManager.OnCircuitEntered(circuit);
		challangeModeManager.checkpointMaster.Restart();
		challangeModeManager.checkpointMaster.ShowAll();
		if (crashableChallengeManager != null)
		{
			crashableChallengeManager.chronoEvents.OnTimeOver -= OnTimeOver;
			crashableChallengeManager.chronoEvents.OnTimeOver += OnTimeOver;
		}
		CheckpointMaster checkpointMaster = challangeModeManager.checkpointMaster;
		checkpointMaster.OnPassThroughAllCheckpointEvent = (CheckpointMasterCallbackEvent)Delegate.Remove(checkpointMaster.OnPassThroughAllCheckpointEvent, new CheckpointMasterCallbackEvent(OnPassThroughAllCheckpoint));
		CheckpointMaster checkpointMaster2 = challangeModeManager.checkpointMaster;
		checkpointMaster2.OnPassThroughAllCheckpointEvent = (CheckpointMasterCallbackEvent)Delegate.Combine(checkpointMaster2.OnPassThroughAllCheckpointEvent, new CheckpointMasterCallbackEvent(OnPassThroughAllCheckpoint));
		SpawnerMaster spawnerMaster = challangeModeManager.spawnerMaster;
		spawnerMaster.OnSpawnerMasterEnter = (SpawnerMasterCallbackEvent)Delegate.Remove(spawnerMaster.OnSpawnerMasterEnter, new SpawnerMasterCallbackEvent(OnSpawnerMasterEnter));
		SpawnerMaster spawnerMaster2 = challangeModeManager.spawnerMaster;
		spawnerMaster2.OnSpawnerMasterEnter = (SpawnerMasterCallbackEvent)Delegate.Combine(spawnerMaster2.OnSpawnerMasterEnter, new SpawnerMasterCallbackEvent(OnSpawnerMasterEnter));
		challangeModeManager.circuitSplineMaster.SetEnableAll(true);
		challangeModeManager.circuitSplineMaster.finishCallbackEvent += OnRouteFinish;
		MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarCameras>().showFixedCams = false;
		challangeModeManager.circuitSplineMaster.Play(MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarCameras>().Current());
	}

	public void OnPassThroughAllCheckpoint(CheckpointScript checkpoint)
	{
		MonoBehaviour.print("============ CIRCUITO FINALIZADO! =============");
		ChallangeModeInactivateAll();
		guiGameLogic.HideHUD();
		NGUITools.SetActive(arrow.gameObject, false);
		MySingleton<Manager>.Instance.gameLogic.mainArrowPointer.gameObject.SetActive(false);
		if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Trial)
		{
			ForwardTransition(pnFinish.GetComponent<TweenAlpha>(), OnPassThroughAllCheckpointTransitionEnd);
		}
		else
		{
			NGUITools.SetActive(pnFinish.gameObject, true);
		}
		if (crashableChallengeManager != null)
		{
			crashableChallengeManager.chronoEvents.OnTimeOver -= OnTimeOver;
			crashableChallengeManager.chronoEvents.OnTimeOver += OnTimeOver;
		}
		CheckpointMaster checkpointMaster = challangeModeManager.checkpointMaster;
		checkpointMaster.OnPassThroughAllCheckpointEvent = (CheckpointMasterCallbackEvent)Delegate.Remove(checkpointMaster.OnPassThroughAllCheckpointEvent, new CheckpointMasterCallbackEvent(OnPassThroughAllCheckpoint));
		SpawnerMaster spawnerMaster = challangeModeManager.spawnerMaster;
		spawnerMaster.OnSpawnerMasterEnter = (SpawnerMasterCallbackEvent)Delegate.Remove(spawnerMaster.OnSpawnerMasterEnter, new SpawnerMasterCallbackEvent(OnSpawnerMasterEnter));
		challangeModeManager.OnPassThroughAllCheckpoint(checkpoint);
		challangeModeManager.DisableCarControl();
		if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Crashable)
		{
			if (totalCrashables >= CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE)
			{
				SetRewardGUI();
			}
			else
			{
				NGUITools.SetActive(spReward.gameObject, false);
				NGUITools.SetActive(spChallangeProgress.gameObject, false);
			}
		}
		else
		{
			SetRewardGUI();
		}
		SubmitReward();
		SetLbBestTime();
		UpdatePnFinish(challangeModeManager.GetChallangeInActivationZone().key);
		MySingleton<Manager>.Instance.CheckChallengesCompleted();
		ShowAdsAfterChallange();
	}

	private void ShowAdsAfterChallange()
	{
		MySingleton<Manager>.Instance.ads.ShowAdsFromChallange();
	}

	private void OnCircuitLeave(ChallangeCircuit circuit)
	{
		MySingleton<Manager>.Instance.gameLogic.challangeInProgress = false;
		Time.timeScale = 1f;
		ChallangeModeInactivateAll();
		guiGameLogic.ShowHUD();
		NGUITools.SetActive(guiGameLogic.btResetCar.gameObject, true);
		NGUITools.SetActive(btResetCar.gameObject, false);
		NGUITools.SetActive(arrow.gameObject, false);
		MySingleton<Manager>.Instance.gameLogic.mainArrowPointer.gameObject.SetActive(false);
		SetEnableCollectablesParent(true);
		MySingleton<Manager>.Instance.ads.EnableAds();
		guiGameLogic.backButton.function = _lastDefaultBackButtonFunction;
		challangeModeManager.currentChallange.GetCurrentCircuit().OnEnteredEvent -= OnCircuitEntered;
		challangeModeManager.currentChallange.GetCurrentCircuit().OnLeaveEvent -= OnCircuitLeave;
		SpawnerMaster spawnerMaster = challangeModeManager.spawnerMaster;
		spawnerMaster.OnSpawnerMasterEnter = (SpawnerMasterCallbackEvent)Delegate.Remove(spawnerMaster.OnSpawnerMasterEnter, new SpawnerMasterCallbackEvent(OnSpawnerMasterEnter));
		challangeModeManager.OnCircuitLeave(circuit);
	}

	private void OnSpawnerMasterEnter(Vector3 position)
	{
		ChallangeModeInactivateAll();
		guiGameLogic.HideHUD();
		Transform transform = MySingleton<Manager>.Instance.gameLogic.currentCamera.transform;
		cSpawnerCamera.transform.position = transform.position;
		cSpawnerCamera.transform.rotation = transform.rotation;
		TrialForwardTransition(pnSpawner.GetComponent<TweenAlpha>(), OnSpawnerMasterEnterTransitionEnd);
		MySingleton<Manager>.Instance.ads.ShowAdmobOrHeyzapAboutOccurredEvents();
	}

	public void OnTimeOver()
	{
		MonoBehaviour.print(string.Concat("CIRCUITO QUE HA FINALIZADO: ", challangeModeManager.currentChallange.key, " ", challangeModeManager.currentChallange.GetCurrentCircuit()));
		bool flag = false;
		ChallangeModeInactivateAll();
		guiGameLogic.HideHUD();
		NGUITools.SetActive(arrow.gameObject, false);
		MySingleton<Manager>.Instance.gameLogic.mainArrowPointer.gameObject.SetActive(false);
		if (crashableChallengeManager != null)
		{
			totalCrashables = crashableChallengeManager.crashableCounterLocal;
			if (totalCrashables >= crashableChallengeManager.minimumCrashesLocal)
			{
				flag = true;
			}
		}
		if (crashableChallengeManager != null)
		{
			crashableChallengeManager.chronoEvents.OnTimeOver -= OnTimeOver;
		}
		CheckpointMaster checkpointMaster = challangeModeManager.checkpointMaster;
		checkpointMaster.OnPassThroughAllCheckpointEvent = (CheckpointMasterCallbackEvent)Delegate.Remove(checkpointMaster.OnPassThroughAllCheckpointEvent, new CheckpointMasterCallbackEvent(OnPassThroughAllCheckpoint));
		SpawnerMaster spawnerMaster = challangeModeManager.spawnerMaster;
		spawnerMaster.OnSpawnerMasterEnter = (SpawnerMasterCallbackEvent)Delegate.Remove(spawnerMaster.OnSpawnerMasterEnter, new SpawnerMasterCallbackEvent(OnSpawnerMasterEnter));
		challangeModeManager.DisableCarControl();
		NGUITools.SetActive(pnFinish.gameObject, true);
		if (flag)
		{
			spCrashableResult.spriteName = "checkbox_marked";
		}
		else
		{
			spCrashableResult.spriteName = "checkbox_crossed";
		}
		if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Crashable)
		{
			if (totalCrashables >= CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE)
			{
				SetRewardGUI();
			}
			else
			{
				NGUITools.SetActive(spReward.gameObject, false);
				NGUITools.SetActive(spChallangeProgress.gameObject, false);
			}
		}
		else
		{
			SetRewardGUI();
		}
		SubmitReward();
		SetLbBestTime();
		UpdatePnFinish(challangeModeManager.GetChallangeInActivationZone().key);
		ShowAdsAfterChallange();
	}

	private void OnSpawnerMasterEnterTransitionEnd()
	{
		NGUITools.SetActive(pnSpawner.gameObject, false);
		NGUITools.SetActive(pnPlaying.gameObject, true);
		guiGameLogic.ShowHUD();
		challangeModeManager.EnableCarControl();
		challangeModeManager.RespawnLastCheckpoint();
	}

	private void OnPassThroughAllCheckpointTransitionEnd()
	{
		Time.timeScale = 0.01f;
	}

	private void OnRouteFinish()
	{
		challangeModeManager.circuitSplineMaster.finishCallbackEvent -= OnRouteFinish;
		BreakRoute(true);
	}

	private void DirectTransition(TweenAlpha transition, EventDelegate.Callback del)
	{
		transition.from = 1f;
		NGUITools.SetActive(transition.gameObject, true);
	}

	private void ForwardTransition(TweenAlpha transition, EventDelegate.Callback del)
	{
		NGUITools.SetActive(transition.gameObject, true);
		transition.SetOnFinished(del);
		transition.from = 0f;
		transition.to = 1f;
		transition.PlayForward();
	}

	private void TrialForwardTransition(TweenAlpha transition, EventDelegate.Callback del)
	{
		NGUITools.SetActive(transition.gameObject, true);
		transition.SetOnFinished(del);
		transition.from = 0f;
		transition.ResetToBeginning();
		transition.PlayForward();
	}

	private void ReverseTransition(TweenAlpha transition, EventDelegate.Callback del)
	{
		transition.SetOnFinished(del);
		transition.PlayReverse();
	}

	private void ChallangeModeInactivateAll()
	{
		NGUITools.SetActive(pnActivationZone.gameObject, false);
		NGUITools.SetActive(pnPreview.gameObject, false);
		NGUITools.SetActive(pnPlaying.gameObject, false);
		NGUITools.SetActive(pnFinish.gameObject, false);
		NGUITools.SetActive(pnSpawner.gameObject, false);
	}

	private void SetActivePanelWhenPaused()
	{
		if (pnActivationZone.gameObject.activeSelf)
		{
			_activePanelWhenPaused = pnActivationZone;
		}
		else if (pnPreview.gameObject.activeSelf)
		{
			_activePanelWhenPaused = pnPreview;
		}
		else if (pnPlaying.gameObject.activeSelf)
		{
			_activePanelWhenPaused = pnPlaying;
		}
		else if (pnFinish.gameObject.activeSelf)
		{
			_activePanelWhenPaused = pnFinish;
		}
		else if (pnSpawner.gameObject.activeSelf)
		{
			_activePanelWhenPaused = pnSpawner;
		}
		else
		{
			_activePanelWhenPaused = null;
		}
	}

	private void InitializeChallangeMode()
	{
		ChallangeModeInactivateAll();
	}

	public void UpdateChallangeMode()
	{
		if (challangeModeManager == null)
		{
			return;
		}
		if (challangeModeManager.IsInCircuit())
		{
			if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Trial)
			{
				lbElapsedTime.text = Converter.GetRaceTimeFromSeconds(Chronometer.GetElapsed());
			}
			else if (challangeModeManager.GetChallangeInActivationZone().key != ChallangeKey.Crashable)
			{
			}
			AnimateRewardGUI();
		}
		else if (challangeModeManager.CanPlayCircuit())
		{
			if (_insideActivationZone)
			{
				return;
			}
			ActivateChallangeGUI(challangeModeManager.GetChallangeInActivationZone().key);
			ForwardTransition(pnActivationZone.GetComponent<TweenAlpha>(), null);
			_insideActivationZone = true;
			lbActivationZoneName.text = challangeModeManager.GetChallangeInActivationZone().GetCircuitName(challangeModeManager.GetCircuitInActivationZone());
			lbPreviewName.text = lbActivationZoneName.text;
			lbFinishName.text = lbActivationZoneName.text;
			switch (challangeModeManager.GetChallangeInActivationZone().key)
			{
			case ChallangeKey.Trial:
				if (challangeModeManager.GetCircuitInActivationZone().bestTime > 0f)
				{
					lbActivationZoneBestTime.text = Converter.GetRaceTimeFromSeconds(challangeModeManager.GetCircuitInActivationZone().bestTime);
				}
				else
				{
					lbActivationZoneBestTime.text = "-:--:---";
				}
				break;
			case ChallangeKey.Radar:
				lbActivationZoneBestTotalSpeed.text = Converter.MSToCurrentUnits(challangeModeManager.GetCircuitInActivationZone().bestSpeed, true);
				break;
			case ChallangeKey.Crashable:
				lbActivationZoneBestTotalCrashables.text = challangeModeManager.GetCircuitInActivationZone().bestCrashable.ToString();
				break;
			}
		}
		else if (_insideActivationZone)
		{
			ReverseTransition(pnActivationZone.GetComponent<TweenAlpha>(), null);
			_insideActivationZone = false;
		}
	}

	private void SetLbBestTime()
	{
		switch (challangeModeManager.GetChallangeInActivationZone().key)
		{
		case ChallangeKey.Trial:
			if (challangeModeManager.currentChallange.GetCurrentCircuit().bestTime == 0f)
			{
				lbBestTime.text = "-:--:---";
			}
			else
			{
				lbBestTime.text = Converter.GetRaceTimeFromSeconds(challangeModeManager.currentChallange.GetCurrentCircuit().bestTime);
			}
			lbFinishBestTime.text = lbBestTime.text;
			lbFinishElapsed.text = Converter.GetRaceTimeFromSeconds(Chronometer.GetElapsed());
			break;
		case ChallangeKey.Radar:
			lbBestSpeed.text = Converter.MSToCurrentUnits(challangeModeManager.currentChallange.GetCurrentCircuit().bestSpeed, true);
			lbFinishBestSpeed.text = lbBestSpeed.text;
			break;
		case ChallangeKey.Crashable:
			lbBestCrashables.text = challangeModeManager.currentChallange.GetCurrentCircuit().bestCrashable.ToString();
			lbFinishBestCrashables.text = totalCrashables + " / 30";
			break;
		}
	}

	private void UpdatePnFinish(ChallangeKey key)
	{
		switch (key)
		{
		case ChallangeKey.Radar:
		{
			int num = 0;
			foreach (Transform item in radarsGrid.transform)
			{
				radarLabelInfo component = item.GetComponent<radarLabelInfo>();
				component.lbSpeed.text = Converter.MSToCurrentUnits(radarScoresMPS[num], true);
				component.lbName.text = string.Format("RADAR {0}:", num + 1);
				num++;
			}
			lbFinalTotalSpeed.text = Converter.MSToCurrentUnits(radarTotalSpeedMPS, true);
			radarsGrid.GetComponent<UIGrid>().Reposition();
			break;
		}
		case ChallangeKey.Crashable:
			if (MySingleton<Manager>.Instance.isNewGUI)
			{
				lbFinalCrashables.text = totalCrashables + " / 30";
			}
			if (totalCrashables < CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE)
			{
				NGUITools.SetActive(wdChallangeSuccess.gameObject, false);
				NGUITools.SetActive(wdChallangeFailed.gameObject, true);
			}
			else
			{
				NGUITools.SetActive(wdChallangeSuccess.gameObject, true);
				NGUITools.SetActive(wdChallangeFailed.gameObject, false);
			}
			break;
		}
	}

	private void ActivateChallangeGUI(ChallangeKey key)
	{
		NGUITools.SetActiveSelf(lbActivationZoneBestTime.gameObject, key == ChallangeKey.Trial);
		NGUITools.SetActiveSelf(lbBestTime.gameObject, key == ChallangeKey.Trial);
		NGUITools.SetActiveSelf(lbFinishElapsed.gameObject, key == ChallangeKey.Trial);
		NGUITools.SetActiveSelf(lbFinishBestTime.gameObject, key == ChallangeKey.Trial);
		NGUITools.SetActiveSelf(lbActivationZoneBestTotalSpeed.gameObject, key == ChallangeKey.Radar);
		NGUITools.SetActiveSelf(lbBestSpeed.gameObject, key == ChallangeKey.Radar);
		NGUITools.SetActiveSelf(spRadarSpeeds.gameObject, key == ChallangeKey.Radar);
		NGUITools.SetActiveSelf(spFinalRadarSpeeds.gameObject, key == ChallangeKey.Radar);
		NGUITools.SetActiveSelf(lbFinishBestSpeed.gameObject, key == ChallangeKey.Radar);
		NGUITools.SetActiveSelf(lbActivationZoneBestTotalCrashables.gameObject, key == ChallangeKey.Crashable);
		NGUITools.SetActiveSelf(lbBestCrashables.gameObject, key == ChallangeKey.Crashable);
		NGUITools.SetActiveSelf(SpCrashablesCounter.gameObject, key == ChallangeKey.Crashable);
		NGUITools.SetActiveSelf(lbFinishBestCrashables.gameObject, key == ChallangeKey.Crashable);
		NGUITools.SetActive(wgCrashableResultBar.gameObject, key == ChallangeKey.Crashable);
		NGUITools.SetActiveSelf(lbElapsedTime.gameObject, key == ChallangeKey.Trial || key == ChallangeKey.Crashable);
		if (!MySingleton<Manager>.Instance.isNewGUI)
		{
			NGUITools.SetActive(wdChallangeSuccess.gameObject, true);
			NGUITools.SetActive(wdChallangeFailed.gameObject, false);
		}
	}

	private void SetRewardGUI()
	{
		spChallangeProgress.GetComponent<GUIGameButton>().function = GUIGameButton.GameButtonFunction.None;
		NGUITools.SetActive(spChallangeProgress.gameObject, false);
		NGUITools.SetActive(spNewCarUnlocked.gameObject, false);
		NGUITools.SetActive(spReward.gameObject, false);
		if (challangeModeManager.currentChallange.infoCar != null)
		{
			spChallangeProgress.spriteName = challangeModeManager.currentChallange.infoCar.iconName;
			spChallangeProgressBackground.spriteName = challangeModeManager.currentChallange.infoCar.iconName;
		}
		if (!challangeModeManager.currentChallange.GetReward().completed || challangeModeManager.currentChallange.GetReward().rewardType == ChallangeRewardType.Car)
		{
			if (challangeModeManager.currentChallange.GetReward().rewardType == ChallangeRewardType.Car)
			{
				if (!challangeModeManager.currentChallange.infoCar.isBought)
				{
					MySingleton<Manager>.Instance.carUnlockedId = challangeModeManager.currentChallange.infoCar.id;
					spChallangeProgress.GetComponent<GUIGameButton>().function = GUIGameButton.GameButtonFunction.GoToGarajeFromChallange;
					NGUITools.SetActive(spNewCarUnlocked.gameObject, true);
					NGUITools.SetActive(spChallangeProgress.gameObject, true);
					spChallangeProgress.spriteName = challangeModeManager.currentChallange.infoCar.iconName;
				}
			}
			else if (challangeModeManager.currentChallange.GetReward().rewardType == ChallangeRewardType.Percentage)
			{
				if (challangeModeManager.currentChallange.infoCar.isBought)
				{
					NGUITools.SetActive(spReward.gameObject, true);
					NGUITools.SetActive(spChallangeProgress.gameObject, false);
					lbReward.text = challangeModeManager.currentChallange.GetReward().money.ToString("0");
					_fillingFrom = 1f;
				}
				else
				{
					NGUITools.SetActive(spChallangeProgress.gameObject, true);
					NGUITools.SetActive(spReward.gameObject, false);
					spChallangeProgress.spriteName = challangeModeManager.currentChallange.infoCar.iconName;
					_fillingFrom = (challangeModeManager.currentChallange.GetReward().progress - challangeModeManager.currentChallange.GetReward().proportion) * 0.01f;
				}
			}
			else if (challangeModeManager.currentChallange.GetReward().rewardType != ChallangeRewardType.Money)
			{
				NGUITools.SetActive(spChallangeProgress.gameObject, false);
				NGUITools.SetActive(spReward.gameObject, true);
				_fillingFrom = (challangeModeManager.currentChallange.GetReward().progress - challangeModeManager.currentChallange.GetReward().proportion) * 0.01f;
			}
			else
			{
				_fillingFrom = challangeModeManager.currentChallange.GetReward().progress * 0.01f;
			}
		}
		else if (challangeModeManager.currentChallange.GetReward().completed && challangeModeManager.currentChallange.GetReward().rewardType == ChallangeRewardType.Money)
		{
			_fillingFrom = 1f;
		}
		if (challangeModeManager.currentChallange.GetReward().rewardType == ChallangeRewardType.Money)
		{
			NGUITools.SetActive(spReward.gameObject, true);
			lbReward.text = challangeModeManager.currentChallange.GetReward().money.ToString("0");
		}
		spChallangeProgress.fillAmount = _fillingFrom;
		_fillingTo = challangeModeManager.currentChallange.GetReward().progress * 0.01f;
		_fillingAnimationDuration = pnFinish.GetComponent<TweenAlpha>().duration + fillingAnimationPlusTime;
		_fillingAnimateInvDuration = 1f / _fillingAnimationDuration;
		_fillingAnimateStart = Time.unscaledTime;
		_fillingAnimateEnd = _fillingAnimateStart + _fillingAnimationDuration;
		_fillingAnimate = true;
	}

	private void AnimateRewardGUI()
	{
		if (!_fillingAnimate)
		{
			return;
		}
		if (Time.unscaledTime < _fillingAnimateEnd)
		{
			if ((!challangeModeManager.currentChallange.GetReward().completed || challangeModeManager.currentChallange.GetReward().rewardType == ChallangeRewardType.Car) && _fillingFrom < _fillingTo)
			{
				spChallangeProgress.fillAmount = Mathf.Lerp(_fillingFrom, _fillingTo, (Time.unscaledTime - _fillingAnimateStart) * _fillingAnimateInvDuration);
				if (spChallangeProgress.fillAmount >= _fillingTo)
				{
					spChallangeProgress.fillAmount = _fillingTo;
					_fillingAnimate = false;
				}
			}
		}
		else
		{
			spChallangeProgress.fillAmount = _fillingTo;
		}
	}

	private void SubmitReward()
	{
		string result = "success";
		if (challangeModeManager.currentChallange.GetReward().rewardType == ChallangeRewardType.Car)
		{
			challangeModeManager.currentChallange.infoCar.isBought = true;
			ManagerAnalytics.instance.FireCarUnlocked(challangeModeManager.currentChallange.infoCar.idName, "challenges_" + challangeModeManager.GetChallangeInActivationZone().key);
		}
		if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Trial)
		{
			challangeModeManager.currentChallange.SaveCurrentCircuitPlayerPrefs(Chronometer.GetElapsed());
			challangeModeManager.currentChallange.ShareCurrentCircuitScoreLeaderboardTime(Chronometer.GetElapsed());
		}
		if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Radar)
		{
			challangeModeManager.currentChallange.SaveRadarsScorePlayerPrefs(radarTotalSpeedMPS);
			challangeModeManager.currentChallange.ShareCurrentCircuitScoreLeaderboardSpeed(Converter.MSToKMH(radarTotalSpeedMPS));
			Debug.Log("Enviado a leaderboards: " + Converter.MSToKMH(radarTotalSpeedMPS) + " kilometros por hora");
		}
		if (challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Crashable)
		{
			challangeModeManager.currentChallange.SaveCrashableScorePlayerPrefs(totalCrashables);
			challangeModeManager.currentChallange.ShareCurrentCircuitScoreLeaderboardCrashables(totalCrashables);
			Debug.Log("Enviado a leaderboards: " + totalCrashables + "crashables");
			if (totalCrashables < CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE)
			{
				result = "fail";
			}
		}
		string[] array = MySingleton<Manager>.Instance.challangeModeManager.currentChallange.GetCurrentCircuit().sceneIndex.ToString().Split('_');
		ManagerAnalytics.instance.FireChallengeEnds(array[3], Chronometer.GetElapsed().ToString(), MySingleton<Manager>.Instance.challangeModeManager.currentChallange.key.ToString(), MySingleton<Manager>.Instance.gameLogic.instantiatedCar.GetComponent<InfoCar>().idName, MySingleton<Manager>.Instance.levelZone.gameObject.name, result);
	}

	private void HideToPause()
	{
		SetActivePanelWhenPaused();
		ChallangeModeInactivateAll();
	}

	private void ShowToPause()
	{
		if (_activePanelWhenPaused != null)
		{
			NGUITools.SetActive(_activePanelWhenPaused.gameObject, true);
			if (_activePanelWhenPaused == pnPreview || _activePanelWhenPaused == pnFinish || _activePanelWhenPaused == pnSpawner)
			{
				guiGameLogic.HideHUD();
			}
		}
	}

	private void SetPauseMode()
	{
		if ((bool)challangeModeManager)
		{
			if (challangeModeManager.IsInCircuit())
			{
				NGUITools.SetActive(guiGameLogic.btRestartChallangeCircuit.gameObject, true);
				NGUITools.SetActive(guiGameLogic.btBackToGame.gameObject, true);
				NGUITools.SetActive(guiGameLogic.btLanguage.gameObject, false);
				NGUITools.SetActive(guiGameLogic.btBackToGarage.gameObject, false);
			}
			else
			{
				NGUITools.SetActive(guiGameLogic.btRestartChallangeCircuit.gameObject, false);
				NGUITools.SetActive(guiGameLogic.btBackToGame.gameObject, false);
				NGUITools.SetActive(guiGameLogic.btLanguage.gameObject, true);
				NGUITools.SetActive(guiGameLogic.btBackToGarage.gameObject, true);
			}
		}
	}

	public void ShowPause()
	{
		HideToPause();
		SetPauseMode();
		Chronometer.Pause();
		if ((bool)crashableChallengeManager)
		{
			crashableChallengeManager.PauseChronometer();
		}
	}

	public void ResumePause()
	{
		ShowToPause();
		Chronometer.Resume();
		if ((bool)crashableChallengeManager)
		{
			crashableChallengeManager.PauseChronometer();
		}
	}

	private void SetEnableCollectablesParent(bool enable)
	{
		if ((bool)MySingleton<Manager>.Instance.gameLogic.persistentCollectablesParent)
		{
			MySingleton<Manager>.Instance.gameLogic.persistentCollectablesParent.SetActive(enable);
		}
		if ((bool)MySingleton<Manager>.Instance.gameLogic.actualZoneCollectablesParent)
		{
			MySingleton<Manager>.Instance.gameLogic.actualZoneCollectablesParent.SetActive(enable);
		}
	}
}
