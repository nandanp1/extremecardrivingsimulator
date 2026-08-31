using System;
using System.Collections;
using UnityEngine;

public class RewardHourLogic : MonoBehaviour
{
	private const float TIME_FOR_REFRESH = 0.25f;

	private const string KEY_TIME_FOR_HOUR_REWARD = "keyTimeForHourReward";

	private DateTime lastTimeCollected;

	private UIRewardRow rewardRow;

	private void OnEnable()
	{
		rewardRow = GetComponent<UIRewardRow>();
		lastTimeCollected = SettingsManager.GetTime("keyTimeForHourReward");
		StartCoroutine("UpdateHourReward");
	}

	private void OnDisable()
	{
		StopCoroutine("UpdateHourReward");
	}

	private IEnumerator UpdateHourReward()
	{
		yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(0.25f));
		if (MySingleton<Manager>.Instance.timeManager.HasTimeFromInternet())
		{
			float dif = (float)MySingleton<Manager>.Instance.timeManager.GetTimeFromInternet().Subtract(lastTimeCollected).TotalSeconds;
			rewardRow.Refresh();
		}
		yield return null;
		StartCoroutine("UpdateHourReward");
	}

	public void ResetHourClaimed()
	{
		DateTime timeFromInternet = MySingleton<Manager>.Instance.timeManager.GetTimeFromInternet();
		SettingsManager.SetTime("keyTimeForHourReward", timeFromInternet);
		lastTimeCollected = timeFromInternet;
	}
}
