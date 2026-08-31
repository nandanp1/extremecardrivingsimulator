using AxesInMotion.Network;
using AxesInMotion.Network.Minigames;
using ExitGames.Client.Photon;
using UnityEngine;

public class NetworkLog : MonoBehaviour
{
	private const string TAG = "[NET] ";

	private NetworkManager networkManager;

	private void Awake()
	{
		Object.DestroyImmediate(this);
	}

	private void Start()
	{
		networkManager = MySingleton<Manager>.Instance.networkManager;
		if ((bool)networkManager)
		{
			networkManager.OnAIMConnect += OnConnectLog;
			networkManager.OnAIMDisconnect += OnDisconnectLog;
			networkManager.OnAIMConnectToMasterServer += OnConnectToMasterServerLog;
			networkManager.OnAIMConnectionFail += OnConnectionFailLog;
			networkManager.OnAIMFailedToConnect += OnFailedToConnectLog;
			networkManager.OnAIMCreateRoom += OnCreateRoomLog;
			networkManager.OnAIMLeftRoom += OnLeftRoomLog;
			networkManager.OnAIMCreateRoomFailed += OnCreateRoomFailedLog;
			networkManager.OnAIMJoinRoomFailed += OnJoinRoomFailedLog;
			networkManager.OnAIMJoinedLobby += OnJoinedLobbyLog;
			networkManager.OnAIMLeftLobby += OnLeftLobbyLog;
			networkManager.OnAIMMasterClientSwitched += OnMasterClientSwitchedLog;
			networkManager.OnAIMReceiveRoomListUpdate += OnReceiveRoomListUpdateLog;
			networkManager.OnAIMPlayerConnected += OnPlayerConnectedLog;
			networkManager.OnAIMPlayerDisconnected += OnPlayerDisconnectedLog;
			networkManager.OnAIMRandomJoinFailed += OnRandomJoinFailedLog;
			networkManager.OnAIMMaxCccuReached += OnMaxCccuReachedLog;
			networkManager.OnAIMRoomPropertiesChanged += OnRoomPropertiesChangedLog;
			networkManager.OnAIMPlayerPropertiesChanged += OnPlayerPropertiesChangedLog;
		}
	}

	private void OnConnectLog()
	{
		Debug.Log("[NET] Conectado a servidor");
	}

	private void OnDisconnectLog()
	{
		Debug.Log("[NET] Desconectado del servidor");
	}

	private void OnConnectToMasterServerLog()
	{
		Debug.Log("[NET] Conectado a MasterServer");
	}

	private void OnConnectionFailLog(DisconnectionCause cause)
	{
		Debug.Log("[NET] La conexión actual ha fallado. Causa: " + cause);
	}

	private void OnFailedToConnectLog(DisconnectionCause cause)
	{
		Debug.Log("[NET] Intento de conexión fallido. Causa: " + cause);
	}

	private void OnCreateRoomLog()
	{
		Debug.Log("[NET] Room creada");
	}

	private void OnLeftRoomLog()
	{
		Debug.Log("[NET] Room abandonada.");
	}

	private void OnCreateRoomFailedLog(object[] codeAndMsg)
	{
		Debug.Log("[NET] Error al crear Room. Error: " + (short)codeAndMsg[0] + " - " + codeAndMsg[1]);
	}

	private void OnJoinRoomFailedLog(object[] codeAndMsg)
	{
		if (codeAndMsg.Length > 0)
		{
			Debug.Log("[NET] Error al entrar en la Room. Error: " + (short)codeAndMsg[0] + " - " + codeAndMsg[1]);
		}
		else
		{
			Debug.Log("[NET] Error al entrar en la Room.");
		}
	}

	private void OnJoinedLobbyLog()
	{
		Debug.Log("[NET] Te has conectado al Lobby.");
	}

	private void OnLeftLobbyLog()
	{
		Debug.Log("[NET] Te has desconectado del Lobby.");
	}

	private void OnMasterClientSwitchedLog(NetworkPlayer newMasterClient)
	{
		Debug.Log("[NET] " + newMasterClient.name + " es ahora el Master Client.");
	}

	private void OnReceiveRoomListUpdateLog()
	{
		Debug.Log("[NET] Lista de Rooms actualizada.");
	}

	private void OnPlayerConnectedLog(NetworkPlayer player)
	{
		Debug.Log("[NET] Jugador conectado. " + player.name + ":" + player.UPID);
	}

	private void OnPlayerDisconnectedLog(NetworkPlayer player)
	{
		Debug.Log("[NET] Jugador desconectado. " + player.name + ":" + player.UPID);
	}

	private void OnRandomJoinFailedLog(object[] codeAndMsg)
	{
		Debug.Log("[NET] Error al entrar en una Room de forma aleatoria. Error: " + (short)codeAndMsg[0] + " - " + codeAndMsg[1]);
	}

	private void OnMaxCccuReachedLog()
	{
		Debug.Log("[NET] Limite de jugadores concurrentes alcanzado");
	}

	private void OnRoomPropertiesChangedLog(Hashtable propertiesThatChanged)
	{
		Debug.Log("[NET] Propiedades de la Room actual modificados.\n" + propertiesThatChanged.ToStringFull());
	}

	private void OnPlayerPropertiesChangedLog(object[] codeAndMsg)
	{
		string text = "[NET] El jugador " + new NetworkPlayer((PhotonPlayer)codeAndMsg[0]).name + " ha cambiado sus propiedades";
		Hashtable origin = codeAndMsg[1] as Hashtable;
		text = text + "\nPropiedades nuevas: " + origin.ToStringFull();
		text = text + "\nPropiedades totales: " + new NetworkPlayer((PhotonPlayer)codeAndMsg[0]).properties.ToStringFull();
		Debug.Log(text);
	}

	private void OnGUI()
	{
		GUIStyle gUIStyle = new GUIStyle(GUI.skin.label);
		gUIStyle.fontStyle = FontStyle.Bold;
		gUIStyle.fontSize = 12;
		gUIStyle.normal.textColor = Color.red;
		gUIStyle.fontSize = 25;
		if (MySingleton<Manager>.Instance.networkGame.miniGameSelected == MiniGameLogic.MiniGameType.None)
		{
			GUILayout.Label("Frame rate: " + GetComponent<AutoQualityChooser>().currentFramerate, gUIStyle);
			GUILayout.Label("FB Logged: " + MySingleton<Manager>.Instance.facebookManager.isLoggedIn, gUIStyle);
			GUILayout.Label("Offline Mode: " + MySingleton<Manager>.Instance.networkManager.offline, gUIStyle);
			GUILayout.Label("GameMode " + MySingleton<Manager>.Instance.gameMode, gUIStyle);
			GUILayout.Label("Connection Lost: " + MySingleton<Manager>.Instance.connection.connectionLost, gUIStyle);
			GUILayout.Label("Connected: " + MySingleton<Manager>.Instance.networkManager.connected, gUIStyle);
			GUILayout.Label("In lobby: " + MySingleton<Manager>.Instance.networkManager.inLobby, gUIStyle);
			if (!networkManager.connected)
			{
				return;
			}
			GUILayout.Label("Ping: " + networkManager.ping, gUIStyle);
			GUILayout.Label("Public name: " + networkManager.player.name, gUIStyle);
			GUILayout.Label("UPID: " + networkManager.player.UPID, gUIStyle);
			if (networkManager.inLobby)
			{
				GUILayout.Label("Rooms", gUIStyle);
				RoomInfo[] roomList = networkManager.roomList;
				foreach (RoomInfo roomInfo in roomList)
				{
					GUILayout.Label(roomInfo.ToStringFull(), gUIStyle);
				}
				GUILayout.Label("Friends", gUIStyle);
				foreach (Friend friend in MySingleton<Manager>.Instance.networkFriends.friends)
				{
					GUILayout.Label(friend.ToString(), gUIStyle);
				}
			}
			if (!MySingleton<Manager>.Instance.networkManager.inRoom)
			{
				return;
			}
			GUILayout.Label("Room properties", gUIStyle);
			GUILayout.Label(MySingleton<Manager>.Instance.networkRooms.currentRoom.ToStringFull(), gUIStyle);
			GUILayout.Label("Players count " + networkManager.playersCount, gUIStyle);
			NetworkPlayer[] playerList = MySingleton<Manager>.Instance.networkManager.playerList;
			foreach (NetworkPlayer networkPlayer in playerList)
			{
				if (networkPlayer.isMaster)
				{
					GUILayout.Label("Master", gUIStyle);
				}
				GUILayout.Label(networkPlayer.ToStringFull(), gUIStyle);
			}
		}
		else if (MySingleton<Manager>.Instance.networkGame.miniGameSelected == MiniGameLogic.MiniGameType.Racing)
		{
			GUILayout.BeginVertical();
			GUILayout.BeginHorizontal(gUIStyle);
			GUILayout.BeginVertical(gUIStyle);
			GUILayout.Label("Name:", gUIStyle);
			GUILayout.Label("UPID:", gUIStyle);
			GUILayout.Label("Ready:", gUIStyle);
			GUILayout.Label("Spectator:", gUIStyle);
			GUILayout.Label("Master:", gUIStyle);
			GUILayout.EndVertical();
			GUILayout.BeginVertical(gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.EndVertical();
			for (int k = 0; k < MySingleton<Manager>.Instance.networkGame.playerCount; k++)
			{
				NetworkPlayer networkPlayer2 = MySingleton<Manager>.Instance.networkGame.playerList[k];
				GUILayout.BeginVertical(gUIStyle);
				GUILayout.Label(networkPlayer2.name, gUIStyle);
				GUILayout.Label(networkPlayer2.UPID, gUIStyle);
				GUILayout.Label(networkPlayer2.isReady.ToString(), gUIStyle);
				GUILayout.Label(networkPlayer2.isSpectator.ToString(), gUIStyle);
				GUILayout.Label(networkPlayer2.isMaster.ToString(), gUIStyle);
				GUILayout.EndVertical();
				GUILayout.BeginVertical(gUIStyle);
				GUILayout.Label("|", gUIStyle);
				GUILayout.Label("|", gUIStyle);
				GUILayout.Label("|", gUIStyle);
				GUILayout.Label("|", gUIStyle);
				GUILayout.Label("|", gUIStyle);
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			GUILayout.BeginHorizontal(gUIStyle);
			GUILayout.BeginVertical(gUIStyle);
			GUILayout.Label("cFull:", gUIStyle);
			GUILayout.Label("GStatus:", gUIStyle);
			GUILayout.Label("Track:", gUIStyle);
			GUILayout.Label("GMode:", gUIStyle);
			GUILayout.Label("LG:", gUIStyle);
			GUILayout.Label("Name:", gUIStyle);
			GUILayout.EndVertical();
			GUILayout.BeginVertical();
			GUILayout.Label("|", gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.Label("|", gUIStyle);
			GUILayout.EndVertical();
			if (MySingleton<Manager>.Instance.networkRooms.currentRoom != null)
			{
				GUILayout.BeginVertical(gUIStyle);
				GUILayout.Label(MySingleton<Manager>.Instance.networkRooms.GetCustomFull().ToString(), gUIStyle);
				GUILayout.Label(MySingleton<Manager>.Instance.networkRooms.GetGameStatus().ToString(), gUIStyle);
				GUILayout.Label(MySingleton<Manager>.Instance.networkRooms.GetSelectedTrack().ToString(), gUIStyle);
				GUILayout.Label(MySingleton<Manager>.Instance.networkRooms.currentRoom.customProperties["gameMode"].ToString(), gUIStyle);
				GUILayout.Label(MySingleton<Manager>.Instance.networkRooms.currentRoom.customProperties["lg"].ToString(), gUIStyle);
				GUILayout.Label(MySingleton<Manager>.Instance.networkRooms.currentRoom.name, gUIStyle);
				GUILayout.EndVertical();
			}
			GUILayout.EndHorizontal();
			GUILayout.EndVertical();
		}
	}
}
