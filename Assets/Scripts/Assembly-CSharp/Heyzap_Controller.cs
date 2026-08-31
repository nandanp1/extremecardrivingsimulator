using System.Collections;
using UnityEngine;

public class Heyzap_Controller : MonoBehaviour
{
	private const string HEYZAP_PUBLISHER_ID = "3afe13c93d96a2a882c749550304ce3b";

	private const float VIDEO_AVAILABILITY_CHECK_TIME = 2f;

	private Task videoTask;

	public bool currentVideoAvailableStatus;

	public void Init()
	{
		currentVideoAvailableStatus = false;
	}

	public bool GetHeyzapInterstitialReady()
	{
		return false;
	}

	public void ShowHeyzapInterstitial()
	{
	}

	private void EventSubscriptionsForRewards()
	{
	}

	private void EventSubscriptionsForInterstitials()
	{
	}

	private IEnumerator UpdateVideoAvailableStatus()
	{
		bool isVideoRewardsAvailable = IsVideoRewardAvailable();
		if (isVideoRewardsAvailable != currentVideoAvailableStatus)
		{
			currentVideoAvailableStatus = isVideoRewardsAvailable;
			if ((bool)MySingleton<Manager>.Instance.guiCurrentLogic)
			{
				MySingleton<Manager>.Instance.guiCurrentLogic.OnVideoRewardsAvailabilityChanged(currentVideoAvailableStatus);
			}
		}
		yield return StartCoroutine(CoroutineUtil.WaitForRealSeconds(2f));
		StartCoroutine(UpdateVideoAvailableStatus());
	}

	private void OnVideoRewardComplete(bool success)
	{
		if (success)
		{
			videoTask.SetProgress(1f);
			MySingleton<Manager>.Instance.guiCurrentLogic.OnVideoRewardsFinished(videoTask);
		}
	}

	public bool IsVideoRewardAvailable()
	{
		return false;
	}

	public void ShowVideoReward(Task task)
	{
		currentVideoAvailableStatus = false;
	}

	public void Loadincentivizedvideo()
	{
	}

	private IEnumerator DoLoadincentivizedvideo()
	{
		yield return null;
	}
}
