using System.Collections.Generic;
using UnityEngine;

public class ChallangeModeManager : MonoBehaviour
{
	public const string TRAFFIC_MODE_CARS_PRARENT_GAMEOBJECT_NAME = "CarsParent";

	public float startChronoSpeedThreshold = 5f;

	public UIPanel pnChallangeMode;

	public InfoCar challangeInfoCar;

	public VehicleCamera vehicleCamera;

	[HideInInspector]
	public Challange currentChallange;

	[HideInInspector]
	public CheckpointMaster checkpointMaster;

	[HideInInspector]
	public SpawnerMaster spawnerMaster;

	[HideInInspector]
	public GameObject sceneGameObject;

	[HideInInspector]
	public Transform startingPoint;

	[HideInInspector]
	public CircuitSplineMaster circuitSplineMaster;

	[HideInInspector]
	public InfoCar _gameInfoCar;

	private Challange[] _challanges;

	private List<Challange> _enabledChallanges;

	private Challange _challangeInActivationZone;

	private ChallangeCircuit _circuitInActivationZone;

	private Vector3 _lastPosition;

	private Quaternion _lastRotation;

	private List<Challange> _auxChallanges;

	private List<ChallangeCircuit> _auxCircuits;

	public AIContoller _aiContollerCity;

	public GameObject _carsParent;

	public ChallangeCircuit[] allChallanges;

	private void Awake()
	{
		if (!MySingleton<Manager>.Instance.isChallangeAvailable)
		{
			Object.DestroyImmediate(this);
		}
		_challanges = GetComponentsInChildren<Challange>();
		_auxChallanges = new List<Challange>();
		_auxCircuits = new List<ChallangeCircuit>();
		MySingleton<Manager>.Instance.challangeModeManager = this;
	}

	private void Start()
	{
		GameModeConfiguration();
		_enabledChallanges = new List<Challange>();
		SetEnabledChallanges();
		MySingleton<Manager>.Instance.gameLogic.challangeModeManager = this;
		if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Checkpoint)
		{
			base.gameObject.SetActive(false);
			NGUITools.SetActive(pnChallangeMode.gameObject, false);
			Object.Destroy(base.gameObject);
			Object.Destroy(pnChallangeMode.gameObject);
		}
	}

	private void OnEnable()
	{
		GetAllChallanges();
	}

	private void OnDestroy()
	{
		if (!(MySingleton<Manager>.Instance == null) && MySingleton<Manager>.Instance.gameLogic != null)
		{
			MySingleton<Manager>.Instance.gameLogic.challangeModeManager = null;
		}
	}

	private void Update()
	{
		if (!IsInCircuit())
		{
			return;
		}
		if (currentChallange.GetCurrentCircuit().isLoaded)
		{
			if (Chronometer.IsRunning())
			{
				checkpointMaster.Check(MySingleton<Manager>.Instance.gameLogic.currentCar.position, new Vector3(1.5f, 2f, 3f));
				MySingleton<Manager>.Instance.guiChallangeModeLogic.lbElapsedTime.text = Converter.GetRaceTimeFromSeconds(Chronometer.GetElapsed());
			}
			else if ((float)MySingleton<Manager>.Instance.gameLogic.GetSpeed() > startChronoSpeedThreshold && checkpointMaster.GetRemaining() > 0 && !Chronometer.IsPaused())
			{
				Chronometer.Start();
			}
		}
		else
		{
			currentChallange.GetCurrentCircuit().CheckEnterAsyncOperation();
		}
	}

	private void SetEnabledChallanges()
	{
		_enabledChallanges.Clear();
		for (int i = 0; i < _challanges.Length; i++)
		{
			if (_challanges[i].gameObject.activeSelf)
			{
				_enabledChallanges.Add(_challanges[i]);
			}
		}
	}

	private bool CheckAndSetInActivationZone()
	{
		for (int i = 0; i < _enabledChallanges.Count; i++)
		{
			_auxCircuits.Clear();
			_auxCircuits = _enabledChallanges[i].GetCircuitsInActivationZone();
			if (_auxCircuits.Count > 0)
			{
				_challangeInActivationZone = _enabledChallanges[i];
				_circuitInActivationZone = _auxCircuits[0];
				_challangeInActivationZone.LoadCircuitPlayerPref(_circuitInActivationZone);
				return true;
			}
		}
		return false;
	}

	private void SetCurrentGameObjects(ChallangeCircuit circuit)
	{
		sceneGameObject = GameObject.Find(circuit.GetSceneName());
		GameObject gameObject = sceneGameObject.transform.Find("ChallangeConfig").gameObject;
		if (gameObject == null)
		{
			Debug.LogError("Falta el objeto ChallangeConfig");
		}
		startingPoint = gameObject.transform.Find("StartingPoint");
		if (startingPoint == null)
		{
			Debug.LogError("Falta el objeto Starting Point");
		}
		checkpointMaster = gameObject.GetComponentInChildren<CheckpointMaster>();
		if (checkpointMaster == null)
		{
			Debug.LogError("Falta el objeto Checkpoint Master");
		}
		spawnerMaster = gameObject.GetComponentInChildren<SpawnerMaster>();
		if (spawnerMaster == null)
		{
			Debug.LogError("Falta el objeto Spawner Master");
		}
		circuitSplineMaster = gameObject.GetComponentInChildren<CircuitSplineMaster>();
		if (circuitSplineMaster == null)
		{
			Debug.LogError("Falta el objeto CircuitSplineMaster Master");
		}
		else
		{
			circuitSplineMaster.enabled = true;
		}
	}

	public void RetryCircuit()
	{
		Chronometer.Reset();
		DisableCarControl();
		SetCarTransform_StartingPoint();
		checkpointMaster.Restart();
	}

	public void RespawnLastCheckpoint()
	{
		if ((bool)checkpointMaster.GetLast())
		{
			SetCarTransform(checkpointMaster.GetLast().transform.position, checkpointMaster.GetLast().transform.rotation);
		}
		else
		{
			SetCarTransform_StartingPoint();
		}
		spawnerMaster.isChecked = false;
	}

	public void OnCircuitEntered(ChallangeCircuit circuit)
	{
		GameModeConfiguration();
		SetCurrentGameObjects(circuit);
		Chronometer.Reset();
		MySingleton<Manager>.Instance.gameLogic.InstantiateCar(challangeInfoCar, vehicleCamera);
		SetCarTransform_StartingPoint();
		DisableCarControl();
		DisableTraffic();
		ShowOrHideChallenges(false);
	}

	public void OnCircuitLeave(ChallangeCircuit circuit)
	{
		MonoBehaviour.print("============= HE TERMINADO EL CIRCUITO ================");
		if (GetChallangeInActivationZone().key != ChallangeKey.Trial)
		{
			MySingleton<Manager>.Instance.challangeModeManager.SetLastTransform();
		}
		Chronometer.Reset();
		MySingleton<Manager>.Instance.gameLogic.InstantiateGameCar(vehicleCamera);
		SetCarTransform_LastPoint();
		EnableCarControl();
		ShowOrHideChallenges(true);
		EnableTraffic();
	}

	public void OnPassThroughAllCheckpoint(CheckpointScript checkpoint)
	{
		Chronometer.Pause();
	}

	public bool CanPlayCircuit()
	{
		return !IsInCircuit() && CheckAndSetInActivationZone();
	}

	public bool IsInCircuit()
	{
		for (int i = 0; i < _challanges.Length; i++)
		{
			if (_challanges[i].IsInCircuit())
			{
				return true;
			}
		}
		return false;
	}

	public void PlayCircuitInActivationZone()
	{
		currentChallange = _challangeInActivationZone;
		currentChallange.PlayCircuit(_circuitInActivationZone);
		SetLastTransform();
	}

	public Challange GetChallangeInActivationZone()
	{
		return _challangeInActivationZone;
	}

	public ChallangeCircuit GetCircuitInActivationZone()
	{
		return _circuitInActivationZone;
	}

	public void SetLastTransform()
	{
		_lastPosition = MySingleton<Manager>.Instance.gameLogic.currentCar.position + new Vector3(0f, 0.1f, 0f);
		_lastRotation = Quaternion.Euler(0f, MySingleton<Manager>.Instance.gameLogic.currentCar.rotation.eulerAngles.y, 0f);
	}

	private void SetCarTransform(Vector3 position, Quaternion rotation)
	{
		MySingleton<Manager>.Instance.gameLogic.ResetCarInput();
		MySingleton<Manager>.Instance.gameLogic.currentCar.position = position;
		MySingleton<Manager>.Instance.gameLogic.currentCar.rotation = rotation;
		MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
		MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
	}

	private void SetCarTransform_StartingPoint()
	{
		SetCarTransform(startingPoint.position, startingPoint.rotation);
	}

	private void SetCarTransform_LastPoint()
	{
		SetCarTransform(_lastPosition + new Vector3(0f, 0.5f, 0f), _lastRotation);
	}

	public void EnableCarControl()
	{
		MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarLogic>().LockXZMovements(false);
		MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarControl>().enabled = true;
	}

	public void DisableCarControl()
	{
		MySingleton<Manager>.Instance.gameLogic.ResetCarInput();
		MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarLogic>().LockXZMovements(true);
		MySingleton<Manager>.Instance.gameLogic.currentCar.GetComponent<CarControl>().enabled = false;
	}

	private void GameModeConfiguration()
	{
		if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Traffic && MySingleton<Manager>.Instance.trafficSystem == Manager.TrafficSystem.Old && !_aiContollerCity && (bool)MySingleton<Manager>.Instance.gameLogic.instanceTraffic.trafficPrefab)
		{
			_aiContollerCity = MySingleton<Manager>.Instance.gameLogic.instanceTraffic.trafficPrefab.GetComponent<AIContoller>();
			_carsParent = GameObject.Find("CarsParent");
		}
	}

	private void EnableTraffic()
	{
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Traffic)
		{
			return;
		}
		if (MySingleton<Manager>.Instance.trafficSystem != Manager.TrafficSystem.ITStraffic)
		{
			if ((bool)MySingleton<Manager>.Instance.gameLogic.instanceTraffic && !_aiContollerCity)
			{
				_aiContollerCity = MySingleton<Manager>.Instance.gameLogic.instanceTraffic.trafficPrefab.GetComponent<AIContoller>();
				_carsParent = GameObject.Find("CarsParent");
			}
			else if (!MySingleton<Manager>.Instance.gameLogic.instanceTraffic)
			{
				_aiContollerCity = null;
				_carsParent = null;
			}
			if ((bool)_aiContollerCity)
			{
				_aiContollerCity.gameObject.SetActive(true);
				MySingleton<Manager>.Instance.gameLogic.trafficModeInstantiater.SetActive(true);
				_carsParent.SetActive(true);
			}
		}
		else
		{
			GameObject gameObject = GameObject.Find("TrafficITSZoneActivation");
			if (gameObject != null)
			{
				gameObject.GetComponent<TrafficITSZoneActivation>().CreateTraffic();
				Debug.Log("Creado el traficou en enabletraffic");
			}
		}
	}

	private void DisableTraffic()
	{
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Traffic)
		{
			return;
		}
		if (MySingleton<Manager>.Instance.trafficSystem != Manager.TrafficSystem.ITStraffic)
		{
			if ((bool)MySingleton<Manager>.Instance.gameLogic.instanceTraffic.trafficPrefab)
			{
				if ((bool)MySingleton<Manager>.Instance.gameLogic.instanceTraffic && !_aiContollerCity)
				{
					_aiContollerCity = MySingleton<Manager>.Instance.gameLogic.instanceTraffic.trafficPrefab.GetComponent<AIContoller>();
					_carsParent = GameObject.Find("CarsParent");
				}
				else if (!MySingleton<Manager>.Instance.gameLogic.instanceTraffic)
				{
					_aiContollerCity = null;
					_carsParent = null;
				}
			}
			if ((bool)_aiContollerCity)
			{
				_aiContollerCity.gameObject.SetActive(false);
				_carsParent.SetActive(false);
			}
		}
		else
		{
			GameObject gameObject = GameObject.Find("TrafficITSZoneActivation");
			if (gameObject != null)
			{
				gameObject.GetComponent<TrafficITSZoneActivation>().DeleteTraffic();
			}
		}
	}

	private void GetAllChallanges()
	{
		allChallanges = Object.FindObjectsByType<ChallangeCircuit>(FindObjectsSortMode.InstanceID);
	}

	private void ShowOrHideChallenges(bool show)
	{
		ChallangeCircuit[] array = allChallanges;
		foreach (ChallangeCircuit challangeCircuit in array)
		{
			challangeCircuit.gameObject.SetActive(show);
		}
	}

	public void ShowLeaderboard()
	{
		if (IsInCircuit())
		{
			currentChallange.ShowCurrentCircuitLeaderboard();
		}
		else if (_circuitInActivationZone != null)
		{
			_challangeInActivationZone.ShowCircuitLeaderboard(_circuitInActivationZone);
		}
	}
}
