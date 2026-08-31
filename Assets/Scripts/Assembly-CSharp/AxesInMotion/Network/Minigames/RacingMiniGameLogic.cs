using System;
using System.Collections;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using UnityEngine;

namespace AxesInMotion.Network.Minigames
{
	public class RacingMiniGameLogic : MiniGameLogic
	{
		public enum MiniGamePhase
		{
			None = 0,
			Waiting = 1,
			Preparing = 2,
			Running = 3,
			Finishing = 4,
			ShowingAds = 5
		}

		protected class ComparableByOwnerID : IComparer<Transform>
		{
			public int Compare(Transform x, Transform y)
			{
				if (!x && !y)
				{
					return 0;
				}
				if (!x)
				{
					int ownerId = y.GetComponent<PhotonView>().ownerId;
					return int.MaxValue.CompareTo(ownerId);
				}
				if (!y)
				{
					int ownerId2 = x.GetComponent<PhotonView>().ownerId;
					return ownerId2.CompareTo(int.MaxValue);
				}
				int ownerId3 = x.GetComponent<PhotonView>().ownerId;
				int ownerId4 = y.GetComponent<PhotonView>().ownerId;
				return ownerId3.CompareTo(ownerId4);
			}
		}

		private const string GUILOGIC_NAME = "pnRacingMiniGame";

		private const int REWARD_FIRST = 50;

		private const int REWARD_SECOND = 30;

		private const int REWARD_THIRTH = 20;

		private const int REWARD_FOURTH_OR_LOWER = 10;

		private const int COUNTDOWN_WAITING = 10;

		private const int COUNTDOWN_START = 3;

		private const int COUNTDOWN_FINAL = 10;

		private const int COUNTDOWN_RESET = 50;

		public const byte MAX_REAL_PLAYER_COUNT = 4;

		public double currentServerTimestamp;

		private int yourPosition;

		public int selectedTrack;

		private GUIRacingMiniGameLogic guiLogic;

		private NetworkPlayer yourPlayer;

		private SortedDictionary<double, NetworkPlayer> playersByFinishTime;

		private List<NetworkPlayer> playerRanking;

		public Track[] tracks;

		public MiniGamePhase currentPhase;

		private bool waitingCountdownActive;

		private bool startCountdownActive;

		private bool finalCountdownActive;

		private bool resetCountdownActive;

		private bool rewardGiven;

		private bool goalReached;

		private int playerCount
		{
			get
			{
				return MySingleton<Manager>.Instance.networkGame.playerCount;
			}
		}

		private int playersReadyCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < MySingleton<Manager>.Instance.networkGame.playerCount; i++)
				{
					if (MySingleton<Manager>.Instance.networkGame.playerList[i].isReady)
					{
						num++;
					}
				}
				return num;
			}
		}

		private int spectatorsCount
		{
			get
			{
				int num = 0;
				for (int i = 0; i < MySingleton<Manager>.Instance.networkGame.playerCount; i++)
				{
					if (MySingleton<Manager>.Instance.networkGame.playerList[i].isSpectator)
					{
						num++;
					}
				}
				return num;
			}
		}

		private bool someCountdownIsActive
		{
			get
			{
				return waitingCountdownActive || startCountdownActive || finalCountdownActive || resetCountdownActive;
			}
		}

		[ContextMenu("Load Race Data")]
		public void LoadRaceData()
		{
			Transform transform = base.transform.Find("Tracks");
			if ((bool)transform)
			{
				tracks = transform.GetComponentsInChildren<Track>();
				Track[] array = tracks;
				foreach (Track track in array)
				{
					track.raceLogic = this;
					track.LoadTrackData();
				}
			}
		}

		protected override void Awake()
		{
			base.Awake();
			if (MySingleton<Manager>.Instance.networkRooms.GetCustomFull())
			{
				MySingleton<Manager>.Instance.connection.Disconnect(DisconnectionCause.DisconnectBecauseOfRoomIsFull);
				return;
			}
			playersByFinishTime = new SortedDictionary<double, NetworkPlayer>();
			playerRanking = new List<NetworkPlayer>();
			yourPlayer = MySingleton<Manager>.Instance.networkManager.player;
			yourPosition = 0;
			InitGUILogic();
			MySingleton<Manager>.Instance.networkManager.OnAIMRoomPropertiesChanged += OnRoomPropertiesChanged;
			MySingleton<Manager>.Instance.networkManager.OnAIMPlayerPropertiesChanged += OnPlayerPropertiesChanged;
			MySingleton<Manager>.Instance.networkManager.OnAIMMasterClientSwitched += OnMasterClientChanged;
			currentPhase = (MiniGamePhase)MySingleton<Manager>.Instance.networkRooms.GetGameStatus();
			DecideWhatToDoOnJoined();
			DecideWhatToDoOnPlayerCountChanged();
			UpdatePlayersCarsOnAwake();
		}

		private void InitGUILogic()
		{
			Transform transform = MySingleton<Manager>.Instance.guiGameLogic.miniGamesParent.Find("pnRacingMiniGame");
			guiLogic = transform.GetComponent<GUIRacingMiniGameLogic>();
			guiLogic.gameObject.SetActive(true);
		}

		private void DecideWhatToDoOnJoined()
		{
			if (yourPlayer.isMaster && currentPhase == MiniGamePhase.None)
			{
				yourPlayer.isReady = true;
				ChangePhase(MiniGamePhase.Waiting);
			}
			else if (currentPhase == MiniGamePhase.Waiting)
			{
				yourPlayer.isReady = true;
				ChangePhase(currentPhase);
			}
			else if (currentPhase == MiniGamePhase.ShowingAds)
			{
				if (playersReadyCount == 0)
				{
					MySingleton<Manager>.Instance.connection.Disconnect(DisconnectionCause.DisconnectBecauseOfRoomIsFull);
					return;
				}
				yourPlayer.isReady = true;
				ChangePhase(currentPhase);
			}
			else if (currentPhase != MiniGamePhase.Waiting)
			{
				yourPlayer.isReady = false;
				yourPlayer.isSpectator = true;
				ChangePhase(currentPhase);
				if (currentPhase == MiniGamePhase.Finishing && !finalCountdownActive)
				{
					StartCoroutine("FinalCountdown");
				}
			}
		}

		private void DecideWhatToDoOnPlayerCountChanged()
		{
			if (yourPlayer.isMaster)
			{
				CheckCustomFull();
			}
			if (currentPhase == MiniGamePhase.Waiting)
			{
				if (playersReadyCount > 1)
				{
					if (!waitingCountdownActive)
					{
						StartCoroutine("WaitingCountdown");
					}
				}
				else if (playersReadyCount <= 1)
				{
					if (waitingCountdownActive)
					{
						waitingCountdownActive = false;
					}
					guiLogic.ShowWaitingForPlayers();
				}
			}
			else if (currentPhase == MiniGamePhase.Running)
			{
				if (playersReadyCount == 0)
				{
					ChangePhase(MiniGamePhase.Waiting);
				}
			}
			else if (currentPhase == MiniGamePhase.ShowingAds)
			{
				if (yourPlayer.isMaster)
				{
					SendFixedCountdownTimestamp(currentServerTimestamp);
				}
				if (playersReadyCount == 4 || playersReadyCount == base.playersCount)
				{
					ChangePhase(MiniGamePhase.Waiting);
				}
			}
		}

		private void UpdatePlayersCarsOnAwake()
		{
			foreach (NetworkPlayer key in MySingleton<Manager>.Instance.networkGame.carsByPlayers.Keys)
			{
				OnNewCarInstantiated(MySingleton<Manager>.Instance.networkGame.carsByPlayers[key].gameObject, key);
			}
		}

		private void SetPlayerAsSpectator(NetworkPlayer player, bool spectator)
		{
			if (player.isLocal)
			{
				if (spectator)
				{
					guiLogic.ShowSpectatorMode();
				}
				else
				{
					guiLogic.HideSpectatorMode();
				}
				Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Car"), LayerMask.NameToLayer("Spectator"), !spectator);
				Camera[] fixedCameras = MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarCameras>().FixedCameras;
				for (int i = 0; i < fixedCameras.Length; i++)
				{
					if ((bool)fixedCameras[i])
					{
						if (spectator)
						{
							int mask = LayerMask.GetMask("RemoteCar", "Spectator");
							fixedCameras[i].cullingMask |= mask;
						}
						else
						{
							int num = ~LayerMask.GetMask("Spectator");
							fixedCameras[i].cullingMask &= num;
							num = LayerMask.GetMask("RemoteCar");
							fixedCameras[i].cullingMask |= num;
						}
					}
				}
			}
			else
			{
				CarVisuals component = MySingleton<Manager>.Instance.networkGame.carsByPlayers[player].GetComponent<CarVisuals>();
				if (spectator)
				{
					ReplaceLayerHierarchically(component.exteriorRoot.transform, "RemoteCar", "Spectator");
					component.colliderBase.gameObject.layer = LayerMask.NameToLayer("Spectator");
					bool isSpectator = yourPlayer.isSpectator;
					component.nameMarker.SetActive(isSpectator);
					component.iconMarker.SetActive(isSpectator);
				}
				else
				{
					ReplaceLayerHierarchically(component.exteriorRoot.transform, "Spectator", "RemoteCar");
					component.colliderBase.gameObject.layer = LayerMask.NameToLayer("RemoteCar");
					component.nameMarker.SetActive(true);
					component.iconMarker.SetActive(false);
				}
			}
		}

		private void ChangePhase(MiniGamePhase newPhase)
		{
			waitingCountdownActive = false;
			startCountdownActive = false;
			finalCountdownActive = false;
			resetCountdownActive = false;
			switch (newPhase)
			{
			case MiniGamePhase.None:
				Debug.LogError("Se eligió cambiar a Fase None");
				break;
			case MiniGamePhase.Waiting:
				ResetGame();
				break;
			case MiniGamePhase.Preparing:
				PrepareCarsToStart();
				break;
			case MiniGamePhase.Running:
				StartGame();
				break;
			case MiniGamePhase.Finishing:
				Finish();
				break;
			case MiniGamePhase.ShowingAds:
				ShowAd();
				break;
			}
			currentPhase = newPhase;
			if (yourPlayer.isMaster)
			{
				MySingleton<Manager>.Instance.networkRooms.SetGameStatus((byte)newPhase);
			}
		}

		private void SelectNextTrack(int index = -1)
		{
			int num = 0;
			num = (selectedTrack = ((index >= 0 && index < tracks.Length) ? index : UnityEngine.Random.Range(0, tracks.Length)));
			MySingleton<Manager>.Instance.networkRooms.SetTrackSelected((byte)num);
		}

		public void SetTargetForWaypointArrow(Transform newTarget = null)
		{
			guiLogic.currentTargetArrowTransform = newTarget;
			guiLogic.arrowIsActive = guiLogic.currentTargetArrowTransform;
			guiLogic.wayPointArrow.gameObject.SetActive(guiLogic.arrowIsActive);
		}

		private void SendFixedCountdownTimestamp(double fixedTimestamp)
		{
			networkView.RPC("RPC_SendFixedCountdownTimestamp", PhotonTargets.All, fixedTimestamp);
		}

		[RPC]
		private void RPC_SendFixedCountdownTimestamp(double fixedTimestamp)
		{
			currentServerTimestamp = fixedTimestamp;
		}

		public void GoalReached()
		{
			if (!goalReached)
			{
				goalReached = true;
				finished = true;
				guiLogic.ShowFinalResultPanel();
				tracks[selectedTrack].DisableTrack();
				networkView.RPC("RPC_GoalReached", PhotonTargets.All, PhotonNetwork.time);
			}
		}

		[RPC]
		private void RPC_GoalReached(double timestamp, PhotonMessageInfo info)
		{
			NetworkPlayer networkPlayer = new NetworkPlayer(info.sender);
			if (!hasWinner)
			{
				hasWinner = true;
				if (!networkPlayer.isLocal)
				{
					guiLogic.ShowPlayerWon(networkPlayer.name);
				}
				RegisterPlayerScore(networkPlayer, timestamp);
				ChangePhase(MiniGamePhase.Finishing);
			}
			else
			{
				RegisterPlayerScore(networkPlayer, timestamp);
			}
		}

		private void RegisterPlayerScore(NetworkPlayer player, double score)
		{
			if (playersByFinishTime.ContainsKey(score))
			{
				playersByFinishTime.Add(score + 0.001, player);
			}
			else
			{
				playersByFinishTime.Add(score, player);
			}
			playerRanking.Clear();
			foreach (double key2 in playersByFinishTime.Keys)
			{
				double key = key2;
				playerRanking.Add(playersByFinishTime[key]);
			}
			if (player.isLocal)
			{
				yourPosition = playerRanking.IndexOf(player) + 1;
				guiLogic.SetFinalResultTextData(yourPosition, CalculateReward(yourPosition));
			}
			if (guiLogic.pnFinalResults.gameObject.activeSelf)
			{
				guiLogic.SetRankingData(playerRanking);
			}
		}

		private int CalculateReward(int position)
		{
			switch (position)
			{
			case 0:
				return 0;
			case 1:
				return 50;
			case 2:
				return 30;
			case 3:
				return 20;
			default:
				return 10;
			}
		}

		private void GiveReward(int position)
		{
			if (!rewardGiven)
			{
				new RewardCredit(CalculateReward(position)).GiveReward();
				rewardGiven = true;
			}
		}

		private void CheckCustomFull()
		{
			if (currentPhase != MiniGamePhase.ShowingAds)
			{
				Debug.Log("CHECK CFULL " + playersReadyCount + " + " + spectatorsCount + " >= " + (byte)4 + " && " + !MySingleton<Manager>.Instance.networkRooms.GetCustomFull());
				if (playersReadyCount + spectatorsCount >= 4 && !MySingleton<Manager>.Instance.networkRooms.GetCustomFull())
				{
					MySingleton<Manager>.Instance.networkRooms.SetCustomFull(true);
				}
				else if (playersReadyCount + spectatorsCount < 4 && MySingleton<Manager>.Instance.networkRooms.GetCustomFull())
				{
					MySingleton<Manager>.Instance.networkRooms.SetCustomFull(false);
				}
			}
		}

		private IEnumerator WaitingCountdown()
		{
			if (waitingCountdownActive)
			{
				Debug.Log("Intento WaitingCountdown fallido");
				yield break;
			}
			Debug.Log("Intento WaitingCountdown con exito");
			waitingCountdownActive = true;
			currentServerTimestamp = PhotonNetwork.time;
			if (yourPlayer.isMaster)
			{
				SendFixedCountdownTimestamp(currentServerTimestamp);
			}
			float seconds = 10f - (float)(PhotonNetwork.time - currentServerTimestamp);
			while (seconds > 1f && waitingCountdownActive)
			{
				seconds = 10f - (float)(PhotonNetwork.time - currentServerTimestamp);
				guiLogic.SetWaitCountdown((int)seconds);
				yield return null;
			}
			if (waitingCountdownActive)
			{
				Debug.Log("WaitingCountdown finalizada");
				waitingCountdownActive = false;
				if (base.playersCount > 1)
				{
					ChangePhase(MiniGamePhase.Preparing);
				}
			}
			else
			{
				Debug.Log("WaitingCountdown cancelada");
			}
			guiLogic.HideWaitCountdown();
		}

		private IEnumerator StartCountdown()
		{
			if (startCountdownActive)
			{
				Debug.Log("Intento StartCountdown fallido");
				yield break;
			}
			Debug.Log("Intento StartCountdown con exito");
			startCountdownActive = true;
			currentServerTimestamp = PhotonNetwork.time;
			if (yourPlayer.isMaster)
			{
				SendFixedCountdownTimestamp(currentServerTimestamp);
			}
			float seconds = 4f;
			while (seconds > 1f && startCountdownActive)
			{
				seconds = 4f - (float)(PhotonNetwork.time - currentServerTimestamp);
				guiLogic.SetStartCountdown((int)seconds);
				yield return null;
			}
			if (startCountdownActive)
			{
				Debug.Log("StartCountdown finalizada");
				startCountdownActive = false;
				guiLogic.SetStartCountdown(0);
				ChangePhase(MiniGamePhase.Running);
				yield return new WaitForSeconds(1f);
			}
			else
			{
				Debug.Log("StartCountdown cancelada");
			}
			guiLogic.HideStartCountdown();
		}

		private IEnumerator FinalCountdown()
		{
			if (finalCountdownActive)
			{
				Debug.Log("Intento FinalCountdown fallido");
				yield break;
			}
			Debug.Log("Intento FinalCountdown con exito");
			finalCountdownActive = true;
			currentServerTimestamp = PhotonNetwork.time;
			if (yourPlayer.isMaster)
			{
				SendFixedCountdownTimestamp(currentServerTimestamp);
			}
			float seconds = 10f - (float)(PhotonNetwork.time - currentServerTimestamp);
			while (seconds > 1f && finalCountdownActive)
			{
				seconds = 10f - (float)(PhotonNetwork.time - currentServerTimestamp);
				if (!finished)
				{
					guiLogic.SetFinalCountdown((int)seconds);
				}
				else
				{
					guiLogic.SetResetCountdown((int)seconds);
				}
				yield return null;
			}
			if (finalCountdownActive)
			{
				Debug.Log("FinalCountdown finalizada");
				finalCountdownActive = false;
				yourPlayer.isReady = false;
				GiveReward(yourPosition);
				yield return new WaitForSeconds(1f);
				ChangePhase(MiniGamePhase.ShowingAds);
			}
			else
			{
				Debug.Log("FinalCountdown cancelada");
			}
			guiLogic.HideFinalCountdown();
			guiLogic.HideFinalResultPanel();
		}

		private IEnumerator ResetCountdown()
		{
			if (resetCountdownActive)
			{
				Debug.Log("Intento ResetCountdown fallido");
				yield break;
			}
			Debug.Log("Intento ResetCountdown con exito");
			resetCountdownActive = true;
			if (yourPlayer.isMaster)
			{
				SendFixedCountdownTimestamp(currentServerTimestamp);
			}
			float seconds = 50f - (float)(PhotonNetwork.time - currentServerTimestamp);
			while (seconds > 1f && resetCountdownActive)
			{
				seconds = 50f - (float)(PhotonNetwork.time - currentServerTimestamp);
				guiLogic.SetFinalCountdown((int)seconds);
				yield return null;
			}
			if (resetCountdownActive)
			{
				Debug.Log("ResetCountdown finalizada");
				resetCountdownActive = false;
				if (yourPlayer.isMaster && playerCount > playersReadyCount)
				{
					MySingleton<Manager>.Instance.networkRooms.SetCustomFull(false);
				}
				ChangePhase(MiniGamePhase.Waiting);
			}
			else
			{
				Debug.Log("ResetCountdown cancelada");
			}
			guiLogic.HideFinalCountdown();
		}

		protected override void ResetGame()
		{
			base.ResetGame();
			Debug.Log("Fase Waiting");
			if (yourPlayer.isSpectator)
			{
				yourPlayer.isSpectator = false;
			}
			if (!yourPlayer.isReady)
			{
				yourPlayer.isReady = true;
			}
			finished = false;
			hasWinner = false;
			rewardGiven = false;
			goalReached = false;
			yourPosition = 0;
			playerRanking.Clear();
			playersByFinishTime.Clear();
			Track[] array = tracks;
			foreach (Track track in array)
			{
				track.DisableTrack();
			}
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Car"), LayerMask.NameToLayer("Car"), false);
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Car"), LayerMask.NameToLayer("RemoteCar"), false);
			if (playersReadyCount > 1)
			{
				StartCoroutine("WaitingCountdown");
			}
			else
			{
				waitingCountdownActive = false;
				guiLogic.ShowWaitingForPlayers();
			}
			if (MySingleton<Manager>.Instance.networkManager.player.isMaster)
			{
				SelectNextTrack();
				CheckCustomFull();
			}
			else
			{
				selectedTrack = MySingleton<Manager>.Instance.networkRooms.GetSelectedTrack();
			}
		}

		protected override void PrepareCarsToStart()
		{
			base.PrepareCarsToStart();
			Debug.Log("Fase Prepare");
			tracks[selectedTrack].PrepareTrack();
			MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarLogic>().LockXZMovements(true);
			MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarControl>().enabled = false;
			int num = 0;
			foreach (NetworkPlayer key in MySingleton<Manager>.Instance.networkGame.carsByPlayers.Keys)
			{
				if (key.isReady)
				{
					playerCars[num] = MySingleton<Manager>.Instance.networkGame.carsByPlayers[key];
					num++;
				}
			}
			Array.Sort(playerCars, new ComparableByOwnerID());
			for (int i = 0; i < playerCars.Length; i++)
			{
				if ((bool)playerCars[i])
				{
					playerCars[i].position = spawnPoints[i].position;
					playerCars[i].rotation = spawnPoints[i].rotation;
					if ((bool)playerCars[i].GetComponent<Rigidbody>())
					{
						playerCars[i].GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
						playerCars[i].GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
					}
				}
			}
			if (!yourPlayer.isSpectator)
			{
				StartCoroutine("StartCountdown");
			}
		}

		protected override void StartGame()
		{
			base.StartGame();
			Debug.Log("Fase Start");
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Car"), LayerMask.NameToLayer("Car"), true);
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Car"), LayerMask.NameToLayer("RemoteCar"), true);
			if (!yourPlayer.isSpectator)
			{
				MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarLogic>().LockXZMovements(false);
				MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarControl>().enabled = true;
				tracks[selectedTrack].StartTrack();
			}
		}

		protected override void Finish()
		{
			base.Finish();
			Debug.Log("Fase Finish");
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Car"), LayerMask.NameToLayer("Car"), true);
			Physics.IgnoreLayerCollision(LayerMask.NameToLayer("Car"), LayerMask.NameToLayer("RemoteCar"), true);
			StartCoroutine("FinalCountdown");
		}

		private void ShowAd()
		{
			Debug.Log("Fase ShowingAd");
			if (yourPlayer.isMaster)
			{
				MySingleton<Manager>.Instance.networkRooms.SetCustomFull(true);
			}
			tracks[selectedTrack].DisableTrack();
			currentServerTimestamp = PhotonNetwork.time;
			PhotonNetwork.SendOutgoingCommands();
			if (yourPlayer.isReady)
			{
				StartCoroutine("ResetCountdown");
			}
			else if (MySingleton<Manager>.Instance.ads.disabledAds)
			{
				OnAdEnded();
			}
		}

		private void ReplaceLayerHierarchically(Transform parentTransform, string previousLayerName, string newLayerName)
		{
			for (int i = 0; i < parentTransform.transform.childCount; i++)
			{
				Transform child = parentTransform.GetChild(i);
				if (LayerMask.LayerToName(child.gameObject.layer).Equals(previousLayerName))
				{
					child.gameObject.layer = LayerMask.NameToLayer(newLayerName);
				}
				ReplaceLayerHierarchically(child, previousLayerName, newLayerName);
			}
		}

		private void OnAdEnded()
		{
			if (yourPlayer.isReady)
			{
				Debug.Log("Este jugador ha vuelto del anuncio y estaba a ready.");
				yourPlayer.isReady = false;
			}
			StartCoroutine("WaitOneFrameToReceiveDelayedNetworkDataAndDoOnAdEnded");
		}

		private IEnumerator WaitOneFrameToReceiveDelayedNetworkDataAndDoOnAdEnded()
		{
			yield return null;
			if (playersReadyCount >= 4)
			{
				MySingleton<Manager>.Instance.connection.Disconnect(DisconnectionCause.DisconnectBecauseOfRoomIsFull);
				yield break;
			}
			if (!yourPlayer.isReady)
			{
				yourPlayer.isReady = true;
			}
			if (playersReadyCount == 1)
			{
				PhotonNetwork.SetMasterClient(yourPlayer.player);
				if (playerCount < 4)
				{
					MySingleton<Manager>.Instance.networkRooms.SetCustomFull(false);
				}
			}
			StartCoroutine("ResetCountdown");
		}

		protected override void OnNewPlayerJoin(NetworkPlayer player)
		{
			base.OnNewPlayerJoin(player);
			DecideWhatToDoOnPlayerCountChanged();
		}

		protected override void OnPlayerLeft(NetworkPlayer player)
		{
			base.OnPlayerLeft(player);
			DecideWhatToDoOnPlayerCountChanged();
		}

		protected override void OnNewCarInstantiated(GameObject car, NetworkPlayer owner)
		{
			base.OnNewCarInstantiated(car, owner);
			if (yourPlayer.isMaster && !owner.isLocal && someCountdownIsActive)
			{
				SendFixedCountdownTimestamp(currentServerTimestamp);
			}
			if (owner.isSpectator)
			{
				SetPlayerAsSpectator(owner, owner.isSpectator);
			}
		}

		private void OnRoomPropertiesChanged(ExitGames.Client.Photon.Hashtable propertiesThatChanged)
		{
			bool flag = propertiesThatChanged.ContainsKey("gameStatus");
			bool flag2 = propertiesThatChanged.ContainsKey("track");
			if (flag)
			{
				MiniGamePhase gameStatus = (MiniGamePhase)MySingleton<Manager>.Instance.networkRooms.GetGameStatus();
				if (gameStatus == MiniGamePhase.Preparing && currentPhase == MiniGamePhase.ShowingAds && !yourPlayer.isReady)
				{
					MySingleton<Manager>.Instance.connection.Disconnect(DisconnectionCause.DisconnectBecauseOfRoomIsFull);
				}
				if (gameStatus != currentPhase && (currentPhase != MiniGamePhase.ShowingAds || yourPlayer.isReady))
				{
					ChangePhase(gameStatus);
				}
			}
			if (flag2)
			{
				selectedTrack = MySingleton<Manager>.Instance.networkRooms.GetSelectedTrack();
			}
		}

		private void OnMasterClientChanged(NetworkPlayer player)
		{
			if (player.isLocal && currentPhase != (MiniGamePhase)MySingleton<Manager>.Instance.networkRooms.GetGameStatus())
			{
				MySingleton<Manager>.Instance.networkRooms.SetGameStatus((byte)currentPhase);
			}
		}

		private void OnPlayerPropertiesChanged(object[] codeAndMsg)
		{
			NetworkPlayer networkPlayer = new NetworkPlayer((PhotonPlayer)codeAndMsg[0]);
			ExitGames.Client.Photon.Hashtable hashtable = codeAndMsg[1] as ExitGames.Client.Photon.Hashtable;
			bool flag = hashtable.ContainsKey("spectator");
			bool flag2 = hashtable.ContainsKey("ready");
			if (flag)
			{
				SetPlayerAsSpectator(networkPlayer, networkPlayer.isSpectator);
				if (yourPlayer.isMaster)
				{
					CheckCustomFull();
				}
			}
			if (flag2)
			{
				DecideWhatToDoOnPlayerCountChanged();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();
			if ((bool)MySingleton<Manager>.Instance)
			{
				MySingleton<Manager>.Instance.networkManager.OnAIMPlayerPropertiesChanged -= OnPlayerPropertiesChanged;
				MySingleton<Manager>.Instance.networkManager.OnAIMRoomPropertiesChanged -= OnRoomPropertiesChanged;
				MySingleton<Manager>.Instance.networkManager.OnAIMMasterClientSwitched -= OnMasterClientChanged;
			}
		}
	}
}
