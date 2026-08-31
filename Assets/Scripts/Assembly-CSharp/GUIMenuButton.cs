using UnityEngine;

public class GUIMenuButton : MonoBehaviour
{
	public enum MenuButtonFunction
	{
		None = -1,
		GoToGame = 0,
		Endless = 1,
		MoreGames = 2,
		Close = 3,
		RateYes = 4,
		RateNo = 5,
		ExitYes = 6,
		ShowMenuPanel = 7,
		Airplane = 8,
		Leaderboard = 9,
		RatePanel = 10,
		GameModePanel = 11,
		GoToCheckpointGame = 12,
		GoToTrafficGame = 13,
		SelectCar = 14,
		SelectThisCar = 15,
		PreviousCar = 16,
		NextCar = 17,
		BuyThisCar = 18,
		RemoveAds = 20,
		RestorePurchases = 21,
		FacebookLogin = 22,
		ShowCar = 23,
		FacebookInvite = 24,
		FacebookLoginLock = 25,
		ShowExit = 30,
		GoToMenuFromGarage = 31,
		OpenStore = 32,
		PurchaseCar = 33,
		HidePurchaseInfo = 34,
		OpenCompleteTask = 35,
		OpenPackStore = 36,
		OpenDailyRewards = 37,
		GoToMenuFromGameMode = 38,
		GoToMultiplayerPanel = 39,
		GoToConnectingToMultiplayerModePanel = 40,
		GoToConnectionTestPanel = 41,
		RefreshFriendList = 42,
		FacebookLogout = 43,
		Update = 44,
		ShowScreenshotPanel = 45,
		HideScreenshotPanel = 46,
		GoToRaceMode = 47,
		ClosePolicy = 48,
		OpenPolicy = 49,
		CloseAchievementPanel = 50,
		OpenAchievementPanel = 51,
		OpenProgressPanel = 52,
		CloseProgressPanel = 53,
		CloseYesNoPopUp = 54,
		BuyStarterPack = 55,
		CloseStarterPack = 56,
		HidePopup = 57,
		DownloadPopupCrosspromoGame = 58
	}

	private GUIMenuLogic _logic;

	public MenuButtonFunction function;

	public GUIMenuLogic logic
	{
		get
		{
			if (_logic == null)
			{
				_logic = MySingleton<Manager>.Instance.guiMenuLogic;
			}
			return _logic;
		}
	}

	public void OnClick()
	{
		switch (function)
		{
		case MenuButtonFunction.GoToGame:
			logic.GoToGame();
			break;
		case MenuButtonFunction.MoreGames:
			logic.ShowMoreGames();
			break;
		case MenuButtonFunction.Airplane:
			break;
		case MenuButtonFunction.RateYes:
			logic.RateYes();
			break;
		case MenuButtonFunction.RateNo:
			logic.RateNo();
			break;
		case MenuButtonFunction.ExitYes:
			logic.ExitYes();
			break;
		case MenuButtonFunction.ShowMenuPanel:
			logic.ShowMenu();
			break;
		case MenuButtonFunction.RatePanel:
			logic.ShowRateMe();
			break;
		case MenuButtonFunction.GameModePanel:
			logic.ShowGameMode();
			break;
		case MenuButtonFunction.GoToCheckpointGame:
			logic.GoToCheckpointGame();
			break;
		case MenuButtonFunction.GoToTrafficGame:
			logic.GoToTrafficGame();
			break;
		case MenuButtonFunction.ShowCar:
		{
			CarButtonInfo component = base.transform.gameObject.GetComponent<CarButtonInfo>();
			logic.ShowCar(component.arrayPos);
			component.newCarUnlocked.SetActive(false);
			break;
		}
		case MenuButtonFunction.PreviousCar:
			logic.ShowPreviousCar();
			break;
		case MenuButtonFunction.NextCar:
			logic.ShowNextCar();
			break;
		case MenuButtonFunction.Update:
			logic.SendMessage("UpdateGame", null, SendMessageOptions.DontRequireReceiver);
			break;
		case MenuButtonFunction.SelectCar:
			logic.ShowSelectCar(true, true);
			break;
		case MenuButtonFunction.SelectThisCar:
			logic.SelectThisCar();
			break;
		case MenuButtonFunction.BuyThisCar:
			logic.ShowConfirmationBuyCar(true, true);
			break;
		case MenuButtonFunction.RemoveAds:
			logic.RemoveAds();
			break;
		case MenuButtonFunction.BuyStarterPack:
			logic.BuyStarterPack();
			break;
		case MenuButtonFunction.CloseStarterPack:
			logic.CloseStarterPack();
			break;
		case MenuButtonFunction.RestorePurchases:
			MySingleton<Manager>.Instance.restorePurchases = true;
			MySingleton<Manager>.Instance.unibill.RestorePurchases();
			break;
		case MenuButtonFunction.FacebookInvite:
		case MenuButtonFunction.FacebookLoginLock:
			break;
		case MenuButtonFunction.ShowExit:
			logic.ShowExit();
			break;
		case MenuButtonFunction.GoToMenuFromGameMode:
			logic.GoMenuFromGameMode();
			break;
		case MenuButtonFunction.GoToMenuFromGarage:
			logic.GoMenuFromGarage();
			break;
		case MenuButtonFunction.OpenStore:
			logic.HidePurchaseInfoPanel();
			MySingleton<Manager>.Instance.restorePurchases = false;
			logic.OpenStore();
			break;
		case MenuButtonFunction.PurchaseCar:
			logic.PurchaseCar();
			break;
		case MenuButtonFunction.HidePurchaseInfo:
			logic.HidePurchaseInfoPanel();
			break;
		case MenuButtonFunction.OpenCompleteTask:
			logic.OpenCompleteTask();
			break;
		case MenuButtonFunction.OpenPackStore:
			logic.OpenPackStore();
			MySingleton<Manager>.Instance.guiMenuLogic.openingPackStoreFromGaraje = true;
			break;
		case MenuButtonFunction.OpenDailyRewards:
			logic.OpenDailyRewards();
			break;
		case MenuButtonFunction.ShowScreenshotPanel:
			logic.ShowScreenshotPanel(true);
			break;
		case MenuButtonFunction.HideScreenshotPanel:
			logic.ShowScreenshotPanel(false);
			break;
		case MenuButtonFunction.ClosePolicy:
			logic.ClosePolicy();
			break;
		case MenuButtonFunction.OpenPolicy:
			logic.OpenPolicy();
			break;
		case MenuButtonFunction.CloseAchievementPanel:
			logic.ActiveAchievementPanel(false);
			break;
		case MenuButtonFunction.OpenAchievementPanel:
			logic.ActiveAchievementPanel(true);
			break;
		case MenuButtonFunction.OpenProgressPanel:
			logic.ActiveProgressPanel(true);
			break;
		case MenuButtonFunction.CloseProgressPanel:
			logic.ActiveProgressPanel(false);
			break;
		case MenuButtonFunction.Endless:
		case MenuButtonFunction.Close:
		case MenuButtonFunction.Leaderboard:
		case (MenuButtonFunction)19:
		case MenuButtonFunction.FacebookLogin:
		case (MenuButtonFunction)26:
		case (MenuButtonFunction)27:
		case (MenuButtonFunction)28:
		case (MenuButtonFunction)29:
		case MenuButtonFunction.GoToMultiplayerPanel:
		case MenuButtonFunction.GoToConnectingToMultiplayerModePanel:
		case MenuButtonFunction.GoToConnectionTestPanel:
		case MenuButtonFunction.RefreshFriendList:
		case MenuButtonFunction.FacebookLogout:
		case MenuButtonFunction.GoToRaceMode:
		case MenuButtonFunction.CloseYesNoPopUp:
			break;
		}
	}
}
