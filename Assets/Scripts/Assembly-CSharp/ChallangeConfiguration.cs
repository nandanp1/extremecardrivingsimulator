using System;
using System.Collections.Generic;

public static class ChallangeConfiguration
{
	public static ChallangeSceneIndex[] GetChallangeCircuitsSceneIndex(ChallangeKey key)
	{
		switch (key)
		{
		case ChallangeKey.Trial:
			return new ChallangeSceneIndex[4]
			{
				ChallangeSceneIndex._2Game_Trial_01,
				ChallangeSceneIndex._2Game_Trial_02,
				ChallangeSceneIndex._2Game_Trial_03,
				ChallangeSceneIndex._2Game_Trial_04
			};
		case ChallangeKey.Radar:
			return new ChallangeSceneIndex[4]
			{
				ChallangeSceneIndex._2Game_Radar_01,
				ChallangeSceneIndex._2Game_Radar_02,
				ChallangeSceneIndex._2Game_Radar_03,
				ChallangeSceneIndex._2Game_Radar_04
			};
		case ChallangeKey.Crashable:
			return new ChallangeSceneIndex[4]
			{
				ChallangeSceneIndex._2Game_Crashable_01,
				ChallangeSceneIndex._2Game_Crashable_02,
				ChallangeSceneIndex._2Game_Crashable_03,
				ChallangeSceneIndex._2Game_Crashable_04
			};
		default:
			return new ChallangeSceneIndex[0];
		}
	}

	public static string GetChallangeLocalizationKey_Name(ChallangeKey key)
	{
		switch (key)
		{
		case ChallangeKey.Trial:
			return "challange_trial_name";
		case ChallangeKey.Radar:
			return "challange_radar_name";
		case ChallangeKey.Crashable:
			return "challange_crashable_name";
		default:
			return string.Empty;
		}
	}

	public static string GetChallangeLocalizationKey_Resume(ChallangeKey key)
	{
		switch (key)
		{
		case ChallangeKey.Trial:
			return "challange_trial_resume";
		case ChallangeKey.Radar:
			return "challange_radar_resume";
		case ChallangeKey.Crashable:
			return "challange_crashable_resume";
		default:
			return string.Empty;
		}
	}

	public static int GetChallangeCircuit_Index(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		switch (key)
		{
		case ChallangeKey.Trial:
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Trial_01:
				return 0;
			case ChallangeSceneIndex._2Game_Trial_02:
				return 1;
			case ChallangeSceneIndex._2Game_Trial_03:
				return 2;
			case ChallangeSceneIndex._2Game_Trial_04:
				return 3;
			default:
				return 0;
			}
		case ChallangeKey.Radar:
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Radar_01:
				return 0;
			case ChallangeSceneIndex._2Game_Radar_02:
				return 1;
			case ChallangeSceneIndex._2Game_Radar_03:
				return 2;
			case ChallangeSceneIndex._2Game_Radar_04:
				return 3;
			default:
				return 0;
			}
		case ChallangeKey.Crashable:
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Crashable_01:
				return 0;
			case ChallangeSceneIndex._2Game_Crashable_02:
				return 1;
			case ChallangeSceneIndex._2Game_Crashable_03:
				return 2;
			case ChallangeSceneIndex._2Game_Crashable_04:
				return 3;
			default:
				return 0;
			}
		default:
			return 0;
		}
	}

	public static int GetChallangeCircuit_Proportion(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		if (key == ChallangeKey.Trial)
		{
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Trial_01:
			case ChallangeSceneIndex._2Game_Trial_02:
			case ChallangeSceneIndex._2Game_Trial_03:
			case ChallangeSceneIndex._2Game_Trial_04:
			case ChallangeSceneIndex._2Game_Crashable_01:
			case ChallangeSceneIndex._2Game_Crashable_02:
			case ChallangeSceneIndex._2Game_Crashable_03:
			case ChallangeSceneIndex._2Game_Crashable_04:
				return 25;
			default:
				return 0;
			}
		}
		return 0;
	}

	public static float GetChallangeCircuit_Money(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		switch (key)
		{
		case ChallangeKey.Trial:
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Trial_01:
			case ChallangeSceneIndex._2Game_Trial_02:
			case ChallangeSceneIndex._2Game_Trial_03:
			case ChallangeSceneIndex._2Game_Trial_04:
				return 50f;
			default:
				return 0f;
			}
		case ChallangeKey.Radar:
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Radar_01:
			case ChallangeSceneIndex._2Game_Radar_02:
			case ChallangeSceneIndex._2Game_Radar_03:
			case ChallangeSceneIndex._2Game_Radar_04:
				return 50f;
			default:
				return 0f;
			}
		case ChallangeKey.Crashable:
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Crashable_01:
			case ChallangeSceneIndex._2Game_Crashable_02:
			case ChallangeSceneIndex._2Game_Crashable_03:
			case ChallangeSceneIndex._2Game_Crashable_04:
				return 50f;
			default:
				return 0f;
			}
		default:
			return 0f;
		}
	}

	public static string GetChallangeCircuit_LeaderboardId(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		switch (key)
		{
		case ChallangeKey.Trial:
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Trial_01:
				return ChallangeManager.LEADERBOARDID_TRIAL_1;
			case ChallangeSceneIndex._2Game_Trial_02:
				return ChallangeManager.LEADERBOARDID_TRIAL_2;
			case ChallangeSceneIndex._2Game_Trial_03:
				return ChallangeManager.LEADERBOARDID_TRIAL_3;
			case ChallangeSceneIndex._2Game_Trial_04:
				return ChallangeManager.LEADERBOARDID_TRIAL_4;
			default:
				throw new NotImplementedException("GetChallangeCircuit_LeaderboardId :: killo, te falta configurar esta parte");
			}
		case ChallangeKey.Radar:
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Radar_01:
				return ChallangeManager.LEADERBOARDID_RADAR_1;
			case ChallangeSceneIndex._2Game_Radar_02:
				return ChallangeManager.LEADERBOARDID_RADAR_2;
			case ChallangeSceneIndex._2Game_Radar_03:
				return ChallangeManager.LEADERBOARDID_RADAR_3;
			case ChallangeSceneIndex._2Game_Radar_04:
				return ChallangeManager.LEADERBOARDID_RADAR_4;
			default:
				throw new NotImplementedException("GetChallangeCircuit_LeaderboardId :: killo, te falta configurar esta parte");
			}
		case ChallangeKey.Crashable:
			switch (sceneIndex)
			{
			case ChallangeSceneIndex._2Game_Crashable_01:
				return ChallangeManager.LEADERBOARDID_CRASHABLE_1;
			case ChallangeSceneIndex._2Game_Crashable_02:
				return ChallangeManager.LEADERBOARDID_CRASHABLE_2;
			case ChallangeSceneIndex._2Game_Crashable_03:
				return ChallangeManager.LEADERBOARDID_CRASHABLE_3;
			case ChallangeSceneIndex._2Game_Crashable_04:
				return ChallangeManager.LEADERBOARDID_CRASHABLE_4;
			default:
				throw new NotImplementedException("GetChallangeCircuit_LeaderboardId :: killo, te falta configurar esta parte");
			}
		default:
			throw new NotImplementedException("GetChallangeCircuit_LeaderboardId :: killo, te falta configurar esta parte");
		}
	}

	public static string GetChallangeCircuitName(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		return string.Format("{0} {1}", Localization.Get(GetChallangeLocalizationKey_Name(key)), GetChallangeCircuit_Index(key, sceneIndex) + 1);
	}

	public static void InitializePlayerPrefs(Dictionary<ChallangePlayerPrefIdentifier, ChallangePlayerPref> playerPrefs)
	{
		AddPlayerPrefs(ChallangeKey.Trial, ChallangeSceneIndex._2Game_Trial_01, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Trial, ChallangeSceneIndex._2Game_Trial_02, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Trial, ChallangeSceneIndex._2Game_Trial_03, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Trial, ChallangeSceneIndex._2Game_Trial_04, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Radar, ChallangeSceneIndex._2Game_Radar_01, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Radar, ChallangeSceneIndex._2Game_Radar_02, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Radar, ChallangeSceneIndex._2Game_Radar_03, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Radar, ChallangeSceneIndex._2Game_Radar_04, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Crashable, ChallangeSceneIndex._2Game_Crashable_01, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Crashable, ChallangeSceneIndex._2Game_Crashable_02, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Crashable, ChallangeSceneIndex._2Game_Crashable_03, playerPrefs);
		AddPlayerPrefs(ChallangeKey.Crashable, ChallangeSceneIndex._2Game_Crashable_04, playerPrefs);
	}

	private static void AddPlayerPrefs(ChallangeKey key, ChallangeSceneIndex sceneIndex, Dictionary<ChallangePlayerPrefIdentifier, ChallangePlayerPref> playerPrefs)
	{
		ChallangePlayerPrefIdentifier challangePlayerPrefIdentifier = new ChallangePlayerPrefIdentifier(key, sceneIndex);
		playerPrefs.Add(challangePlayerPrefIdentifier, new ChallangePlayerPref(challangePlayerPrefIdentifier));
	}

	public static long FormatElapsedTimeToGameServices(float elapsedSecondsTime)
	{
		return (long)(elapsedSecondsTime * 1000f);
	}

	public static long FormatSpeedToGameServices(float totalSpeed)
	{
		return (long)totalSpeed;
	}

	public static long FormatCrashablesToGameServices(int totalCrashables)
	{
		return totalCrashables;
	}
}
