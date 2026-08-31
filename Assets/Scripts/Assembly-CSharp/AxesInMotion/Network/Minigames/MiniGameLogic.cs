using UnityEngine;

namespace AxesInMotion.Network.Minigames
{
	public class MiniGameLogic : NetworkBehaviour
	{
		public enum MiniGameType
		{
			None = 0,
			Racing = 1
		}

		public const string guiParentName = "pnMiniGames";

		public short timeBetweenGames = 50;

		public Transform[] playerCars;

		public Transform[] spawnPoints;

		public MiniGameType miniGameType;

		public bool endless;

		public bool hasWinner;

		public bool finished;

		public int playersCount
		{
			get
			{
				return MySingleton<Manager>.Instance.networkGame.playerCount;
			}
		}

		protected override void Awake()
		{
			base.Awake();
			MySingleton<Manager>.Instance.networkManager.OnAIMPlayerConnected += OnNewPlayerJoin;
			MySingleton<Manager>.Instance.networkManager.OnAIMPlayerDisconnected += OnPlayerLeft;
			MySingleton<Manager>.Instance.networkGame.OnRemoteCarInstantiated += OnNewCarInstantiated;
			MySingleton<Manager>.Instance.networkGame.miniGameLogic = this;
			if (playerCars == null || playerCars.Length != MySingleton<Manager>.Instance.connection.maxPlayersPerRoom)
			{
				playerCars = new Transform[MySingleton<Manager>.Instance.connection.maxPlayersPerRoom];
			}
		}

		protected virtual void ResetGame()
		{
		}

		protected virtual void PrepareCarsToStart()
		{
		}

		protected virtual void StartGame()
		{
		}

		protected virtual void Finish()
		{
		}

		protected virtual void OnNewPlayerJoin(NetworkPlayer player)
		{
		}

		protected virtual void OnNewCarInstantiated(GameObject car, NetworkPlayer owner)
		{
		}

		protected virtual void OnPlayerLeft(NetworkPlayer player)
		{
		}

		protected virtual void OnDestroy()
		{
			if ((bool)MySingleton<Manager>.Instance)
			{
				MySingleton<Manager>.Instance.networkManager.OnAIMPlayerConnected -= OnNewPlayerJoin;
				MySingleton<Manager>.Instance.networkManager.OnAIMPlayerDisconnected -= OnPlayerLeft;
				MySingleton<Manager>.Instance.networkGame.OnRemoteCarInstantiated -= OnNewCarInstantiated;
			}
		}
	}
}
