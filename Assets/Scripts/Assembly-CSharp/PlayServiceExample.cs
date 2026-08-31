using UnityEngine;

public class PlayServiceExample : AndroidNativeExampleBase
{
	private const string LEADERBOARD_NAME = "REPLACE_WITH_YOUR_NAME";

	private const string LEADERBOARD_ID = "CgkIipfs2qcGEAIQAA";

	private int score = 100;

	public GameObject avatar;

	private Texture defaulttexture;

	public DefaultPreviewButton connectButton;

	public SA_Label playerLabel;

	public DefaultPreviewButton[] ConnectionDependedntButtons;

	private void Start()
	{
		playerLabel.text = "Player Diconnected";
		defaulttexture = avatar.GetComponent<Renderer>().material.mainTexture;
		Singleton<GooglePlayConnection>.instance.addEventListener("player_connected", OnPlayerConnected);
		Singleton<GooglePlayConnection>.instance.addEventListener("player_disconnected", OnPlayerDisconnected);
		Singleton<GooglePlayConnection>.instance.addEventListener("connection_result_received", OnConnectionResult);
		Singleton<GooglePlayManager>.instance.addEventListener("achievement_updated", OnAchivmentUpdated);
		Singleton<GooglePlayManager>.instance.addEventListener("score_submited", OnScoreSubmited);
		if (GooglePlayConnection.state == GPConnectionState.STATE_CONNECTED)
		{
			OnPlayerConnected();
		}
	}

	private void ConncetButtonPress()
	{
		Debug.Log("GooglePlayManager State  -> " + GooglePlayConnection.state);
		if (GooglePlayConnection.state == GPConnectionState.STATE_CONNECTED)
		{
			SA_StatusBar.text = "Disconnecting from Play Service...";
			Singleton<GooglePlayConnection>.instance.disconnect();
		}
		else
		{
			SA_StatusBar.text = "Connecting to Play Service...";
			Singleton<GooglePlayConnection>.instance.connect();
		}
	}

	private void showLeaderBoardsUI()
	{
		Singleton<GooglePlayManager>.instance.showLeaderBoardsUI();
		SA_StatusBar.text = "Showing Leader Boards UI";
	}

	private void loadLeaderBoards()
	{
		Singleton<GooglePlayManager>.instance.addEventListener("leaderboards_loeaded", OnLeaderBoardsLoaded);
		Singleton<GooglePlayManager>.instance.loadLeaderBoards();
		SA_StatusBar.text = "Loading Leader Boards Data...";
	}

	private void showLeaderBoard()
	{
		Singleton<GooglePlayManager>.instance.showLeaderBoardById("CgkIipfs2qcGEAIQAA");
		SA_StatusBar.text = "Shwoing Leader Board UI for CgkIipfs2qcGEAIQAA";
	}

	private void submitScore()
	{
		score++;
		Singleton<GooglePlayManager>.instance.submitScore("REPLACE_WITH_YOUR_NAME", score);
		SA_StatusBar.text = "Score " + score + " Submited for REPLACE_WITH_YOUR_NAME";
	}

	private void showAchivmentsUI()
	{
		Singleton<GooglePlayManager>.instance.showAchivmentsUI();
		SA_StatusBar.text = "Showing Achivments UI";
	}

	private void loadAchievements()
	{
		Singleton<GooglePlayManager>.instance.addEventListener("achievements_loaded", OnAchivmentsLoaded);
		Singleton<GooglePlayManager>.instance.loadAchievements();
		SA_StatusBar.text = "Loading Achivments Data...";
	}

	private void reportAchievement()
	{
		Singleton<GooglePlayManager>.instance.reportAchievement("achievement_prime");
		SA_StatusBar.text = "Reporting achievement_prime...";
	}

	private void incrementAchievement()
	{
		Singleton<GooglePlayManager>.instance.incrementAchievement("achievement_bored", 1);
		SA_StatusBar.text = "Incrementing achievement_bored...";
	}

	private void revealAchievement()
	{
		Singleton<GooglePlayManager>.instance.revealAchievement("achievement_humble");
		SA_StatusBar.text = "Revealing achievement_humble...";
	}

	private void FixedUpdate()
	{
		if (GooglePlayConnection.state == GPConnectionState.STATE_CONNECTED)
		{
			if (Singleton<GooglePlayManager>.instance.player.icon != null)
			{
				avatar.GetComponent<Renderer>().material.mainTexture = Singleton<GooglePlayManager>.instance.player.icon;
			}
		}
		else
		{
			avatar.GetComponent<Renderer>().material.mainTexture = defaulttexture;
		}
		string text = "Connect";
		if (GooglePlayConnection.state == GPConnectionState.STATE_CONNECTED)
		{
			text = "Disconnect";
			DefaultPreviewButton[] connectionDependedntButtons = ConnectionDependedntButtons;
			foreach (DefaultPreviewButton defaultPreviewButton in connectionDependedntButtons)
			{
				defaultPreviewButton.EnabledButton();
			}
		}
		else
		{
			DefaultPreviewButton[] connectionDependedntButtons2 = ConnectionDependedntButtons;
			foreach (DefaultPreviewButton defaultPreviewButton2 in connectionDependedntButtons2)
			{
				defaultPreviewButton2.DisabledButton();
			}
			text = ((GooglePlayConnection.state != GPConnectionState.STATE_DISCONNECTED && GooglePlayConnection.state != GPConnectionState.STATE_UNCONFIGURED) ? "Connecting.." : "Connect");
		}
		connectButton.text = text;
	}

	private void OnAchivmentsLoaded(CEvent e)
	{
		Singleton<GooglePlayManager>.instance.removeEventListener("achievements_loaded", OnAchivmentsLoaded);
		GooglePlayResult googlePlayResult = e.data as GooglePlayResult;
		if (googlePlayResult.isSuccess)
		{
			foreach (string key in Singleton<GooglePlayManager>.instance.achievements.Keys)
			{
				GPAchievement achievementd = Singleton<GooglePlayManager>.instance.GetAchievementd(key);
				Debug.Log(achievementd.id);
				Debug.Log(achievementd.name);
				Debug.Log(achievementd.description);
				Debug.Log(achievementd.type);
				Debug.Log(achievementd.state);
				Debug.Log(achievementd.currentSteps);
				Debug.Log(achievementd.totalSteps);
			}
			SA_StatusBar.text = "Total Achivments: " + Singleton<GooglePlayManager>.instance.achievements.Count;
			AndroidNative.showMessage("Achievments Loaded", "Total Achivments: " + Singleton<GooglePlayManager>.instance.achievements.Count);
		}
		else
		{
			SA_StatusBar.text = googlePlayResult.message;
			AndroidNative.showMessage("Achievments Loaded error: ", googlePlayResult.message);
		}
	}

	private void OnAchivmentUpdated(CEvent e)
	{
		GooglePlayResult googlePlayResult = e.data as GooglePlayResult;
		SA_StatusBar.text = "Achievment Updated: Id: " + googlePlayResult.achievementId + "\n status: " + googlePlayResult.message;
		AndroidNative.showMessage("Achievment Updated ", "Id: " + googlePlayResult.achievementId + "\n status: " + googlePlayResult.message);
	}

	private void OnLeaderBoardsLoaded(CEvent e)
	{
		Singleton<GooglePlayManager>.instance.removeEventListener("leaderboards_loeaded", OnLeaderBoardsLoaded);
		GooglePlayResult googlePlayResult = e.data as GooglePlayResult;
		if (googlePlayResult.isSuccess)
		{
			if (Singleton<GooglePlayManager>.instance.GetLeaderBoard("CgkIipfs2qcGEAIQAA") == null)
			{
				AndroidNative.showMessage("Leader boards loaded", "CgkIipfs2qcGEAIQAA not found in leader boards list");
				return;
			}
			SA_StatusBar.text = "REPLACE_WITH_YOUR_NAME  score  " + Singleton<GooglePlayManager>.instance.GetLeaderBoard("CgkIipfs2qcGEAIQAA").GetCurrentPlayerScore(GPBoardTimeSpan.ALL_TIME, GPCollectionType.GLOBAL).score;
			AndroidNative.showMessage("REPLACE_WITH_YOUR_NAME  score", Singleton<GooglePlayManager>.instance.GetLeaderBoard("CgkIipfs2qcGEAIQAA").GetCurrentPlayerScore(GPBoardTimeSpan.ALL_TIME, GPCollectionType.GLOBAL).score.ToString());
		}
		else
		{
			SA_StatusBar.text = googlePlayResult.message;
			AndroidNative.showMessage("Leader-Boards Loaded error: ", googlePlayResult.message);
		}
	}

	private void OnScoreSubmited(CEvent e)
	{
		GooglePlayResult googlePlayResult = e.data as GooglePlayResult;
		AndroidNative.showMessage("Score Submited", googlePlayResult.message);
	}

	private void OnPlayerDisconnected()
	{
		SA_StatusBar.text = "Player Diconnected";
		playerLabel.text = "Player Diconnected";
	}

	private void OnPlayerConnected()
	{
		SA_StatusBar.text = "Player Connected";
		playerLabel.text = Singleton<GooglePlayManager>.instance.player.name;
	}

	private void OnConnectionResult(CEvent e)
	{
		GooglePlayConnectionResult googlePlayConnectionResult = e.data as GooglePlayConnectionResult;
		SA_StatusBar.text = "ConnectionResul:  " + googlePlayConnectionResult.code;
		Debug.Log(googlePlayConnectionResult.code.ToString());
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
