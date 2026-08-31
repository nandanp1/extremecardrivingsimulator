using UnityEngine;

public class PlayServiceCustomLBExample : MonoBehaviour
{
	private const string LEADERBOARD_ID = "CgkIipfs2qcGEAIQAA";

	public GameObject avatar;

	private Texture defaulttexture;

	public DefaultPreviewButton connectButton;

	public SA_Label playerLabel;

	public DefaultPreviewButton GlobalLoadButton;

	public DefaultPreviewButton LocalLoadButton;

	public DefaultPreviewButton ShowGlobalButton;

	public DefaultPreviewButton ShowLocalButton;

	public DefaultPreviewButton[] ConnectionDependedntButtons;

	public CustomLeaderboardFiledsHolder[] lines;

	private GPLeaderBoard loadedLeaderBoard;

	private GPCollectionType diplayCollection = GPCollectionType.FRIENDS;

	private void Start()
	{
		playerLabel.text = "Player Diconnected";
		defaulttexture = avatar.GetComponent<Renderer>().material.mainTexture;
		CustomLeaderboardFiledsHolder[] array = lines;
		foreach (CustomLeaderboardFiledsHolder customLeaderboardFiledsHolder in array)
		{
			customLeaderboardFiledsHolder.Disable();
		}
		Singleton<GooglePlayConnection>.instance.addEventListener("player_connected", OnPlayerConnected);
		Singleton<GooglePlayConnection>.instance.addEventListener("player_disconnected", OnPlayerDisconnected);
		Singleton<GooglePlayConnection>.instance.addEventListener("connection_result_received", OnConnectionResult);
		SA_StatusBar.text = "Custom Leader-board example scene loaded";
		Singleton<GooglePlayManager>.instance.addEventListener("score_request_received", OnScoreListLoaded);
		if (GooglePlayConnection.state == GPConnectionState.STATE_CONNECTED)
		{
			OnPlayerConnected();
		}
	}

	public void LoadGlobal()
	{
		GlobalLoadButton.DisabledButton();
		Singleton<GooglePlayManager>.instance.loadTopScores("CgkIipfs2qcGEAIQAA", GPBoardTimeSpan.ALL_TIME, GPCollectionType.GLOBAL, 10);
	}

	public void LoadLocal()
	{
		LocalLoadButton.DisabledButton();
		Singleton<GooglePlayManager>.instance.loadTopScores("CgkIipfs2qcGEAIQAA", GPBoardTimeSpan.ALL_TIME, GPCollectionType.FRIENDS, 10);
	}

	public void SwitchDisplayCollectionToGlobal()
	{
		diplayCollection = GPCollectionType.GLOBAL;
	}

	public void SwitchDisplayCollectionToLocal()
	{
		diplayCollection = GPCollectionType.FRIENDS;
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

	private void Update()
	{
		if (loadedLeaderBoard != null)
		{
			int num = 1;
			CustomLeaderboardFiledsHolder[] array = lines;
			foreach (CustomLeaderboardFiledsHolder customLeaderboardFiledsHolder in array)
			{
				customLeaderboardFiledsHolder.Disable();
				GPScore score = loadedLeaderBoard.GetScore(num, GPBoardTimeSpan.ALL_TIME, diplayCollection);
				if (score != null)
				{
					customLeaderboardFiledsHolder.rank.text = num.ToString();
					customLeaderboardFiledsHolder.score.text = score.score.ToString();
					customLeaderboardFiledsHolder.playerId.text = score.playerId;
					GooglePlayerTemplate playerById = Singleton<GooglePlayManager>.instance.GetPlayerById(score.playerId);
					if (playerById != null)
					{
						customLeaderboardFiledsHolder.playerName.text = playerById.name;
						customLeaderboardFiledsHolder.avatar.GetComponent<Renderer>().material.mainTexture = playerById.icon;
					}
					else
					{
						customLeaderboardFiledsHolder.playerName.text = "--";
						customLeaderboardFiledsHolder.avatar.GetComponent<Renderer>().material.mainTexture = defaulttexture;
					}
					customLeaderboardFiledsHolder.avatar.GetComponent<Renderer>().enabled = true;
				}
				else
				{
					customLeaderboardFiledsHolder.Disable();
				}
				num++;
			}
		}
		else
		{
			CustomLeaderboardFiledsHolder[] array2 = lines;
			foreach (CustomLeaderboardFiledsHolder customLeaderboardFiledsHolder2 in array2)
			{
				customLeaderboardFiledsHolder2.Disable();
			}
		}
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
			if (diplayCollection == GPCollectionType.FRIENDS)
			{
				ShowGlobalButton.EnabledButton();
				ShowLocalButton.DisabledButton();
			}
			else
			{
				ShowGlobalButton.DisabledButton();
				ShowLocalButton.EnabledButton();
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

	private void OnScoreListLoaded()
	{
		SA_StatusBar.text = "Scores Load Finished";
		loadedLeaderBoard = Singleton<GooglePlayManager>.instance.GetLeaderBoard("CgkIipfs2qcGEAIQAA");
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
}
