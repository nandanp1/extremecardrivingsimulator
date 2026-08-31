using System;
using System.Collections;
using System.Threading;
using UnityEngine;

public class TSTrafficSpawner : MonoBehaviour
{
	[Serializable]
	public class TSSpawnVehicles
	{
		public GameObject cars;

		public int frequency = 1;
	}

	public struct PointsIndex
	{
		public int lane;

		public int point;

		public PointsIndex(int l, int p)
		{
			lane = l;
			point = p;
		}
	}

	[SerializeField]
	public TSSpawnVehicles[] cars;

	public int amount = 50;

	public float maxDistance = 150f;

	public float offset = 140f;

	public float closerRange = 25f;

	public float refreshTime = 0.02f;

	public TSMainManager manager;

	public Vector3 unusedCarsPosition = new Vector3(50000f, 50000f, 50000f);

	public bool respawnIfUpSideDown;

	public float respawnUpSideDownTime = 2f;

	public bool disableMultiThreading;

	private TSTrafficVolume[] trafficVolumes;

	private TSTrafficAI[] trafficCars;

	private Transform[] trafficCarsTransform;

	private Vector3[] trafficCarsPositions;

	private bool[] trafficCarsFarIndexes;

	private PointsIndex[] pointsIndex = new PointsIndex[100];

	private float nextActionTime;

	private Vector3 myPosition = Vector3.zero;

	private Thread[] threads;

	private AutoResetEvent[] jobAvailable;

	private ManualResetEvent[] threadIdle;

	private int threadsCount;

	private bool close;

	private UnityEngine.Object lock2 = new UnityEngine.Object();

	private int currentVolume;

	private int trafficarLastAdded;

	private int _totalFarCars;

	private float maxDistanceSQRMax;

	private float maxDistanceSQRMin;

	private Transform trafficCarsParent;

	public static TSTrafficSpawner mainInstance;

	private int currentAmountOfCar;

	private int laneIndex1;

	private int pointIndex1;

	private int currentPointIndex;

	private int currentFarCar;

	private int time;

	private TSTrafficAI AI;

	private TSNavigation nav;

	public float MaxDistance
	{
		get
		{
			return maxDistance;
		}
		set
		{
			maxDistance = value;
			maxDistanceSQRMax = (maxDistance + offset) * (maxDistance + offset);
			maxDistanceSQRMin = (maxDistance - offset) * (maxDistance - offset);
		}
	}

	public float Offset
	{
		get
		{
			return offset;
		}
		set
		{
			offset = value;
			maxDistanceSQRMax = (maxDistance + offset) * (maxDistance + offset);
			maxDistanceSQRMin = (maxDistance - offset) * (maxDistance - offset);
		}
	}

	public TSTrafficAI[] TrafficCars
	{
		get
		{
			return trafficCars;
		}
	}

	public int totalFarCars
	{
		get
		{
			return _totalFarCars;
		}
	}

	public Transform[] TrafficCarsTransform
	{
		get
		{
			return trafficCarsTransform;
		}
	}

	public Vector3[] TrafficCarsPositions
	{
		get
		{
			return trafficCarsPositions;
		}
	}

	public static float RespawnUpSideDownTime
	{
		get
		{
			return mainInstance.respawnUpSideDownTime;
		}
	}

	public int Amount
	{
		get
		{
			return amount;
		}
		set
		{
			amount = value;
		}
	}

	private void Awake()
	{
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Traffic)
		{
			base.enabled = false;
		}
		else
		{
			base.enabled = true;
		}
		if (base.enabled)
		{
			if ((bool)mainInstance)
			{
				UnityEngine.Object.Destroy(mainInstance.gameObject);
			}
			mainInstance = this;
			if (manager == null)
			{
				manager = UnityEngine.Object.FindFirstObjectByType(typeof(TSMainManager)) as TSMainManager;
			}
			Transform currentCar = MySingleton<Manager>.Instance.gameLogic.currentCar;
			if ((bool)currentCar)
			{
				base.transform.position = currentCar.position;
				base.transform.SetParent(currentCar);
				base.gameObject.SetActive(true);
			}
			GameObject gameObject = GameObject.Find("TrafficCarsContainer");
			if ((bool)gameObject)
			{
				UnityEngine.Object.Destroy(gameObject);
			}
			trafficCarsParent = new GameObject("TrafficCarsContainer").transform;
			PopulateInitialPoints();
			AddCarsStart();
		}
	}

	private void PopulateInitialPoints()
	{
		while (currentPointIndex < pointsIndex.Length)
		{
			for (int i = 0; i < manager.lanes.Length; i++)
			{
				for (int j = 0; j < manager.lanes[i].points.Length; j++)
				{
					pointsIndex[currentPointIndex] = new PointsIndex(i, j);
				}
			}
			currentPointIndex++;
		}
		currentPointIndex = 0;
	}

	private void AddCarsStart()
	{
		maxDistanceSQRMax = (maxDistance + offset) * (maxDistance + offset);
		maxDistanceSQRMin = (maxDistance - offset) * (maxDistance - offset);
		trafficVolumes = UnityEngine.Object.FindObjectsByType(typeof(TSTrafficVolume), FindObjectsSortMode.InstanceID) as TSTrafficVolume[];
		bool flag = false;
		GameObject gameObject = null;
		int returnedIndex = 0;
		int num = 0;
		int num2 = 0;
		Bounds bounds = default(Bounds);
		float num3 = 0f;
		int num4 = 0;
		for (int i = 0; i < cars.Length; i++)
		{
			num4 += cars[i].frequency;
		}
		int num5 = ((amount < cars.Length) ? ((cars.Length >= num4 + cars.Length) ? cars.Length : (num4 + cars.Length)) : ((amount >= num4 + cars.Length) ? amount : (num4 + cars.Length)));
		trafficCars = new TSTrafficAI[num5];
		trafficCarsTransform = new Transform[num5];
		trafficCarsFarIndexes = new bool[num5];
		trafficCarsPositions = new Vector3[num5];
		bool flag2 = false;
		int currentIndex = 0;
		for (int j = 0; j < num5; j++)
		{
			if (!flag)
			{
				if (amount < cars.Length)
				{
					returnedIndex = j;
				}
				else if (!flag2)
				{
					bool flag3 = false;
					while (!flag3)
					{
						flag3 = GetCarByFrequency(ref currentIndex, out returnedIndex);
					}
					if (currentIndex >= cars.Length)
					{
						flag2 = true;
					}
				}
				else
				{
					returnedIndex = UnityEngine.Random.Range(0, cars.Length);
				}
				returnedIndex = Mathf.Clamp(returnedIndex, 0, cars.Length - 1);
				gameObject = UnityEngine.Object.Instantiate(cars[returnedIndex].cars) as GameObject;
				gameObject.GetComponent<PaintCar>().IniPaintMaterials(base.gameObject.GetComponent<CarsColors>().GetColorGroup(gameObject));
				gameObject.transform.parent = trafficCarsParent;
				bounds = CarSize(gameObject);
				num2 = UnityEngine.Random.Range(0, manager.lanes.Length - 1);
				num3 = bounds.size.z + 3f;
				num = UnityEngine.Random.Range(0, manager.lanes[num2].points.Length - 1);
			}
			if ((float)num >= (float)manager.lanes[num2].points.Length - num3)
			{
				num2 = UnityEngine.Random.Range(0, manager.lanes.Length - 1);
				num = UnityEngine.Random.Range(0, manager.lanes[num2].points.Length - 1);
			}
			TSTrafficAI AI = gameObject.GetComponent<TSTrafficAI>();
			float num6 = ((num <= 0) ? 0f : (manager.lanes[num2].points[num - 1].distanceToNextPoint - manager.lanes[num2].points[num].distanceToNextPoint));
			int num7 = 0;
			bool flag4 = true;
			if (!CheckTrafficVolume(out currentVolume, manager.lanes[num2].points[num].point))
			{
				flag4 = false;
			}
			if ((int)((uint)manager.lanes[num2].vehicleType & (uint)(1 << (int)AI.myVehicleType)) <= 0)
			{
				flag4 = false;
			}
			while (num6 < num3)
			{
				num6 += manager.lanes[num2].points[num + num7].distanceToNextPoint;
				num7++;
				if (num + num7 > manager.lanes[num2].points.Length - 1 || manager.lanes[num2].totalOcupation > Mathf.Round(manager.lanes[num2].trafficDensity * 100f))
				{
					flag4 = false;
					break;
				}
			}
			TSNavigation component = gameObject.GetComponent<TSNavigation>();
			component.lanes = manager.lanes;
			for (int k = 0; k < num7; k++)
			{
				if (flag4 && manager.lanes[num2].points[num + k].reservationID != 0)
				{
					flag4 = false;
				}
			}
			if (flag4)
			{
				if (currentVolume != -1)
				{
					trafficVolumes[currentVolume].carsOnThisSection.Add(AI);
				}
				for (int l = 0; l < num7; l++)
				{
					manager.lanes[num2].points[num + l].reservationID = component.GetInstanceID();
					TSNavigation.TSReservedPoints item = new TSNavigation.TSReservedPoints
					{
						point = manager.lanes[num2].points[num + l],
						connector = null
					};
					component.reservedPoints.Enqueue(item);
				}
				Quaternion rotation = Quaternion.LookRotation(manager.lanes[num2].points[num + num7 / 2 + 1].point - manager.lanes[num2].points[num + num7 / 2].point);
				gameObject.transform.position = manager.lanes[num2].points[num + num7 / 2].point + Vector3.up * 0.3f;
				gameObject.transform.rotation = rotation;
				AddTrafficCar(ref AI);
				trafficCarsPositions[j] = AI.transform.position;
				component.lanes = manager.lanes;
				component.wasTravelingOnConnector = false;
				component.travelingOnConector = false;
				component.lastLane = component.currentLane;
				component.RelativeWaypointPosition = Vector3.zero;
				component.RelativeWaypointPositionOnCar = Vector3.zero;
				component.relativeWPosMagnitude = 0f;
				AI.nextPathIndex = 0;
				AI.segDistance = 0f;
				AI.currentConnector = null;
				AI.lane = 0;
				AI.brakeSpeeds.Clear();
				component.selectedConnector = 0;
				component.lastSelectedConnector = null;
				component.starting = true;
				component.currentWaypoint = num + num7 - 1;
				component.currentLane = num2;
				component.currentWaypointOnCar = num;
				component.previousWaypointSteer = num;
				component.waypoints = manager.lanes[num2].points;
				component.lastWaypoints = component.waypoints;
				component.currentMaxSpeed = manager.lanes[num2].maxSpeed;
				AI.carDepth = bounds.size.z;
				AI.carWidth = bounds.size.x;
				AI.carHeight = bounds.size.y;
				AI.previousWaypointCurve = num + num7;
				AI.currenPoint = component.lastWaypoints[AI.previousWaypointCurve];
				component.AddNextTrackToPath();
				flag = false;
				AI.Enable(true);
				component.GetLaneMaxSpeed();
				component.SetCarOccupationLength();
				component.lanes[component.currentLane].totalOcupation += Mathf.Round(component.CarOcupationLenght / component.lanes[component.currentLane].totalDistance * 100f);
				trafficCarsFarIndexes[j] = false;
			}
			else
			{
				component.lanes = manager.lanes;
				AddTrafficCar(ref AI);
				trafficCarsPositions[j] = AI.transform.position;
				AI.carDepth = bounds.size.z;
				AI.carWidth = bounds.size.x;
				AI.carHeight = bounds.size.y;
				component.SetCarOccupationLength();
				AI.Enable(false);
				AI.transform.position = unusedCarsPosition;
				trafficCarsFarIndexes[j] = true;
				_totalFarCars++;
				flag = false;
			}
		}
		Initialize();
	}

	private void OnEnable()
	{
		if (threadsCount == 0)
		{
			StopCoroutine(CheckNearLanesSingleThread());
			StartCoroutine(CheckNearLanesSingleThread());
		}
	}

	private bool GetCarByFrequency(ref int currentIndex, out int returnedIndex)
	{
		returnedIndex = 0;
		if (currentIndex < cars.Length)
		{
			if (cars[currentIndex].frequency != 0 && currentAmountOfCar < cars[currentIndex].frequency)
			{
				returnedIndex = currentIndex;
				currentAmountOfCar++;
				return true;
			}
			currentIndex++;
			currentAmountOfCar = 0;
			return false;
		}
		return true;
	}

	private void AddTrafficCar(ref TSTrafficAI AI)
	{
		trafficCars[trafficarLastAdded] = AI;
		trafficCarsTransform[trafficarLastAdded] = AI.transform;
		trafficarLastAdded++;
	}

	private bool CheckTrafficVolume(out int volume, Vector3 point)
	{
		volume = -1;
		for (int i = 0; i < trafficVolumes.Length; i++)
		{
			if (trafficVolumes[i].GetComponent<Collider>().bounds.Contains(point))
			{
				if (trafficVolumes[i].carsOnThisSection.Count < trafficVolumes[i].maxAllowedCars)
				{
					volume = i;
					return true;
				}
				return false;
			}
		}
		return true;
	}

	private Bounds CarSize(GameObject car)
	{
		Quaternion rotation = car.transform.rotation;
		Vector3 position = car.transform.position;
		car.transform.rotation = Quaternion.Euler(Vector3.zero);
		car.transform.position = Vector3.zero;
		Bounds result = new Bounds(car.transform.position, Vector3.zero);
		Collider[] componentsInChildren = car.GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			result.Encapsulate(collider.bounds);
		}
		car.transform.rotation = rotation;
		car.transform.position = position;
		return result;
	}

	private void Update()
	{
		UpdateCarPositions();
		myPosition = base.transform.position;
		if (Time.time - nextActionTime > refreshTime)
		{
			nextActionTime = Time.time;
			CheckFarCarsSingleThread();
			AddCar();
		}
	}

	private void UpdateCarPositions()
	{
		for (int i = 0; i < trafficCars.Length; i++)
		{
			trafficCarsPositions[i] = trafficCarsTransform[i].position;
		}
	}

	private void CheckNearLanes()
	{
		for (laneIndex1 = 0; laneIndex1 < manager.lanes.Length; laneIndex1++)
		{
			for (pointIndex1 = 0; pointIndex1 < manager.lanes[laneIndex1].points.Length; pointIndex1++)
			{
				float sqrMagnitude = (manager.lanes[laneIndex1].points[pointIndex1].point - myPosition).sqrMagnitude;
				if (sqrMagnitude > maxDistanceSQRMin && sqrMagnitude < maxDistanceSQRMax && manager.lanes[laneIndex1].points[pointIndex1].reservationID == 0 && pointIndex1 + 3 < manager.lanes[laneIndex1].points.Length && manager.lanes[laneIndex1].points[pointIndex1 + 1].reservationID == 0 && manager.lanes[laneIndex1].points[pointIndex1 + 2].reservationID == 0 && manager.lanes[laneIndex1].points[pointIndex1 + 3].reservationID == 0)
				{
					lock (lock2)
					{
						pointsIndex[currentPointIndex].lane = laneIndex1;
						pointsIndex[currentPointIndex].point = pointIndex1;
					}
					currentPointIndex++;
					if (currentPointIndex >= pointsIndex.Length)
					{
						currentPointIndex = 0;
					}
				}
			}
		}
	}

	private void CheckFarCars()
	{
		for (int i = 0; i < trafficCarsPositions.Length; i++)
		{
			float sqrMagnitude = (trafficCarsPositions[i] - myPosition).sqrMagnitude;
			if (sqrMagnitude > maxDistanceSQRMax * 1.2f || (respawnIfUpSideDown && trafficCars[i].ForcedRespawn) || !trafficCars[i].enabled)
			{
				if (!trafficCarsFarIndexes[i])
				{
					_totalFarCars++;
					trafficCarsFarIndexes[i] = true;
				}
			}
			else if (trafficCarsFarIndexes[i])
			{
				_totalFarCars--;
				trafficCarsFarIndexes[i] = false;
			}
			if (sqrMagnitude > closerRange * closerRange)
			{
				if (trafficCars[currentFarCar].OnFarRange != null)
				{
					trafficCars[currentFarCar].OnFarRange();
				}
			}
			else if (trafficCars[currentFarCar].OnCloserRange != null)
			{
				trafficCars[currentFarCar].OnCloserRange();
			}
		}
	}

	private IEnumerator CheckNearLanesSingleThread()
	{
		int time = 0;
		while (true)
		{
			for (int w = 0; w < manager.lanes.Length; w++)
			{
				for (int i = 0; i < manager.lanes[w].points.Length; i++)
				{
					float distance3 = (manager.lanes[w].points[i].point - myPosition).sqrMagnitude;
					if (distance3 > maxDistanceSQRMin && distance3 < maxDistanceSQRMax && manager.lanes[w].points[i].reservationID == 0)
					{
						pointsIndex[currentPointIndex].lane = w;
						pointsIndex[currentPointIndex].point = i;
						currentPointIndex++;
						if (currentPointIndex >= pointsIndex.Length)
						{
							currentPointIndex = 0;
						}
					}
					time++;
					if (time > 250)
					{
						time = 0;
						yield return null;
					}
				}
			}
		}
	}

	private void CheckFarCarsSingleThread()
	{
		while (currentFarCar < trafficCarsPositions.Length)
		{
			if (time > 50)
			{
				time = 0;
				return;
			}
			float sqrMagnitude = (trafficCarsTransform[currentFarCar].position - myPosition).sqrMagnitude;
			if (sqrMagnitude > maxDistanceSQRMax * 1.2f || (respawnIfUpSideDown && trafficCars[currentFarCar].ForcedRespawn) || !trafficCarsTransform[currentFarCar].gameObject.activeSelf)
			{
				if (!trafficCarsFarIndexes[currentFarCar])
				{
					trafficCarsFarIndexes[currentFarCar] = true;
					_totalFarCars++;
					trafficCars[currentFarCar].NavigateToWaypoints.UnreserveAll(true);
					trafficCars[currentFarCar].Enable(false);
					trafficCars[currentFarCar].ForcedRespawn = false;
					trafficCarsTransform[currentFarCar].position = unusedCarsPosition;
				}
			}
			else if (trafficCarsFarIndexes[currentFarCar])
			{
				_totalFarCars--;
				trafficCarsFarIndexes[currentFarCar] = false;
			}
			if (sqrMagnitude > closerRange * closerRange)
			{
				if (trafficCars[currentFarCar].OnFarRange != null)
				{
					trafficCars[currentFarCar].OnFarRange();
				}
			}
			else if (trafficCars[currentFarCar].OnCloserRange != null)
			{
				trafficCars[currentFarCar].OnCloserRange();
			}
			time++;
			currentFarCar++;
		}
		currentFarCar = 0;
		time = 0;
	}

	private void CheckNearLanesLoop(object obj)
	{
		do
		{
			CheckNearLanes();
			Thread.Sleep(0);
		}
		while (!close);
	}

	private void RemoveCar(int carIndex)
	{
		trafficCars[carIndex].NavigateToWaypoints.UnreserveAll(true);
	}

	public void DisableAllAICars()
	{
		_totalFarCars = 0;
		currentFarCar = 0;
		for (int i = 0; i < trafficCarsPositions.Length; i++)
		{
			if (!trafficCarsFarIndexes[i])
			{
				trafficCarsFarIndexes[i] = true;
				_totalFarCars++;
				trafficCars[i].NavigateToWaypoints.UnreserveAll(true);
				trafficCars[i].Enable(false);
				trafficCars[i].ForcedRespawn = false;
				trafficCarsTransform[i].position = unusedCarsPosition;
			}
		}
	}

	private int GetNextCarFarIndex()
	{
		int num = UnityEngine.Random.Range(0, trafficCarsFarIndexes.Length);
		for (int i = 0; i < trafficCarsFarIndexes.Length; i++)
		{
			if (trafficCarsFarIndexes[num])
			{
				return num;
			}
			num++;
			if (num >= trafficCarsFarIndexes.Length)
			{
				num = 0;
			}
		}
		return -1;
	}

	private int GetNextCarFarIndex(int currentIndex)
	{
		for (int i = currentIndex; i < trafficCarsFarIndexes.Length; i++)
		{
			if (trafficCarsFarIndexes[i])
			{
				return i;
			}
		}
		return -1;
	}

	private void AddCar()
	{
		int nextCarFarIndex = GetNextCarFarIndex();
		if (nextCarFarIndex == -1 || trafficCars.Length - _totalFarCars > amount || _totalFarCars < 1)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		int num3 = 0;
		lock (lock2)
		{
			num = UnityEngine.Random.Range(0, pointsIndex.Length - 1);
			num2 = pointsIndex[num].point;
			num3 = pointsIndex[num].lane;
		}
		bool flag = true;
		if (!CheckTrafficVolume(out currentVolume, manager.lanes[num3].points[num2].point))
		{
			flag = false;
		}
		int num4 = nextCarFarIndex;
		float num5 = ((num2 <= 0) ? 0f : (manager.lanes[num3].points[num2 - 1].distanceToNextPoint - manager.lanes[num3].points[num2].distanceToNextPoint));
		int num6 = 0;
		int num7 = nextCarFarIndex;
		AI = trafficCars[num4];
		float num8 = maxDistanceSQRMin;
		float sqrMagnitude = (base.transform.position - manager.lanes[num3].points[num2].point).sqrMagnitude;
		if (sqrMagnitude < num8)
		{
			return;
		}
		int num9 = 0;
		while (num9 < _totalFarCars && flag)
		{
			num4 = num7;
			AI = trafficCars[num4];
			if ((int)((uint)manager.lanes[num3].vehicleType & (uint)(1 << (int)AI.myVehicleType)) > 0)
			{
				break;
			}
			num7 = GetNextCarFarIndex(num7);
			num9++;
			if (num7 == -1 || num9 >= _totalFarCars)
			{
				flag = false;
			}
		}
		if (!flag)
		{
			return;
		}
		nav = trafficCars[num4].NavigateToWaypoints;
		float carDepth = trafficCars[num4].carDepth;
		if (manager.lanes[num3].points[num2].reservationID != 0)
		{
			return;
		}
		while (num5 < carDepth)
		{
			if (num2 + num6 >= manager.lanes[num3].points.Length - 1 || manager.lanes[num3].totalOcupation > Mathf.Round(manager.lanes[num3].trafficDensity * 100f))
			{
				flag = false;
				return;
			}
			num5 += manager.lanes[num3].points[num2 + num6].distanceToNextPoint;
			num6++;
			if (manager.lanes[num3].points[num2 + num6].reservationID != 0)
			{
				return;
			}
		}
		if (!flag || num2 + num6 >= manager.lanes[num3].points.Length)
		{
			return;
		}
		flag = false;
		if (currentVolume != -1)
		{
			trafficVolumes[currentVolume].carsOnThisSection.Add(AI);
		}
		if (nav.lanes.Length == 0)
		{
			nav.lanes = manager.lanes;
		}
		Quaternion rotation = Quaternion.LookRotation(manager.lanes[num3].points[num2 + num6 / 2 + 1].point - manager.lanes[num3].points[num2 + num6 / 2].point);
		if (AI.bodies != null)
		{
			for (int i = 0; i < AI.bodies.Length; i++)
			{
				if (!(AI.bodies[i] == null))
				{
					AI.bodies[i].linearVelocity = Vector3.zero;
					AI.bodies[i].angularVelocity = Vector3.zero;
					AI.bodies[i].transform.localRotation = Quaternion.identity;
					AI.bodies[i].transform.localPosition = Vector3.zero;
				}
			}
		}
		trafficCars[num4].transform.position = manager.lanes[num3].points[num2 + num6 / 2].point + Vector3.up * 0.3f;
		trafficCars[num4].transform.rotation = rotation;
		trafficCarsFarIndexes[num4] = false;
		_totalFarCars--;
		trafficCarsPositions[num4] = trafficCars[num4].transform.position;
		for (int j = 0; j <= num6; j++)
		{
			manager.lanes[num3].points[num2 + j].reservationID = nav.myID;
			TSNavigation.TSReservedPoints item = new TSNavigation.TSReservedPoints
			{
				point = manager.lanes[num3].points[num2 + j],
				connector = null
			};
			nav.reservedPoints.Enqueue(item);
		}
		nav.currentWaypoint = num2 + num6;
		nav.currentLane = num3;
		nav.currentWaypointOnCar = num2;
		nav.previousWaypointSteer = num2;
		AI.previousWaypointCurve = num2 + num6;
		nav.waypoints = manager.lanes[num3].points;
		nav.wasTravelingOnConnector = false;
		nav.travelingOnConector = false;
		nav.lastLane = nav.currentLane;
		nav.lastWaypoints = nav.waypoints;
		nav.RelativeWaypointPosition = Vector3.zero;
		nav.RelativeWaypointPositionOnCar = Vector3.zero;
		nav.relativeWPosMagnitude = 0f;
		AI.nextPathIndex = 0;
		AI.segDistance = 0f;
		AI.currenPoint = nav.lastWaypoints[AI.previousWaypointCurve];
		AI.currentConnector = null;
		AI.lane = 0;
		AI.brakeSpeeds.Clear();
		nav.selectedConnector = 0;
		nav.lastSelectedConnector = null;
		nav.starting = true;
		nav.currentMaxSpeed = manager.lanes[num3].maxSpeed;
		nav.AddNextTrackToPath();
		nav.lanes[nav.currentLane].totalOcupation += Mathf.Round(nav.CarOcupationLenght / nav.lanes[nav.currentLane].totalDistance * 100f);
		nav.GetLaneMaxSpeed();
		trafficCars[num4].Enable(true);
	}

	private void Initialize()
	{
		threadsCount = Environment.ProcessorCount;
		if (disableMultiThreading)
		{
			threadsCount = 0;
		}
		if (threadsCount <= 1)
		{
			threadsCount = 0;
			return;
		}
		threadsCount = Mathf.Clamp(threadsCount, 0, 1);
		jobAvailable = new AutoResetEvent[1];
		threadIdle = new ManualResetEvent[1];
		threads = new Thread[1];
		jobAvailable[0] = new AutoResetEvent(false);
		threadIdle[0] = new ManualResetEvent(true);
		threads[0] = new Thread(CheckNearLanesLoop);
		threads[0].IsBackground = false;
		threads[0].Start(0);
	}

	public void Close()
	{
		for (int i = 0; i < threadsCount; i++)
		{
			jobAvailable[i].Set();
		}
	}

	private void OnDisable()
	{
		close = true;
	}

	private void OnDestroy()
	{
		close = true;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, maxDistance + offset);
		Gizmos.DrawWireSphere(base.transform.position, maxDistance - offset);
	}
}
