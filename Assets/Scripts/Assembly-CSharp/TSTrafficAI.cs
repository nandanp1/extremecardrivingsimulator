using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(TSNavigation))]
public class TSTrafficAI : MonoBehaviour
{
	public struct TSBrakeSpeeds
	{
		public float distance;

		public TSPoints point;
	}

	public delegate void OnUpdateAIDelegate(float steering, float brake, float throttle, bool isUpSideDown);

	public delegate void GetCarSpeedDelegate(out float carSpeed);

	public delegate void OnTurnRigthDelegate(bool isTurning);

	public delegate void OnTurnLeftDelegate(bool isTurning);

	public delegate void OnCloserRangeDelegate();

	public delegate void OnFarRangeDelegate();

	public Color myColor;

	private TSNavigation _navigateToWaypoints;

	[HideInInspector]
	public float maxLockaheadDistance;

	[HideInInspector]
	public float maxLockaheadDistanceFullStop;

	public float LOOKAHEAD_FACTOR = 0.33f;

	public Transform[] frontWheels;

	public float LOOKAHEAD_CONST = 2f;

	public Rigidbody[] bodies;

	[HideInInspector]
	public float carDepth;

	[HideInInspector]
	public float carWidth;

	[HideInInspector]
	public float carHeight;

	public TSLaneInfo.VehicleType myVehicleType = TSLaneInfo.VehicleType.Light;

	[HideInInspector]
	public List<TSBrakeSpeeds> brakeSpeeds = new List<TSBrakeSpeeds>(100);

	[HideInInspector]
	public TSPoints currenPoint = new TSPoints();

	[HideInInspector]
	public TSLaneConnector currentConnector;

	[HideInInspector]
	public TSLaneConnector nextConnectorInstance;

	[HideInInspector]
	public int previousWaypointCurve;

	[HideInInspector]
	public float segDistance;

	[HideInInspector]
	public int nextPathIndex;

	[HideInInspector]
	public int lane;

	[HideInInspector]
	public float changeLaneTime;

	public BoxCollider playerSensor;

	public float playerSensorWidthMultiplier = 1.05f;

	public float minPlayerSensorLenght = 5f;

	public float playerSensorLengthMultiplier = 1f;

	[HideInInspector]
	public float lengthMargin = 2f;

	public float lengthMarginMin = 1.5f;

	public float lengthMarginMax = 3f;

	public float myLaneOffsetMin;

	public float myLaneOffsetMax;

	public string playerTag = "Player";

	public AudioSource playerDetectedSoundAudioSource;

	public AudioClip[] playerDetectedSoundClips = new AudioClip[0];

	public float minplayerDetectedSoundTime = 0.2f;

	public float maxplayerDetectedSoundTime = 0.5f;

	public Animator playerDetectedAnimator;

	public string[] playerDetectedStates;

	public float minplayerDetectedAnimatorTime = 0.5f;

	public float maxplayerDetectedAnimatorTime = 0.5f;

	public Animation playerDetectedAnimationController;

	public string[] playerDetectedAnimations;

	public float minplayerDetectedAnimationTime = 0.5f;

	public float maxplayerDetectedAnimationTime = 0.5f;

	private float _lookAheadDistance;

	private List<Transform> players = new List<Transform>();

	private float _carSpeed;

	private float MAXSPEED = float.MaxValue;

	private Transform _wheelsCenter;

	private Transform myTransform;

	private float[] kFriction;

	public float inputTorque;

	private float currentSpeedSqr;

	private float c;

	private float brake;

	public float throttle;

	private float steering;

	private bool fullStop;

	private float timerForChangeLane = 5f;

	private bool isUpSideDown;

	private bool forcedRespawn;

	private float upSideDownTimer;

	private float halfCarWidth;

	private float halfCarDepth;

	private float myLaneOffset;

	private float playerDetectedSoundTimming;

	private float playerDetectedSoundNext;

	private float playerDetectedAnimatorTimming;

	private float playerDetectedAnimatorNext;

	private float playerDetectedAnimationTimming;

	private float playerDetectedAnimationNext;

	private bool canPlayPlayerDetectedAudio;

	private bool canPlayPlayerDetectedAnimator;

	private bool canPlayPlayerDetectedAnimation;

	public OnFarRangeDelegate OnFarRange;

	public OnCloserRangeDelegate OnCloserRange;

	public OnTurnRigthDelegate OnTurnRight;

	public OnTurnLeftDelegate OnTurnLeft;

	public GetCarSpeedDelegate UpdateCarSpeed;

	public OnUpdateAIDelegate OnUpdateAI;

	public bool physicalLocomotion;

	private float accelResult;

	private float distanceToRun;

	private float nextConnectorDistance;

	private float nextCarSpeedSqr;

	private float returningValue;

	private float sqrMaxLockaheadDistance;

	private bool reservedConnector;

	private Transform shape;

	public TSNavigation NavigateToWaypoints
	{
		get
		{
			return _navigateToWaypoints;
		}
	}

	public float LookAheadDistance
	{
		get
		{
			return _lookAheadDistance;
		}
	}

	public Transform WheelsCenter
	{
		get
		{
			return _wheelsCenter;
		}
	}

	public float carSpeed
	{
		get
		{
			return _carSpeed;
		}
	}

	public bool IsUpSideDown
	{
		get
		{
			return isUpSideDown;
		}
		set
		{
			isUpSideDown = value;
		}
	}

	public bool ForcedRespawn
	{
		get
		{
			return forcedRespawn;
		}
		set
		{
			forcedRespawn = value;
		}
	}

	public int GetCurrentWaypoint()
	{
		return _navigateToWaypoints.currentWaypoint;
	}

	public string GetCarName()
	{
		return myTransform.name.Replace("(Clone)", string.Empty);
	}

	public float GetLookahead_factor()
	{
		return LOOKAHEAD_FACTOR;
	}

	public void SetLookahead_factor(float Lookahead_factor)
	{
		LOOKAHEAD_FACTOR = Lookahead_factor;
	}

	private void Awake()
	{
		myColor = new Color(UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f), UnityEngine.Random.Range(0f, 1f));
		myTransform = base.transform;
		_navigateToWaypoints = base.gameObject.GetComponent<TSNavigation>();
		IRDSWheelsCenter();
		bodies = GetComponentsInChildren<Rigidbody>();
		myLaneOffset = UnityEngine.Random.Range(myLaneOffsetMin, myLaneOffsetMax);
		canPlayPlayerDetectedAudio = playerDetectedSoundAudioSource != null && (playerDetectedSoundClips.Length > 0 || playerDetectedSoundAudioSource.clip != null);
		canPlayPlayerDetectedAnimator = playerDetectedAnimator != null && playerDetectedStates.Length > 0;
		canPlayPlayerDetectedAnimation = playerDetectedAnimationController != null && playerDetectedAnimations.Length > 0;
		lengthMargin = UnityEngine.Random.Range(lengthMarginMin, lengthMarginMax);
	}

	private void Start()
	{
		if (carWidth == 0f)
		{
			Bounds bounds = CarSize();
			carWidth = bounds.size.x;
			carDepth = bounds.size.z;
			carHeight = bounds.size.y;
		}
		if ((bool)playerSensor)
		{
			playerSensor.isTrigger = true;
			Vector3 size = new Vector3(carWidth * playerSensorWidthMultiplier, carHeight, 80f);
			playerSensor.size = size;
			playerSensor.center = new Vector3(0f, carHeight / 2f, 40f);
		}
		halfCarWidth = carWidth / 2f;
		halfCarDepth = carDepth / 2f;
		kFriction = new float[2];
		kFriction[0] = 1f;
		kFriction[1] = 0.4f;
		c = kFriction[1] * (0f - Physics.gravity.y);
	}

	private void Update()
	{
		if (!physicalLocomotion)
		{
			if (UpdateCarSpeed != null)
			{
				UpdateCarSpeed(out _carSpeed);
			}
			GeneralCalculations();
			controllerAI();
		}
	}

	private void FixedUpdate()
	{
		if (physicalLocomotion)
		{
			if (UpdateCarSpeed != null)
			{
				UpdateCarSpeed(out _carSpeed);
			}
			GeneralCalculations();
			controllerAI();
		}
	}

	private void controllerAI()
	{
		steering = GetSteer(_navigateToWaypoints.waypoints[_navigateToWaypoints.currentWaypoint].point, _navigateToWaypoints.waypoints[_navigateToWaypoints.previousWaypointSteer].point, true);
		float num = GetBrake();
		if (num == 1f)
		{
			num++;
		}
		if (num > brake)
		{
			brake += Time.deltaTime / 1.2f;
		}
		else
		{
			brake -= Time.deltaTime / 0.6f;
		}
		brake = Mathf.Clamp01(num);
		inputTorque = Mathf.Clamp01(GetAccel());
		if (inputTorque == 1f)
		{
			inputTorque++;
		}
		if (inputTorque > throttle)
		{
			throttle += Time.deltaTime / 1.6f;
		}
		else
		{
			throttle -= Time.deltaTime / 0.6f;
		}
		throttle = Mathf.Clamp01(throttle);
		CheckUpsideDown();
		if (OnUpdateAI != null)
		{
			OnUpdateAI(steering, brake, throttle, isUpSideDown);
		}
	}

	private float GetAccel()
	{
		if (carSpeed < MAXSPEED - 2f)
		{
			accelResult = 1f;
		}
		else
		{
			accelResult = 2f - carSpeed / MAXSPEED;
		}
		if (brake != 0f)
		{
			accelResult = 0f;
		}
		return accelResult;
	}

	private void GetMaxLockAheadDistance()
	{
		if (nextConnectorInstance != null && nextConnectorInstance.remainingGreenLightTime != -1f)
		{
			distanceToRun = carSpeed * nextConnectorInstance.remainingGreenLightTime;
			float num = (nextConnectorInstance.points[0].point - myTransform.position).magnitude + nextConnectorInstance.totalDistance;
			if (distanceToRun < num)
			{
				nextCarSpeedSqr = 0f;
			}
		}
		maxLockaheadDistance = (currentSpeedSqr - nextCarSpeedSqr) / (2f * c);
		maxLockaheadDistanceFullStop = currentSpeedSqr / (2f * c);
		if (maxLockaheadDistance < 0f)
		{
			maxLockaheadDistance = 0f;
		}
		if (maxLockaheadDistanceFullStop < 0f)
		{
			maxLockaheadDistanceFullStop = 0f;
		}
		maxLockaheadDistance += lengthMargin;
		maxLockaheadDistanceFullStop += halfCarDepth + lengthMargin;
	}

	private void GetMaxSpeed()
	{
		if (_navigateToWaypoints.nextTrackPath.Count > 0 && nextPathIndex != -1)
		{
			MAXSPEED = _navigateToWaypoints.lanes[_navigateToWaypoints.nextTrackPath[nextPathIndex].nextLane].maxSpeed / 3.6f;
		}
		else
		{
			MAXSPEED = _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].maxSpeed / 3.6f;
		}
		if (MAXSPEED * 3.6f > _navigateToWaypoints.currentMaxSpeed)
		{
			MAXSPEED = _navigateToWaypoints.currentMaxSpeed / 3.6f;
		}
	}

	private void GetBrakeForCorners()
	{
		if (brakeSpeeds.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < brakeSpeeds.Count; i++)
		{
			TSBrakeSpeeds value = default(TSBrakeSpeeds);
			value.point = brakeSpeeds[i].point;
			value.distance = (myTransform.position - value.point.point).sqrMagnitude;
			brakeSpeeds[i] = value;
			if (brakeSpeeds[i].distance < sqrMaxLockaheadDistance && carSpeed > brakeSpeeds[i].point.maxSpeedLimit / 3.6f)
			{
				returningValue += (carSpeed - brakeSpeeds[i].point.maxSpeedLimit / 3.6f) / 5f;
			}
			if (brakeSpeeds[i].point.reservationID != _navigateToWaypoints.myID)
			{
				brakeSpeeds.RemoveAt(i);
			}
		}
	}

	private float GetBrake()
	{
		returningValue = 0f;
		GetMaxLockAheadDistance();
		sqrMaxLockaheadDistance = maxLockaheadDistanceFullStop * maxLockaheadDistanceFullStop;
		GetMaxSpeed();
		float num = Mathf.Max(minPlayerSensorLenght, maxLockaheadDistanceFullStop * playerSensorLengthMultiplier);
		playerSensor.size = new Vector3(playerSensor.size.x, playerSensor.size.y, num);
		playerSensor.center = new Vector3(0f, playerSensor.center.y, num / 2f);
		segDistance = ((!(segDistance < 0f)) ? segDistance : 0f);
		GetBrakeForCorners();
		bool stillMine = true;
		bool flag = true;
		if (fullStop)
		{
			float nearestPlayerDistance = GetNearestPlayerDistance();
			if (nearestPlayerDistance < maxLockaheadDistanceFullStop * playerSensorLengthMultiplier + 2f)
			{
				returningValue += 1f - (nearestPlayerDistance - maxLockaheadDistanceFullStop * playerSensorLengthMultiplier) / 2f;
			}
		}
		float num2 = ((!(5f > maxLockaheadDistance)) ? maxLockaheadDistance : 5f) * playerSensorLengthMultiplier;
		while (((segDistance <= num2 || _navigateToWaypoints.reservedPoints.Count <= 1) && stillMine) || (flag && currenPoint.reservationID == 0))
		{
			flag = false;
			if (_navigateToWaypoints.overTaking && Time.time - changeLaneTime > 1f)
			{
				changeLaneTime = Time.time;
				_navigateToWaypoints.LaneChange(false);
			}
			if (_navigateToWaypoints.nextTrackPath.Count != 0)
			{
				if (!GetCurrentPoint())
				{
					return returningValue;
				}
			}
			else
			{
				if (previousWaypointCurve < 0 || previousWaypointCurve >= _navigateToWaypoints.lastWaypoints.Length)
				{
					return returningValue;
				}
				currenPoint = _navigateToWaypoints.lastWaypoints[previousWaypointCurve];
			}
			if (MAXSPEED == 0f)
			{
				returningValue++;
			}
			if (carSpeed >= ((!_navigateToWaypoints.overTaking) ? MAXSPEED : (MAXSPEED * 1.2f)))
			{
				returningValue += (carSpeed - MAXSPEED) / 5f;
			}
			if (!TryToReservePoint(ref stillMine))
			{
				break;
			}
		}
		CheckIfNeedsToBrake();
		return Mathf.Clamp01(returningValue);
	}

	private void CheckIfNeedsToBrake()
	{
		if (currenPoint.reservationID == _navigateToWaypoints.myID && !fullStop)
		{
			return;
		}
		float num = myTransform.InverseTransformPoint(currenPoint.point).z - (halfCarDepth - 2f);
		if (!fullStop && currenPoint.carwhoReserved != null && (currentConnector == null || (currentConnector != null && !currentConnector.connectorReservedByTrafficLight)))
		{
			nextCarSpeedSqr = currenPoint.carwhoReserved.currentSpeedSqr;
			if (_navigateToWaypoints.overTaking && Time.time - changeLaneTime > 1f)
			{
				changeLaneTime = Time.time;
				_navigateToWaypoints.LaneChange(false);
				returningValue = 1f;
				return;
			}
			if (currenPoint.carwhoReserved._navigateToWaypoints.overTaking)
			{
				returningValue = 1f;
				return;
			}
			float num2 = (currenPoint.carwhoReserved.myTransform.position - myTransform.position).magnitude - halfCarDepth - currenPoint.carwhoReserved.carDepth / 2f;
			if (num2 < num)
			{
				num = num2;
			}
		}
		else
		{
			nextCarSpeedSqr = 0f;
		}
		if (!(num < maxLockaheadDistance + lengthMargin) && !fullStop)
		{
			return;
		}
		if ((currenPoint.reservationID != 0 || fullStop) && (fullStop || (num > halfCarDepth && !_navigateToWaypoints.travelingOnConector && Time.time - changeLaneTime > timerForChangeLane)))
		{
			changeLaneTime = Time.time;
			try
			{
				if (_navigateToWaypoints.currentWaypoint >= _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].points.Length)
				{
					_navigateToWaypoints.currentWaypoint = _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].points.Length - 1;
					Debug.Log("Duck!!");
				}
				if (_navigateToWaypoints.currentWaypoint < _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].points.Length && _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].points[_navigateToWaypoints.currentWaypoint].leftParalelLaneIndex != -1)
				{
					int laneLinkLeft = _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].laneLinkLeft;
					if (_navigateToWaypoints.lanes[laneLinkLeft].laneLinkRight == _navigateToWaypoints.currentLane)
					{
						_navigateToWaypoints.LaneChange(true);
					}
					else if (!_navigateToWaypoints.overTaking && _navigateToWaypoints.lanes[laneLinkLeft].laneLinkLeft == _navigateToWaypoints.currentLane && carSpeed < MAXSPEED && (currenPoint.carwhoReserved == null || !currenPoint.carwhoReserved._navigateToWaypoints.overTaking))
					{
						_navigateToWaypoints.OverTaking(true);
					}
				}
			}
			catch (IndexOutOfRangeException)
			{
				throw new IndexOutOfRangeException();
			}
			if (_navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].points[_navigateToWaypoints.currentWaypoint].rightParalelLaneIndex != -1)
			{
				int laneLinkRight = _navigateToWaypoints.lanes[_navigateToWaypoints.currentLane].laneLinkRight;
				if (_navigateToWaypoints.lanes[laneLinkRight].laneLinkLeft == _navigateToWaypoints.currentLane)
				{
					_navigateToWaypoints.LaneChange(false);
				}
				else if (!_navigateToWaypoints.overTaking && _navigateToWaypoints.lanes[laneLinkRight].laneLinkRight == _navigateToWaypoints.currentLane && carSpeed < MAXSPEED && (currenPoint.carwhoReserved == null || !currenPoint.carwhoReserved._navigateToWaypoints.overTaking))
				{
					_navigateToWaypoints.OverTaking(false);
				}
			}
		}
		if (currenPoint.reservationID != _navigateToWaypoints.myID)
		{
			returningValue += Mathf.Max(1f - (((!(num < 0f)) ? num : 0f) - maxLockaheadDistance) / lengthMargin, returningValue);
		}
	}

	private float GetNearestPlayerDistance()
	{
		float num = float.MaxValue;
		int num2 = -1;
		for (int i = 0; i < players.Count; i++)
		{
			if (players[i] == null)
			{
				players.Remove(players[i]);
				break;
			}
			float sqrMagnitude = (_wheelsCenter.position - players[i].position).sqrMagnitude;
			if (sqrMagnitude < num)
			{
				num = sqrMagnitude;
				num2 = i;
			}
		}
		if (num2 != -1)
		{
			RaycastHit hitInfo;
			if (Physics.CapsuleCast(_wheelsCenter.position, _wheelsCenter.position + _wheelsCenter.forward, (!(halfCarWidth > 0.7f)) ? 0.7f : halfCarWidth, (currenPoint.point - new Vector3(_wheelsCenter.position.x, currenPoint.point.y, _wheelsCenter.position.z)).normalized, out hitInfo, (currenPoint.point - _wheelsCenter.position).magnitude, 1 << players[num2].gameObject.layer))
			{
				if (hitInfo.transform.root == players[num2].root)
				{
					float num3 = Mathf.Clamp01(Vector3.Dot(myTransform.forward, players[num2].forward));
					if (num3 < 0.5f)
					{
						PlayPlayerDetectedAudio();
						PlayPlayerDetectedAnimator();
						PlayPlayerDetectedAnimation();
					}
					return hitInfo.distance * num3;
				}
				return float.MaxValue;
			}
			if (num > (carDepth / 2f + lengthMargin) * (carDepth / 2f + lengthMargin))
			{
				return float.MaxValue;
			}
			return 0f;
		}
		return float.MaxValue;
	}

	private void PlayPlayerDetectedAudio()
	{
		if (canPlayPlayerDetectedAudio && Time.time - playerDetectedSoundTimming > playerDetectedSoundNext)
		{
			playerDetectedSoundNext = UnityEngine.Random.Range(minplayerDetectedSoundTime, maxplayerDetectedSoundTime);
			playerDetectedSoundTimming = Time.time;
			if (playerDetectedSoundClips.Length > 0)
			{
				playerDetectedSoundAudioSource.PlayOneShot(playerDetectedSoundClips[UnityEngine.Random.Range(0, playerDetectedSoundClips.Length)]);
			}
			else if (playerDetectedSoundAudioSource.clip != null)
			{
				playerDetectedSoundAudioSource.PlayOneShot(playerDetectedSoundAudioSource.clip);
			}
		}
	}

	private void PlayPlayerDetectedAnimator()
	{
		if (canPlayPlayerDetectedAnimator && Time.time - playerDetectedAnimatorTimming > playerDetectedAnimatorNext)
		{
			playerDetectedAnimatorNext = UnityEngine.Random.Range(minplayerDetectedAnimatorTime, maxplayerDetectedAnimatorTime);
			playerDetectedAnimatorTimming = Time.time;
			playerDetectedAnimator.Play(playerDetectedStates[UnityEngine.Random.Range(0, playerDetectedStates.Length)]);
		}
	}

	private void PlayPlayerDetectedAnimation()
	{
		if (canPlayPlayerDetectedAnimation && Time.time - playerDetectedAnimationTimming > playerDetectedAnimationNext)
		{
			playerDetectedAnimationNext = UnityEngine.Random.Range(minplayerDetectedAnimationTime, maxplayerDetectedAnimationTime);
			playerDetectedAnimationTimming = Time.time;
			playerDetectedAnimationController.Play(playerDetectedAnimations[UnityEngine.Random.Range(0, playerDetectedAnimations.Length)]);
		}
	}

	private void RenderVolume(Vector3 p1, Vector3 p2, float radius, Vector3 dir, float distance)
	{
		if (!shape)
		{
			shape = GameObject.CreatePrimitive(PrimitiveType.Cube).transform;
			shape.GetComponent<BoxCollider>().isTrigger = true;
			UnityEngine.Object.Destroy(shape.GetComponent<BoxCollider>());
		}
		Vector3 localScale = default(Vector3);
		float num = (localScale.x = 2f * radius);
		localScale.y = Vector3.Distance(p2, p1) + num;
		localScale.z = distance + num;
		shape.localScale = localScale;
		shape.position = (p1 + p2 + dir.normalized * distance) / 2f;
		shape.rotation = Quaternion.LookRotation(dir, p2 - p1);
		shape.GetComponent<Renderer>().enabled = true;
	}

	private void HideVolume()
	{
		if ((bool)shape)
		{
			shape.GetComponent<Renderer>().enabled = false;
		}
	}

	private bool TryToReservePoint(ref bool stillMine)
	{
		TSNavigation.TSReservedPoints item = new TSNavigation.TSReservedPoints
		{
			connector = null,
			point = currenPoint
		};
		if ((currenPoint.reservationID == 0 || currenPoint.reservationID == _navigateToWaypoints.myID) && stillMine)
		{
			currenPoint.reservationID = _navigateToWaypoints.myID;
			currenPoint.carwhoReserved = this;
			if (currentConnector != null)
			{
				bool flag = true;
				if (currentConnector.remainingGreenLightTime != -1f)
				{
					float num = Mathf.Max(10f, carSpeed) * currentConnector.remainingGreenLightTime;
					float num2 = (currentConnector.points[0].point - myTransform.position).magnitude + currentConnector.totalDistance;
					flag = num > num2;
				}
				if (!reservedConnector && flag)
				{
					reservedConnector = _navigateToWaypoints.ReserveNearConnectorPoints(ref currentConnector, lane, previousWaypointCurve);
				}
				if (reservedConnector && flag)
				{
					if (!_navigateToWaypoints.ReserveNearConnectorPoints(ref currenPoint, ref currentConnector))
					{
						currenPoint.reservationID = 0;
						currenPoint.carwhoReserved = null;
						stillMine = false;
					}
					else
					{
						item.connector = currentConnector;
					}
				}
				else
				{
					currenPoint.reservationID = 0;
					currenPoint.carwhoReserved = null;
					stillMine = false;
				}
			}
			if (currenPoint.reservationID != _navigateToWaypoints.myID)
			{
				stillMine = false;
				return false;
			}
			nextCarSpeedSqr = 0f;
			_navigateToWaypoints.reservedPoints.Enqueue(item);
			CheckPointSpeed();
			segDistance += currenPoint.distanceToNextPoint;
			if (_navigateToWaypoints.overTaking)
			{
				previousWaypointCurve--;
			}
			else
			{
				previousWaypointCurve++;
			}
			return true;
		}
		return stillMine = false;
	}

	private void CheckPointSpeed()
	{
		float num = currenPoint.maxSpeedLimit / 3.6f;
		if (num < MAXSPEED)
		{
			TSBrakeSpeeds item = default(TSBrakeSpeeds);
			item.point = currenPoint;
			item.distance = (myTransform.position - item.point.point).sqrMagnitude;
			brakeSpeeds.Add(item);
			if (item.distance < sqrMaxLockaheadDistance && item.point.maxSpeedLimit / 3.6f > carSpeed)
			{
				returningValue++;
			}
		}
	}

	private void CheckUpsideDown()
	{
		if (myTransform.localEulerAngles.z > 60f && myTransform.localEulerAngles.z < 310f)
		{
			upSideDownTimer += Time.deltaTime;
			isUpSideDown = true;
			if (upSideDownTimer > TSTrafficSpawner.RespawnUpSideDownTime)
			{
				forcedRespawn = true;
			}
		}
		else
		{
			forcedRespawn = (isUpSideDown = false);
		}
	}

	private bool GetCurrentPoint()
	{
		if (_navigateToWaypoints.nextTrackPath[nextPathIndex].isConnector)
		{
			bool flag = false;
			if (previousWaypointCurve >= _navigateToWaypoints.lanes[_navigateToWaypoints.nextTrackPath[nextPathIndex].nextLane].connectors[_navigateToWaypoints.nextTrackPath[nextPathIndex].nextConnector].points.Length)
			{
				if (nextPathIndex + 1 >= _navigateToWaypoints.nextTrackPath.Count)
				{
					return false;
				}
				flag = true;
				previousWaypointCurve = 0;
				nextPathIndex++;
			}
			if (!flag)
			{
				lane = _navigateToWaypoints.nextTrackPath[nextPathIndex].nextLane;
				currenPoint = _navigateToWaypoints.lanes[lane].connectors[_navigateToWaypoints.nextTrackPath[nextPathIndex].nextConnector].points[previousWaypointCurve];
				currentConnector = _navigateToWaypoints.lanes[lane].connectors[_navigateToWaypoints.nextTrackPath[nextPathIndex].nextConnector];
				nextConnectorInstance = null;
			}
			else
			{
				lane = _navigateToWaypoints.nextTrackPath[nextPathIndex].nextLane;
				currenPoint = _navigateToWaypoints.lanes[lane].points[previousWaypointCurve];
				currentConnector = null;
				try
				{
					if (_navigateToWaypoints.reservedConnectors.Count > 0)
					{
						_navigateToWaypoints.UnReserveNearConnectorPoints(_navigateToWaypoints.reservedConnectors.Peek());
					}
				}
				catch (InvalidOperationException)
				{
					if (base.gameObject.GetComponent<TSCommonProperties>() != null)
					{
						base.gameObject.GetComponent<TSCommonProperties>().crashed = true;
						Debug.Log("Car BreakDown - Peek");
					}
				}
				try
				{
					if (_navigateToWaypoints.reservedConnectors.Count > 0)
					{
						_navigateToWaypoints.reservedConnectors.Dequeue();
					}
				}
				catch (InvalidOperationException)
				{
					if (base.gameObject.GetComponent<TSCommonProperties>() != null)
					{
						base.gameObject.GetComponent<TSCommonProperties>().crashed = true;
						Debug.Log("Car BreakDown - Dequeue");
					}
				}
				if (nextPathIndex + 1 < _navigateToWaypoints.nextTrackPath.Count)
				{
					nextConnectorInstance = _navigateToWaypoints.lanes[_navigateToWaypoints.nextTrackPath[nextPathIndex + 1].nextLane].connectors[_navigateToWaypoints.nextTrackPath[nextPathIndex + 1].nextConnector];
				}
			}
		}
		else
		{
			bool flag2 = false;
			if (previousWaypointCurve >= _navigateToWaypoints.lanes[_navigateToWaypoints.nextTrackPath[nextPathIndex].nextLane].points.Length)
			{
				if (nextPathIndex + 1 >= _navigateToWaypoints.nextTrackPath.Count)
				{
					return false;
				}
				flag2 = true;
				previousWaypointCurve = 0;
				nextPathIndex++;
			}
			if (!flag2)
			{
				lane = _navigateToWaypoints.nextTrackPath[nextPathIndex].nextLane;
				currenPoint = _navigateToWaypoints.lanes[lane].points[previousWaypointCurve];
				currentConnector = null;
				if (nextPathIndex + 1 < _navigateToWaypoints.nextTrackPath.Count)
				{
					nextConnectorInstance = _navigateToWaypoints.lanes[_navigateToWaypoints.nextTrackPath[nextPathIndex + 1].nextLane].connectors[_navigateToWaypoints.nextTrackPath[nextPathIndex + 1].nextConnector];
				}
			}
			else
			{
				lane = _navigateToWaypoints.nextTrackPath[nextPathIndex].nextLane;
				currenPoint = _navigateToWaypoints.lanes[lane].connectors[_navigateToWaypoints.nextTrackPath[nextPathIndex].nextConnector].points[previousWaypointCurve];
				currentConnector = _navigateToWaypoints.lanes[lane].connectors[_navigateToWaypoints.nextTrackPath[nextPathIndex].nextConnector];
				reservedConnector = false;
				nextConnectorInstance = null;
			}
		}
		return true;
	}

	private float GetSteer(Vector3 targetPoint, Vector3 previousT, bool useOvertakes)
	{
		Vector2 targetPoint2 = GetTargetPoint(targetPoint, previousT, useOvertakes);
		float x = targetPoint2.x;
		return x / targetPoint2.magnitude;
	}

	private Vector2 GetTargetPoint(Vector3 Point, Vector3 prev, bool useOvertakes)
	{
		if (!float.IsNaN(Vector3.SqrMagnitude(prev)))
		{
			float num = Vector3.Distance(_navigateToWaypoints.waypoints[_navigateToWaypoints.currentWaypoint].point, _navigateToWaypoints.waypoints[_navigateToWaypoints.previousWaypointSteer].point);
			Point = Vector3.Lerp(prev, Point, Mathf.Abs((GetLookahead() - (_navigateToWaypoints.relativeWPosMagnitude - num)) / num));
		}
		Vector3 vector = _wheelsCenter.InverseTransformPoint(Point);
		float x = vector.x + myLaneOffset;
		float y = Mathf.Max(vector.z, 2f);
		return new Vector2(x, y);
	}

	public float GetLookahead()
	{
		_lookAheadDistance = LOOKAHEAD_CONST + carSpeed * LOOKAHEAD_FACTOR * Mathf.Clamp(carSpeed / 150f, 0.1f, 1f);
		return _lookAheadDistance;
	}

	private void OnTriggerEnter(Collider colInfo)
	{
		if (colInfo.tag == playerTag)
		{
			players.Add(colInfo.transform);
			fullStop = true;
		}
	}

	private void OnTriggerExit(Collider colInfo)
	{
		if (colInfo.tag == playerTag)
		{
			players.Remove(colInfo.transform);
			fullStop = false;
			HideVolume();
		}
	}

	private void GeneralCalculations()
	{
		currentSpeedSqr = carSpeed * carSpeed;
	}

	private void IRDSWheelsCenter()
	{
		Quaternion rotation = myTransform.rotation;
		Vector3 position = myTransform.position;
		myTransform.rotation = Quaternion.Euler(Vector3.zero);
		myTransform.position = Vector3.zero;
		Vector3 zero = Vector3.zero;
		zero = frontWheels[0].position;
		Bounds bounds = new Bounds(zero, Vector3.zero);
		_wheelsCenter = new GameObject().transform;
		_wheelsCenter.transform.position = Vector3.zero;
		_wheelsCenter.transform.eulerAngles = Vector3.zero;
		Transform[] array = frontWheels;
		foreach (Transform transform in array)
		{
			bounds.Encapsulate(transform.position);
		}
		_wheelsCenter.transform.position = new Vector3(bounds.center.x, 0f, bounds.center.z);
		_wheelsCenter.transform.parent = myTransform;
		_wheelsCenter.name = "FrontIRDSWheelsCenter";
		myTransform.rotation = rotation;
		myTransform.position = position;
	}

	private Bounds CarSize()
	{
		Quaternion rotation = myTransform.rotation;
		Vector3 position = myTransform.position;
		myTransform.rotation = Quaternion.Euler(Vector3.zero);
		myTransform.position = Vector3.zero;
		Bounds result = new Bounds(myTransform.position, Vector3.zero);
		Collider[] componentsInChildren = GetComponentsInChildren<Collider>();
		Collider[] array = componentsInChildren;
		foreach (Collider collider in array)
		{
			result.Encapsulate(collider.bounds);
		}
		myTransform.rotation = rotation;
		myTransform.position = position;
		return result;
	}

	public void Enable(bool flag)
	{
		if (flag)
		{
			base.gameObject.SetActive(true);
			for (int i = 0; i < bodies.Length; i++)
			{
				bodies[i].constraints = RigidbodyConstraints.None;
				bodies[i].linearVelocity = Vector3.zero;
			}
			return;
		}
		throttle = 0f;
		steering = 0f;
		brake = 0f;
		if (OnUpdateAI != null)
		{
			OnUpdateAI(0f, 0f, 0f, isUpSideDown);
		}
		for (int j = 0; j < bodies.Length; j++)
		{
			bodies[j].constraints = RigidbodyConstraints.FreezeAll;
		}
		_navigateToWaypoints.TurnOffTurningLights();
		base.gameObject.SetActive(false);
	}
}
