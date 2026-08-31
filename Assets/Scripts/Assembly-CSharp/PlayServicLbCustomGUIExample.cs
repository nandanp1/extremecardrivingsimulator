using UnityEngine;

public class PlayServicLbCustomGUIExample : MonoBehaviour
{
	private const string LEADERBOARD_ID = "CgkIipfs2qcGEAIQAA";

	public int hiScore = 100;

	public GUIStyle headerStyle;

	public GUIStyle boardStyle;

	private GPLeaderBoard loadedLeaderBoard;

	private GPCollectionType diplayCollection = GPCollectionType.FRIENDS;

	private void Awake()
	{
		Singleton<GooglePlayConnection>.instance.addEventListener("player_connected", OnAuth);
		Singleton<GooglePlayConnection>.instance.addEventListener("player_disconnected", OnAuthFailed);
		Singleton<GooglePlayManager>.instance.addEventListener("score_request_received", OnScoreListLoaded);
		Singleton<GooglePlayConnection>.instance.connect();
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(10f, 20f, 400f, 40f), "Custom Leader Board GUI Example", headerStyle);
		if (GUI.Button(new Rect(400f, 10f, 150f, 50f), "Load Friends Scores"))
		{
			Singleton<GooglePlayManager>.instance.loadTopScores("CgkIipfs2qcGEAIQAA", GPBoardTimeSpan.ALL_TIME, GPCollectionType.FRIENDS, 10);
		}
		if (GUI.Button(new Rect(600f, 10f, 150f, 50f), "Load Global Scores"))
		{
			Singleton<GooglePlayManager>.instance.loadTopScores("CgkIipfs2qcGEAIQAA", GPBoardTimeSpan.ALL_TIME, GPCollectionType.GLOBAL, 10);
		}
		if (GUI.Button(new Rect(1000f, 10f, 170f, 50f), "Open Scores"))
		{
			Singleton<GooglePlayManager>.instance.showLeaderBoardById("CgkIipfs2qcGEAIQAA");
		}
		Color color = GUI.color;
		if (diplayCollection == GPCollectionType.GLOBAL)
		{
			GUI.color = Color.green;
		}
		if (GUI.Button(new Rect(800f, 10f, 170f, 50f), "Displying Global Scores"))
		{
			diplayCollection = GPCollectionType.GLOBAL;
		}
		GUI.color = color;
		if (diplayCollection == GPCollectionType.FRIENDS)
		{
			GUI.color = Color.green;
		}
		if (GUI.Button(new Rect(800f, 70f, 170f, 50f), "Displying Friends Scores"))
		{
			diplayCollection = GPCollectionType.FRIENDS;
		}
		GUI.color = color;
		GUI.Label(new Rect(10f, 90f, 100f, 40f), "rank", boardStyle);
		GUI.Label(new Rect(100f, 90f, 100f, 40f), "score", boardStyle);
		GUI.Label(new Rect(200f, 90f, 100f, 40f), "playerId", boardStyle);
		GUI.Label(new Rect(400f, 90f, 100f, 40f), "name ", boardStyle);
		GUI.Label(new Rect(550f, 90f, 100f, 40f), "avatar ", boardStyle);
		if (loadedLeaderBoard == null)
		{
			return;
		}
		for (int i = 1; i < 10; i++)
		{
			GPScore score = loadedLeaderBoard.GetScore(i, GPBoardTimeSpan.ALL_TIME, diplayCollection);
			if (score == null)
			{
				continue;
			}
			GUI.Label(new Rect(10f, 90 + 70 * i, 100f, 40f), i.ToString(), boardStyle);
			GUI.Label(new Rect(100f, 90 + 70 * i, 100f, 40f), score.score.ToString(), boardStyle);
			GUI.Label(new Rect(200f, 90 + 70 * i, 100f, 40f), score.playerId, boardStyle);
			GooglePlayerTemplate playerById = Singleton<GooglePlayManager>.instance.GetPlayerById(score.playerId);
			if (playerById != null)
			{
				GUI.Label(new Rect(400f, 90 + 70 * i, 100f, 40f), playerById.name, boardStyle);
				if (playerById.icon != null)
				{
					GUI.DrawTexture(new Rect(550f, 75 + 70 * i, 50f, 50f), playerById.icon);
				}
				else
				{
					GUI.Label(new Rect(550f, 90 + 70 * i, 100f, 40f), "no photo ", boardStyle);
				}
			}
		}
	}

	private void OnScoreListLoaded()
	{
		Debug.Log("Scores loaded");
		Debug.Log("CgkIipfs2qcGEAIQAA");
		loadedLeaderBoard = Singleton<GooglePlayManager>.instance.GetLeaderBoard("CgkIipfs2qcGEAIQAA");
		Debug.Log(loadedLeaderBoard);
	}

	private void OnAuth()
	{
		Debug.Log("Player Authed");
	}

	private void OnAuthFailed()
	{
		Debug.Log("Player auntification failed");
	}
}
