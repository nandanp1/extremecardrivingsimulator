using System;
using UnityEngine;

public class CarController : MonoBehaviour
{
	public enum WayMove
	{
		Center = 1,
		Right = 2,
		Left = 3
	}

	private class WheelData
	{
		public Transform transform;

		public GameObject go;

		public WheelCollider col;

		public Vector3 startPos;

		public float rotation;

		public float maxSteer;

		public bool motor;

		public float curY;
	}

	private const float TWO_WAYS_CENTER_DISTANCE = 2f;

	public const float LOOK_AT_TIME = 0.25f;

	public bool carParked;

	public GameObject checkForActive;

	public CarMode carMode;

	public Transform wheelFR_LOD0;

	public Transform wheelFL_LOD0;

	public Transform wheelRR_LOD0;

	public Transform wheelRL_LOD0;

	public float suspensionDistance = 0.2f;

	public float springs = 1000f;

	public float dampers = 2f;

	public bool AutomaticWheelRadius = true;

	public float wheelRadius = 0.1f;

	public float torque = 100f;

	public float brakeTorque = 2000f;

	public float wheelWeight = 3f;

	public Vector3 shiftCentre = new Vector3(0f, -0.25f, 0f);

	public float maxSteerAngle = 30f;

	public float steerSpeed = 3f;

	public JWheelDrive wheelDrive;

	public float fwdStiffness = 0.2f;

	public float swyStiffness = 0.2f;

	public Transform carBody;

	private AudioSource audioSource;

	private AudioSource brakeAudioSource;

	private Rigidbody m_rigidbody;

	public float stiffness = 0.1f;

	private float clearDistance = 110f;

	public GameObject brakeLightsParent;

	public GameObject frontLightsParent;

	public GameObject[] brakeLights;

	public GameObject[] frontLights;

	private GameObject player;

	public float sideLength;

	public float minSideLength;

	public float maxSideLength;

	private float steerEvent;

	private RaycastHit carHit;

	private float hurnTime;

	private float targetAngle;

	[HideInInspector]
	public float pointspeed;

	public Transform currentPoint;

	public Transform wayPoint;

	public Transform wayPoint2;

	[HideInInspector]
	public int randomStart;

	public Transform next_point;

	public string myStatue;

	private WayMove wayMove = WayMove.Center;

	private bool wayActive;

	private bool waysActive;

	private int waysRandom;

	public Transform hitFront;

	public Transform hitRight;

	public Transform hitLeft;

	public float nextPointDistance = 5f;

	private float steer;

	private float rotateSpeed = 1f;

	private bool brake;

	private AIContoller ACScript;

	private float shiftDelay;

	public AudioSource switchGear;

	[HideInInspector]
	public int currentGear = 1;

	[HideInInspector]
	public float motorRPM;

	[HideInInspector]
	public float speed;

	public bool automatic = true;

	public float[] gears = new float[6] { -10f, 9f, 6f, 4.5f, 3f, 2.5f };

	private float[] efficiencyTable = new float[22]
	{
		0.6f, 0.65f, 0.7f, 0.75f, 0.8f, 0.85f, 0.9f, 1f, 1f, 0.95f,
		0.8f, 0.7f, 0.6f, 0.5f, 0.45f, 0.4f, 0.36f, 0.33f, 0.3f, 0.2f,
		0.1f, 0.05f
	};

	private float efficiencyTableStep = 250f;

	public float shiftDownRPM = 1500f;

	public float shiftUpRPM = 2500f;

	public float idleRPM = 500f;

	private float wantedRPM;

	private float curRotation;

	private float brackF;

	public AudioClip brakeSound;

	private GameObject[] Particle = new GameObject[4];

	private float accel;

	private WheelData[] wheels;

	public bool showNormalGizmos;

	private WheelData SetWheelParams(Transform wheel, float maxSteer, bool motor, float curY)
	{
		if (wheel == null)
		{
			throw new Exception("wheel not connected to script!");
		}
		WheelData wheelData = new WheelData();
		GameObject gameObject = new GameObject("WheelCollider");
		gameObject.transform.parent = base.transform;
		gameObject.layer = LayerMask.NameToLayer("Traffic");
		gameObject.transform.position = wheel.position;
		gameObject.transform.eulerAngles = base.transform.eulerAngles;
		curY = wheel.transform.localPosition.y;
		wheel.transform.gameObject.AddComponent<WheelCollider>();
		WheelCollider wheelCollider = (WheelCollider)gameObject.AddComponent(typeof(WheelCollider));
		wheelCollider.transform.localScale = wheel.transform.localScale;
		wheelCollider.radius = wheel.transform.GetComponent<WheelCollider>().radius;
		UnityEngine.Object.Destroy(wheel.transform.GetComponent<WheelCollider>());
		wheelCollider.motorTorque = 0f;
		wheelData.transform = wheel;
		wheelData.curY = curY;
		wheelData.go = gameObject;
		wheelData.col = wheelCollider;
		wheelData.startPos = wheel.transform.localPosition;
		wheelData.maxSteer = maxSteer;
		wheelData.motor = motor;
		return wheelData;
	}

	private void Awake()
	{
		m_rigidbody = GetComponent<Rigidbody>();
		InstanceTraffic instanceTraffic = UnityEngine.Object.FindFirstObjectByType(typeof(InstanceTraffic)) as InstanceTraffic;
		ACScript = instanceTraffic.trafficPrefab.GetComponent<AIContoller>();
		minSideLength = ACScript.minSideLength;
		maxSideLength = ACScript.maxSideLength;
		if (randomStart == 0)
		{
			wayMove = WayMove.Right;
			sideLength = UnityEngine.Random.Range(-0.5f, 0.5f);
		}
		else if (randomStart == 1)
		{
			wayMove = WayMove.Left;
			sideLength = UnityEngine.Random.Range(minSideLength, maxSideLength);
		}
		else if (randomStart == 2)
		{
			wayMove = WayMove.Right;
			sideLength = UnityEngine.Random.Range(minSideLength, maxSideLength);
		}
		player = MySingleton<Manager>.Instance.gameLogic.instantiatedCar;
		wheels = new WheelData[4];
		bool motor = wheelDrive == JWheelDrive.Front || wheelDrive == JWheelDrive.All;
		bool motor2 = wheelDrive == JWheelDrive.Back || wheelDrive == JWheelDrive.All;
		wheels[0] = SetWheelParams(wheelFR_LOD0, maxSteerAngle, motor, wheelFR_LOD0.localPosition.y);
		wheels[1] = SetWheelParams(wheelFL_LOD0, maxSteerAngle, motor, wheelFL_LOD0.localPosition.y);
		wheels[2] = SetWheelParams(wheelRR_LOD0, 0f, motor2, wheelRR_LOD0.localPosition.y);
		wheels[3] = SetWheelParams(wheelRL_LOD0, 0f, motor2, wheelRL_LOD0.localPosition.y);
		WheelData[] array = wheels;
		foreach (WheelData wheelData in array)
		{
			WheelCollider col = wheelData.col;
			col.suspensionDistance = suspensionDistance;
			JointSpring suspensionSpring = col.suspensionSpring;
			suspensionSpring.spring = springs;
			suspensionSpring.damper = dampers;
			col.suspensionSpring = suspensionSpring;
			if (!AutomaticWheelRadius)
			{
				col.radius = wheelRadius;
			}
			col.mass = wheelWeight;
			WheelFrictionCurve forwardFriction = col.forwardFriction;
			forwardFriction.asymptoteValue = 5000f;
			forwardFriction.extremumSlip = 2f;
			forwardFriction.asymptoteSlip = 20f;
			forwardFriction.stiffness = stiffness;
			col.forwardFriction = forwardFriction;
			forwardFriction = col.sidewaysFriction;
			forwardFriction.asymptoteValue = 7500f;
			forwardFriction.asymptoteSlip = 2f;
			forwardFriction.stiffness = stiffness;
			col.sidewaysFriction = forwardFriction;
		}
		audioSource = (AudioSource)GetComponent(typeof(AudioSource));
		if (audioSource == null)
		{
			Debug.Log("No audio source, add one to the car with looping engine noise (but can be turned off");
		}
	}

	private void OnEnable()
	{
		player = MySingleton<Manager>.Instance.gameLogic.instantiatedCar;
		ACScript.numCars++;
		clearDistance = ACScript.bigWaypointsDetectionArea;
		CancelInvoke("LookAtNextPoint");
		if (randomStart == 0)
		{
			wayMove = WayMove.Right;
			sideLength = UnityEngine.Random.Range(-0.5f, 0.5f);
		}
		else if (randomStart == 1)
		{
			wayMove = WayMove.Left;
			sideLength = UnityEngine.Random.Range(minSideLength, maxSideLength);
		}
		else if (randomStart == 2)
		{
			wayMove = WayMove.Right;
			sideLength = UnityEngine.Random.Range(minSideLength, maxSideLength);
		}
		if (carMode == CarMode.AICar)
		{
			Invoke("LookAtNextPoint", 0.25f);
		}
	}

	private void OnDisable()
	{
		ACScript.numCars--;
		currentPoint = null;
		wayPoint = null;
		wayPoint2 = null;
		myStatue = string.Empty;
		randomStart = 0;
	}

	private void LookAtNextPoint()
	{
		if (next_point == null)
		{
			return;
		}
		base.transform.LookAt(next_point);
		TrafficWaypoint nextWaypoint = next_point.GetComponent<TrafficWaypoint>();
		if (nextWaypoint == null || !nextWaypoint.onlyOneWay)
		{
			base.transform.position = base.transform.TransformPoint(2f, 0f, 0f);
		}
	}

	private void Update()
	{
		speed = m_rigidbody.linearVelocity.magnitude * 3.6f;
		if ((bool)player && Vector3.Distance(base.transform.position, player.transform.position) > clearDistance)
		{
			ACScript.GetCarForInactiveTraffic(base.gameObject);
		}
		if (carMode == CarMode.Empty)
		{
			brake = true;
		}
		bool flag = false;
		if (carMode == CarMode.Player || carMode == CarMode.Empty)
		{
			return;
		}
		if (steerEvent != 0f)
		{
			steerEvent = Mathf.SmoothStep(steerEvent, 0f, Time.deltaTime * 5f);
		}
		if ((bool)player && (Vector3.Distance(hitFront.position, player.transform.position) < 2f || (Vector3.Distance(hitRight.position, player.transform.position) < 1.5f && Mathf.Abs(steer) > 0.1f) || (Vector3.Distance(hitLeft.position, player.transform.position) < 1.5f && Mathf.Abs(steer) > 0.1f)))
		{
			brake = true;
			flag = true;
		}
		else
		{
			brake = false;
		}
		float num = 0f;
		float x = 0f;
		rotateSpeed = 1f;
		for (int i = 0; i != 3; i++)
		{
			#if UNITY_EDITOR
			Color color = Color.white;
			#endif
			if (i == 0)
			{
				#if UNITY_EDITOR
				color = Color.blue;
				#endif
				x = -1.5f;
				num = 6f;
			}
			if (i == 1)
			{
				#if UNITY_EDITOR
				color = Color.magenta;
				#endif
				x = 0f;
				num = 3f;
			}
			if (i == 2)
			{
				#if UNITY_EDITOR
				color = Color.yellow;
				#endif
				x = 1.5f;
				num = 6f;
			}
			Vector3 rayOrigin = hitFront.TransformPoint((Mathf.Repeat(i, 3f) - 1f) * 10f, 0f, 0f);
			Vector3 rayDirection = hitFront.TransformDirection(x, 0f, num);
			#if UNITY_EDITOR
			Debug.DrawRay(rayOrigin, rayDirection, color);
			#endif
			if (!Physics.Raycast(rayOrigin, rayDirection, out carHit, num) || !carHit.transform.CompareTag("Car"))
			{
				continue;
			}
			CarController component = carHit.transform.GetComponent<CarController>();
			switch (i)
			{
			case 0:
				rotateSpeed = 3f;
				steerEvent = Mathf.SmoothStep(steerEvent, 60f, Time.deltaTime * 8f);
				break;
			case 1:
				if (carHit.transform.CompareTag("Player") || component.carMode == CarMode.Empty || component.carMode == CarMode.Player)
				{
					flag = true;
				}
				brake = true;
				break;
			case 2:
				rotateSpeed = 3f;
				steerEvent = Mathf.SmoothStep(steerEvent, -60f, Time.deltaTime * 8f);
				break;
			}
		}
		if (flag)
		{
			hurnTime += Time.deltaTime;
		}
		AIWayPoints();
		if (next_point == null)
		{
			brake = true;
			return;
		}
		Vector3 vector = base.transform.InverseTransformPoint(next_point.position);
		TrafficWaypoint nextWaypoint = next_point.GetComponent<TrafficWaypoint>();
		if (nextWaypoint != null && nextWaypoint.onlyOneWay)
		{
			sideLength = 0f;
		}
		else
		{
			sideLength = 4f;
		}
		targetAngle = Mathf.Atan2(vector.x + sideLength, vector.z);
		targetAngle *= 57.29578f;
		targetAngle = Mathf.Clamp(targetAngle + steerEvent, -65f, 65f);
		steer = Mathf.SmoothStep(steer, targetAngle / 60f, steerSpeed / 3f);
	}

	private void AIWayPoints()
	{
		if ((bool)next_point)
		{
			Crossing nextCrossing = next_point.GetComponent<Crossing>();
			if ((bool)nextCrossing)
			{
				if (Vector3.Distance(hitFront.position, next_point.position) < nextCrossing.stopDistance && next_point != currentPoint)
				{
					currentPoint = next_point;
				}
			}
			else if (Vector3.Distance(hitFront.position, next_point.position) < nextPointDistance && next_point != currentPoint)
			{
				currentPoint = next_point;
			}
		}
		if (!(currentPoint != null))
		{
			return;
		}
		Crossing currentCrossing = currentPoint.GetComponent<Crossing>();
		if ((bool)currentCrossing)
		{
			if (!waysActive)
			{
				next_point = RandomWay(next_point, currentCrossing.ways);
				waysActive = true;
			}
			return;
		}
		TrafficWaypoint component2 = currentPoint.GetComponent<TrafficWaypoint>();
		if (((component2.current == "next" && myStatue == "before") || (component2.current == "before" && myStatue == "next")) && component2.stop)
		{
			brake = true;
			pointspeed = 0f;
		}
		else
		{
			pointspeed = 10f;
		}
		if (wayMove == WayMove.Right)
		{
			wayPoint2 = currentPoint;
			next_point = component2.way_next;
		}
		else if (wayMove == WayMove.Left)
		{
			wayPoint2 = currentPoint;
			next_point = component2.way_before;
		}
		waysActive = false;
	}

	private Transform RandomWay(Transform wayPoint, int ways)
	{
		Crossing component = currentPoint.GetComponent<Crossing>();
		if (!wayActive)
		{
			if (ways == 1)
			{
				waysRandom = 1;
				wayActive = true;
			}
			else
			{
				Vector3 lhs = base.transform.TransformDirection(Vector3.left);
				for (int i = 0; i < 35; i++)
				{
					if (wayActive)
					{
						continue;
					}
					if (i > 15 && Debug.isDebugBuild)
					{
						MonoBehaviour.print("La decision en los cruces ha tomado mas de 15 iteraciones, si esto ocurre con frecuencia hay que arreglarlo" + i);
					}
					waysRandom = UnityEngine.Random.Range(1, ways + 1);
					if (waysRandom == 1 && component.way1 != wayPoint2)
					{
						Vector3 rhs = base.transform.position - component.way1.position;
						if (Vector3.Dot(lhs, rhs) > -10f)
						{
							waysRandom = 1;
							wayActive = true;
							break;
						}
					}
					else if (waysRandom == 2 && component.way2 != wayPoint2)
					{
						Vector3 rhs2 = base.transform.position - component.way2.position;
						if (Vector3.Dot(lhs, rhs2) > -10f)
						{
							waysRandom = 2;
							wayActive = true;
							break;
						}
					}
					else if (waysRandom == 3 && component.way3 != wayPoint2)
					{
						Vector3 rhs3 = base.transform.position - component.way3.position;
						if (Vector3.Dot(lhs, rhs3) > -10f)
						{
							waysRandom = 3;
							wayActive = true;
							break;
						}
					}
					else if (waysRandom == 4 && component.way4 != wayPoint2)
					{
						Vector3 rhs4 = base.transform.position - component.way4.position;
						if (Vector3.Dot(lhs, rhs4) > -10f)
						{
							waysRandom = 4;
							wayActive = true;
							break;
						}
					}
				}
			}
		}
		switch (waysRandom)
		{
		case 1:
			if (int.Parse(component.way1.name) > 1)
			{
				wayMove = WayMove.Left;
			}
			else
			{
				wayMove = WayMove.Right;
			}
			wayPoint = component.way1;
			myStatue = component.way1.GetComponent<TrafficWaypoint>().current;
			wayActive = false;
			break;
		case 2:
			if (int.Parse(component.way2.name) > 1)
			{
				wayMove = WayMove.Left;
			}
			else
			{
				wayMove = WayMove.Right;
			}
			wayPoint = component.way2;
			myStatue = component.way2.GetComponent<TrafficWaypoint>().current;
			wayActive = false;
			break;
		case 3:
			if (int.Parse(component.way3.name) > 1)
			{
				wayMove = WayMove.Left;
			}
			else
			{
				wayMove = WayMove.Right;
			}
			wayPoint = component.way3;
			myStatue = component.way3.GetComponent<TrafficWaypoint>().current;
			wayActive = false;
			break;
		case 4:
			if (int.Parse(component.way4.name) > 1)
			{
				wayMove = WayMove.Left;
			}
			else
			{
				wayMove = WayMove.Right;
			}
			wayPoint = component.way4;
			myStatue = component.way4.GetComponent<TrafficWaypoint>().current;
			wayActive = false;
			break;
		}
		return wayPoint;
	}

	public void ShiftUp()
	{
		float timeSinceLevelLoad = Time.timeSinceLevelLoad;
		if (!(timeSinceLevelLoad < shiftDelay) && currentGear < gears.Length - 1)
		{
			currentGear++;
			shiftDelay = timeSinceLevelLoad + 1f;
		}
	}

	public void ShiftDown()
	{
		float timeSinceLevelLoad = Time.timeSinceLevelLoad;
		if (!(timeSinceLevelLoad < shiftDelay) && currentGear > 0)
		{
			currentGear--;
			shiftDelay = timeSinceLevelLoad + 0.1f;
		}
	}

	private void FixedUpdate()
	{
		float fixedDeltaTime = Time.fixedDeltaTime;
		m_rigidbody.centerOfMass = shiftCentre;
		if (base.gameObject.activeSelf && (checkForActive == null || checkForActive.activeSelf))
		{
			if (carMode == CarMode.Player)
			{
				steer = Input.GetAxis("Horizontal");
				accel = Input.GetAxis("Vertical");
				brake = Input.GetButton("Jump");
			}
			else if (carMode == CarMode.AICar)
			{
				if (!brake)
				{
					accel = pointspeed / rotateSpeed / 250f;
				}
				else
				{
					accel = 0f;
				}
			}
			else if (carMode == CarMode.Empty)
			{
				accel = 0f;
			}
		}
		if (automatic && currentGear == 1 && accel < 0f)
		{
			if (speed < 5f)
			{
				ShiftDown();
			}
		}
		else if (automatic && currentGear == 0 && accel > 0f)
		{
			if (speed < 5f)
			{
				ShiftUp();
			}
		}
		else if (automatic && motorRPM > shiftUpRPM && accel > 0f && !brake)
		{
			ShiftUp();
		}
		else if (automatic && motorRPM < shiftDownRPM && currentGear > 1)
		{
			ShiftDown();
		}
		if (currentGear == 0)
		{
			shiftCentre.z = (0f - accel) / 3f;
			if (speed < gears[0] * -10f)
			{
				accel = 0f - accel;
			}
		}
		else
		{
			shiftCentre.z = (0f - accel / (float)currentGear) / 2f;
		}
		shiftCentre.x = 0f - Mathf.Clamp(steer * (speed / 100f), -0.1f, 0.1f);
		wantedRPM = 5500f * accel * 0.1f + wantedRPM * 0.9f;
		float num = 0f;
		int num2 = 0;
		bool flag = false;
		int num3 = 0;
		WheelData[] array = wheels;
		foreach (WheelData wheelData in array)
		{
			WheelCollider col = wheelData.col;
			if (wheelData.motor)
			{
				if (brake && currentGear < 2)
				{
					num += accel * idleRPM;
					if (num > 1f)
					{
						shiftCentre.z = Mathf.PingPong(Time.time * (accel * 10f), 1f) - 0.5f;
					}
					else
					{
						shiftCentre.z = 0f;
					}
				}
				else
				{
					num += col.rpm;
				}
				num2++;
			}
			if (brake || accel < 0f)
			{
				if (accel < 0f || (brake && (wheelData == wheels[2] || wheelData == wheels[3])))
				{
					if (brake && accel > 0f)
					{
						brackF = Mathf.Lerp(brackF, 10f, accel * Time.deltaTime);
					}
					else
					{
						brackF = Mathf.Lerp(brackF, 5f, 0.2f * Time.deltaTime);
					}
					wantedRPM = 0f;
					col.brakeTorque = brakeTorque;
					wheelData.rotation = curRotation;
				}
			}
			else
			{
				float num6;
				if (accel == 0f)
				{
					float num4 = (col.brakeTorque = 200f);
					num6 = num4;
				}
				else
				{
					float num4 = (col.brakeTorque = 1f);
					num6 = num4;
				}
				col.brakeTorque = num6;
				brackF = ((!(speed > 1f)) ? (brackF = Mathf.Lerp(brackF, 0f, 0.01f)) : ((!(speed > 20f)) ? (brackF = Mathf.Lerp(brackF, 3f, 0.01f)) : (brackF = Mathf.Lerp(brackF, 1f, 0.01f))));
				curRotation = wheelData.rotation;
			}
			WheelFrictionCurve forwardFriction = col.forwardFriction;
			float frictionDivisor = Mathf.Max(0.1f, brackF);
			forwardFriction.asymptoteValue = 5000f;
			forwardFriction.extremumSlip = 2f;
			forwardFriction.asymptoteSlip = 20f;
			forwardFriction.stiffness = stiffness / frictionDivisor;
			col.forwardFriction = forwardFriction;
			forwardFriction = col.sidewaysFriction;
			forwardFriction.stiffness = stiffness / frictionDivisor;
			col.sidewaysFriction = forwardFriction;
			wheelData.rotation = Mathf.Repeat(wheelData.rotation + fixedDeltaTime * col.rpm * 360f / 60f, 360f);
			wheelData.transform.localRotation = Quaternion.Euler(wheelData.rotation, col.steerAngle, 0f);
			Vector3 localPosition = wheelData.transform.localPosition;
			WheelHit hit;
			if (col.GetGroundHit(out hit))
			{
				if (Particle[0] == null)
				{
					Particle[0] = new GameObject();
					Particle[0].name = "AudioBreak";
					Particle[0].transform.parent = base.transform;
					Particle[0].transform.localPosition = Vector3.zero;
					brakeAudioSource = Particle[0].AddComponent<AudioSource>();
					brakeAudioSource.clip = brakeSound;
				}
				if ((brake || brackF > 3f || accel < 0f) && speed > 10f)
				{
					if (accel < 0f || ((brake || brackF > 3f) && (wheelData == wheels[2] || wheelData == wheels[3])))
					{
						if (!brakeAudioSource.isPlaying)
						{
							brakeAudioSource.Play();
						}
						brakeAudioSource.volume = Mathf.Clamp(speed / 25f, 0f, 1f);
					}
				}
				else
				{
					brakeAudioSource.volume = Mathf.Lerp(brakeAudioSource.volume, 0f, 0.1f);
				}
				localPosition.y -= Vector3.Dot(wheelData.transform.position - hit.point, Vector3.up / base.transform.lossyScale.x) - col.radius;
				localPosition.y = Mathf.Clamp(localPosition.y, -10f, wheelData.curY);
				flag = flag || wheelData.motor;
			}
			else
			{
				localPosition.y = wheelData.startPos.y - suspensionDistance;
			}
			num3++;
			wheelData.transform.localPosition = localPosition;
		}
		if (num2 > 1)
		{
			num /= (float)num2;
		}
		motorRPM = 0.95f * motorRPM + 0.05f * Mathf.Abs(num * gears[currentGear]);
		if (motorRPM > 5500f)
		{
			motorRPM = 5200f;
		}
		int num8 = (int)(motorRPM / efficiencyTableStep);
		if (num8 >= efficiencyTable.Length)
		{
			num8 = efficiencyTable.Length - 1;
		}
		if (num8 < 0)
		{
			num8 = 0;
		}
		float num9 = torque * gears[currentGear] * efficiencyTable[num8];
		WheelData[] array2 = wheels;
		foreach (WheelData wheelData2 in array2)
		{
			WheelCollider col2 = wheelData2.col;
			if (wheelData2.motor)
			{
				if (Mathf.Abs(col2.rpm) > Mathf.Abs(wantedRPM))
				{
					col2.motorTorque = 0f;
				}
				else
				{
					float motorTorque = col2.motorTorque;
					if (!brake)
					{
						col2.motorTorque = motorTorque * 0.9f + num9 * 0.1f;
					}
				}
			}
			if (carMode == CarMode.Player)
			{
				if (brake)
				{
					col2.steerAngle = Mathf.Lerp(col2.steerAngle, steer * wheelData2.maxSteer, 0.02f);
					continue;
				}
				float num10 = Mathf.Clamp(speed / base.transform.lossyScale.x / maxSteerAngle, 1f, maxSteerAngle);
				col2.steerAngle = steer * (wheelData2.maxSteer / num10);
			}
			else if (carMode == CarMode.AICar)
			{
				col2.steerAngle = steer * wheelData2.maxSteer;
			}
			else if (carMode == CarMode.Empty)
			{
				col2.steerAngle = 0f;
			}
		}
		if (audioSource != null)
		{
			float pitch = Mathf.Clamp(1f + (motorRPM - idleRPM) / (shiftUpRPM - idleRPM) * 0.5f, 1f, 10f);
			audioSource.pitch = pitch;
			audioSource.volume = Mathf.MoveTowards(audioSource.volume, 1f, 0.02f);
		}
	}

	private void OnDrawGizmos()
	{
		Matrix4x4 matrix = Matrix4x4.TRS(base.transform.position, base.transform.rotation, base.transform.lossyScale);
		Gizmos.matrix = matrix;
		Gizmos.color = new Color(1f, 0f, 0f, 0.5f);
		Color color = Gizmos.color;
		Gizmos.color = Color.red;
		if ((bool)currentPoint)
		{
			Gizmos.DrawLine(base.transform.position, currentPoint.position);
		}
		Gizmos.color = new Color(0.5f, 0.5f, 0.5f, 0.5f);
		Gizmos.color = color;
	}
}
