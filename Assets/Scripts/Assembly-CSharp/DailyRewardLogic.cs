using System;
using UnityEngine;

public class DailyRewardLogic : MonoBehaviour
{
	public const string KEY_DAILY_REWARD_DAY = "keyDailyRewardDay";

	public const string KEY_LAST_TIME_DAILY_REWARD_CLAIMED = "keyLastDailyRewardClaimed";

	public const string KEY_FIRST_TIME_LAST_DAY_DAILY_REWARD = "keyFirstTimeLastDayDayliReward";

	public const int KEY_TOTAL_REWARDS = 5;

	public GUIStoreLogic guiStoreLogic;

	private void OnEnable()
	{
		UpdateDailyReward();
		guiStoreLogic.UpdateCredits();
	}

	public bool UpdateDailyReward(int test = 0)
	{
		if (MySingleton<Manager>.Instance.timeManager.HasTimeFromInternet())
		{
			guiStoreLogic.lbDailyRewardsInfo.text = Localization.Get("come_every_day");
			DateTime time = SettingsManager.GetTime("keyLastDailyRewardClaimed");
			DateTime dateTime = MySingleton<Manager>.Instance.timeManager.GetTimeFromInternetPlusLocalTimenZone();
			if (test < 0)
			{
				dateTime = dateTime.AddDays(0.95);
			}
			if (test > 0)
			{
				dateTime = dateTime.AddDays(2.0);
			}
			double totalHours = dateTime.Date.Subtract(time.Date).TotalHours;
			if (time.Ticks > 1 && totalHours > 24.0)
			{
				guiStoreLogic.flagReset = true;
				Reset();
				CanCollect(SettingsManager.GetDayForDailyReward());
			}
			else
			{
				if (time.DayOfYear != dateTime.DayOfYear)
				{
					CanCollect(SettingsManager.GetDayForDailyReward());
					return true;
				}
				CannotCollect(SettingsManager.GetDayForDailyReward());
			}
		}
		else
		{
			guiStoreLogic.ShowInternetFailedPanel(true);
			guiStoreLogic.lbDailyRewardsInfo.text = Localization.Get("check_internet");
		}
		return false;
	}

	private void CanCollect(int day)
	{
		guiStoreLogic.CanCollectLogic(day);
	}

	private void CannotCollect(int day)
	{
		guiStoreLogic.CannotCollectLogic(day);
	}

	public void Reset(bool lastDay = false)
	{
		guiStoreLogic.ResetLogic(lastDay);
		SettingsManager.SetDayForDailyReward(0);
		MySingleton<Manager>.Instance.taskManager.taskDaily = MySingleton<Manager>.Instance.taskManager.InitTaskDaily();
		guiStoreLogic.UpdateRewards();
	}

	public void ClaimDay()
	{
		int dayForDailyReward = SettingsManager.GetDayForDailyReward();
		switch (dayForDailyReward)
		{
		case 0:
			Unibiller.CreditBalance("credits", 50m);
			guiStoreLogic.ShowPanelInfoReward(50m);
			MySingleton<Manager>.Instance.taskManager.taskDaily.Reset(-1f);
			break;
		case 1:
			Unibiller.CreditBalance("credits", 100m);
			guiStoreLogic.ShowPanelInfoReward(100m);
			MySingleton<Manager>.Instance.taskManager.taskDaily.Reset(-1f);
			break;
		case 2:
			Unibiller.CreditBalance("credits", 150m);
			guiStoreLogic.ShowPanelInfoReward(150m);
			MySingleton<Manager>.Instance.taskManager.taskDaily.Reset(-1f);
			break;
		case 3:
			Unibiller.CreditBalance("credits", 300m);
			guiStoreLogic.ShowPanelInfoReward(300m);
			MySingleton<Manager>.Instance.taskManager.taskDaily.Reset(-1f);
			break;
		case 4:
			if (SettingsManager.IsFirstTimeLastDayReward("keyFirstTimeLastDayDayliReward") && !SettingsManager.IsCarBought("keyCar" + guiStoreLogic.dailyRewardCarID))
			{
				SettingsManager.SetFirstTimeLastDayReward("keyFirstTimeLastDayDayliReward", false);
				guiStoreLogic.UpdateRewards();
				guiStoreLogic.ShowPanelInfoReward(-1m);
				SettingsManager.SetCarBought("keyCar" + guiStoreLogic.dailyRewardCarID, true);
				guiStoreLogic.btOkInfoReward.GetComponent<GUIStoreButton>().function = GUIStoreButton.StoreButtonFunction.ShowEarnedCar;
				ManagerAnalytics.instance.FireCarUnlocked(MySingleton<Manager>.Instance.GetCarNameIDFromID(guiStoreLogic.dailyRewardCarID), "daily_reward");
			}
			else
			{
				int num = UnityEngine.Random.Range(500, 700);
				Unibiller.CreditBalance("credits", num);
				guiStoreLogic.ShowPanelInfoReward(num);
			}
			break;
		}
		ManagerAnalytics.instance.FireRetentionClaim("daily", (dayForDailyReward + 1).ToString());
		guiStoreLogic.UpdateCredits();
		if (dayForDailyReward >= 5)
		{
			Reset(true);
		}
		else
		{
			NextDay(dayForDailyReward);
		}
	}

	private void NextDay(int currentDay)
	{
		SettingsManager.SetDayForDailyReward(currentDay + 1);
		SettingsManager.SetTime("keyLastDailyRewardClaimed", MySingleton<Manager>.Instance.timeManager.GetTimeFromInternetPlusLocalTimenZone());
		MySingleton<Manager>.Instance.taskManager.taskDaily = MySingleton<Manager>.Instance.taskManager.InitTaskDaily();
		UpdateDailyReward();
	}

	public void TestAlmostADay()
	{
		UpdateDailyReward(-1);
	}

	public void TestMoreThanADay()
	{
		UpdateDailyReward(2);
	}
}
