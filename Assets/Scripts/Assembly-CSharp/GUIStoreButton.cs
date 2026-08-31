using UnityEngine;

public class GUIStoreButton : MonoBehaviour
{
	public enum StoreButtonFunction
	{
		None = -1,
		ExitStore = 0,
		HideInfoPanel = 1,
		VideoRewardForCredits = 2,
		HideInfoPurchaseFailed = 3,
		ShowThisPack = 4,
		BuyThisPack = 5,
		BuyThisCar = 6,
		HideOffersPanel = 7,
		ShowStore = 8,
		HideStore = 9,
		ClaimReward = 10,
		TestAlmostADay = 11,
		TestMoreThanADay = 12,
		HideInternetFailPanel = 13,
		ShowDailyRewardsPanel = 14,
		HideDailyRewardsPanel = 15,
		ShowEarnedCar = 16,
		BackToPackStore = 17,
		RightArrow = 18,
		LeftArrow = 19,
		DownArrow = 20,
		UpArrow = 21,
		CloseStarterPack = 22
	}

	private GUIStoreLogic _logic;

	public InfoCar.PurchasablePack pack;

	public InfoPack infoPack;

	public StoreButtonFunction function;

	public GUIStoreLogic logic
	{
		get
		{
			if (_logic == null)
			{
				_logic = MySingleton<Manager>.Instance.guiStoreLogic;
			}
			return _logic;
		}
	}

	public void OnClick()
	{
		switch (function)
		{
		case StoreButtonFunction.CloseStarterPack:
			logic.CloseStarterPackPanel();
			break;
		case StoreButtonFunction.ExitStore:
			logic.ExitStore();
			break;
		case StoreButtonFunction.HideInfoPanel:
			logic.HideInfoPanel();
			break;
		case StoreButtonFunction.VideoRewardForCredits:
			logic.ShowVideoRewardForCredits();
			break;
		case StoreButtonFunction.HideInfoPurchaseFailed:
			logic.HideInfoPurchaseFailedPanel();
			break;
		case StoreButtonFunction.ShowThisPack:
			if ((bool)infoPack)
			{
				logic.ShowThisPack(infoPack, true, true);
			}
			else
			{
				logic.ShowThisPack(pack, true, true);
			}
			break;
		case StoreButtonFunction.BuyThisPack:
			logic.BuyThisPack();
			break;
		case StoreButtonFunction.BuyThisCar:
			logic.BuyThisCar();
			break;
		case StoreButtonFunction.HideOffersPanel:
			logic.ShowOffersPanel(false);
			break;
		case StoreButtonFunction.ShowStore:
			logic.ShowOrHideStore(true);
			break;
		case StoreButtonFunction.HideStore:
			logic.ShowOrHideStore(false);
			break;
		case StoreButtonFunction.ClaimReward:
			logic.ClaimReward();
			break;
		case StoreButtonFunction.TestAlmostADay:
			logic.TestAlmostADay();
			break;
		case StoreButtonFunction.TestMoreThanADay:
			logic.TestMoreThanADay();
			break;
		case StoreButtonFunction.HideInternetFailPanel:
			logic.ShowInternetFailedPanel(false);
			break;
		case StoreButtonFunction.ShowDailyRewardsPanel:
			logic.ShowDailyRewardsPanel(true);
			break;
		case StoreButtonFunction.HideDailyRewardsPanel:
			logic.ShowDailyRewardsPanel(false);
			break;
		case StoreButtonFunction.ShowEarnedCar:
			logic.ShowEarnedCar();
			break;
		case StoreButtonFunction.BackToPackStore:
			logic.BackToPackStore();
			break;
		case StoreButtonFunction.RightArrow:
			logic.SelectCar(1);
			break;
		case StoreButtonFunction.LeftArrow:
			logic.SelectCar(-1);
			break;
		case StoreButtonFunction.DownArrow:
			logic.UpDownArrow(-1);
			break;
		case StoreButtonFunction.UpArrow:
			logic.UpDownArrow(1);
			break;
		}
	}
}
