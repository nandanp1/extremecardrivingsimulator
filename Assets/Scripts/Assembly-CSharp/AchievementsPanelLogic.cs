using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class AchievementsPanelLogic : MonoBehaviour
{
	public const int TOTAL_TRIAL_CHALLENGES = 4;

	public const int TOTAL_RADAR_CHALLENGES = 4;

	public const int TOTAL_CRASHABLES_CHALLENGES = 4;

	public const string TRIAL_NAME = "challange_trial_name";

	public const string RADAR_NAME = "challange_radar_name";

	public const string CRASH_NAME = "challange_crashable_name";

	public const string TRIAL_NAME_ICON = "icon_trialchagenge";

	public const string RADAR_NAME_ICON = "icon_radarchagenge";

	public const string CRASH_NAME_ICON = "icon_crasheablechagenge";

	public const string TRIAL_CHALLENGE_NAMEBASE_KEY = "chgBest_Trial__2Game_Trial_0";

	public const string RADAR_CHALLENGE_NAMEBASE_KEY = "chgBest_Radar__2Game_Radar_0";

	public const string CRASHABLES_CHALLENGE_NAMEBASE_KEY = "chgBest_Crashable__2Game_Crashable_0";

	public const string COLLECTION_CATEGORY_NAME = "collection_general";

	public const string CITY_ECDS_CATEGORY_NAME = "collection_city";

	public const string FOREST_ECDS_CATEGORY_NAME = "collection_forest";

	public const string AIRPORT_ECDS_CATEGORY_NAME = "collection_airport";

	public const string DESERT_SUV_CATEGORY_NAME = "collection_desert_suv";

	public const string MOUNTAIN_SUV_CATEGORY_NAME = "collection_mountain_suv";

	public const string FOREST_SUV_CATEGORY_NAME = "collection_forest_suv";

	public string tempCategory_1 = string.Empty;

	public string tempCategory_2 = string.Empty;

	public string tempCategory_3 = string.Empty;

	public UIAchievementsBehaviour UIref;

	private InfoCar trialCarReward;

	private InfoCar radarCarReward;

	private InfoCar crashableCarReward;

	private int trialChallengesTotal;

	private int trialChallengesCompleted;

	private int radarChallengesTotal;

	private int radarChallengesCompleted;

	private int crashChallengesTotal;

	private int crashChallengesCompleted;

	private List<string> trialLeaderboards;

	private List<string> radarLeaderboards;

	private List<string> crashLeaderboards;

	private bool initialize;

	private void OnEnable()
	{
		trialLeaderboards = new List<string>();
		string lEADERBOARDID_TRIAL_ = ChallangeManager.LEADERBOARDID_TRIAL_1;
		string lEADERBOARDID_TRIAL_2 = ChallangeManager.LEADERBOARDID_TRIAL_2;
		string lEADERBOARDID_TRIAL_3 = ChallangeManager.LEADERBOARDID_TRIAL_3;
		string lEADERBOARDID_TRIAL_4 = ChallangeManager.LEADERBOARDID_TRIAL_4;
		trialLeaderboards.Add(lEADERBOARDID_TRIAL_);
		trialLeaderboards.Add(lEADERBOARDID_TRIAL_2);
		trialLeaderboards.Add(lEADERBOARDID_TRIAL_3);
		trialLeaderboards.Add(lEADERBOARDID_TRIAL_4);
		radarLeaderboards = new List<string>();
		string lEADERBOARDID_RADAR_ = ChallangeManager.LEADERBOARDID_RADAR_1;
		string lEADERBOARDID_RADAR_2 = ChallangeManager.LEADERBOARDID_RADAR_2;
		string lEADERBOARDID_RADAR_3 = ChallangeManager.LEADERBOARDID_RADAR_3;
		string lEADERBOARDID_RADAR_4 = ChallangeManager.LEADERBOARDID_RADAR_4;
		radarLeaderboards.Add(lEADERBOARDID_RADAR_);
		radarLeaderboards.Add(lEADERBOARDID_RADAR_2);
		radarLeaderboards.Add(lEADERBOARDID_RADAR_3);
		radarLeaderboards.Add(lEADERBOARDID_RADAR_4);
		crashLeaderboards = new List<string>();
		string lEADERBOARDID_CRASHABLE_ = ChallangeManager.LEADERBOARDID_CRASHABLE_1;
		string lEADERBOARDID_CRASHABLE_2 = ChallangeManager.LEADERBOARDID_CRASHABLE_2;
		string lEADERBOARDID_CRASHABLE_3 = ChallangeManager.LEADERBOARDID_CRASHABLE_3;
		string lEADERBOARDID_CRASHABLE_4 = ChallangeManager.LEADERBOARDID_CRASHABLE_4;
		crashLeaderboards.Add(lEADERBOARDID_CRASHABLE_);
		crashLeaderboards.Add(lEADERBOARDID_CRASHABLE_2);
		crashLeaderboards.Add(lEADERBOARDID_CRASHABLE_3);
		crashLeaderboards.Add(lEADERBOARDID_CRASHABLE_4);
		ReadCollectablesProgress();
		if (!initialize)
		{
			switch (MySingleton<Manager>.Instance.selectedGame)
			{
			case Manager.SelectedGame.ExtremeCarDriving:
				tempCategory_1 = "collection_city";
				tempCategory_2 = "collection_forest";
				tempCategory_3 = "collection_airport";
				break;
			case Manager.SelectedGame.ExtremeSUVDriving:
				tempCategory_1 = "collection_forest_suv";
				tempCategory_2 = "collection_desert_suv";
				tempCategory_3 = "collection_mountain_suv";
				break;
			case Manager.SelectedGame.SanFrancisco:
				tempCategory_1 = "collection_city";
				break;
			}
			List<List<ChallengeEntryInfo>> challengeScores = GetChallengeScores();
			List<CarEntryInfo> carsInfo = GetCarsInfo();
			challengeScores.Clear();
			challengeScores = GetChallengeScores();
			List<List<CollectablesEntryInfo>> collectables = GetCollectables();
			List<List<AchievementsEntryInfo>> achievements = GetAchievements();
			UIref.Init(carsInfo, challengeScores, collectables, achievements);
			initialize = true;
		}
	}

	private List<CarEntryInfo> GetCarsInfo()
	{
		List<CarEntryInfo> list = new List<CarEntryInfo>();
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		foreach (InfoCar infoCar in carsInfoPrefabs)
		{
			if (infoCar.isBought)
			{
				CarEntryInfo carEntryInfo = new CarEntryInfo();
				carEntryInfo.carIconName = infoCar.iconName;
				carEntryInfo.carID = infoCar.id;
				carEntryInfo.infoCar = infoCar;
				carEntryInfo.entryView = CarEntryView.Unlocked;
				carEntryInfo.unlockInfo[0].completed = true;
				list.Add(carEntryInfo);
			}
			else if (infoCar.collectableType == Collectables.CollectableType.None && infoCar.price > 0f)
			{
				CarEntryInfo carEntryInfo = new CarEntryInfo();
				carEntryInfo.carIconName = infoCar.iconName;
				carEntryInfo.carID = infoCar.id;
				carEntryInfo.infoCar = infoCar;
				carEntryInfo.entryView = CarEntryView.OneChallenge;
				carEntryInfo.unlockInfo[0].completed = false;
				carEntryInfo.unlockInfo[0].challengeKind = Challenge.Distance;
				carEntryInfo.unlockInfo[0].challengeTarget = infoCar.price;
				carEntryInfo.unlockInfo[0].challengePogress = MySingleton<Manager>.Instance.stats.metersDrove;
				list.Add(carEntryInfo);
			}
			else if (infoCar.collectableType != Collectables.CollectableType.None && infoCar.price > 0f)
			{
				CarEntryInfo carEntryInfo = new CarEntryInfo(2);
				carEntryInfo.carIconName = infoCar.iconName;
				carEntryInfo.carID = infoCar.id;
				carEntryInfo.infoCar = infoCar;
				carEntryInfo.entryView = CarEntryView.TwoChallenges;
				carEntryInfo.unlockInfo[0].completed = false;
				carEntryInfo.unlockInfo[0].challengeKind = Challenge.Distance;
				carEntryInfo.unlockInfo[0].challengeTarget = infoCar.price;
				carEntryInfo.unlockInfo[0].challengePogress = MySingleton<Manager>.Instance.stats.metersDrove;
				carEntryInfo.unlockInfo[1].completed = false;
				carEntryInfo.unlockInfo[1].challengeKind = Challenge.Collectables;
				carEntryInfo.unlockInfo[1].challengePogress = infoCar.quantity - MySingleton<Manager>.Instance.stats.GetCollectablesRemainingForThisCar(infoCar);
				carEntryInfo.unlockInfo[1].challengeTarget = infoCar.quantity;
				list.Add(carEntryInfo);
			}
			else if (infoCar.isChallangeReward && infoCar.purchasablePack != InfoCar.PurchasablePack.none)
			{
				CarEntryInfo carEntryInfo = new CarEntryInfo(2);
				carEntryInfo.carIconName = infoCar.iconName;
				carEntryInfo.carID = infoCar.id;
				carEntryInfo.infoCar = infoCar;
				carEntryInfo.entryView = CarEntryView.BuyPackAndChallenge;
				carEntryInfo.unlockInfo[0].carPrice = Convert.ToInt32(5500m);
				int playerMoney = (int)Unibiller.GetCurrencyBalance("credits");
				carEntryInfo.unlockInfo[0].playerMoney = playerMoney;
				switch (infoCar.challegeRewardType)
				{
				case InfoCar.ChallengeRewardType.TRIAL:
					carEntryInfo.unlockInfo[1].challengesCompleted = trialChallengesCompleted;
					carEntryInfo.unlockInfo[1].challengesTotal = trialChallengesTotal;
					carEntryInfo.unlockInfo[1].iconUnlockOptionName = "icon_trialchagenge";
					break;
				case InfoCar.ChallengeRewardType.RADAR:
					carEntryInfo.unlockInfo[1].challengesCompleted = radarChallengesCompleted;
					carEntryInfo.unlockInfo[1].challengesTotal = radarChallengesTotal;
					carEntryInfo.unlockInfo[1].iconUnlockOptionName = "icon_radarchagenge";
					break;
				case InfoCar.ChallengeRewardType.CRASHABLE:
					carEntryInfo.unlockInfo[1].challengesCompleted = crashChallengesCompleted;
					carEntryInfo.unlockInfo[1].challengesTotal = crashChallengesTotal;
					carEntryInfo.unlockInfo[1].iconUnlockOptionName = "icon_crasheablechagenge";
					break;
				}
				list.Add(carEntryInfo);
			}
			else if (infoCar.purchasable && infoCar.purchasablePack != InfoCar.PurchasablePack.none)
			{
				CarEntryInfo carEntryInfo = new CarEntryInfo();
				carEntryInfo.carIconName = infoCar.iconName;
				carEntryInfo.carID = infoCar.id;
				carEntryInfo.infoCar = infoCar;
				carEntryInfo.entryView = CarEntryView.BuyPack;
				carEntryInfo.unlockInfo[0].carPrice = Convert.ToInt32(3500m);
				int playerMoney2 = (int)Unibiller.GetCurrencyBalance("credits");
				carEntryInfo.unlockInfo[0].playerMoney = playerMoney2;
				list.Add(carEntryInfo);
			}
			else if (infoCar.purchasable && infoCar.purchasablePack == InfoCar.PurchasablePack.none)
			{
				CarEntryInfo carEntryInfo = new CarEntryInfo();
				carEntryInfo.carIconName = infoCar.iconName;
				carEntryInfo.carID = infoCar.id;
				carEntryInfo.infoCar = infoCar;
				carEntryInfo.entryView = CarEntryView.BuyOption;
				carEntryInfo.unlockInfo[0].carPrice = Convert.ToInt32(3500m);
				int playerMoney3 = (int)Unibiller.GetCurrencyBalance("credits");
				carEntryInfo.unlockInfo[0].playerMoney = playerMoney3;
				list.Add(carEntryInfo);
			}
			else if (infoCar.purchasablePack != InfoCar.PurchasablePack.none)
			{
				CarEntryInfo carEntryInfo = new CarEntryInfo();
				carEntryInfo.carIconName = infoCar.iconName;
				carEntryInfo.carID = infoCar.id;
				carEntryInfo.infoCar = infoCar;
				carEntryInfo.entryView = CarEntryView.BuyPack;
				carEntryInfo.unlockInfo[0].carPrice = Convert.ToInt32(5500m);
				int playerMoney4 = (int)Unibiller.GetCurrencyBalance("credits");
				carEntryInfo.unlockInfo[0].playerMoney = playerMoney4;
				list.Add(carEntryInfo);
			}
			else if (!infoCar.purchasable && infoCar.isChallangeReward)
			{
				CarEntryInfo carEntryInfo = new CarEntryInfo();
				carEntryInfo.carIconName = infoCar.iconName;
				carEntryInfo.carID = infoCar.id;
				carEntryInfo.infoCar = infoCar;
				carEntryInfo.entryView = CarEntryView.BuyOnlyChallenge;
				switch (infoCar.challegeRewardType)
				{
				case InfoCar.ChallengeRewardType.TRIAL:
					carEntryInfo.unlockInfo[0].challengesCompleted = trialChallengesCompleted;
					carEntryInfo.unlockInfo[0].challengesTotal = trialChallengesTotal;
					carEntryInfo.unlockInfo[0].iconUnlockOptionName = "icon_trialchagenge";
					break;
				case InfoCar.ChallengeRewardType.RADAR:
					carEntryInfo.unlockInfo[0].challengesCompleted = radarChallengesCompleted;
					carEntryInfo.unlockInfo[0].challengesTotal = radarChallengesTotal;
					carEntryInfo.unlockInfo[0].iconUnlockOptionName = "icon_radarchagenge";
					break;
				case InfoCar.ChallengeRewardType.CRASHABLE:
					carEntryInfo.unlockInfo[0].challengesCompleted = crashChallengesCompleted;
					carEntryInfo.unlockInfo[0].challengesTotal = crashChallengesTotal;
					carEntryInfo.unlockInfo[0].iconUnlockOptionName = "icon_crasheablechagenge";
					break;
				}
				list.Add(carEntryInfo);
			}
			else
			{
				Debug.LogError("CASO NO CONTEMPLADO - Este coche no se mostrara: " + infoCar.idName);
			}
			if (infoCar.isChallangeReward)
			{
				switch (infoCar.challegeRewardType)
				{
				case InfoCar.ChallengeRewardType.TRIAL:
					trialCarReward = infoCar;
					break;
				case InfoCar.ChallengeRewardType.RADAR:
					radarCarReward = infoCar;
					break;
				case InfoCar.ChallengeRewardType.CRASHABLE:
					crashableCarReward = infoCar;
					break;
				}
			}
		}
		return list;
	}

	private List<List<ChallengeEntryInfo>> GetChallengeScores()
	{
		float[] array = new float[4];
		float[] array2 = new float[4];
		float[] array3 = new float[4];
		List<List<ChallengeEntryInfo>> list = new List<List<ChallengeEntryInfo>>();
		List<ChallengeEntryInfo> list2 = new List<ChallengeEntryInfo>();
		for (int i = 1; i <= 4; i++)
		{
			ChallengeEntryInfo challengeEntryInfo = new ChallengeEntryInfo();
			string key = "chgBest_Trial__2Game_Trial_0" + i;
			float num = ObscuredPrefs.GetFloat(key);
			challengeEntryInfo.challengeCategory = ChallengeKind.Trial;
			if (num > 0f)
			{
				challengeEntryInfo.challengeCompleted = true;
				trialChallengesCompleted++;
			}
			else
			{
				challengeEntryInfo.challengeCompleted = false;
			}
			trialChallengesTotal++;
			challengeEntryInfo.challengeName = Localization.Get("challange_trial_name") + " " + i;
			challengeEntryInfo.challengeCategoryIconName = "icon_trialchagenge";
			challengeEntryInfo.challengeScore = num;
			challengeEntryInfo.carRewardInfo = trialCarReward;
			challengeEntryInfo.challengeLeaderboardKey = trialLeaderboards[i - 1];
			list2.Add(challengeEntryInfo);
		}
		List<ChallengeEntryInfo> list3 = new List<ChallengeEntryInfo>();
		for (int j = 1; j <= 4; j++)
		{
			ChallengeEntryInfo challengeEntryInfo = new ChallengeEntryInfo();
			string key2 = "chgBest_Radar__2Game_Radar_0" + j;
			float num2 = ObscuredPrefs.GetFloat(key2);
			challengeEntryInfo.challengeCategory = ChallengeKind.Radar;
			if (num2 > 0f)
			{
				challengeEntryInfo.challengeCompleted = true;
				radarChallengesCompleted++;
			}
			else
			{
				challengeEntryInfo.challengeCompleted = false;
			}
			radarChallengesTotal++;
			challengeEntryInfo.challengeName = Localization.Get("challange_radar_name") + " " + j;
			challengeEntryInfo.challengeCategoryIconName = "icon_radarchagenge";
			challengeEntryInfo.challengeScore = num2;
			challengeEntryInfo.carRewardInfo = radarCarReward;
			challengeEntryInfo.challengeLeaderboardKey = radarLeaderboards[j - 1];
			list3.Add(challengeEntryInfo);
		}
		List<ChallengeEntryInfo> list4 = new List<ChallengeEntryInfo>();
		for (int k = 1; k <= 4; k++)
		{
			ChallengeEntryInfo challengeEntryInfo = new ChallengeEntryInfo();
			string key3 = "chgBest_Crashable__2Game_Crashable_0" + k;
			float num3 = ObscuredPrefs.GetInt(key3);
			challengeEntryInfo.challengeCategory = ChallengeKind.Crash;
			if (num3 >= (float)CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE)
			{
				challengeEntryInfo.challengeCompleted = true;
				crashChallengesCompleted++;
			}
			else
			{
				challengeEntryInfo.challengeCompleted = false;
			}
			crashChallengesTotal++;
			challengeEntryInfo.challengeName = Localization.Get("challange_crashable_name") + " " + k;
			challengeEntryInfo.challengeCategoryIconName = "icon_crasheablechagenge";
			challengeEntryInfo.challengeScore = num3;
			challengeEntryInfo.carRewardInfo = crashableCarReward;
			challengeEntryInfo.challengeLeaderboardKey = crashLeaderboards[k - 1];
			list4.Add(challengeEntryInfo);
		}
		list.Add(list2);
		list.Add(list3);
		list.Add(list4);
		return list;
	}

	private List<List<CollectablesEntryInfo>> GetCollectables()
	{
		List<List<CollectablesEntryInfo>> list = new List<List<CollectablesEntryInfo>>();
		List<CollectablesEntryInfo> list2 = new List<CollectablesEntryInfo>();
		if (MySingleton<Manager>.Instance.collectablesCollection.Count != 0)
		{
			int count = MySingleton<Manager>.Instance.collectablesCollection.Count;
			CollectablesEntryInfo collectablesEntryInfo = new CollectablesEntryInfo();
			for (int i = 0; i < count; i++)
			{
				MySingleton<Manager>.Instance.collectablesCollection[i].countdata();
				collectablesEntryInfo.categoryName = Localization.Get("collection_general");
				collectablesEntryInfo.collectableIconName = MySingleton<Manager>.Instance.collectablesCollection[i].collectablesList[0].collectableIconName;
				collectablesEntryInfo.collectablesGot = MySingleton<Manager>.Instance.collectablesCollection[i].numberCollected;
				collectablesEntryInfo.collectablesTotal = MySingleton<Manager>.Instance.collectablesCollection[i].totalCollectables;
				list2.Add(collectablesEntryInfo);
				collectablesEntryInfo = new CollectablesEntryInfo();
			}
			list.Add(list2);
		}
		list2 = new List<CollectablesEntryInfo>();
		if (MySingleton<Manager>.Instance.collectablesZone1.Count != 0)
		{
			int count2 = MySingleton<Manager>.Instance.collectablesZone1.Count;
			CollectablesEntryInfo collectablesEntryInfo2 = new CollectablesEntryInfo();
			for (int j = 0; j < count2; j++)
			{
				MySingleton<Manager>.Instance.collectablesZone1[j].countdata();
				collectablesEntryInfo2.categoryName = Localization.Get(MySingleton<Manager>.Instance.collectablesZone1[j].categoryName);
				collectablesEntryInfo2.categoryIconName = MySingleton<Manager>.Instance.collectablesZone1[j].categoryIconName;
				collectablesEntryInfo2.collectableIconName = MySingleton<Manager>.Instance.collectablesZone1[j].collectablesList[0].collectableIconName;
				collectablesEntryInfo2.collectablesGot = MySingleton<Manager>.Instance.collectablesZone1[j].numberCollected;
				collectablesEntryInfo2.collectablesTotal = MySingleton<Manager>.Instance.collectablesZone1[j].totalCollectables;
				list2.Add(collectablesEntryInfo2);
				collectablesEntryInfo2 = new CollectablesEntryInfo();
			}
			list.Add(list2);
		}
		list2 = new List<CollectablesEntryInfo>();
		if (MySingleton<Manager>.Instance.collectablesZone2.Count != 0)
		{
			int count3 = MySingleton<Manager>.Instance.collectablesZone2.Count;
			CollectablesEntryInfo collectablesEntryInfo3 = new CollectablesEntryInfo();
			for (int k = 0; k < count3; k++)
			{
				MySingleton<Manager>.Instance.collectablesZone2[k].countdata();
				collectablesEntryInfo3.categoryName = Localization.Get(MySingleton<Manager>.Instance.collectablesZone2[k].categoryName);
				collectablesEntryInfo3.categoryIconName = MySingleton<Manager>.Instance.collectablesZone2[k].categoryIconName;
				collectablesEntryInfo3.collectableIconName = MySingleton<Manager>.Instance.collectablesZone2[k].collectablesList[0].collectableIconName;
				collectablesEntryInfo3.collectablesGot = MySingleton<Manager>.Instance.collectablesZone2[k].numberCollected;
				collectablesEntryInfo3.collectablesTotal = MySingleton<Manager>.Instance.collectablesZone2[k].totalCollectables;
				list2.Add(collectablesEntryInfo3);
				collectablesEntryInfo3 = new CollectablesEntryInfo();
			}
			list.Add(list2);
		}
		list2 = new List<CollectablesEntryInfo>();
		if (MySingleton<Manager>.Instance.collectablesZone3.Count != 0)
		{
			int count4 = MySingleton<Manager>.Instance.collectablesZone3.Count;
			CollectablesEntryInfo collectablesEntryInfo4 = new CollectablesEntryInfo();
			for (int l = 0; l < count4; l++)
			{
				MySingleton<Manager>.Instance.collectablesZone3[l].countdata();
				collectablesEntryInfo4.categoryName = Localization.Get(MySingleton<Manager>.Instance.collectablesZone3[l].categoryName);
				collectablesEntryInfo4.categoryIconName = MySingleton<Manager>.Instance.collectablesZone3[l].categoryIconName;
				collectablesEntryInfo4.collectableIconName = MySingleton<Manager>.Instance.collectablesZone3[l].collectablesList[0].collectableIconName;
				collectablesEntryInfo4.collectablesGot = MySingleton<Manager>.Instance.collectablesZone3[l].numberCollected;
				collectablesEntryInfo4.collectablesTotal = MySingleton<Manager>.Instance.collectablesZone3[l].totalCollectables;
				list2.Add(collectablesEntryInfo4);
				collectablesEntryInfo4 = new CollectablesEntryInfo();
			}
			list.Add(list2);
		}
		return list;
	}

	public void ReadCollectablesProgress()
	{
		foreach (CollectablesInfoList item in MySingleton<Manager>.Instance.collectablesCollection)
		{
			List<int> value = new List<int>();
			MySingleton<Manager>.Instance.stats.collectablesCounter.TryGetValue(item.collectablesList[0].collectableType, out value);
			foreach (CollectablesInfo collectables in item.collectablesList)
			{
				if (value != null && value.Contains(collectables.collectableID))
				{
					collectables.collected = true;
				}
			}
		}
		if (MySingleton<Manager>.Instance.collectablesZone1.Count != 0)
		{
			foreach (CollectablesInfoList item2 in MySingleton<Manager>.Instance.collectablesZone1)
			{
				List<int> value2 = new List<int>();
				MySingleton<Manager>.Instance.stats.collectablesCounter.TryGetValue(item2.collectablesList[0].collectableType, out value2);
				foreach (CollectablesInfo collectables2 in item2.collectablesList)
				{
					if (value2 != null && value2.Contains(collectables2.collectableID))
					{
						collectables2.collected = true;
					}
				}
			}
		}
		if (MySingleton<Manager>.Instance.collectablesZone2.Count != 0)
		{
			foreach (CollectablesInfoList item3 in MySingleton<Manager>.Instance.collectablesZone2)
			{
				List<int> value3 = new List<int>();
				MySingleton<Manager>.Instance.stats.collectablesCounter.TryGetValue(item3.collectablesList[0].collectableType, out value3);
				foreach (CollectablesInfo collectables3 in item3.collectablesList)
				{
					if (value3 != null && value3.Contains(collectables3.collectableID))
					{
						collectables3.collected = true;
					}
				}
			}
		}
		if (MySingleton<Manager>.Instance.collectablesZone3.Count == 0)
		{
			return;
		}
		foreach (CollectablesInfoList item4 in MySingleton<Manager>.Instance.collectablesZone3)
		{
			List<int> value4 = new List<int>();
			MySingleton<Manager>.Instance.stats.collectablesCounter.TryGetValue(item4.collectablesList[0].collectableType, out value4);
			foreach (CollectablesInfo collectables4 in item4.collectablesList)
			{
				if (value4 != null && value4.Contains(collectables4.collectableID))
				{
					collectables4.collected = true;
				}
			}
		}
	}

	private List<List<AchievementsEntryInfo>> GetAchievements()
	{
		List<List<AchievementsEntryInfo>> list = new List<List<AchievementsEntryInfo>>();
		foreach (int value in Enum.GetValues(typeof(AchievementManager.AchievementCategory)))
		{
			List<AchievementsEntryInfo> list2 = new List<AchievementsEntryInfo>();
			List<AchievementManager.Achievement> list3 = new List<AchievementManager.Achievement>();
			if (!MySingleton<Manager>.Instance.isMultiplayerAvailable && value == 0)
			{
				continue;
			}
			list3 = MySingleton<AchievementManager>.Instance.achievementsByCategory[(AchievementManager.AchievementCategory)value];
			foreach (AchievementManager.Achievement item in list3)
			{
				AchievementsEntryInfo achievementsEntryInfo = new AchievementsEntryInfo();
				achievementsEntryInfo.categoryName = Localization.Get(MySingleton<AchievementManager>.Instance.categoryNameDictionary[(AchievementManager.AchievementCategory)value]);
				int achievement = SettingsManager.GetAchievement(item.ToString());
				if (achievement == 1)
				{
					achievementsEntryInfo.completed = true;
				}
				else
				{
					achievementsEntryInfo.completed = false;
				}
				achievementsEntryInfo.description = Localization.Get(MySingleton<AchievementManager>.Instance.internDescriptionDictionary[item]);
				achievementsEntryInfo.reward = MySingleton<AchievementManager>.Instance.internMoneyDictionary[item];
				list2.Add(achievementsEntryInfo);
			}
			list.Add(list2);
		}
		return list;
	}
}
