using UnityEngine;

public class PlayServices : MonoBehaviour
{
	private const string LEADERBOARD_NAME = "leaderboard_arcade__most_money";

	private const string LEADERBOARD_ID = "CgkI7IePnrQUEAIQBg";

	public void StartService()
	{
		Singleton<GooglePlayConnection>.instance.addEventListener("player_connected", OnPlayerConnected);
		Singleton<GooglePlayConnection>.instance.addEventListener("player_disconnected", OnPlayerDisconnected);
		Singleton<GooglePlayConnection>.instance.addEventListener("connection_result_received", OnConnectionResult);
		Singleton<GooglePlayManager>.instance.addEventListener("achievement_updated", OnAchivmentUpdated);
		Singleton<GooglePlayManager>.instance.addEventListener("score_submited", OnScoreSubmited);
		if (GooglePlayConnection.state == GPConnectionState.STATE_CONNECTED)
		{
			OnPlayerConnected();
		}
		else
		{
			Singleton<GooglePlayConnection>.instance.connect();
		}
	}

	private void ConncetButtonPress()
	{
		Debug.Log("GooglePlayManager State  -> " + GooglePlayConnection.state);
		if (GooglePlayConnection.state == GPConnectionState.STATE_CONNECTED)
		{
			Singleton<GooglePlayConnection>.instance.disconnect();
		}
		else
		{
			Singleton<GooglePlayConnection>.instance.connect();
		}
	}

	private void showLeaderBoardsUI()
	{
		Singleton<GooglePlayManager>.instance.showLeaderBoardsUI();
	}

	private void loadLeaderBoards()
	{
		Singleton<GooglePlayManager>.instance.addEventListener("leaderboards_loeaded", OnLeaderBoardsLoaded);
		Singleton<GooglePlayManager>.instance.loadLeaderBoards();
	}

	public void showLeaderBoard()
	{
		Singleton<GooglePlayManager>.instance.showLeaderBoardById("CgkI7IePnrQUEAIQBg");
	}

	public void submitScore(int score)
	{
		Singleton<GooglePlayManager>.instance.submitScore("leaderboard_arcade__most_money", score);
	}

	private void showAchivmentsUI()
	{
		Singleton<GooglePlayManager>.instance.showAchivmentsUI();
	}

	private void loadAchievements()
	{
		Singleton<GooglePlayManager>.instance.addEventListener("achievements_loaded", OnAchivmentsLoaded);
		Singleton<GooglePlayManager>.instance.loadAchievements();
	}

	private void reportAchievement()
	{
		Singleton<GooglePlayManager>.instance.reportAchievement("achievement_prime");
	}

	private void incrementAchievement()
	{
		Singleton<GooglePlayManager>.instance.incrementAchievement("achievement_bored", 1);
	}

	private void revealAchievement()
	{
		Singleton<GooglePlayManager>.instance.revealAchievement("achievement_humble");
	}

	private void OnAchivmentsLoaded(CEvent e)
	{
		Singleton<GooglePlayManager>.instance.removeEventListener("achievements_loaded", OnAchivmentsLoaded);
	}

	private void OnAchivmentUpdated(CEvent e)
	{
	}

	private void OnLeaderBoardsLoaded(CEvent e)
	{
	}

	private void OnScoreSubmited(CEvent e)
	{
	}

	private void OnPlayerDisconnected()
	{
	}

	private void OnPlayerConnected()
	{
	}

	private void OnConnectionResult(CEvent e)
	{
	}

	private void OnDestroy()
	{
		if (!Singleton<GooglePlayConnection>.IsDestroyed)
		{
			Singleton<GooglePlayConnection>.instance.removeEventListener("player_connected", OnPlayerConnected);
			Singleton<GooglePlayConnection>.instance.removeEventListener("player_disconnected", OnPlayerDisconnected);
		}
		if (!Singleton<GooglePlayManager>.IsDestroyed)
		{
			Singleton<GooglePlayManager>.instance.removeEventListener("achievement_updated", OnAchivmentUpdated);
			Singleton<GooglePlayManager>.instance.removeEventListener("score_submited", OnScoreSubmited);
		}
	}
}
