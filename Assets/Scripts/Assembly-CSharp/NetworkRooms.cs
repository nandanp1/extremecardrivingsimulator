using ExitGames.Client.Photon;
using UnityEngine;

public class NetworkRooms : MonoBehaviour
{
	public const string ROOMPROP_LATENCYGROUP = "lg";

	public const string ROOMPROP_LEVELNAME = "level";

	public const string ROOMPROP_GAMEMODE = "gameMode";

	public const string ROOMPROP_TRACK = "track";

	public const string ROOMPROP_GAMESTATUS = "gameStatus";

	public const string ROOMPROP_CUSTOMFULL = "customFull";

	public RoomOptions defaultRoomOptions;

	public string targetRoom;

	public RoomOptions targetRoomOptions;

	public RoomInfo[] roomList
	{
		get
		{
			return MySingleton<Manager>.Instance.networkManager.roomList;
		}
	}

	public int roomsCount
	{
		get
		{
			return roomList.Length;
		}
	}

	public Room currentRoom
	{
		get
		{
			return MySingleton<Manager>.Instance.networkManager.currentRoom;
		}
	}

	private void Start()
	{
		if ((bool)MySingleton<Manager>.Instance.networkManager)
		{
			MySingleton<Manager>.Instance.networkManager.OnAIMJoinedRoom += OnJoinedRoomCallback;
			MySingleton<Manager>.Instance.networkManager.OnAIMReceiveRoomListUpdate += OnRoomListUpdatedCallback;
			MySingleton<Manager>.Instance.networkManager.OnAIMJoinRoomFailed += OnJoinToRoomFailedCallback;
			MySingleton<Manager>.Instance.networkManager.OnAIMCreateRoomFailed += OnCreateRoomFailedCallback;
			MySingleton<Manager>.Instance.networkManager.OnAIMRandomJoinFailed += OnRandomJoinRoomFailedCallback;
		}
		defaultRoomOptions = GenerateDefaultRoomProperties();
		defaultRoomOptions.isOpen = true;
		defaultRoomOptions.isVisible = true;
		defaultRoomOptions.maxPlayers = MySingleton<Manager>.Instance.connection.maxPlayersPerRoom;
	}

	public void Play()
	{
		if (targetRoom != null && targetRoom.Length > 0)
		{
			JoinRoom(targetRoom);
		}
		else if (roomsCount > 0)
		{
			JoinRoom();
		}
		else
		{
			CreateRoom();
		}
	}

	private RoomOptions GenerateDefaultRoomProperties()
	{
		RoomOptions roomOptions = new RoomOptions();
		string[] customRoomPropertiesForLobby = new string[6] { "lg", "level", "customFull", "gameMode", "track", "gameStatus" };
		Hashtable hashtable = new Hashtable();
		hashtable.Add("lg", (byte)1);
		hashtable.Add("level", GetDefaultLevelByGame(MySingleton<Manager>.Instance.selectedGame));
		hashtable.Add("customFull", (byte)0);
		hashtable.Add("gameMode", (byte)1);
		hashtable.Add("track", (byte)0);
		hashtable.Add("gameStatus", (byte)0);
		Hashtable customRoomProperties = hashtable;
		roomOptions.customRoomProperties = customRoomProperties;
		roomOptions.customRoomPropertiesForLobby = customRoomPropertiesForLobby;
		roomOptions.isOpen = true;
		roomOptions.isVisible = true;
		roomOptions.maxPlayers = MySingleton<Manager>.Instance.connection.maxPlayersPerRoom;
		return roomOptions;
	}

	private byte GetDefaultLevelByGame(Manager.SelectedGame game)
	{
		switch (game)
		{
		case Manager.SelectedGame.ExtremeCarDriving:
			return 0;
		case Manager.SelectedGame.ExtremeSUVDriving:
			return 1;
		case Manager.SelectedGame.MultiplayerDriving:
			return 0;
		case Manager.SelectedGame.SanFrancisco:
			return 0;
		default:
			return 0;
		}
	}

	private Hashtable GetFilterByLatencyGroup(byte group)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add("lg", group);
		return hashtable;
	}

	public void SetTrackSelected(byte trackIndex)
	{
		currentRoom.SetCustomProperties(new Hashtable { { "track", trackIndex } });
	}

	public byte GetSelectedTrack()
	{
		return (byte)currentRoom.customProperties["track"];
	}

	public bool GetCustomFull()
	{
		byte b = (byte)currentRoom.customProperties["customFull"];
		return b == 1;
	}

	public void SetCustomFull(bool isFull)
	{
		byte b = (byte)(isFull ? 1 : 0);
		currentRoom.SetCustomProperties(new Hashtable { { "customFull", b } });
	}

	public byte GetGameStatus()
	{
		return (byte)currentRoom.customProperties["gameStatus"];
	}

	public void SetGameStatus(byte status)
	{
		currentRoom.SetCustomProperties(new Hashtable { { "gameStatus", status } });
	}

	public void FindRoomByLatencyGroupAndPlay(byte latencyGroup)
	{
		targetRoomOptions = GenerateDefaultRoomProperties();
		targetRoomOptions.customRoomProperties["lg"] = latencyGroup;
		targetRoomOptions.customRoomProperties["gameMode"] = (int)MySingleton<Manager>.Instance.networkGame.miniGameSelected;
		Hashtable hashtable = new Hashtable();
		hashtable.Add("lg", latencyGroup);
		hashtable.Add("level", GetDefaultLevelByGame(MySingleton<Manager>.Instance.selectedGame));
		hashtable.Add("gameMode", (int)MySingleton<Manager>.Instance.networkGame.miniGameSelected);
		hashtable.Add("customFull", (byte)0);
		byte b = 6;
		Debug.Log("[NET]Buscando partida con el siguiente filtro:\n" + b + " jugadores de límite.\nPropiedades:" + hashtable);
		MySingleton<Manager>.Instance.networkManager.JoinRandomRoom(hashtable, b);
	}

	public void CreateRoom(RoomOptions roomOptions = null)
	{
		if (roomOptions == null)
		{
			MySingleton<Manager>.Instance.networkManager.CreateRoom(defaultRoomOptions);
		}
		else
		{
			MySingleton<Manager>.Instance.networkManager.CreateRoom(roomOptions);
		}
	}

	public void JoinRoom(string roomName = null)
	{
		if (roomName != null && roomName != string.Empty)
		{
			MySingleton<Manager>.Instance.networkManager.JoinRoom(roomName, defaultRoomOptions);
		}
		else
		{
			MySingleton<Manager>.Instance.networkManager.JoinRandomRoom();
		}
	}

	private void OnJoinedRoomCallback()
	{
		MySingleton<Manager>.Instance.gameMode = Manager.GameMode.Multiplayer;
		MySingleton<Manager>.Instance.networkManager.LoadLevel(2);
	}

	private void OnCreateRoomFailedCallback(object[] codeAndMsg)
	{
		Debug.LogError("Error al crear la sala.\nError: " + (short)codeAndMsg[0] + "\nMensaje:" + (string)codeAndMsg[1]);
	}

	private void OnJoinToRoomFailedCallback(object[] codeAndMsg)
	{
		Debug.LogError("Error al unirse a sala.\nError: " + (short)codeAndMsg[0] + "\nMensaje:" + (string)codeAndMsg[1]);
		if ((short)codeAndMsg[0] == 32765)
		{
			MySingleton<Manager>.Instance.guiMenuLogic.ShowMultiplayerNotification(MultiplayerNotificationPanel.NotificationType.RoomFull);
		}
		else if (MySingleton<Manager>.Instance.networkManager.connected)
		{
			MySingleton<Manager>.Instance.guiMenuLogic.ShowMultiplayerPanel();
		}
		else
		{
			MySingleton<Manager>.Instance.guiMenuLogic.ShowConnectingToMultiplayerModePanel();
		}
		targetRoom = null;
	}

	private void OnRandomJoinRoomFailedCallback(object[] codeAndMsg)
	{
		Debug.LogWarning("Error al unirse a sala random.\nError: " + (short)codeAndMsg[0] + "\nMensaje:" + (string)codeAndMsg[1]);
		if (targetRoomOptions != null)
		{
			CreateRoom(targetRoomOptions);
			targetRoomOptions = null;
		}
		else
		{
			CreateRoom();
		}
	}

	private void OnRoomListUpdatedCallback()
	{
	}

	public RoomInfo GetRoom(string name)
	{
		RoomInfo result = null;
		for (int i = 0; i < roomList.Length; i++)
		{
			if (roomList[i].name == name)
			{
				result = roomList[i];
				break;
			}
		}
		return result;
	}

	public Vector2 GetPlayersInRoom(string roomName)
	{
		Vector2 zero = Vector2.zero;
		RoomInfo room = GetRoom(roomName);
		if (room != null)
		{
			zero.x = room.playerCount;
			zero.y = (int)room.maxPlayers;
		}
		return zero;
	}
}
