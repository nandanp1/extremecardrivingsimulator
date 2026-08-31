using System.Collections.Generic;
using AxesInMotion.Network.Minigames;
using UnityEngine;

namespace AxesInMotion.Network
{
	public class NetworkGame : MonoBehaviour
	{
		public delegate void RemoteCarInstantiatedCallback(GameObject car, NetworkPlayer owner);

		public const string RACING_ADDITIVE_SCENE = "RacingMiniGame";

		public Dictionary<NetworkPlayer, Transform> carsByPlayers;

		public MiniGameLogic.MiniGameType miniGameSelected;

		private MiniGameLogic _miniGameLogic;

		public NetworkPlayer[] playerList
		{
			get
			{
				return MySingleton<Manager>.Instance.networkManager.playerList;
			}
		}

		public int playerCount
		{
			get
			{
				return MySingleton<Manager>.Instance.networkManager.playersCount;
			}
		}

		public MiniGameLogic miniGameLogic
		{
			get
			{
				switch (miniGameSelected)
				{
				case MiniGameLogic.MiniGameType.None:
					Debug.LogError("Se ha intentado acceder a un MiniGameLogic cuando no estamos en un minijuego");
					return null;
				case MiniGameLogic.MiniGameType.Racing:
					return _miniGameLogic;
				default:
					return null;
				}
			}
			set
			{
				_miniGameLogic = value;
			}
		}

		public event RemoteCarInstantiatedCallback OnRemoteCarInstantiated;

		private void Awake()
		{
			carsByPlayers = new Dictionary<NetworkPlayer, Transform>();
			MySingleton<Manager>.Instance.networkManager.OnAIMJoinedRoom += OnPlayerJoinRoom;
			MySingleton<Manager>.Instance.networkManager.OnAIMPlayerConnected += OnPlayerConnect;
			MySingleton<Manager>.Instance.networkManager.OnAIMPlayerDisconnected += OnPlayerDisconnect;
			MySingleton<Manager>.Instance.networkManager.OnAIMDisconnect += OnDisconnect;
		}

		private void OnPlayerJoinRoom()
		{
			NetworkPlayer[] array = MySingleton<Manager>.Instance.networkManager.playerList;
			foreach (NetworkPlayer networkPlayer in array)
			{
				if (!networkPlayer.isLocal)
				{
					NetworkVersion.CheckPlayerVersionOf(networkPlayer);
					if (MySingleton<Manager>.Instance.networkFriends.IsFriend(networkPlayer))
					{
						UpdateNameFriend(networkPlayer);
					}
				}
			}
		}

		private void OnPlayerConnect(NetworkPlayer newPlayer)
		{
			if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Multiplayer)
			{
				NetworkVersion.CheckPlayerVersionOf(newPlayer);
				MySingleton<Manager>.Instance.guiGameLogic.pnChat.GetComponent<ChatPanel>().PlayerAddMessage(newPlayer);
				if (MySingleton<Manager>.Instance.networkFriends.IsFriend(newPlayer))
				{
					UpdateNameFriend(newPlayer);
				}
			}
		}

		private void OnPlayerDisconnect(NetworkPlayer otherPlayer)
		{
			MySingleton<Manager>.Instance.guiGameLogic.pnChat.GetComponent<ChatPanel>().PlayerLeftMessage(otherPlayer);
			carsByPlayers.Remove(otherPlayer);
		}

		private void OnDisconnect()
		{
			carsByPlayers.Clear();
		}

		public void RemoteCarInstantiated(GameObject car, NetworkPlayer player)
		{
			if (carsByPlayers.ContainsKey(player))
			{
				carsByPlayers[player] = car.transform;
			}
			else
			{
				carsByPlayers.Add(player, car.transform);
			}
			Debug.Log("RemoteCarInstantiated " + player.ID + " " + carsByPlayers[player]);
			if (OnRemoteCarInstantiated != null)
			{
				OnRemoteCarInstantiated(car, player);
			}
		}

		public void SetIgnorePlayer(NetworkPlayer player, bool ignored)
		{
			string uPID = player.UPID;
			string text = SettingsManager.GetIgnorePlayers();
			if (ignored)
			{
				if (uPID != "none")
				{
					if (!text.Contains(uPID))
					{
						text = string.Concat(text, uPID + "-");
					}
				}
				else if (player.name != "New Player" && !text.Contains(player.name))
				{
					text = string.Concat(text, player.name + "-");
				}
			}
			else
			{
				text = text.Replace(uPID + "-", string.Empty);
				text = text.Replace(player.name + "-", string.Empty);
			}
			SettingsManager.SetIgnorePlayers(text);
		}

		private void UpdateNameFriend(NetworkPlayer player)
		{
			MySingleton<Manager>.Instance.networkFriends.UpdateNameFriend(player.UPID, player.name);
		}

		public void DisableOfflineElements()
		{
			InfoZone infoZone = Object.FindFirstObjectByType<InfoZone>();
			infoZone.DisableWorkshops();
			Object.FindFirstObjectByType<GamePersistentInfo>().EnableNetworkBounds();
			if (infoZone.collectablesZoneParent != null)
			{
				infoZone.collectablesZoneParent.SetActive(false);
			}
			GamePersistentInfo gamePersistentInfo = Object.FindFirstObjectByType<GamePersistentInfo>();
			gamePersistentInfo.collectablesAllZones.SetActive(false);
			ChallangeModeManager challangeModeManager = Object.FindFirstObjectByType<ChallangeModeManager>();
			if (challangeModeManager != null)
			{
				challangeModeManager.gameObject.SetActive(false);
			}
			MySingleton<Manager>.Instance.guiGameLogic.exclusiveFreeElements.SetActive(false);
		}

		public void InitializeMiniGame()
		{
			MiniGameLogic.MiniGameType miniGameType = miniGameSelected;
			if (miniGameType == MiniGameLogic.MiniGameType.Racing)
			{
				LegacySceneManager.LoadLevelAdditive("RacingMiniGame");
			}
		}

		public void InstantiatePlayerCar()
		{
			Transform spawn = Object.FindFirstObjectByType<RandomSpawns>().GetSpawn();
			Vector3 position;
			Quaternion rotation;
			if (MySingleton<Manager>.Instance.connection.connectionLost)
			{
				position = MySingleton<Manager>.Instance.connection.reconnectionData.position;
				rotation = MySingleton<Manager>.Instance.connection.reconnectionData.rotation;
				MySingleton<Manager>.Instance.connection.reconnectionData = new Connection.ReconnectData(MySingleton<Manager>.Instance.networkRooms.currentRoom.name, MySingleton<Manager>.Instance.networkGame.miniGameSelected, base.transform.position, base.transform.rotation);
				MySingleton<Manager>.Instance.connection.connectionLost = false;
			}
			else
			{
				position = spawn.position;
				rotation = spawn.rotation;
			}
			MySingleton<Manager>.Instance.networkManager.Instantiate("NetworkCarInstantiater", position, rotation);
		}
	}
}
