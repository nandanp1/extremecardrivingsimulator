using System;
using System.Collections;
using AxesInMotion.Network.Minigames;
using UnityEngine;

namespace AxesInMotion.Network
{
	public class Connection : MonoBehaviour
	{
		[Serializable]
		public struct ReconnectData
		{
			public string roomName;

			public Vector3 position;

			public Quaternion rotation;

			public MiniGameLogic.MiniGameType miniGameType;

			public ReconnectData(string roomName, MiniGameLogic.MiniGameType minigameType, Vector3 p, Quaternion r)
			{
				this.roomName = roomName;
				miniGameType = minigameType;
				position = p;
				rotation = r;
			}
		}

		public int maxPlayersPerRoom = 6;

		public bool autoJoinInLobby = true;

		public bool autoJoinInRoom;

		public bool connectionLost;

		public bool lostInGame;

		public int averagePing = -1;

		public int pingSamples;

		public DisconnectionCause connectionLostCause;

		public ReconnectData reconnectionData;

		public bool connected
		{
			get
			{
				return MySingleton<Manager>.Instance.networkManager.connected;
			}
		}

		public bool connecting
		{
			get
			{
				return MySingleton<Manager>.Instance.networkManager.connecting;
			}
		}

		private void Start()
		{
			if ((bool)MySingleton<Manager>.Instance.networkManager)
			{
				MySingleton<Manager>.Instance.networkManager.OnAIMJoinedLobby += OnJoinedLobbyCallback;
				MySingleton<Manager>.Instance.networkManager.OnAIMConnect += OnConnectCallback;
				MySingleton<Manager>.Instance.networkManager.OnAIMConnectionFail += OnConnectionFailCallback;
				MySingleton<Manager>.Instance.networkManager.OnAIMDisconnect += OnDisconnectCallback;
			}
		}

		private void OnJoinedLobbyCallback()
		{
			if (autoJoinInRoom)
			{
				if (connectionLost)
				{
					MySingleton<Manager>.Instance.networkGame.miniGameSelected = reconnectionData.miniGameType;
					MySingleton<Manager>.Instance.networkRooms.JoinRoom(reconnectionData.roomName);
				}
				else
				{
					MySingleton<Manager>.Instance.networkRooms.Play();
				}
			}
		}

		private void OnConnectionFailCallback(DisconnectionCause cause)
		{
			Debug.Log("Fallo de conexion. Causa: " + cause);
			connectionLost = true;
			lostInGame = MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Multiplayer;
			connectionLostCause = cause;
			LegacySceneManager.LoadLevel(1);
		}

		private void OnConnectCallback()
		{
			MySingleton<Manager>.Instance.networkManager.offline = false;
			StartToCalculateAveragePing();
		}

		private void OnDisconnectCallback()
		{
			if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Multiplayer)
			{
				LegacySceneManager.LoadLevel(1);
			}
			MySingleton<Manager>.Instance.networkManager.offline = true;
		}

		public void Connect(bool autoJoinInLobby = true, bool autoJoinRoom = true, bool forceConnection = false)
		{
			if (MySingleton<Manager>.Instance.networkManager.connected)
			{
				if (!forceConnection)
				{
					return;
				}
				Disconnect();
			}
			autoJoinInRoom = autoJoinRoom;
			InitializePlayerProperties();
			MySingleton<Manager>.Instance.networkManager.Connect(autoJoinInLobby);
		}

		public void Reconnect()
		{
			Connect();
		}

		public void Disconnect(DisconnectionCause cause = DisconnectionCause.DisconnectByServerLogic)
		{
			if (connected)
			{
				if (cause != DisconnectionCause.DisconnectByClient && cause != DisconnectionCause.DisconnectByServerLogic)
				{
					connectionLost = true;
					lostInGame = MySingleton<Manager>.Instance.networkManager.inRoom;
					connectionLostCause = cause;
				}
				MySingleton<Manager>.Instance.networkManager.Disconnect();
			}
		}

		private void InitializePlayerProperties()
		{
			InfoCar component = MySingleton<Manager>.Instance.guiMenuLogic.currentCar.GetComponent<InfoCar>();
			Color carColor = SettingsManager.GetCarColor(component.id, component.defaultColor);
			NetworkPlayer player = MySingleton<Manager>.Instance.networkManager.player;
			player.SetColor(carColor);
			player.SetVersion(SettingsManager.GetMultiplayerVersion());
			player.SetDateVersion(SettingsManager.GetMultiplayerDateVersion());
			player.SetIsObsolete(SettingsManager.GetIsObsolete());
			player.isSpectator = false;
			player.isReady = false;
			player.name = SettingsManager.GetPlayername();
			player.UPID = SettingsManager.GetUPID();
		}

		public void StartToCalculateAveragePing()
		{
			StartCoroutine("CalculateAveragePing");
		}

		public void StopToCalculateAveragePing()
		{
			StopCoroutine("CalculateAveragePing");
		}

		private IEnumerator CalculateAveragePing()
		{
			int[] data = new int[50];
			averagePing = -1;
			pingSamples = 0;
			while (MySingleton<Manager>.Instance.connection.connected && !MySingleton<Manager>.Instance.networkManager.inRoom)
			{
				int lastPing = MySingleton<Manager>.Instance.networkManager.ping;
				pingSamples++;
				if (pingSamples >= 50)
				{
					for (int i = 1; i < data.Length; i++)
					{
						data[i - 1] = data[i];
					}
					data[data.Length - 1] = lastPing;
				}
				else
				{
					data[pingSamples] = lastPing;
				}
				int acum = 0;
				for (int j = 0; j < data.Length; j++)
				{
					acum += data[j];
				}
				averagePing = acum / Mathf.Min(pingSamples, 50);
				yield return new WaitForSeconds(0.5f);
			}
		}
	}
}
