using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

namespace AxesInMotion.Network
{
	public class NetworkManager : MonoBehaviour
	{
		public delegate void NetworkCallback();

		public delegate void NetworkCallbackWithMsg(object[] codeAndMsg);

		public delegate void NetworkCallbackWithCause(DisconnectionCause cause);

		public delegate void NetworkCallbackWithPlayer(NetworkPlayer player);

		public delegate void NetworkCallbackWithProperties(Hashtable propertiesThatChanged);

		public bool _connected;

		public bool _connecting;

		public bool _offline;

		public bool _inLobby;

		public string serverVersion;

		public PhotonLogLevel logLevel;

		public int sendRate = 20;

		public int sendRateOnSerialize = 10;

		public bool connected
		{
			get
			{
				return PhotonNetwork.connected && !offline;
			}
		}

		public bool connecting
		{
			get
			{
				return PhotonNetwork.connecting;
			}
		}

		public bool offline
		{
			get
			{
				return PhotonNetwork.offlineMode;
			}
			set
			{
				PhotonNetwork.offlineMode = value;
			}
		}

		public bool inLobby
		{
			get
			{
				return PhotonNetwork.insideLobby;
			}
		}

		public bool inRoom
		{
			get
			{
				return PhotonNetwork.inRoom;
			}
		}

		public int ping
		{
			get
			{
				return PhotonNetwork.GetPing();
			}
		}

		public NetworkPlayer player
		{
			get
			{
				return new NetworkPlayer(PhotonNetwork.player);
			}
		}

		public int playersCount
		{
			get
			{
				return PhotonNetwork.playerList.Length;
			}
		}

		public List<FriendInfo> friends
		{
			get
			{
				return PhotonNetwork.Friends;
			}
		}

		public NetworkPlayer[] playerList
		{
			get
			{
				NetworkPlayer[] array = new NetworkPlayer[PhotonNetwork.playerList.Length];
				for (int i = 0; i < PhotonNetwork.playerList.Length; i++)
				{
					array[i] = new NetworkPlayer(PhotonNetwork.playerList[i]);
				}
				return array;
			}
		}

		public Room currentRoom
		{
			get
			{
				return PhotonNetwork.room;
			}
		}

		public RoomInfo[] roomList
		{
			get
			{
				return PhotonNetwork.GetRoomList();
			}
		}

		public event NetworkCallback OnAIMConnect;

		public event NetworkCallback OnAIMDisconnect;

		public event NetworkCallback OnAIMConnectToMasterServer;

		public event NetworkCallbackWithCause OnAIMConnectionFail;

		public event NetworkCallbackWithCause OnAIMFailedToConnect;

		public event NetworkCallback OnAIMCreateRoom;

		public event NetworkCallback OnAIMJoinedRoom;

		public event NetworkCallback OnAIMLeftRoom;

		public event NetworkCallbackWithMsg OnAIMCreateRoomFailed;

		public event NetworkCallbackWithMsg OnAIMJoinRoomFailed;

		public event NetworkCallback OnAIMJoinedLobby;

		public event NetworkCallback OnAIMLeftLobby;

		public event NetworkCallbackWithPlayer OnAIMMasterClientSwitched;

		public event NetworkCallback OnAIMReceiveRoomListUpdate;

		public event NetworkCallbackWithPlayer OnAIMPlayerConnected;

		public event NetworkCallbackWithPlayer OnAIMPlayerDisconnected;

		public event NetworkCallbackWithMsg OnAIMRandomJoinFailed;

		public event NetworkCallback OnAIMMaxCccuReached;

		public event NetworkCallbackWithProperties OnAIMRoomPropertiesChanged;

		public event NetworkCallbackWithMsg OnAIMPlayerPropertiesChanged;

		public event NetworkCallback OnAIMUpdateFriendsList;

		private void Awake()
		{
			PhotonNetwork.sendRate = sendRate;
			PhotonNetwork.sendRateOnSerialize = sendRateOnSerialize;
		}

		private void Update()
		{
		}

		private void OnConnectedToPhoton()
		{
			if (OnAIMConnect != null)
			{
				OnAIMConnect();
			}
		}

		private void OnLeftRoom()
		{
			if (OnAIMLeftRoom != null)
			{
				OnAIMLeftRoom();
			}
		}

		private void OnMasterClientSwitched(PhotonPlayer newMasterClient)
		{
			if (OnAIMMasterClientSwitched != null)
			{
				OnAIMMasterClientSwitched(new NetworkPlayer(newMasterClient));
			}
		}

		private void OnPhotonCreateRoomFailed(object[] codeAndMsg)
		{
			if (OnAIMCreateRoomFailed != null)
			{
				OnAIMCreateRoomFailed(codeAndMsg);
			}
		}

		private void OnPhotonJoinRoomFailed(object[] codeAndMsg)
		{
			if (OnAIMJoinRoomFailed != null)
			{
				OnAIMJoinRoomFailed(codeAndMsg);
			}
		}

		private void OnCreatedRoom()
		{
			if (OnAIMCreateRoom != null)
			{
				OnAIMCreateRoom();
			}
		}

		private void OnJoinedLobby()
		{
			if (OnAIMJoinedLobby != null)
			{
				OnAIMJoinedLobby();
			}
		}

		private void OnLeftLobby()
		{
			if (OnAIMLeftLobby != null)
			{
				OnAIMLeftLobby();
			}
		}

		private void OnFailedToConnectToPhoton(DisconnectCause cause)
		{
			if (OnAIMFailedToConnect != null)
			{
				OnAIMFailedToConnect(ParseDisconnectionCause(cause));
			}
		}

		private void OnConnectionFail(DisconnectCause cause)
		{
			if (OnAIMConnectionFail != null)
			{
				OnAIMConnectionFail(ParseDisconnectionCause(cause));
			}
		}

		private void OnDisconnectedFromPhoton()
		{
			if (OnAIMDisconnect != null)
			{
				OnAIMDisconnect();
			}
		}

		private void OnReceivedRoomListUpdate()
		{
			if (OnAIMReceiveRoomListUpdate != null)
			{
				OnAIMReceiveRoomListUpdate();
			}
		}

		private void OnJoinedRoom()
		{
			if (OnAIMJoinedRoom != null)
			{
				OnAIMJoinedRoom();
			}
		}

		private void OnPhotonPlayerConnected(PhotonPlayer newPlayer)
		{
			if (OnAIMPlayerConnected != null)
			{
				OnAIMPlayerConnected(new NetworkPlayer(newPlayer));
			}
		}

		private void OnPhotonPlayerDisconnected(PhotonPlayer otherPlayer)
		{
			if (OnAIMPlayerDisconnected != null)
			{
				OnAIMPlayerDisconnected(new NetworkPlayer(otherPlayer));
			}
		}

		private void OnPhotonRandomJoinFailed(object[] codeAndMsg)
		{
			if (OnAIMRandomJoinFailed != null)
			{
				OnAIMRandomJoinFailed(codeAndMsg);
			}
		}

		private void OnConnectedToMaster()
		{
			if (OnAIMConnectToMasterServer != null)
			{
				OnAIMConnectToMasterServer();
			}
		}

		private void OnPhotonMaxCccuReached()
		{
			if (OnAIMMaxCccuReached != null)
			{
				OnAIMMaxCccuReached();
			}
		}

		private void OnPhotonCustomRoomPropertiesChanged(Hashtable propertiesThatChanged)
		{
			if (OnAIMRoomPropertiesChanged != null)
			{
				OnAIMRoomPropertiesChanged(propertiesThatChanged);
			}
		}

		private void OnPhotonPlayerPropertiesChanged(object[] playerAndUpdatedProps)
		{
			if (OnAIMPlayerPropertiesChanged != null)
			{
				OnAIMPlayerPropertiesChanged(playerAndUpdatedProps);
			}
		}

		private void OnUpdatedFriendList()
		{
			if (OnAIMUpdateFriendsList != null)
			{
				OnAIMUpdateFriendsList();
			}
		}

		public void Connect(bool autoJoinLobby)
		{
			PhotonNetwork.autoJoinLobby = autoJoinLobby;
			PhotonNetwork.ConnectUsingSettings(serverVersion);
		}

		public void Disconnect()
		{
			PhotonNetwork.Disconnect();
		}

		public void CreateRoom(RoomOptions roomOptions, string roomName = null, TypedLobby typedLobby = null)
		{
			string roomName2 = ((roomName != null) ? roomName : Guid.NewGuid().ToString());
			TypedLobby typedLobby2 = ((typedLobby != null) ? typedLobby : TypedLobby.Default);
			PhotonNetwork.CreateRoom(roomName2, roomOptions, typedLobby2);
		}

		public void JoinRoom(string roomName, RoomOptions fallbackRoomOptions = null)
		{
			RoomOptions roomOptions = ((fallbackRoomOptions != null) ? fallbackRoomOptions : MySingleton<Manager>.Instance.networkRooms.defaultRoomOptions);
			PhotonNetwork.JoinOrCreateRoom(roomName, roomOptions, TypedLobby.Default);
		}

		public void JoinRandomRoom()
		{
			PhotonNetwork.JoinRandomRoom();
		}

		public void JoinRandomRoom(Hashtable filter, byte maxPlayers)
		{
			PhotonNetwork.JoinRandomRoom(filter, maxPlayers);
		}

		public void LoadLevel(string sceneName)
		{
			PhotonNetwork.LoadLevel(sceneName);
		}

		public void LoadLevel(int sceneNumber)
		{
			PhotonNetwork.LoadLevel(sceneNumber);
		}

		public GameObject Instantiate(string prefabName, Vector3 position, Quaternion rotation, int group = 0, object[] data = null)
		{
			return PhotonNetwork.Instantiate(prefabName, position, rotation, group, data);
		}

		public void FindFriends(string[] friendsToFind)
		{
			PhotonNetwork.FindFriends(friendsToFind);
		}

		private DisconnectionCause ParseDisconnectionCause(DisconnectCause cause)
		{
			switch (cause)
			{
			case DisconnectCause.Inactivity:
				return DisconnectionCause.Inactivity;
			case DisconnectCause.LatencyTooHigh:
				return DisconnectionCause.LatencyTooHigh;
			case DisconnectCause.ExceptionOnConnect:
				return DisconnectionCause.ExceptionOnConnect;
		case DisconnectCause.DisconnectByClientTimeout:
				return DisconnectionCause.DisconnectByClientTimeout;
			case DisconnectCause.InternalReceiveException:
				return DisconnectionCause.InternalReceiveException;
		case DisconnectCause.DisconnectByServerTimeout:
				return DisconnectionCause.DisconnectByServerTimeout;
			case DisconnectCause.DisconnectByServerLogic:
				return DisconnectionCause.DisconnectByServerLogic;
			case DisconnectCause.DisconnectByServerUserLimit:
				return DisconnectionCause.DisconnectByServerUserLimit;
			case DisconnectCause.Exception:
				return DisconnectionCause.Exception;
			case DisconnectCause.InvalidRegion:
				return DisconnectionCause.InvalidRegion;
			case DisconnectCause.MaxCcuReached:
				return DisconnectionCause.MaxCcuReached;
			case DisconnectCause.InvalidAuthentication:
				return DisconnectionCause.InvalidAuthentication;
			default:
				return DisconnectionCause.Exception;
			}
		}
	}
}
