using UnityEngine;

public class GUIGameButton : MonoBehaviour
{
	public enum GameButtonFunction
	{
		None = 0,
		BackToMenu_NoAd = 1,
		Pause = 2,
		Resume = 3,
		Restart = 4,
		Back = 5,
		Reset = 6,
		Leaderboard = 7,
		BackWithAd = 8,
		Abs = 9,
		Esp = 10,
		Tc = 11,
		NextCamera = 12,
		ResetCar = 13,
		Gas = 14,
		Brake = 15,
		Handbrake = 16,
		Repair = 17,
		SettingControl = 18,
		SettingUnits = 19,
		SettingMusic = 20,
		Settings = 21,
		DirLeft = 22,
		DirRight = 23,
		MoreCars = 24,
		HideInfoPanel = 25,
		ControlArrows = 30,
		ControlSteeringWheel = 31,
		ControlAccelerometer = 32,
		UnitsKMH = 33,
		UnitsMPH = 34,
		NewCar = 35,
		GoToIsland = 36,
		BillingRemoveAds = 40,
		BillingRestorePurchases = 41,
		MusicVolumeSlider = 42,
		OpenStore = 43,
		OpenCompleteTask = 44,
		PlayCircuit = 60,
		Play = 61,
		ShowLeaderboard = 62,
		BackToGame = 63,
		RestartCircuit = 64,
		RespawnLastCheckpoint = 65,
		BreakRoute = 66,
		RestartCircuitFromMenu = 67,
		BackToGameFromMenu = 68,
		GoToGarajeFromChallange = 69,
		ShowAllLeaderboards = 70,
		ScreenshotMode = 75,
		CancelScreenshot = 76,
		TakeScreenshot = 77,
		ResumeScreenshot = 78,
		MinimizeResumeScreenshot = 79,
		SubmitScreenshot = 80,
		InviteFriends = 85,
		RemoveAdsOffer = 86,
		CloseOfferPanel = 87,
		GotoPlayersList = 88,
		BackToPauseFromPlayersList = 89,
		ConfirmateAddFriend = 90,
		ShowAddFriendConfirmation = 91,
		HideAddFriendConfirmation = 92,
		PlayHorn = 93,
		SwitchCarLights = 94,
		CloseAchievementPanel = 95,
		OpenAchievementPanel = 96,
		OpenProgressPanel = 97,
		CloseProgressPanel = 98,
		CloseStartingPackPopup = 99,
		HidePopup = 100,
		DownloadPopupCrosspromoGame = 101
	}

	private GUIGameLogic _logic;

	public GameButtonFunction function;

	private GUIGameLogic logic
	{
		get
		{
			if (_logic == null)
			{
				_logic = MySingleton<Manager>.Instance.guiGameLogic;
			}
			return _logic;
		}
	}

	public void OnClick()
	{
		switch (function)
		{
		case GameButtonFunction.CloseStartingPackPopup:
			logic.CloseStarterPack();
			break;
		case GameButtonFunction.Pause:
			logic.PauseMode();
			break;
		case GameButtonFunction.Resume:
			logic.Resume();
			break;
		case GameButtonFunction.Restart:
			logic.Restart();
			break;
		case GameButtonFunction.Back:
			logic.BackToMenu();
			break;
		case GameButtonFunction.BackWithAd:
			logic.BackToMenuWithAd();
			break;
		case GameButtonFunction.Reset:
			logic.Reset();
			break;
		case GameButtonFunction.Abs:
			logic.ChangeAbs();
			break;
		case GameButtonFunction.Esp:
			logic.ChangeEsp();
			break;
		case GameButtonFunction.Tc:
			logic.ChangeTc();
			break;
		case GameButtonFunction.NextCamera:
			logic.NextCamera();
			break;
		case GameButtonFunction.ResetCar:
			logic.Reset();
			break;
		case GameButtonFunction.Repair:
			logic.Repair();
			break;
		case GameButtonFunction.Settings:
			break;
		case GameButtonFunction.SettingControl:
			SettingsManager.ToggleControl();
			logic.ReloadSettings();
			break;
		case GameButtonFunction.MoreCars:
			MySingleton<Manager>.Instance.MoreCars();
			break;
		case GameButtonFunction.HideInfoPanel:
			logic.HidePanelInfo();
			break;
		case GameButtonFunction.ControlAccelerometer:
			SettingsManager.SetControl(Manager.Controls.Accelerometer);
			logic.ReloadSettings();
			break;
		case GameButtonFunction.ControlArrows:
			SettingsManager.SetControl(Manager.Controls.Buttons);
			logic.ReloadSettings();
			break;
		case GameButtonFunction.ControlSteeringWheel:
			SettingsManager.SetControl(Manager.Controls.Wheel);
			logic.ReloadSettings();
			break;
		case GameButtonFunction.UnitsKMH:
			logic.SetUnits(SettingsManager.Units.KMH);
			break;
		case GameButtonFunction.UnitsMPH:
			logic.SetUnits(SettingsManager.Units.MPH);
			break;
		case GameButtonFunction.NewCar:
			logic.GoToGarage();
			break;
		case GameButtonFunction.GoToIsland:
			logic.GoToIsland();
			break;
		case GameButtonFunction.BillingRemoveAds:
			logic.RemoveAds();
			break;
		case GameButtonFunction.RemoveAdsOffer:
			logic.RemoveAdsOffer();
			break;
		case GameButtonFunction.CloseOfferPanel:
			logic.UpdateOfferPanel();
			break;
		case GameButtonFunction.BillingRestorePurchases:
			MySingleton<Manager>.Instance.unibill.RestorePurchases();
			break;
		case GameButtonFunction.OpenStore:
			logic.OpenStore();
			break;
		case GameButtonFunction.OpenCompleteTask:
			logic.OpenCompleteTask();
			break;
		case GameButtonFunction.PlayCircuit:
			logic.PlayCircuit();
			break;
		case GameButtonFunction.Play:
			logic.Play();
			break;
		case GameButtonFunction.ShowLeaderboard:
			logic.ShowLeaderboard();
			break;
		case GameButtonFunction.ShowAllLeaderboards:
			MySingleton<Manager>.Instance.gameServicesManager.loginSucceedBeforeShowAllLeaderboardEvent += () =>
			{
				MySingleton<Manager>.Instance.challangeManager.ReportAllScoresWhenLoginSucceed();
			};
			MySingleton<Manager>.Instance.gameServicesManager.ShowAllLeaderboardOrLogin();
			break;
		case GameButtonFunction.BackToGame:
			logic.BackToGame();
			break;
		case GameButtonFunction.RestartCircuit:
			logic.Retry();
			break;
		case GameButtonFunction.RespawnLastCheckpoint:
			logic.RespawnLastCheckpoint();
			break;
		case GameButtonFunction.BreakRoute:
			logic.BreakRoute();
			break;
		case GameButtonFunction.RestartCircuitFromMenu:
		{
			string[] array2 = MySingleton<Manager>.Instance.challangeModeManager.currentChallange.GetCurrentCircuit().sceneIndex.ToString().Split('_');
			ManagerAnalytics.instance.FireChallengeEnds(array2[3], Chronometer.GetElapsed().ToString(), MySingleton<Manager>.Instance.challangeModeManager.currentChallange.key.ToString(), MySingleton<Manager>.Instance.gameLogic.instantiatedCar.GetComponent<InfoCar>().idName, MySingleton<Manager>.Instance.levelZone.gameObject.name, "restart_leave");
			logic.Resume();
			logic.Retry();
			break;
		}
		case GameButtonFunction.BackToGameFromMenu:
		{
			string[] array = MySingleton<Manager>.Instance.challangeModeManager.currentChallange.GetCurrentCircuit().sceneIndex.ToString().Split('_');
			ManagerAnalytics.instance.FireChallengeEnds(array[3], Chronometer.GetElapsed().ToString(), MySingleton<Manager>.Instance.challangeModeManager.currentChallange.key.ToString(), MySingleton<Manager>.Instance.gameLogic.instantiatedCar.GetComponent<InfoCar>().idName, MySingleton<Manager>.Instance.levelZone.gameObject.name, "pause_leave");
			logic.Resume();
			logic.BackToGame();
			break;
		}
		case GameButtonFunction.GoToGarajeFromChallange:
			logic.GoToGarajeFromChallange();
			break;
		case GameButtonFunction.ScreenshotMode:
			ManagerAnalytics.instance.FirePhotoMode(MySingleton<Manager>.Instance.gameLogic.instantiatedCar.GetComponent<InfoCar>().idName, "action_phase", MySingleton<Manager>.Instance.levelZone.gameObject.name);
			if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Multiplayer)
			{
				MySingleton<Manager>.Instance.notificationManager.RequestNotificationOK(Localization.Get("multiplayer_classic_no_available"));
				break;
			}
			GUIFacebookScreenshotMode.Instance().ScreenshotMode();
			NGUITools.SetActive(MySingleton<Manager>.Instance.guiChallangeModeLogic.pnActivationZone.gameObject, false);
			break;
		case GameButtonFunction.CancelScreenshot:
			NGUITools.SetActive(MySingleton<Manager>.Instance.guiCurrentLogic.pnAchievements, false);
			GUIFacebookScreenshotMode.Instance().CancelScreenshot();
			if ((bool)MySingleton<Manager>.Instance.gameLogic.checkPointPrefab)
			{
				MySingleton<Manager>.Instance.gameLogic.checkPointPrefab.SetActive(true);
			}
			break;
		case GameButtonFunction.TakeScreenshot:
			NGUITools.SetActive(MySingleton<Manager>.Instance.guiCurrentLogic.pnAchievements, false);
			GUIFacebookScreenshotMode.Instance().TakeScreenshot();
			break;
		case GameButtonFunction.ResumeScreenshot:
			GUIFacebookScreenshotMode.Instance().ResumeScreenshot();
			break;
		case GameButtonFunction.MinimizeResumeScreenshot:
			GUIFacebookScreenshotMode.Instance().OnFacebookMinimizeResumeScreenshot();
			break;
		case GameButtonFunction.SubmitScreenshot:
			GUIFacebookScreenshotMode.Instance().SubmitScreenshot();
			break;
		case GameButtonFunction.InviteFriends:
			logic.InviteFriends();
			break;
		case GameButtonFunction.SwitchCarLights:
			logic.SwitchCarLights();
			break;
		case GameButtonFunction.CloseAchievementPanel:
			logic.ActiveAchievementPanel(false);
			break;
		case GameButtonFunction.OpenAchievementPanel:
			logic.ActiveAchievementPanel(true);
			break;
		case GameButtonFunction.OpenProgressPanel:
			logic.ActiveProgressPanel(true);
			break;
		case GameButtonFunction.CloseProgressPanel:
			logic.ActiveProgressPanel(false);
			break;
		case GameButtonFunction.Leaderboard:
		case GameButtonFunction.Gas:
		case GameButtonFunction.Brake:
		case GameButtonFunction.Handbrake:
		case GameButtonFunction.SettingUnits:
		case GameButtonFunction.SettingMusic:
		case GameButtonFunction.DirLeft:
		case GameButtonFunction.DirRight:
		case (GameButtonFunction)26:
		case (GameButtonFunction)27:
		case (GameButtonFunction)28:
		case (GameButtonFunction)29:
		case (GameButtonFunction)37:
		case (GameButtonFunction)38:
		case (GameButtonFunction)39:
		case GameButtonFunction.MusicVolumeSlider:
		case (GameButtonFunction)45:
		case (GameButtonFunction)46:
		case (GameButtonFunction)47:
		case (GameButtonFunction)48:
		case (GameButtonFunction)49:
		case (GameButtonFunction)50:
		case (GameButtonFunction)51:
		case (GameButtonFunction)52:
		case (GameButtonFunction)53:
		case (GameButtonFunction)54:
		case (GameButtonFunction)55:
		case (GameButtonFunction)56:
		case (GameButtonFunction)57:
		case (GameButtonFunction)58:
		case (GameButtonFunction)59:
		case (GameButtonFunction)71:
		case (GameButtonFunction)72:
		case (GameButtonFunction)73:
		case (GameButtonFunction)74:
		case (GameButtonFunction)81:
		case (GameButtonFunction)82:
		case (GameButtonFunction)83:
		case (GameButtonFunction)84:
		case GameButtonFunction.GotoPlayersList:
		case GameButtonFunction.BackToPauseFromPlayersList:
		case GameButtonFunction.ConfirmateAddFriend:
		case GameButtonFunction.ShowAddFriendConfirmation:
		case GameButtonFunction.HideAddFriendConfirmation:
		case GameButtonFunction.PlayHorn:
			break;
		}
	}

	private void OnPress(bool isPressed)
	{
		if (!base.enabled)
		{
			return;
		}
		if (function == GameButtonFunction.Gas)
		{
			logic.Gas(isPressed);
		}
		if (function == GameButtonFunction.Brake)
		{
			logic.Brake(isPressed);
		}
		if (function == GameButtonFunction.Handbrake)
		{
			logic.HandBrake(isPressed);
		}
		if (function == GameButtonFunction.DirLeft)
		{
			logic.DirLeft(isPressed);
		}
		if (function == GameButtonFunction.DirRight)
		{
			logic.DirRight(isPressed);
		}
		if (function == GameButtonFunction.PlayHorn)
		{
			logic.PlayHorn(isPressed);
		}
		if (base.enabled && !isPressed)
		{
			GameButtonFunction gameButtonFunction = function;
			if (gameButtonFunction == GameButtonFunction.MusicVolumeSlider)
			{
				logic.VolumeChanged(NGUITools.FindInParents<UISlider>(base.transform).value);
			}
		}
	}
}
