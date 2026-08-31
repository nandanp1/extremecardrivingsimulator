using System.Collections;
using System.Collections.Generic;
using AxesInMotion.Network.Minigames;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameLogic : MonoBehaviour
{
	private const float STEP_DECREMENT = 0.075f;

	private const float STEP_INCREMENT = 0.075f;

	private const float STEP_HANDBRAKE = 0.085f;

	private const float STEER_ENGAGE_RATE = 0.75f;

	private const float STEER_RETURN_RATE = 0.75f;

	public const float INGAME_MUSIC_MAX_VOLUME = 0.6f;

	public float farCamera = 300f;

	public float speedMS;

	public Transform mainArrowPointer;

	public Transform firstSpawnPoint;

	public Transform currentCar;

	public Camera currentCamera;

	public GameObject carsParent;

	public GameObject persistentCollectablesParent;

	public GameObject actualZoneCollectablesParent;

	public bool challangeInProgress;

	public CarSteeringWheel steer;

	public CarSound carSound;

	public CarControl carControl;

	public List<Billboard> flares;

	public GameObject positionLights;

	public bool lightsOn;

	[HideInInspector]
	public float gas;

	[HideInInspector]
	public float brakes;

	[HideInInspector]
	public float handbreak;

	public AudioSource musicAudioSource;

	public float dirLeft;

	[HideInInspector]
	public float dirRight;

	private bool m_dirLeftPressed;

	private bool m_dirRightPressed;

	private float m_directionalInput;

	public GameObject trafficModeInstantiater;

	public InstanceTraffic instanceTraffic;

	public GameObject checkPointPrefab;

	[HideInInspector]
	public GUIGameLogic guiLogic;

	public LevelZone firstLevelZone;

	public LevelZone[] allZones;

	[HideInInspector]
	public InfoWorkshop[] allInfoWorkshop;

	public List<Transform> carComponents;

	public GameObject instantiatedCar;

	public GameObject rootLoading;

	public ChallangeModeManager challangeModeManager;

	public bool jump;

	private Coroutine playerCarStabilization;

	private RigidbodyStabilizationState[] playerCarStabilizationStates;

	private GameObject playerCarStabilizationCar;

	private bool playerCarStabilizationOwnsLoading;

	private struct RigidbodyStabilizationState
	{
		public Rigidbody body;

		public bool useGravity;

		public bool isKinematic;

		public RigidbodyConstraints constraints;

		public RigidbodyStabilizationState(Rigidbody rigidbody)
		{
			body = rigidbody;
			useGravity = rigidbody.useGravity;
			isKinematic = rigidbody.isKinematic;
			constraints = rigidbody.constraints;
		}
	}

	private void InitializeNetworkGame()
	{
		MySingleton<Manager>.Instance.networkGame.InstantiatePlayerCar();
		StartCoroutine(DisableOfflineElements());
		StartCoroutine(InitGame());
		if (MySingleton<Manager>.Instance.networkGame.miniGameSelected != MiniGameLogic.MiniGameType.None)
		{
			StartCoroutine(InitializeMiniGame());
		}
	}

	private IEnumerator DisableOfflineElements()
	{
		yield return null;
		MySingleton<Manager>.Instance.networkGame.DisableOfflineElements();
	}

	private IEnumerator InitializeMiniGame()
	{
		yield return null;
		MySingleton<Manager>.Instance.networkGame.InitializeMiniGame();
	}

	private void Awake()
	{
		mainArrowPointer.gameObject.SetActive(false);
		rootLoading = Object.Instantiate(MySingleton<Manager>.Instance.UIRootLoading) as GameObject;
		carComponents = new List<Transform>();
		flares = new List<Billboard>();
		MySingleton<Manager>.Instance.gameLogic = this;
		InstantiateCar();
		musicAudioSource = base.gameObject.GetComponent<AudioSource>();
		musicAudioSource.volume = SettingsManager.GetMusicVolume();
		GetCarLightSystem();
	}

	private void Start()
	{
		if ((bool)MySingleton<Manager>.Instance.guiGameLogic && (bool)MySingleton<Manager>.Instance.guiGameLogic.exclusiveFreeElements && (bool)MySingleton<Manager>.Instance.guiGameLogic.exclusiveNetworkElements)
		{
			MySingleton<Manager>.Instance.guiGameLogic.exclusiveFreeElements.SetActive(true);
		}
		MySingleton<Manager>.Instance.stats.DistanceInitData();
		MySingleton<Manager>.Instance.taskManager.StartingSaveDataRewardsLoop();
		CarSettings component = currentCar.GetComponent<CarSettings>();
		component.abs = true;
		component.tc = true;
		component.esp = true;
		CarLightRay componentInChildren = currentCar.GetComponentInChildren<CarLightRay>();
		if ((bool)componentInChildren)
		{
			componentInChildren.enabled = true;
		}
		lightsOn = false;
	}

	private void Update()
	{
		float target = ((m_dirLeftPressed == m_dirRightPressed) ? 0f : (m_dirLeftPressed ? (-1f) : 1f));
		bool reversing = !Mathf.Approximately(target, 0f) && !Mathf.Approximately(m_directionalInput, 0f) && Mathf.Sign(target) != Mathf.Sign(m_directionalInput);
		float rate = ((Mathf.Approximately(target, 0f) || reversing) ? STEER_RETURN_RATE : STEER_ENGAGE_RATE);
		m_directionalInput = Mathf.MoveTowards(m_directionalInput, target, rate * Time.deltaTime);
		dirLeft = Mathf.Min(0f, m_directionalInput);
		dirRight = Mathf.Max(0f, m_directionalInput);
	}

	private void OnEnable()
	{
		carComponents.Clear();
	}

	private void OnDisable()
	{
		CancelPlayerCarStabilization(true);
		if (!(MySingleton<Manager>.Instance == null))
		{
			if (MySingleton<Manager>.Instance.stats != null)
			{
				MySingleton<Manager>.Instance.stats.SaveDataIntoPlayerPrefs();
			}
			if (MySingleton<Manager>.Instance.taskManager != null)
			{
				MySingleton<Manager>.Instance.taskManager.CancelSaveDataRewardsLoop();
			}
		}
	}

	private void OnApplicationPause(bool pause)
	{
		if (!pause)
		{
			MySingleton<Manager>.Instance.AdsEnded();
		}
	}

	public void InitLogic()
	{
		if (MySingleton<Manager>.Instance.gameMode == Manager.GameMode.Checkpoint)
		{
			checkPointPrefab = Object.Instantiate(MySingleton<Manager>.Instance.checkPointPrefab) as GameObject;
			ReloadCheckPoints();
		}
	}

	public void ReloadCheckPoints()
	{
		CheckpointLogic component = checkPointPrefab.GetComponent<CheckpointLogic>();
		Transform[] checkpointsSorted = component.checkpointsSorted;
		NGUITools.SetActive(checkpointsSorted[component.actualIndex].gameObject, false);
		NGUITools.SetActive(checkpointsSorted[MySingleton<Manager>.Instance.currentCheckPoint].gameObject, true);
		component.actualIndex = MySingleton<Manager>.Instance.currentCheckPoint;
	}

	public void SetUnits(SettingsManager.Units newUnits)
	{
		SettingsManager.SetUnits(newUnits);
	}

	public void Reset()
	{
		ResetCarInput();
		jump = true;
		CancelPlayerCarStabilization(true);
		Rigidbody component = currentCar.GetComponent<Rigidbody>();
		if (!component.isKinematic)
		{
			component.linearVelocity = Vector3.zero;
			component.angularVelocity = Vector3.zero;
		}
		currentCar.transform.position = new Vector3(currentCar.transform.position.x, currentCar.transform.position.y + 3f, currentCar.transform.position.z);
		currentCar.transform.localRotation = Quaternion.Euler(new Vector3(0f, currentCar.transform.localRotation.eulerAngles.y, 0f));
		Physics.SyncTransforms();
		// Manual reset must remain a fail-open, source-era upright-and-drop. The
		// scene-loading stabilizer requires four nearly level downward ground hits;
		// starting it on a dish wall, loop, ramp seam, or while airborne can freeze
		// the car forever. Spawn and additive-zone loading still use stabilization.
		CancelInvoke("ResetJumpFlag");
		Invoke("ResetJumpFlag", 5f);
	}

	private void ResetJumpFlag()
	{
		jump = false;
	}

	public bool ChangeAbs()
	{
		currentCar.GetComponent<CarSettings>().abs = !currentCar.GetComponent<CarSettings>().abs;
		return currentCar.GetComponent<CarSettings>().abs;
	}

	public bool ChangeEsp()
	{
		currentCar.GetComponent<CarSettings>().esp = !currentCar.GetComponent<CarSettings>().esp;
		return currentCar.GetComponent<CarSettings>().esp;
	}

	public bool ChangeTc()
	{
		currentCar.GetComponent<CarSettings>().tc = !currentCar.GetComponent<CarSettings>().tc;
		return currentCar.GetComponent<CarSettings>().tc;
	}

	public void NextCamera()
	{
		currentCar.GetComponent<CarCameras>().Next();
	}

	public void Repair()
	{
		currentCar.GetComponent<CarDamage>().DoBounce();
	}

	public void Gas(bool isPressed)
	{
		if (isPressed)
		{
			CancelInvoke("DecrementGas");
			InvokeRepeating("IncrementGas", 0f, 0.1f);
		}
		else
		{
			CancelInvoke("IncrementGas");
			InvokeRepeating("DecrementGas", 0f, 0.1f);
		}
	}

	public void Brake(bool isPressed)
	{
		if (isPressed)
		{
			CancelInvoke("DecrementBrake");
			InvokeRepeating("IncrementBrake", 0f, 0.1f);
		}
		else
		{
			CancelInvoke("IncrementBrake");
			InvokeRepeating("DecrementBrake", 0f, 0.1f);
		}
	}

	public void Handbrake(bool isPressed)
	{
		if (isPressed)
		{
			CancelInvoke("DecrementHandbrake");
			InvokeRepeating("IncrementHandbrake", 0f, 0.1f);
		}
		else
		{
			CancelInvoke("IncrementHandbrake");
			InvokeRepeating("DecrementHandbrake", 0f, 0.1f);
		}
	}

	public void DirLeft(bool isPressed)
	{
		m_dirLeftPressed = isPressed;
	}

	public void DirRight(bool isPressed)
	{
		m_dirRightPressed = isPressed;
	}

	private void IncrementGas()
	{
		gas += 0.075f;
		if (gas >= 1f)
		{
			gas = 1f;
			CancelInvoke("IncrementGas");
		}
	}

	private void IncrementBrake()
	{
		brakes -= 0.075f;
		if (brakes <= -1f)
		{
			brakes = -1f;
			CancelInvoke("IncrementBrake");
		}
	}

	private void DecrementGas()
	{
		gas -= 0.075f;
		if (gas <= 0f)
		{
			gas = 0f;
			CancelInvoke("DecrementGas");
		}
	}

	private void DecrementBrake()
	{
		brakes += 0.075f;
		if (brakes >= 0f)
		{
			brakes = 0f;
			CancelInvoke("DecrementBrake");
		}
	}

	private void IncrementHandbrake()
	{
		handbreak += 0.085f;
		if (handbreak >= 1f)
		{
			handbreak = 1f;
			CancelInvoke("IncrementHandbrake");
		}
	}

	private void DecrementHandbrake()
	{
		handbreak -= 0.085f;
		if (handbreak <= 0f)
		{
			handbreak = 0f;
			CancelInvoke("DecrementHandbrake");
		}
	}

	public void ResetCarInput()
	{
		gas = 0f;
		brakes = 0f;
		m_dirLeftPressed = false;
		m_dirRightPressed = false;
		m_directionalInput = 0f;
		dirRight = 0f;
		dirLeft = 0f;
		CancelInvoke();
	}

	public int GetSpeed()
	{
		speedMS = Vector3.Dot(carControl.GetComponent<Rigidbody>().linearVelocity, carControl.transform.forward);
		if (MySingleton<Manager>.Instance.units == SettingsManager.Units.KMH)
		{
			return (int)Mathf.Abs(speedMS * 3.6f * 2f);
		}
		return (int)Mathf.Abs(speedMS * 2.237f * 2f);
	}

	public float GetSpeedMS(Vector3 velocity)
	{
		return Vector3.Dot(velocity, carControl.transform.forward);
	}

	public string GetGear()
	{
		int gearInput = carControl.gearInput;
		if (gearInput > 0)
		{
			int currentGear = carSound.currentGear;
			return Mathf.Clamp(currentGear, 1, carSound.gearCount).ToString();
		}
		return (gearInput >= 0) ? "N" : "R";
	}

	public float GetRevs01Range()
	{
		float engineRPM = carSound.engineRPM;
		float engineGearUpRPM = carSound.engineGearUpRPM;
		float num = carSound.engineIdleRPM - 500f;
		return (engineRPM - num) / (engineGearUpRPM - num);
	}

	public void Driving(float deltaMeters)
	{
		MySingleton<Manager>.Instance.stats.IncrementMetersDrove(deltaMeters);
		MySingleton<Manager>.Instance.taskManager.taskDistance1.AddProgress(deltaMeters);
	}

	public void GoToWorkshop(InfoWorkshop infoWorkshop)
	{
		guiLogic.ShowLoading();
		MySingleton<Manager>.Instance.currentWorkshopId = infoWorkshop.Id;
		if ((bool)MySingleton<Manager>.Instance.levelZone)
		{
			MySingleton<Manager>.Instance.currentLevelZoneName = MySingleton<Manager>.Instance.levelZone.nameSceneZone;
		}
		LegacySceneManager.LoadLevel(3);
	}

	public void InstantiateCar()
	{
		InfoCar infoCar = MySingleton<Manager>.Instance.carsInfoPrefabs[MySingleton<Manager>.Instance.GetIndexArrayPosFromId(MySingleton<Manager>.Instance.indexCurrentCar)];
		instantiatedCar = Object.Instantiate(Resources.Load(infoCar.idName) as GameObject) as GameObject;
		InfoCar infoCar2 = instantiatedCar.AddMissingComponent<InfoCar>();
		infoCar2.Initialize(infoCar);
		currentCar = instantiatedCar.transform;
		carControl = currentCar.GetComponent<CarControl>();
		carSound = currentCar.GetComponent<CarSound>();
		instantiatedCar.SetActive(false);
		carComponents = new List<Transform>();
		carControl.FindGameLogic();
		StartCoroutine(InitGame());
	}

	private void ActiveCarComponents(Transform car)
	{
		foreach (Transform item in car)
		{
			if (!item.gameObject.activeSelf && item.gameObject.tag != "InstantiateAI" && item.gameObject.tag != "FrontLight")
			{
				item.gameObject.SetActive(true);
				carComponents.Add(item);
			}
			else if (item.gameObject.tag == "InstantiateAI")
			{
				trafficModeInstantiater = item.gameObject;
			}
			ActiveCarComponents(item);
		}
		instantiatedCar.GetComponent<CarVisuals>().shadow.SetActive(true);
	}

	private IEnumerator InitGame()
	{
		string groundSceneName = string.Empty;
		ActiveCarComponents(instantiatedCar.transform);
		if (!MySingleton<Manager>.Instance.returningFromWorkshop)
		{
			if (firstLevelZone != null)
			{
				groundSceneName = firstLevelZone.nameSceneZone;
				firstLevelZone.ForceLoadLevel();
			}
			if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
			{
				instantiatedCar.transform.position = firstSpawnPoint.transform.position;
				instantiatedCar.transform.rotation = firstSpawnPoint.transform.rotation;
			}
		}
		else
		{
			MySingleton<Manager>.Instance.returningFromWorkshop = false;
			if ((allZones != null) & (allZones.Length > 0))
			{
				LevelZone[] array = allZones;
				foreach (LevelZone zone in array)
				{
					if (!(zone.nameSceneZone == MySingleton<Manager>.Instance.currentLevelZoneName))
					{
						continue;
					}
					groundSceneName = zone.nameSceneZone;
					zone.ForceLoadLevel();
					yield return null;
					InfoWorkshop[] allInfoWorkshop = GameObject.Find(zone.nameSceneZone).GetComponent<InfoZone>().infoWorkshopArray;
					InfoWorkshop[] array2 = allInfoWorkshop;
					foreach (InfoWorkshop workshop in array2)
					{
						if (workshop.Id == MySingleton<Manager>.Instance.currentWorkshopId)
						{
							Transform spawnPoint = workshop.gameObject.GetComponent<WorkshopDoorLogic>().spawnPoint;
							instantiatedCar.transform.position = spawnPoint.position;
							instantiatedCar.transform.rotation = spawnPoint.rotation;
						}
					}
				}
			}
			else
			{
				InfoWorkshop[] allInfoWorkshop2 = GameObject.Find("2Game_Persistent_Additive").GetComponent<InfoZone>().infoWorkshopArray;
				InfoWorkshop[] array3 = allInfoWorkshop2;
				foreach (InfoWorkshop workshop2 in array3)
				{
					if (workshop2.Id == MySingleton<Manager>.Instance.currentWorkshopId)
					{
						Transform spawnPoint2 = workshop2.gameObject.GetComponent<WorkshopDoorLogic>().spawnPoint;
						instantiatedCar.transform.position = spawnPoint2.position;
						instantiatedCar.transform.rotation = spawnPoint2.rotation;
					}
				}
			}
		}
		GameObject trafficDetector = new GameObject("Traffic Detector");
		trafficDetector.transform.position = instantiatedCar.transform.position;
		trafficDetector.transform.SetParent(instantiatedCar.transform);
		trafficDetector.AddComponent<TestSwitcher>();
		rootLoading.SetActive(false);
		instantiatedCar.GetComponent<CarLogic>().LockXZMovements(true);
		instantiatedCar.GetComponent<CarLogic>().LockXZMovements(false);
		CarCameras camerasScript = instantiatedCar.GetComponent<CarCameras>();
		camerasScript.FixedCameras[0] = Camera.main;
		currentCamera = Camera.main;
		camerasScript.enabled = true;
		InitLogic();
		instantiatedCar.GetComponent<CarControl>().enabled = true;
		instantiatedCar.GetComponent<CarLogic>().enabled = true;
		KeepRandomInputDisabledForPlayerCar();
		instantiatedCar.GetComponent<CarSound>().enabled = true;
		instantiatedCar.GetComponent<CarCameras>().enabled = true;
		instantiatedCar.GetComponent<CarVisuals>().enabled = true;
		instantiatedCar.AddMissingComponent<Lifeguard>();
		instantiatedCar.SetActive(true);
		BeginPlayerCarStabilization(groundSceneName);
	}

	public void PlayAudioCollectableCollected()
	{
		StartCoroutine(PlayAudioCollectable());
	}

	public void PlayHorn(bool isPressed)
	{
		if (isPressed)
		{
			if (!carSound.horn.isPlaying)
			{
				carSound.horn.Play();
			}
		}
		else
		{
			carSound.horn.Stop();
		}
	}

	private IEnumerator PlayAudioCollectable()
	{
		GameObject go = new GameObject();
		go.transform.parent = base.transform;
		AudioClip audioCollectable = Resources.Load("collectable_collected") as AudioClip;
		AudioSource audioSource = go.AddMissingComponent<AudioSource>();
		audioSource.playOnAwake = false;
		audioSource.clip = audioCollectable;
		audioSource.Play();
		yield return null;
		while (audioSource.isPlaying)
		{
			yield return new WaitForSeconds(0.3f);
		}
		Object.Destroy(go);
		yield return null;
		Resources.UnloadUnusedAssets();
		yield return null;
	}

	private void GetCarLightSystem()
	{
		if (!(instantiatedCar != null))
		{
			return;
		}
		CarVisuals component = instantiatedCar.GetComponent<CarVisuals>();
		if (component.PositionLights != null && component.Flares != null)
		{
			FrontFlares component2 = component.Flares.GetComponent<FrontFlares>();
			if (component2 != null && component2.flares.Length > 0)
			{
				flares.Clear();
				flares.AddRange(component2.flares);
				positionLights = component.PositionLights;
			}
		}
		else
		{
			Debug.LogWarning("Este coche no tiene luces de posicion!!");
		}
	}

	public void SwitchCarLights()
	{
		GetCarLightSystem();
		CarVisuals component = instantiatedCar.GetComponent<CarVisuals>();
		if (component.LightProjectors != null)
		{
			if (!lightsOn)
			{
				if ((bool)component.LightProjectors)
				{
					component.LightProjectors.SetActive(true);
				}
			}
			else if ((bool)component.LightProjectors)
			{
				component.LightProjectors.SetActive(false);
			}
		}
		if ((bool)positionLights && flares.Count == 2)
		{
			for (int i = 0; i < 2; i++)
			{
				if (!lightsOn)
				{
					positionLights.SetActive(true);
					if (flares[i] != null)
					{
						flares[i].targetScale = 1f;
					}
				}
				else
				{
					positionLights.SetActive(false);
					if (flares[i] != null)
					{
						flares[i].targetScale = 0f;
					}
				}
			}
		}
		if (lightsOn)
		{
			lightsOn = false;
		}
		else
		{
			lightsOn = true;
		}
	}

	public void ForceSwitchCarLights()
	{
		if (!SettingsManager.GetStarterPackBoughtOrEnded() && !lightsOn)
		{
			SwitchCarLights();
		}
	}

	public void VolumeChanged(float value)
	{
		float num = Mathf.Lerp(0f, 0.6f, value);
		SettingsManager.SetMusicVolume(value);
	}

	public void InstantiateCar(InfoCar info, VehicleCamera vehicleCamera)
	{
		DestroyInstantiatedCar();
		instantiatedCar = Object.Instantiate(Resources.Load(info.idName) as GameObject) as GameObject;
		ActiveCarComponents(instantiatedCar.transform);
		InfoCar infoCar = instantiatedCar.AddMissingComponent<InfoCar>();
		infoCar.Initialize(info);
		currentCar = instantiatedCar.transform;
		carControl = currentCar.GetComponent<CarControl>();
		carSound = currentCar.GetComponent<CarSound>();
		CarCameras component = instantiatedCar.GetComponent<CarCameras>();
		component.FixedCameras[0] = vehicleCamera.GetComponent<Camera>();
		vehicleCamera.ChangeTarget(currentCar);
		PrepareInstantiatedCar();
	}

	public void PrepareInstantiatedCar()
	{
		instantiatedCar.SetActive(false);
		carControl.FindGameLogic();
		foreach (Transform item in instantiatedCar.transform)
		{
			if (!item.gameObject.activeSelf && item.gameObject.tag != "InstantiateAI" && item.gameObject.tag != "FrontLight")
			{
				item.gameObject.SetActive(true);
			}
			else if (item.gameObject.tag == "InstantiateAI")
			{
				trafficModeInstantiater = item.gameObject;
			}
		}
		CarLogic component = instantiatedCar.GetComponent<CarLogic>();
		component.LockXZMovements(true);
		component.LockXZMovements(false);
		carControl.enabled = true;
		component.enabled = true;
		KeepRandomInputDisabledForPlayerCar();
		instantiatedCar.GetComponent<CarSound>().enabled = true;
		instantiatedCar.GetComponent<CarCameras>().enabled = true;
		CarVisuals component2 = instantiatedCar.GetComponent<CarVisuals>();
		component2.enabled = true;
		component2.shadow.SetActive(true);
		instantiatedCar.AddMissingComponent<Lifeguard>();
		instantiatedCar.SetActive(true);
		BeginPlayerCarStabilization(GetCurrentPlayerCarGroundSceneName());
	}

	private void KeepRandomInputDisabledForPlayerCar()
	{
		CarSettings settings = instantiatedCar.GetComponent<CarSettings>();
		if (settings != null)
		{
			// CarSettings reapplies this flag every frame, so clearing only the
			// CarExternalInputRandom.enabled bit is not sufficient.
			settings.externalInput = false;
		}
		CarExternalInputRandom[] components = instantiatedCar.GetComponentsInChildren<CarExternalInputRandom>(true);
		foreach (CarExternalInputRandom component in components)
		{
			component.enabled = false;
		}
		CarControl component2 = instantiatedCar.GetComponent<CarControl>();
		if (component2 != null)
		{
			component2.readUserInput = true;
			component2.steerInput = 0f;
			component2.motorInput = 0f;
			component2.brakeInput = 0f;
			component2.handbrakeInput = 0f;
		}
	}

	private bool AlignPlayerCarWheelsToGround(string requiredSceneName)
	{
		if (instantiatedCar == null)
		{
			return false;
		}
		Rigidbody body = instantiatedCar.GetComponent<Rigidbody>();
		WheelCollider[] wheels = instantiatedCar.GetComponentsInChildren<WheelCollider>(true);
		if (body == null || wheels.Length == 0)
		{
			return false;
		}
		Physics.SyncTransforms();
		float verticalOffset = float.NegativeInfinity;
		float minimumVerticalOffset = float.PositiveInfinity;
		int groundedWheels = 0;
		foreach (WheelCollider wheel in wheels)
		{
			RaycastHit[] hits = Physics.RaycastAll(wheel.transform.position + Vector3.up * 2f, Vector3.down, 12f, Physics.DefaultRaycastLayers, QueryTriggerInteraction.Ignore);
			float closestDistance = float.PositiveInfinity;
			float closestSoilDistance = float.PositiveInfinity;
			RaycastHit closestHit = default(RaycastHit);
			RaycastHit closestSoilHit = default(RaycastHit);
			foreach (RaycastHit hit in hits)
			{
				if (hit.collider == null || hit.collider.attachedRigidbody == body || hit.transform.IsChildOf(instantiatedCar.transform))
				{
					continue;
				}
				if (!string.IsNullOrEmpty(requiredSceneName) && hit.collider.gameObject.scene.name != requiredSceneName)
				{
					continue;
				}
				if (Vector3.Dot(hit.normal, Vector3.up) < 0.25f)
				{
					continue;
				}
				if (hit.distance < closestDistance)
				{
					closestDistance = hit.distance;
					closestHit = hit;
				}
				if (hit.collider.gameObject.layer == 12 && hit.distance < closestSoilDistance)
				{
					closestSoilDistance = hit.distance;
					closestSoilHit = hit;
				}
			}
			// Prefer recovered road/soil colliders only when they represent the
			// same nearby surface. A distant road beneath a bridge must not win.
			if (closestSoilHit.collider != null && closestSoilDistance <= closestDistance + 0.05f)
			{
				closestHit = closestSoilHit;
			}
			if (closestHit.collider == null)
			{
				return false;
			}
			float desiredWheelCenterY = closestHit.point.y + wheel.radius + wheel.suspensionDistance + 0.02f;
			float wheelOffset = desiredWheelCenterY - wheel.transform.position.y;
			verticalOffset = Mathf.Max(verticalOffset, wheelOffset);
			minimumVerticalOffset = Mathf.Min(minimumVerticalOffset, wheelOffset);
			groundedWheels++;
			if (Debug.isDebugBuild)
			{
				Debug.Log("Player car ground hit: wheel=" + wheel.name + ", collider=" + closestHit.collider.name + ", scene=" + closestHit.collider.gameObject.scene.name + ", layer=" + closestHit.collider.gameObject.layer + ", distance=" + closestHit.distance.ToString("F3"));
			}
		}
		if (groundedWheels != wheels.Length || verticalOffset < -5f || verticalOffset > 5f || verticalOffset - minimumVerticalOffset > 1f)
		{
			return false;
		}
		instantiatedCar.transform.position += Vector3.up * verticalOffset;
		if (!body.isKinematic)
		{
			body.linearVelocity = Vector3.zero;
			body.angularVelocity = Vector3.zero;
		}
		Physics.SyncTransforms();
		if (Debug.isDebugBuild)
		{
			Debug.Log("Aligned player car to recovered ground by " + verticalOffset.ToString("F3") + "m in scene " + requiredSceneName);
		}
		return true;
	}

	private string GetCurrentPlayerCarGroundSceneName()
	{
		if (MySingleton<Manager>.Instance != null && MySingleton<Manager>.Instance.levelZone != null)
		{
			string text = MySingleton<Manager>.Instance.levelZone.nameSceneZone;
			if (!string.IsNullOrEmpty(text))
			{
				return text;
			}
		}
		if (firstLevelZone != null)
		{
			string text2 = firstLevelZone.nameSceneZone;
			if (!string.IsNullOrEmpty(text2))
			{
				return text2;
			}
		}
		return string.Empty;
	}

	private void CancelPlayerCarStabilization(bool restoreRigidbodies)
	{
		if (playerCarStabilization != null)
		{
			StopCoroutine(playerCarStabilization);
			playerCarStabilization = null;
		}
		if (restoreRigidbodies && playerCarStabilizationStates != null)
		{
			RestorePlayerCarRigidbodies(playerCarStabilizationStates);
		}
		playerCarStabilizationStates = null;
		playerCarStabilizationCar = null;
		if (playerCarStabilizationOwnsLoading && rootLoading != null)
		{
			rootLoading.SetActive(false);
		}
		playerCarStabilizationOwnsLoading = false;
	}

	private void ShowPlayerCarStabilizationLoading()
	{
		if (rootLoading != null && !rootLoading.activeSelf)
		{
			rootLoading.SetActive(true);
			playerCarStabilizationOwnsLoading = true;
		}
	}

	private static void RestorePlayerCarRigidbodies(RigidbodyStabilizationState[] states)
	{
		foreach (RigidbodyStabilizationState state in states)
		{
			Rigidbody body = state.body;
			if (body == null)
			{
				continue;
			}
			body.constraints = state.constraints;
			body.isKinematic = state.isKinematic;
			body.useGravity = state.useGravity;
			if (!state.isKinematic)
			{
				body.linearVelocity = Vector3.zero;
				body.angularVelocity = Vector3.zero;
				body.WakeUp();
			}
		}
	}

	private bool IsCurrentPlayerCarStabilization(GameObject car, RigidbodyStabilizationState[] states)
	{
		return car != null && car == instantiatedCar && car == playerCarStabilizationCar && states == playerCarStabilizationStates;
	}

	private void BeginPlayerCarStabilization()
	{
		BeginPlayerCarStabilization(string.Empty);
	}

	private void BeginPlayerCarStabilization(string requiredSceneName)
	{
		CancelPlayerCarStabilization(true);
		if (instantiatedCar == null)
		{
			return;
		}
		Rigidbody[] bodies = instantiatedCar.GetComponentsInChildren<Rigidbody>(true);
		if (bodies.Length == 0)
		{
			return;
		}
		RigidbodyStabilizationState[] states = new RigidbodyStabilizationState[bodies.Length];
		for (int i = 0; i < bodies.Length; i++)
		{
			Rigidbody body = bodies[i];
			states[i] = new RigidbodyStabilizationState(body);
			if (!body.isKinematic)
			{
				body.linearVelocity = Vector3.zero;
				body.angularVelocity = Vector3.zero;
			}
			body.useGravity = false;
			body.isKinematic = true;
			body.constraints = RigidbodyConstraints.FreezeAll;
			body.Sleep();
		}
		playerCarStabilizationStates = states;
		playerCarStabilizationCar = instantiatedCar;
		playerCarStabilizationOwnsLoading = false;
		Physics.SyncTransforms();
		playerCarStabilization = StartCoroutine(ReleasePlayerCarAfterStabilization(instantiatedCar, requiredSceneName, states));
		if (Debug.isDebugBuild)
		{
			Debug.Log("Holding " + states.Length + " player-car rigidbodies for scene-gated stabilization; required scene=" + requiredSceneName);
		}
	}

	private IEnumerator ReleasePlayerCarAfterStabilization(GameObject car, string requiredSceneName, RigidbodyStabilizationState[] states)
	{
		float sceneDeadline = Time.realtimeSinceStartup + 30f;
		while (!string.IsNullOrEmpty(requiredSceneName))
		{
			if (!IsCurrentPlayerCarStabilization(car, states))
			{
				yield break;
			}
			Scene scene = SceneManager.GetSceneByName(requiredSceneName);
			if (scene.IsValid() && scene.isLoaded)
			{
				break;
			}
			if (Time.realtimeSinceStartup >= sceneDeadline)
			{
				Debug.LogError("Player car is still safely frozen while waiting for required ground scene: " + requiredSceneName);
				ShowPlayerCarStabilizationLoading();
				sceneDeadline = Time.realtimeSinceStartup + 30f;
			}
			yield return null;
		}
		// SceneManager reports a scene loaded before every recovered collider has
		// necessarily completed its first transform/physics synchronization.
		yield return null;
		yield return null;
		Physics.SyncTransforms();
		float groundDeadline = Time.realtimeSinceStartup + 10f;
		while (IsCurrentPlayerCarStabilization(car, states) && !AlignPlayerCarWheelsToGround(requiredSceneName))
		{
			if (Time.realtimeSinceStartup >= groundDeadline)
			{
				Debug.LogError("Player car is still safely frozen while waiting for four valid ground hits in scene " + requiredSceneName);
				ShowPlayerCarStabilizationLoading();
				groundDeadline = Time.realtimeSinceStartup + 10f;
			}
			yield return null;
			Physics.SyncTransforms();
		}
		if (!IsCurrentPlayerCarStabilization(car, states))
		{
			yield break;
		}
		Physics.SyncTransforms();
		RestorePlayerCarRigidbodies(states);
		playerCarStabilizationStates = null;
		playerCarStabilizationCar = null;
		playerCarStabilization = null;
		if (playerCarStabilizationOwnsLoading && rootLoading != null)
		{
			rootLoading.SetActive(false);
		}
		playerCarStabilizationOwnsLoading = false;
		if (Debug.isDebugBuild)
		{
			Debug.Log("Released stabilized player car at " + car.transform.position + " after loading scene " + requiredSceneName);
			StartCoroutine(LogPlayerCarPhysicsAfterRelease(car));
		}
	}

	private IEnumerator LogPlayerCarPhysicsAfterRelease(GameObject car)
	{
		for (int step = 1; step <= 1500; step++)
		{
			yield return new WaitForFixedUpdate();
			if (car == null || car != instantiatedCar)
			{
				yield break;
			}
			if (step > 10 && step % 25 != 0)
			{
				continue;
			}
			Rigidbody body = car.GetComponent<Rigidbody>();
			Rigidbody[] bodies = car.GetComponentsInChildren<Rigidbody>(true);
			float maximumChildSpeed = 0f;
			float maximumChildAngularSpeed = 0f;
			foreach (Rigidbody childBody in bodies)
			{
				if (childBody != body)
				{
					maximumChildSpeed = Mathf.Max(maximumChildSpeed, childBody.linearVelocity.magnitude);
					maximumChildAngularSpeed = Mathf.Max(maximumChildAngularSpeed, childBody.angularVelocity.magnitude);
				}
			}
			WheelCollider[] wheels = car.GetComponentsInChildren<WheelCollider>(true);
			int grounded = 0;
			float maximumWheelForce = 0f;
			float maximumForwardSlip = 0f;
			float maximumSidewaysSlip = 0f;
			float maximumMotorTorque = 0f;
			string wheelTelemetry = string.Empty;
			foreach (WheelCollider wheel in wheels)
			{
				WheelHit hit;
				float wheelForce = 0f;
				float forwardSlip = 0f;
				if (wheel.GetGroundHit(out hit))
				{
					grounded++;
					wheelForce = hit.force;
					forwardSlip = hit.forwardSlip;
					maximumWheelForce = Mathf.Max(maximumWheelForce, wheelForce);
					maximumForwardSlip = Mathf.Max(maximumForwardSlip, Mathf.Abs(forwardSlip));
					maximumSidewaysSlip = Mathf.Max(maximumSidewaysSlip, Mathf.Abs(hit.sidewaysSlip));
				}
				maximumMotorTorque = Mathf.Max(maximumMotorTorque, Mathf.Abs(wheel.motorTorque));
				wheelTelemetry += wheel.name + "[rpm=" + wheel.rpm.ToString("F1") + ",motorNm=" + wheel.motorTorque.ToString("F1") + ",brakeNm=" + wheel.brakeTorque.ToString("F1") + ",loadN=" + wheelForce.ToString("F0") + ",slip=" + forwardSlip.ToString("F3") + "] ";
			}
			CarControl control = car.GetComponent<CarControl>();
			CarSound sound = car.GetComponent<CarSound>();
			CarVisuals visuals = car.GetComponent<CarVisuals>();
			WheelFrictionCurve forward = wheels[0].forwardFriction;
			WheelFrictionCurve sideways = wheels[0].sidewaysFriction;
			float forwardSpeed = Vector3.Dot(body.linearVelocity, car.transform.forward);
			float displayedKmh = Mathf.Abs(forwardSpeed * 3.6f * 2f);
			Debug.Log("[PlayerCarPhysics] step=" + step + ", time=" + (step * Time.fixedDeltaTime).ToString("F2") + ", position=" + car.transform.position + ", rotation=" + car.transform.rotation.eulerAngles + ", velocity=" + body.linearVelocity + ", forwardMS=" + forwardSpeed.ToString("F2") + ", displayedKMH=" + displayedKmh.ToString("F1") + ", angularVelocity=" + body.angularVelocity + ", childSpeed=" + maximumChildSpeed.ToString("F3") + ", childAngular=" + maximumChildAngularSpeed.ToString("F3") + ", grounded=" + grounded + "/" + wheels.Length + ", maxWheelForce=" + maximumWheelForce.ToString("F1") + ", slips=" + maximumForwardSlip.ToString("F3") + "/" + maximumSidewaysSlip.ToString("F3") + ", friction=" + forward.extremumSlip.ToString("F3") + "/" + forward.extremumValue.ToString("F4") + "/" + forward.stiffness.ToString("F2") + " forward, " + sideways.extremumValue.ToString("F4") + " sideways, maxMotorNm=" + maximumMotorTorque.ToString("F1") + ", touchInput=" + gas.ToString("F3") + "/" + brakes.ToString("F3") + "/" + handbreak.ToString("F3") + ", carInput=" + control.motorInput.ToString("F3") + "/" + control.brakeInput.ToString("F3") + "/" + control.steerInput.ToString("F3") + ", drive=" + control.gearInput + ", displayGear=" + sound.currentGear + ", transmissionRPM=" + sound.transmissionRPM.ToString("F0") + ", engineRPM=" + sound.engineRPM.ToString("F0") + ", visualSpin=" + visuals.spinRateFL.ToString("F1") + "/" + visuals.spinRateFR.ToString("F1") + "/" + visuals.spinRateRL.ToString("F1") + "/" + visuals.spinRateRR.ToString("F1") + ", wheels=" + wheelTelemetry);
		}
	}

	public void InstantiateGameCar(VehicleCamera vehicleCamera)
	{
		DestroyInstantiatedCar();
		InfoCar info = MySingleton<Manager>.Instance.carsInfoPrefabs[MySingleton<Manager>.Instance.GetIndexArrayPosFromId(MySingleton<Manager>.Instance.indexCurrentCar)];
		InstantiateCar(info, vehicleCamera);
	}

	private void DestroyInstantiatedCar()
	{
		CancelPlayerCarStabilization(false);
		ResetCarInput();
		if (instantiatedCar != null)
		{
			instantiatedCar.SetActive(false);
		}
		Object.Destroy(instantiatedCar);
		Resources.UnloadUnusedAssets();
		instantiatedCar = null;
	}

	public void OnAIMPlayerConnected(NetworkPlayer networkPlayer)
	{
		if (MySingleton<Manager>.Instance.selectedGame == Manager.SelectedGame.MultiplayerDriving)
		{
			return;
		}
		int num = 0;
		for (int i = 0; i <= MySingleton<Manager>.Instance.networkManager.playersCount; i++)
		{
			if (!MySingleton<Manager>.Instance.networkFriends.IsFriend(networkPlayer))
			{
				continue;
			}
			num++;
			if (num >= 1)
			{
				MySingleton<AchievementManager>.Instance.ApplyAchivement(AchievementManager.Achievement.MULTIPLAYER_PLAY_FACEBOOK_FRIEND);
				MySingleton<Manager>.Instance.guiGameLogic.ActiveAchievementPanel(true);
				if (num >= MySingleton<Manager>.Instance.connection.maxPlayersPerRoom)
				{
					MySingleton<AchievementManager>.Instance.ApplyAchivement(AchievementManager.Achievement.MULTIPLAYER_PLAY_FACEBOOK_FRIEND_FULL_ROOM);
					MySingleton<Manager>.Instance.guiGameLogic.ActiveAchievementPanel(true);
				}
			}
		}
	}
}
