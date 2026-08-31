using System;
using UnityEngine;

public class ChallengeEntryBehaviour : MonoBehaviour
{
	public GameObject resultOK;

	public GameObject resultKO;

	public UILabel challengeName;

	public UILabel challengeScore;

	public GameObject leaderBoardsButton;

	private ChallengeEntryInfo challengeInfo;

	public void Init(ChallengeEntryInfo info)
	{
		challengeInfo = new ChallengeEntryInfo();
		challengeInfo = info;
		base.transform.gameObject.name = info.challengeName;
		switch (info.challengeCategory)
		{
		case ChallengeKind.Crash:
			InitCrasheables(info.challengeName, Convert.ToInt32(info.challengeScore), info.challengeCompleted);
			break;
		case ChallengeKind.Radar:
			InitRadar(info.challengeName, Convert.ToInt32(info.challengeScore), info.challengeCompleted);
			break;
		case ChallengeKind.Trial:
			InitTrial(info.challengeName, info.challengeScore, info.challengeCompleted);
			break;
		}
	}

	public void SetToDefault()
	{
		resultKO.SetActive(true);
		resultOK.SetActive(false);
		challengeName.text = string.Empty;
		challengeScore.text = string.Empty;
		challengeInfo = null;
		leaderBoardsButton.SetActive(true);
	}

	private void InitCrasheables(string challengename, int chrasheables, bool completed)
	{
		if (completed)
		{
			resultKO.SetActive(false);
			resultOK.SetActive(true);
		}
		else
		{
			resultKO.SetActive(true);
			resultOK.SetActive(false);
		}
		challengeName.text = challengename;
		challengeScore.text = string.Empty + chrasheables;
	}

	private void InitRadar(string challengename, float radar, bool completed)
	{
		if (completed)
		{
			resultKO.SetActive(false);
			resultOK.SetActive(true);
		}
		else
		{
			resultKO.SetActive(true);
			resultOK.SetActive(false);
		}
		challengeName.text = challengename;
		challengeScore.text = string.Empty + Converter.MSToCurrentUnits(radar, true);
	}

	private void InitTrial(string challengename, float timeTrial, bool completed)
	{
		if (completed)
		{
			resultKO.SetActive(false);
			resultOK.SetActive(true);
		}
		else
		{
			resultKO.SetActive(true);
			resultOK.SetActive(false);
		}
		challengeName.text = challengename;
		challengeScore.text = string.Empty + Converter.GetRaceTimeFromSeconds(timeTrial);
	}

	public void OnClickButtonLeaderBoards()
	{
		MySingleton<Manager>.Instance.gameServicesManager.ShowLeaderboardOrLogin(challengeInfo.challengeLeaderboardKey);
	}
}
