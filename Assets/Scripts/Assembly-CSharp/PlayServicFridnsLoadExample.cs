using UnityEngine;

public class PlayServicFridnsLoadExample : MonoBehaviour
{
	public GUIStyle headerStyle;

	public GUIStyle boardStyle;

	private bool renderFriendsList;

	private void Awake()
	{
		Singleton<GooglePlayConnection>.instance.addEventListener("player_connected", OnAuth);
		Singleton<GooglePlayConnection>.instance.addEventListener("player_disconnected", OnAuthFailed);
		Singleton<GooglePlayManager>.instance.addEventListener("players_loaded", OnFriendListLoaded);
		Singleton<GooglePlayConnection>.instance.connect();
	}

	private void OnGUI()
	{
		GUI.Label(new Rect(10f, 20f, 400f, 40f), "Friend List Load Example", headerStyle);
		if (GUI.Button(new Rect(300f, 10f, 150f, 50f), "Load Friends"))
		{
			Singleton<GooglePlayManager>.instance.loadConnectedPlayers();
		}
		if (!renderFriendsList)
		{
			return;
		}
		GUI.Label(new Rect(10f, 90f, 100f, 40f), "id", boardStyle);
		GUI.Label(new Rect(150f, 90f, 100f, 40f), "name", boardStyle);
		GUI.Label(new Rect(300f, 90f, 100f, 40f), "avatar ", boardStyle);
		int num = 1;
		foreach (string friends in Singleton<GooglePlayManager>.instance.friendsList)
		{
			GooglePlayerTemplate playerById = Singleton<GooglePlayManager>.instance.GetPlayerById(friends);
			if (playerById != null)
			{
				GUI.Label(new Rect(10f, 90 + 70 * num, 100f, 40f), playerById.playerId, boardStyle);
				GUI.Label(new Rect(150f, 90 + 70 * num, 100f, 40f), playerById.name, boardStyle);
				if (playerById.icon != null)
				{
					GUI.DrawTexture(new Rect(300f, 75 + 70 * num, 50f, 50f), playerById.icon);
				}
				else
				{
					GUI.Label(new Rect(300f, 90 + 70 * num, 100f, 40f), "no photo ", boardStyle);
				}
				num++;
			}
		}
	}

	private void OnFriendListLoaded()
	{
		renderFriendsList = true;
	}

	private void OnAuth()
	{
		Debug.Log("Player Authed");
	}

	private void OnAuthFailed()
	{
		Debug.Log(" Player auntification failed");
	}
}
