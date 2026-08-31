using UnityEngine;

public class ChallangeCircuit : MonoBehaviour
{
	[Tooltip("")]
	public Transform activationZone;

	[Tooltip("")]
	public ChallangeSceneIndex sceneIndex;

	[HideInInspector]
	public float proportion;

	[HideInInspector]
	public float money;

	[HideInInspector]
	public bool isInActivationZone;

	[HideInInspector]
	public bool earnProportion = true;

	[HideInInspector]
	public bool completed;

	[HideInInspector]
	public bool isLoaded;

	[HideInInspector]
	public float bestTime;

	[HideInInspector]
	public float bestSpeed;

	[HideInInspector]
	public int bestCrashable;

	private bool _hasEnteredInActivationZone;

	private AsyncOperation _asyncOperation;

	private string _sceneName;

	public event ChallangeCircuitEventCallback OnEnterInActivationZoneEvent;

	public event ChallangeCircuitEventCallback OnStayInActivationZoneEvent;

	public event ChallangeCircuitEventCallback OnExitInActivationZoneEvent;

	public event ChallangeCircuitEventCallback OnEnteringEvent;

	public event ChallangeCircuitEventCallback OnEnteredEvent;

	public event ChallangeCircuitEventCallback OnLeaveEvent;

	private void Awake()
	{
		SetSceneName();
	}

	public void CheckIsInActivationZone(Vector3 position)
	{
		if (isInActivationZone)
		{
			if (!_hasEnteredInActivationZone)
			{
				_hasEnteredInActivationZone = true;
				if (OnEnterInActivationZoneEvent != null)
				{
					OnEnterInActivationZoneEvent(this);
				}
			}
			else if (OnStayInActivationZoneEvent != null)
			{
				OnStayInActivationZoneEvent(this);
			}
		}
		else if (_hasEnteredInActivationZone)
		{
			_hasEnteredInActivationZone = false;
			if (OnExitInActivationZoneEvent != null)
			{
				OnExitInActivationZoneEvent(this);
			}
		}
	}

	public void LoadCircuitAsync()
	{
		isLoaded = false;
		isInActivationZone = false;
		activationZone.gameObject.SetActive(false);
		if (OnEnteringEvent != null)
		{
			OnEnteringEvent(this);
		}
		_asyncOperation = LegacySceneManager.LoadLevelAdditiveAsync(_sceneName);
	}

	public void UnloadCircuit()
	{
		Object.Destroy(GameObject.Find(_sceneName));
		Resources.UnloadUnusedAssets();
		isLoaded = false;
		activationZone.gameObject.SetActive(true);
		if (OnLeaveEvent != null)
		{
			OnLeaveEvent(this);
		}
	}

	public void CheckEnterAsyncOperation()
	{
		if (_asyncOperation != null && _asyncOperation.isDone)
		{
			if (OnEnteredEvent != null)
			{
				OnEnteredEvent(this);
			}
			_asyncOperation = null;
			isLoaded = true;
		}
	}

	private void OnTriggerEnter(Collider col)
	{
		if (col.CompareTag("Car"))
		{
			isInActivationZone = true;
			MySingleton<Manager>.Instance.guiChallangeModeLogic.UpdateChallangeMode();
		}
	}

	private void OnTriggerExit(Collider col)
	{
		if (col.CompareTag("Car"))
		{
			isInActivationZone = false;
			MySingleton<Manager>.Instance.guiChallangeModeLogic.UpdateChallangeMode();
		}
	}

	private void SetSceneName()
	{
		if (sceneIndex.ToString("g").IndexOf("_") == 0)
		{
			_sceneName = sceneIndex.ToString("g").Substring(1);
		}
		else
		{
			_sceneName = sceneIndex.ToString("g");
		}
	}

	public string GetSceneName()
	{
		return _sceneName;
	}

	public void ShareScoreTrialLeaderboard(ChallangeKey key, float elapsedTime)
	{
		if (!(elapsedTime <= 0f) && MySingleton<Manager>.Instance.platform != Manager.Platform.Amazon)
		{
			MySingleton<Manager>.Instance.gameServicesManager.ShareLeaderboardScore(ChallangeConfiguration.GetChallangeCircuit_LeaderboardId(key, sceneIndex), ChallangeConfiguration.FormatElapsedTimeToGameServices(elapsedTime));
		}
	}

	public void ShareScoreRadarLeaderboard(ChallangeKey key, float totalSpeed)
	{
		if (!(totalSpeed <= 0f) && MySingleton<Manager>.Instance.platform != Manager.Platform.Amazon)
		{
			MySingleton<Manager>.Instance.gameServicesManager.ShareLeaderboardScore(ChallangeConfiguration.GetChallangeCircuit_LeaderboardId(key, sceneIndex), ChallangeConfiguration.FormatSpeedToGameServices(totalSpeed));
		}
	}

	public void ShareScoreCrashablesLeaderboard(ChallangeKey key, int totalCrashables)
	{
		if (totalCrashables >= CrashableChallangeManager.CRASHABLE_MINIMUM_SCORE && MySingleton<Manager>.Instance.platform != Manager.Platform.Amazon)
		{
			MySingleton<Manager>.Instance.gameServicesManager.ShareLeaderboardScore(ChallangeConfiguration.GetChallangeCircuit_LeaderboardId(key, sceneIndex), ChallangeConfiguration.FormatCrashablesToGameServices(totalCrashables));
		}
	}

	public void ShowLeaderboard(ChallangeKey key)
	{
		MySingleton<Manager>.Instance.gameServicesManager.loginSucceedBeforeShowLeaderboardEvent += OnLoginSucceedBeforeShowLeaderboard;
		Debug.Log(ChallangeConfiguration.GetChallangeCircuit_LeaderboardId(key, sceneIndex));
		MySingleton<Manager>.Instance.gameServicesManager.ShowLeaderboardOrLogin(ChallangeConfiguration.GetChallangeCircuit_LeaderboardId(key, sceneIndex));
	}

	private void OnLoginSucceedBeforeShowLeaderboard()
	{
		MySingleton<Manager>.Instance.challangeManager.ReportAllScoresWhenLoginSucceed();
	}
}
