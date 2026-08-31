using System;
using System.Collections.Generic;
using UnityEngine;

public class ChallangeManager : MonoBehaviour
{
	public static string LEADERBOARDID_TRIAL_1 = "CgkIzM6OqvYfEAIQCg";

	public static string LEADERBOARDID_TRIAL_2 = "CgkIzM6OqvYfEAIQBw";

	public static string LEADERBOARDID_TRIAL_3 = "CgkIzM6OqvYfEAIQCA";

	public static string LEADERBOARDID_TRIAL_4 = "CgkIzM6OqvYfEAIQCQ";

	public static string LEADERBOARDID_RADAR_1 = "CgkIzM6OqvYfEAIQCw";

	public static string LEADERBOARDID_RADAR_2 = "CgkIzM6OqvYfEAIQDA";

	public static string LEADERBOARDID_RADAR_3 = "CgkIzM6OqvYfEAIQDQ";

	public static string LEADERBOARDID_RADAR_4 = "CgkIzM6OqvYfEAIQDg";

	public static string LEADERBOARDID_CRASHABLE_1 = "CgkIzM6OqvYfEAIQDw";

	public static string LEADERBOARDID_CRASHABLE_2 = "CgkIzM6OqvYfEAIQEA";

	public static string LEADERBOARDID_CRASHABLE_3 = "CgkIzM6OqvYfEAIQEQ";

	public static string LEADERBOARDID_CRASHABLE_4 = "CgkIzM6OqvYfEAIQEg";

	public bool noReport = true;

	[HideInInspector]
	public Dictionary<ChallangePlayerPrefIdentifier, ChallangePlayerPref> playerPrefs;

	private bool _pausedByGameCenter;

	private void Awake()
	{
		playerPrefs = new Dictionary<ChallangePlayerPrefIdentifier, ChallangePlayerPref>();
	}

	private void Start()
	{
		ChallangeConfiguration.InitializePlayerPrefs(playerPrefs);
	}

	public ChallangePlayerPref GetPlayerPref(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		return playerPrefs[ChallangePlayerPrefIdentifier.Identifier(key, sceneIndex)];
	}

	public void ReportAllScoresWhenLoginSucceed()
	{
		ChallangeKey key = ChallangeKey.Trial;
		ChallangeSceneIndex[] challangeCircuitsSceneIndex = ChallangeConfiguration.GetChallangeCircuitsSceneIndex(ChallangeKey.Trial);
		for (int i = 0; i < challangeCircuitsSceneIndex.Length; i++)
		{
			ReportTimeScoreWhenLoginSucceed(key, challangeCircuitsSceneIndex[i]);
		}
		ChallangeKey key2 = ChallangeKey.Radar;
		ChallangeSceneIndex[] challangeCircuitsSceneIndex2 = ChallangeConfiguration.GetChallangeCircuitsSceneIndex(ChallangeKey.Radar);
		for (int j = 0; j < challangeCircuitsSceneIndex2.Length; j++)
		{
			ReportFloatScoreWhenLoginSucceed(key2, challangeCircuitsSceneIndex2[j]);
		}
		ChallangeKey key3 = ChallangeKey.Crashable;
		ChallangeSceneIndex[] challangeCircuitsSceneIndex3 = ChallangeConfiguration.GetChallangeCircuitsSceneIndex(ChallangeKey.Crashable);
		for (int k = 0; k < challangeCircuitsSceneIndex2.Length; k++)
		{
			ReportIntScoreWhenLoginSucceed(key3, challangeCircuitsSceneIndex3[k]);
		}
	}

	public void ReportTimeScoreWhenLoginSucceed(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		string challangeCircuit_LeaderboardId = ChallangeConfiguration.GetChallangeCircuit_LeaderboardId(key, sceneIndex);
		ChallangePlayerPref playerPref = MySingleton<Manager>.Instance.challangeManager.GetPlayerPref(key, sceneIndex);
		if (playerPref.bestTime > 0f)
		{
			MySingleton<Manager>.Instance.gameServicesManager.ShareLeaderboardScore(challangeCircuit_LeaderboardId, ChallangeConfiguration.FormatElapsedTimeToGameServices(playerPref.bestTime));
		}
	}

	public void ReportFloatScoreWhenLoginSucceed(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		string challangeCircuit_LeaderboardId = ChallangeConfiguration.GetChallangeCircuit_LeaderboardId(key, sceneIndex);
		ChallangePlayerPref playerPref = MySingleton<Manager>.Instance.challangeManager.GetPlayerPref(key, sceneIndex);
		if (playerPref.totalSpeed > 0f)
		{
			MySingleton<Manager>.Instance.gameServicesManager.ShareLeaderboardScore(challangeCircuit_LeaderboardId, ChallangeConfiguration.FormatSpeedToGameServices(Converter.MSToKMH(playerPref.totalSpeed)));
		}
	}

	public void ReportIntScoreWhenLoginSucceed(ChallangeKey key, ChallangeSceneIndex sceneIndex)
	{
		string challangeCircuit_LeaderboardId = ChallangeConfiguration.GetChallangeCircuit_LeaderboardId(key, sceneIndex);
		ChallangePlayerPref playerPref = MySingleton<Manager>.Instance.challangeManager.GetPlayerPref(key, sceneIndex);
		if (playerPref.totalCrashables >= CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE)
		{
			MySingleton<Manager>.Instance.gameServicesManager.ShareLeaderboardScore(challangeCircuit_LeaderboardId, ChallangeConfiguration.FormatCrashablesToGameServices(playerPref.totalCrashables));
		}
	}

	private void OnGameCenterViewClosed(object sender, EventArgs e)
	{
		if (_pausedByGameCenter)
		{
			Time.timeScale = 1f;
		}
		_pausedByGameCenter = false;
	}

	private void OnGameCenterViewLoading(object sender, EventArgs e)
	{
		if (Time.timeScale > 0f)
		{
			Time.timeScale = 0f;
			_pausedByGameCenter = true;
		}
	}
}
