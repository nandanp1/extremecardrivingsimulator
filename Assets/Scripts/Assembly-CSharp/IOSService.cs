using UnityEngine;

public class IOSService : MonoBehaviour
{
	private string leaderBoardId = "com.aim.taxicity.mostmoney";

	private static bool IsInited;

	private void Awake()
	{
		if (!IsInited)
		{
			IsInited = true;
		}
	}

	private void OnLeaderBoarScoreLoaded(CEvent e)
	{
	}

	private void OnAuth()
	{
	}

	private void OnAuthFailed()
	{
	}

	public void ShowLeaderBoard()
	{
	}

	public void SubmitScore(int score)
	{
	}
}
