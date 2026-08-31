using System;
using System.Collections.Generic;
using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class StatsManager : MonoBehaviour
{
	private const float TIME_FOR_SAVE_STATS = 5f;

	private const string OLD_SUV_COLLECTABLES_FLAG = "oldSuvCollectablesFlag";

	private const string KEY_DIST_OFFER = "keyDistanceOffer";

	public const int TOTAL_TRIAL_CHALLENGES = 4;

	public const int TOTAL_RADAR_CHALLENGES = 4;

	public const int TOTAL_CRASH_CHALLENGES = 4;

	public const string TRIAL_CHALLENGE_NAMEBASE_KEY = "chgBest_Trial__2Game_Trial_0";

	public const string RADAR_CHALLENGE_NAMEBASE_KEY = "chgBest_Radar__2Game_Radar_0";

	public const string CRASHABLES_CHALLENGE_NAMEBASE_KEY = "chgBest_Crashable__2Game_Crashable_0";

	private bool AllTrialChallengesCompleted;

	private bool AllRadarChallengesCompleted;

	private bool AllCrashChallengesCompleted;

	private bool AllChallengesComplete;

	public int completeChallenges;

	public int metersDrove;

	public int carId;

	public int startingTotalDistance;

	public int startingIdDistance;

	public int startingOfferDistance;

	private int startingTotalDistanceAux;

	public bool allCollected;

	[HideInInspector]
	public Dictionary<Collectables.CollectableType, List<int>> collectablesCounter;

	[HideInInspector]
	public Dictionary<Collectables.CollectableType, List<int>> maxCollectables;

	public Dictionary<Collectables.CollectableType, List<int>> collectablesOldSUV;

	public bool AllTrialsCompleted
	{
		get
		{
			return AllTrialChallengesCompleted;
		}
	}

	public bool AllRadarsCompleted
	{
		get
		{
			return AllRadarChallengesCompleted;
		}
	}

	public bool AllCrasheablesCompleted
	{
		get
		{
			return AllCrashChallengesCompleted;
		}
	}

	public bool AllChallengesCompleted
	{
		get
		{
			return AllChallengesComplete;
		}
	}

	private void Awake()
	{
		carId = MySingleton<Manager>.Instance.indexCurrentCar;
		collectablesCounter = new Dictionary<Collectables.CollectableType, List<int>>();
		maxCollectables = new Dictionary<Collectables.CollectableType, List<int>>();
	}

	private void OnEnable()
	{
		UpdateDataFromPlayerPrefs();
		DistanceInitData();
	}

	private void OnApplicationPause()
	{
		SaveDataIntoPlayerPrefs();
	}

	private void OnDisable()
	{
		CancelInvoke("SaveDataIntoPlayerPrefs");
		SaveDataIntoPlayerPrefs();
	}

	private void UpdateDataFromPlayerPrefs()
	{
		metersDrove = SettingsManager.GetMetersDrove();
		InitCollectables();
	}

	public void SaveDataIntoPlayerPrefs()
	{
		SettingsManager.SetMetersDrove(metersDrove);
		SettingsManager.SetDistanceDrove(carId.ToString(), GetDistanceDrove());
		SettingsManager.SetDistanceDrove("keyDistanceOffer", GetDistanceOffer());
	}

	private void InitCollectables()
	{
		if (MySingleton<Manager>.Instance.selectedGame == Manager.SelectedGame.ExtremeCarDriving)
		{
			int[] pickedCollectablesIdArray = SettingsManager.GetPickedCollectablesIdArray();
			if (pickedCollectablesIdArray.Length > 0)
			{
				List<int> list = new List<int>();
				int[] array = pickedCollectablesIdArray;
				foreach (int item in array)
				{
					list.Add(item);
				}
				collectablesCounter.Add(Collectables.CollectableType.Baylon, list);
				SettingsManager.SetCollectablesStatus(collectablesCounter);
				SettingsManager.DeleteOldCollectables();
			}
		}
		if (MySingleton<Manager>.Instance.selectedGame == Manager.SelectedGame.ExtremeSUVDriving)
		{
			collectablesOldSUV = SettingsManager.GetCollectablesStatus();
			if (!ObscuredPrefs.HasKey("oldSuvCollectablesFlag"))
			{
				if (collectablesOldSUV.ContainsKey(Collectables.CollectableType.Rubito))
				{
					List<int> list2 = collectablesOldSUV[Collectables.CollectableType.Rubito];
					List<int> list3 = new List<int>();
					foreach (int item2 in list2)
					{
						list3.Add(item2);
					}
					collectablesCounter.Add(Collectables.CollectableType.Rubito, list3);
				}
				if (collectablesOldSUV.ContainsKey(Collectables.CollectableType.Raptor))
				{
					List<int> list4 = collectablesOldSUV[Collectables.CollectableType.Raptor];
					List<int> list5 = new List<int>();
					foreach (int item3 in list4)
					{
						list5.Add(item3);
					}
					collectablesCounter.Add(Collectables.CollectableType.Traptor, list5);
				}
				SettingsManager.SetCollectablesStatus(collectablesCounter);
				ObscuredPrefs.SetBool("oldSuvCollectablesFlag", true);
			}
		}
		collectablesCounter = SettingsManager.GetCollectablesStatus();
	}

	public void IncrementMetersDrove(float deltaMeters)
	{
		metersDrove += Mathf.RoundToInt(deltaMeters);
	}

	public bool AllCollectedForThisCar(InfoCar infoCar)
	{
		return AllCollected(infoCar.collectableType, infoCar);
	}

	private bool AllCollected(Collectables.CollectableType collectableType, InfoCar infoCar = null)
	{
		if (collectablesCounter.ContainsKey(collectableType))
		{
			int count = collectablesCounter[collectableType].Count;
			int num = -1;
			num = ((!(infoCar == null)) ? infoCar.quantity : maxCollectables[collectableType].Count);
			return count == num;
		}
		return false;
	}

	public int GetCollectablesRemainingForThisCar(InfoCar infoCar)
	{
		return GetCollectablesRemaining(infoCar.collectableType, infoCar);
	}

	public int GetCollectablesRemaining(Collectables.CollectableType collectableType, InfoCar infoCar = null)
	{
		int num = 0;
		if (collectablesCounter.ContainsKey(collectableType))
		{
			num = collectablesCounter[collectableType].Count;
		}
		if (infoCar == null)
		{
			int count = maxCollectables[collectableType].Count;
			return count - num;
		}
		return infoCar.quantity - num;
	}

	public void DistanceInitData()
	{
		startingTotalDistance = SettingsManager.GetMetersDrove();
		startingIdDistance = SettingsManager.GetDistanceDrove(carId.ToString());
		startingOfferDistance = SettingsManager.GetDistanceDrove("keyDistanceOffer");
		startingTotalDistanceAux = startingTotalDistance;
	}

	public int GetDistanceDrove()
	{
		return metersDrove - startingTotalDistance + startingIdDistance;
	}

	public int GetDistanceMilestone()
	{
		if (metersDrove > 1000000)
		{
			return 1000;
		}
		if (metersDrove > 500000)
		{
			return 500;
		}
		if (metersDrove > 250000)
		{
			return 250;
		}
		if (metersDrove > 100000)
		{
			return 100;
		}
		if (metersDrove > 50000)
		{
			return 50;
		}
		if (metersDrove > 25000)
		{
			return 25;
		}
		if (metersDrove > 10000)
		{
			return 10;
		}
		if (metersDrove > 5000)
		{
			return 5;
		}
		return 0;
	}

	public int GetDistanceOffer()
	{
		if (MySingleton<Manager>.Instance.ads.disabledAds)
		{
			return 0;
		}
		if (SettingsManager.IsRemoveAdsOffer())
		{
			startingTotalDistanceAux = metersDrove;
			return 0;
		}
		if (SettingsManager.IsSecondRemoveAdsOffer() && SettingsManager.IsFirstRemoveAdsOffer())
		{
			return 0;
		}
		if (!SettingsManager.IsFirstRemoveAdsOffer())
		{
			if (metersDrove - startingTotalDistance + startingOfferDistance > 50000)
			{
				Debug.Log("NOW FIRST OFFER");
				startingTotalDistanceAux = metersDrove;
				SettingsManager.SetFirstRemoveAdsOffer(true);
				SettingsManager.SetRemoveAdsOffer(true);
				return 0;
			}
			return metersDrove - startingTotalDistance + startingOfferDistance;
		}
		if (metersDrove - startingTotalDistanceAux + startingOfferDistance > 20000)
		{
			Debug.Log("NOW SECOND OFFER");
			SettingsManager.SetSecondRemoveAdsOffer(true);
			SettingsManager.SetRemoveAdsOffer(true);
			return 0;
		}
		return metersDrove - startingTotalDistanceAux + startingOfferDistance;
	}

	public void RefreshChallengesCompleted()
	{
		float[] array = new float[4];
		float[] array2 = new float[4];
		float[] array3 = new float[4];
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		for (int i = 1; i <= 4; i++)
		{
			string key = "chgBest_Trial__2Game_Trial_0" + i;
			float num4 = ObscuredPrefs.GetFloat(key);
			if (num4 > 0f)
			{
				num++;
			}
		}
		for (int j = 1; j <= 4; j++)
		{
			string key2 = "chgBest_Radar__2Game_Radar_0" + j;
			float num5 = ObscuredPrefs.GetFloat(key2);
			if (num5 > 0f)
			{
				num2++;
			}
		}
		for (int k = 1; k <= 4; k++)
		{
			string key3 = "chgBest_Crashable__2Game_Crashable_0" + k;
			float num6 = ObscuredPrefs.GetInt(key3);
			if (num6 >= (float)CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE)
			{
				num3++;
			}
		}
		completeChallenges = num + num2 + num3;
		if (num >= 4)
		{
			AllTrialChallengesCompleted = true;
		}
		if (num2 >= 4)
		{
			AllRadarChallengesCompleted = true;
		}
		if (num3 >= 4)
		{
			AllCrashChallengesCompleted = true;
		}
		if (AllTrialChallengesCompleted == AllRadarChallengesCompleted == AllCrashChallengesCompleted)
		{
			AllChallengesComplete = true;
		}
	}

	public int GetCompleteChallenges()
	{
		RefreshChallengesCompleted();
		return completeChallenges;
	}

	public int GetUnlockedCars()
	{
		int num = 0;
		InfoCar[] carsInfoPrefabs = MySingleton<Manager>.Instance.carsInfoPrefabs;
		foreach (InfoCar infoCar in carsInfoPrefabs)
		{
			if (infoCar.isBought)
			{
				num++;
			}
		}
		return num;
	}

	public int GetNumPlayedDays()
	{
		return ObscuredPrefs.GetInt("KEY_PLAYED_DAYS", 0);
	}

	public void ChekPlayedDays(DateTime currentDateTime, DateTime lastDateTime)
	{
		if (currentDateTime.DayOfYear != lastDateTime.DayOfYear)
		{
			ObscuredPrefs.SetInt("KEY_PLAYED_DAYS", ObscuredPrefs.GetInt("KEY_PLAYED_DAYS", 0) + 1);
		}
	}

	public int GetNumPacksBought()
	{
		return ObscuredPrefs.GetInt("KEY_CAR_PACKS_BOUGHT", 0);
	}

	public void AddCarPackBought()
	{
		ObscuredPrefs.SetInt("KEY_CAR_PACKS_BOUGHT", ObscuredPrefs.GetInt("KEY_CAR_PACKS_BOUGHT", 0) + 1);
	}
}
