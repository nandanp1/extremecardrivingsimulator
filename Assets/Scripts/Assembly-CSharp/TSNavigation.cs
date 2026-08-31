using System.Collections.Generic;
using UnityEngine;

public class TSNavigation : MonoBehaviour
{
	public struct TSNextLaneSelection
	{
		public int nextLane;

		public int nextConnector;

		public bool isConnector;
	}

	public struct TSReservedPoints
	{
		public TSLaneConnector connector;

		public TSPoints point;

		public TSReservedPoints(TSLaneConnector _connector, TSPoints _point)
		{
			connector = _connector;
			point = _point;
		}
	}

	[HideInInspector]
	public int currentWaypoint;

	[HideInInspector]
	public int currentWaypointOnCar;

	[HideInInspector]
	public Vector3 RelativeWaypointPosition = Vector3.zero;

	[HideInInspector]
	public Vector3 RelativeWaypointPositionOnCar = Vector3.zero;

	[HideInInspector]
	public int previousWaypointSteer;

	[HideInInspector]
	public int currentLane;

	[HideInInspector]
	public TSNextLaneSelection nextTrack = default(TSNextLaneSelection);

	[HideInInspector]
	public List<TSNextLaneSelection> nextTrackPath = new List<TSNextLaneSelection>(20);

	public float currentMaxSpeed = 50f;

	[HideInInspector]
	public TSPoints[] waypoints;

	[HideInInspector]
	public TSPoints[] lastWaypoints;

	[HideInInspector]
	public TSLaneInfo[] lanes;

	[HideInInspector]
	public bool travelingOnConector;

	[HideInInspector]
	public bool wasTravelingOnConnector;

	[HideInInspector]
	public int selectedConnector;

	[HideInInspector]
	public TSLaneConnector lastSelectedConnector;

	[HideInInspector]
	public int lastLane;

	[HideInInspector]
	public int myID;

	[HideInInspector]
	public float relativeWPosMagnitude;

	[HideInInspector]
	public bool changingLane;

	[HideInInspector]
	public bool overTaking;

	[HideInInspector]
	public Queue<TSReservedPoints> reservedPoints = new Queue<TSReservedPoints>(100);

	[HideInInspector]
	public int nextTrackIndex;

	[HideInInspector]
	public Queue<TSReservedPoints> reservedChangeLanePoints = new Queue<TSReservedPoints>(100);

	[HideInInspector]
	public Queue<TSLaneConnector> reservedConnectors = new Queue<TSLaneConnector>(5);

	[HideInInspector]
	public bool starting = true;

	private TSTrafficAI carControllerAI;

	private Transform myTransform;

	private bool isTurning;

	private float halfDepth;

	private bool overtakingFromLeft;

	private float halfDepthSqr;

	private float carOcupation;

	public bool physicalLocomotion;

	private int reservLane;

	private int reservPoint;

	private int reservConnector;

	private bool roolback;

	private int i;

	private int pointIndex;

	private bool changed;

	private bool isConnector;

	private float distanceForCheck;

	private int newCurrentWaypointOnCar;

	private int newLane;

	private int initialIndex;

	private int point;

	private int lane;

	private bool found;

	private float segDistance;

	private int counter;

	private int reservedCLIndex;

	private TSReservedPoints newPoint1 = default(TSReservedPoints);

	private TSReservedPoints newPoint = default(TSReservedPoints);

	private bool wasConnector;

	private float totalDistance;

	private float dist;

	private int totalConnectors;

	private float maxInt;

	private int index;

	public float CarOcupationLenght
	{
		get
		{
			return carOcupation;
		}
	}

	private void Awake()
	{
		myTransform = base.transform;
		carControllerAI = GetComponent<TSTrafficAI>();
		myID = GetInstanceID();
		Transform obj = myTransform;
		obj.name = obj.name + "-(" + myID + ")";
	}

	private void Start()
	{
		if (lanes == null)
		{
			lanes = (Object.FindFirstObjectByType(typeof(TSMainManager)) as TSMainManager).lanes;
		}
		halfDepth = carControllerAI.carDepth / 2f;
		halfDepthSqr = (0f - halfDepth) * (0f - halfDepth);
		SetCarOccupationLength();
	}

	public void SetCarOccupationLength()
	{
		carOcupation = carControllerAI.carDepth + carControllerAI.lengthMargin;
	}

	private void Update()
	{
		if (base.gameObject.GetComponent<TSCommonProperties>() != null && !base.gameObject.GetComponent<TSCommonProperties>().crashed && !base.gameObject.GetComponent<TSCommonProperties>().upSideDown && !physicalLocomotion)
		{
			NavigateToWaypoints();
		}
	}

	private void FixedUpdate()
	{
		if (base.gameObject.GetComponent<TSCommonProperties>() != null && !base.gameObject.GetComponent<TSCommonProperties>().crashed && !base.gameObject.GetComponent<TSCommonProperties>().upSideDown && physicalLocomotion)
		{
			NavigateToWaypoints();
		}
	}

	public bool SwitchTrack()
	{
		if (nextTrackIndex >= nextTrackPath.Count)
		{
			return false;
		}
		if (lanes[nextTrackPath[nextTrackIndex].nextLane].connectors.Length == 0)
		{
			return false;
		}
		currentWaypoint = 0;
		previousWaypointSteer = 0;
		if (nextTrackPath[nextTrackIndex].isConnector)
		{
			currentLane = nextTrackPath[nextTrackIndex].nextLane;
			selectedConnector = nextTrackPath[nextTrackIndex].nextConnector;
			waypoints = lanes[currentLane].connectors[selectedConnector].points;
			lanes[lanes[currentLane].connectors[selectedConnector].nextLane].totalOcupation += Mathf.Round(carOcupation / lanes[lanes[currentLane].connectors[selectedConnector].nextLane].totalDistance * 100f);
			travelingOnConector = true;
			nextTrackIndex++;
			return true;
		}
		currentLane = nextTrackPath[nextTrackIndex].nextLane;
		waypoints = lanes[currentLane].points;
		GetLaneMaxSpeed();
		travelingOnConector = false;
		nextTrackIndex++;
		return true;
	}

	public void GetLaneMaxSpeed()
	{
		currentMaxSpeed = Random.Range(lanes[currentLane].maxSpeed / 2f, lanes[currentLane].maxSpeed);
	}

	public bool ReserveNearConnectorPoints(ref TSLaneConnector connector, int lane, int currentPoint)
	{
		roolback = false;
		connector.isRequested = true;
		if (!connector.isReserved)
		{
			if (lanes[connector.nextLane].totalOcupation > 75f)
			{
				connector.isRequested = false;
				return false;
			}
			if (connector.forcedStop && carControllerAI.carSpeed > 1f)
			{
				return false;
			}
			for (i = currentPoint; i < connector.points.Length; i++)
			{
				for (pointIndex = 0; pointIndex < connector.points[i].otherConnectorsPoints.Length; pointIndex++)
				{
					reservLane = connector.points[i].otherConnectorsPoints[pointIndex].lane;
					reservConnector = connector.points[i].otherConnectorsPoints[pointIndex].connector;
					reservPoint = connector.points[i].otherConnectorsPoints[pointIndex].pointIndex;
					if (lanes[reservLane].connectors[reservConnector].isRequested && lanes[reservLane].connectors[reservConnector].priority > connector.priority)
					{
						roolback = true;
						break;
					}
					if (lanes[reservLane].connectors[reservConnector].isReserved && lane != lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID)
					{
						roolback = true;
						break;
					}
					if (lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID != -1 && lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID != lane)
					{
						roolback = true;
						break;
					}
				}
				if (roolback)
				{
					break;
				}
				if (connector.points[i].laneReservationID != -1 && connector.points[i].laneReservationID != lane)
				{
					roolback = true;
					break;
				}
			}
			if (!roolback)
			{
				for (i = 0; i < connector.points.Length; i++)
				{
					for (pointIndex = 0; pointIndex < connector.points[i].otherConnectorsPoints.Length; pointIndex++)
					{
						reservLane = connector.points[i].otherConnectorsPoints[pointIndex].lane;
						reservConnector = connector.points[i].otherConnectorsPoints[pointIndex].connector;
						reservPoint = connector.points[i].otherConnectorsPoints[pointIndex].pointIndex;
						lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID = lane;
						lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount++;
						connector.OtherConnectors.Add(lanes[reservLane].connectors[reservConnector]);
					}
					connector.points[i].laneReservationID = lane;
					connector.points[i].connectorReservationCount++;
				}
				connector.isReserved = true;
				connector.reservedByID.Add(myID);
				reservedConnectors.Enqueue(connector);
			}
		}
		else if (!connector.reservedByID.Contains(myID))
		{
			if (CheckConnectorsPriority(connector))
			{
				return false;
			}
			if (connector.forcedStop && carControllerAI.carSpeed > 1f)
			{
				return false;
			}
			if (lanes[connector.nextLane].totalOcupation > 75f)
			{
				return false;
			}
			connector.reservedByID.Add(myID);
			reservedConnectors.Enqueue(connector);
		}
		return connector.isReserved;
	}

	public bool CheckConnectorsPriority(TSLaneConnector connector)
	{
		foreach (TSLaneConnector otherConnector in connector.OtherConnectors)
		{
			if (otherConnector.isRequested && otherConnector.priority > connector.priority)
			{
				return true;
			}
		}
		return false;
	}

	public bool ReserveNearConnectorPoints(ref TSPoints points, ref TSLaneConnector connector)
	{
		if (connector.isReserved)
		{
			roolback = false;
			for (pointIndex = 0; pointIndex < points.otherConnectorsPoints.Length; pointIndex++)
			{
				reservLane = points.otherConnectorsPoints[pointIndex].lane;
				reservConnector = points.otherConnectorsPoints[pointIndex].connector;
				reservPoint = points.otherConnectorsPoints[pointIndex].pointIndex;
				if (lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID == 0)
				{
					lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID = myID;
				}
				if (lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID != myID && !lanes[reservLane].connectors[reservConnector].connectorReservedByTrafficLight)
				{
					roolback = true;
					break;
				}
			}
			if (roolback)
			{
				for (pointIndex = 0; pointIndex < points.otherConnectorsPoints.Length; pointIndex++)
				{
					reservLane = points.otherConnectorsPoints[pointIndex].lane;
					reservConnector = points.otherConnectorsPoints[pointIndex].connector;
					reservPoint = points.otherConnectorsPoints[pointIndex].pointIndex;
					if (lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID == myID)
					{
						lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID = 0;
					}
				}
			}
			return !roolback;
		}
		return false;
	}

	public void UnReserveNearConnectorPoints(TSLaneConnector connector)
	{
		if (connector.isReserved && connector.reservedByID.Count == 1 && connector.reservedByID.Contains(myID))
		{
			for (int i = 0; i < connector.points.Length; i++)
			{
				for (int j = 0; j < connector.points[i].otherConnectorsPoints.Length; j++)
				{
					reservLane = connector.points[i].otherConnectorsPoints[j].lane;
					reservConnector = connector.points[i].otherConnectorsPoints[j].connector;
					reservPoint = connector.points[i].otherConnectorsPoints[j].pointIndex;
					if (lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID == connector.previousLane)
					{
						if (lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount < 2)
						{
							lanes[reservLane].connectors[reservConnector].points[reservPoint].laneReservationID = -1;
							lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount = 0;
						}
						else
						{
							lanes[reservLane].connectors[reservConnector].points[reservPoint].connectorReservationCount--;
						}
					}
				}
				if (connector.points[i].laneReservationID == connector.previousLane)
				{
					if (connector.points[i].connectorReservationCount < 2)
					{
						connector.points[i].laneReservationID = -1;
						connector.points[i].connectorReservationCount = 0;
					}
					else
					{
						connector.points[i].connectorReservationCount--;
					}
				}
			}
			connector.isReserved = false;
			connector.isRequested = false;
			connector.reservedByID.Remove(myID);
		}
		else if (connector.isReserved)
		{
			connector.reservedByID.Remove(myID);
		}
	}

	public void UnReserveNearConnectorPoints(TSPoints points)
	{
		for (int i = 0; i < points.otherConnectorsPoints.Length; i++)
		{
			reservLane = points.otherConnectorsPoints[i].lane;
			reservConnector = points.otherConnectorsPoints[i].connector;
			reservPoint = points.otherConnectorsPoints[i].pointIndex;
			if (lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID == myID)
			{
				lanes[reservLane].connectors[reservConnector].points[reservPoint].reservationID = 0;
			}
		}
	}

	private void NavigateToWaypoints()
	{
		RelativeWaypointPosition = myTransform.InverseTransformPoint(waypoints[currentWaypoint].point);
		if (reservedPoints.Count > 0)
		{
			RelativeWaypointPositionOnCar = myTransform.InverseTransformPoint(reservedPoints.Peek().point.point);
		}
		relativeWPosMagnitude = RelativeWaypointPosition.magnitude;
		if (!overTaking)
		{
			AddNextTrackToPath();
			if (RelativeWaypointPosition.z < carControllerAI.LookAheadDistance && Mathf.Abs(RelativeWaypointPosition.x) < 10f)
			{
				if (nextTrackPath.Count > 1 && !isTurning && nextTrackPath[1].isConnector && waypoints.Length - currentWaypoint < 15)
				{
					if (carControllerAI.OnTurnLeft != null && lanes[nextTrackPath[1].nextLane].connectors[nextTrackPath[1].nextConnector].direction == TSLaneConnector.Direction.Left)
					{
						isTurning = true;
						carControllerAI.OnTurnLeft(true);
					}
					if (carControllerAI.OnTurnRight != null && lanes[nextTrackPath[1].nextLane].connectors[nextTrackPath[1].nextConnector].direction == TSLaneConnector.Direction.Right)
					{
						isTurning = true;
						carControllerAI.OnTurnRight(true);
					}
				}
				previousWaypointSteer = currentWaypoint;
				currentWaypoint++;
				if (currentWaypoint >= waypoints.Length)
				{
					if (lanes[currentLane].connectors.Length > 0)
					{
						if (SwitchTrack())
						{
							currentWaypoint = 0;
						}
						else
						{
							currentWaypoint--;
						}
					}
					else
					{
						currentWaypoint--;
					}
				}
				if (previousWaypointSteer >= waypoints.Length)
				{
					previousWaypointSteer = 0;
				}
				RelativeWaypointPosition = myTransform.InverseTransformPoint(waypoints[currentWaypoint].point);
				relativeWPosMagnitude = RelativeWaypointPosition.magnitude;
			}
		}
		else if (RelativeWaypointPosition.z < carControllerAI.LookAheadDistance && Mathf.Abs(RelativeWaypointPosition.x) < 10f)
		{
			previousWaypointSteer = currentWaypoint;
			currentWaypoint--;
			if (currentWaypoint <= 0)
			{
				currentWaypoint = 0;
			}
			if (previousWaypointSteer <= 0)
			{
				previousWaypointSteer = 0;
			}
			RelativeWaypointPosition = myTransform.InverseTransformPoint(waypoints[currentWaypoint].point);
			relativeWPosMagnitude = RelativeWaypointPosition.magnitude;
		}
		CheckLastPoints();
	}

	private void CheckLastPoints()
	{
		distanceForCheck = Mathf.Sign(RelativeWaypointPositionOnCar.z) * RelativeWaypointPositionOnCar.sqrMagnitude;
		if (!(distanceForCheck < 0f - halfDepthSqr) || !(Mathf.Abs(RelativeWaypointPositionOnCar.x) < 10f) || reservedPoints.Count <= 1)
		{
			return;
		}
		changed = false;
		isConnector = reservedPoints.Peek().connector != null;
		if (!wasTravelingOnConnector)
		{
			totalDistance -= reservedPoints.Peek().point.distanceToNextPoint;
			if (isConnector)
			{
				changed = true;
			}
		}
		else
		{
			totalDistance -= reservedPoints.Peek().point.distanceToNextPoint;
			if (!isConnector)
			{
				changed = true;
			}
		}
		if ((changingLane && !overTaking) || (!changingLane && overTaking))
		{
			if (reservedChangeLanePoints.Count == 0)
			{
				if (changingLane)
				{
					changingLane = false;
				}
			}
			else if (reservedChangeLanePoints.Peek().point.reservationID == myID)
			{
				reservedChangeLanePoints.Peek().point.reservationID = 0;
				reservedChangeLanePoints.Peek().point.carwhoReserved = null;
				reservedChangeLanePoints.Dequeue();
			}
		}
		if (reservedPoints.Peek().point.reservationID == myID)
		{
			reservedPoints.Peek().point.reservationID = 0;
			reservedPoints.Peek().point.carwhoReserved = null;
		}
		carControllerAI.segDistance -= reservedPoints.Peek().point.distanceToNextPoint;
		UnReserveNearConnectorPoints(reservedPoints.Peek().point);
		if (changed)
		{
			if (nextTrackPath.Count < 1 && !overTaking)
			{
				AddNextTrackToPath();
			}
			if (nextTrackPath.Count > 0)
			{
				if (isConnector)
				{
					lastLane = nextTrackPath[0].nextLane;
					lastSelectedConnector = reservedPoints.Peek().connector;
					wasTravelingOnConnector = true;
					lanes[lastLane].totalOcupation -= Mathf.Round(carOcupation / lanes[lastLane].totalDistance * 100f);
					if (lanes[lastLane].totalOcupation < 0f)
					{
						lanes[lastLane].totalOcupation = 0f;
					}
				}
				else
				{
					if (carControllerAI.OnTurnLeft != null)
					{
						isTurning = false;
						carControllerAI.OnTurnLeft(false);
					}
					if (carControllerAI.OnTurnRight != null)
					{
						isTurning = false;
						carControllerAI.OnTurnRight(false);
					}
					wasTravelingOnConnector = false;
					lastSelectedConnector = null;
				}
				carControllerAI.nextPathIndex--;
				if (carControllerAI.nextPathIndex < 0)
				{
					carControllerAI.nextPathIndex = 0;
				}
				nextTrackPath.RemoveAt(0);
				nextTrackIndex--;
			}
		}
		reservedPoints.Dequeue();
	}

	public void LaneChange(bool left)
	{
		if (changingLane || reservedPoints.Count <= 0)
		{
			return;
		}
		newCurrentWaypointOnCar = 0;
		newLane = 0;
		initialIndex = currentWaypointOnCar;
		if (overTaking)
		{
			left = overtakingFromLeft;
			newCurrentWaypointOnCar = ((!left) ? reservedPoints.Peek().point.leftParalelLaneIndex : reservedPoints.Peek().point.rightParalelLaneIndex);
			newLane = ((!left) ? lanes[currentLane].laneLinkLeft : lanes[currentLane].laneLinkRight);
		}
		else
		{
			point = 0;
			lane = 0;
			found = false;
			if (left)
			{
				if (reservedPoints.Peek().point.leftParalelLaneIndex != -1)
				{
					found = true;
					point = reservedPoints.Peek().point.leftParalelLaneIndex;
					lane = lanes[currentLane].laneLinkLeft;
				}
			}
			else if (reservedPoints.Peek().point.rightParalelLaneIndex != -1)
			{
				found = true;
				point = reservedPoints.Peek().point.rightParalelLaneIndex;
				lane = lanes[currentLane].laneLinkRight;
			}
			if (!found)
			{
				return;
			}
			newCurrentWaypointOnCar = point;
			newLane = lane;
		}
		if ((int)((uint)lanes[newLane].vehicleType & (uint)(1 << (int)carControllerAI.myVehicleType)) <= 0)
		{
			return;
		}
		totalDistance = 0f;
		segDistance = (myTransform.position - reservedPoints.Peek().point.point).magnitude;
		counter = 0;
		reservedCLIndex = 0;
		while (segDistance < Mathf.Clamp(carControllerAI.maxLockaheadDistanceFullStop + carControllerAI.carDepth * 2f + 3f, carControllerAI.LookAheadDistance + carControllerAI.carDepth * 2f + 3f, float.MaxValue))
		{
			if (newCurrentWaypointOnCar + counter >= lanes[newLane].points.Length || (lanes[newLane].points[newCurrentWaypointOnCar + counter].reservationID != 0 && lanes[newLane].points[newCurrentWaypointOnCar + counter].reservationID != myID))
			{
				while (reservedChangeLanePoints.Count > 0)
				{
					if (reservedChangeLanePoints.Peek().point.reservationID == myID)
					{
						reservedChangeLanePoints.Peek().point.reservationID = 0;
						reservedChangeLanePoints.Peek().point.carwhoReserved = null;
					}
					reservedChangeLanePoints.Dequeue();
				}
				return;
			}
			if (segDistance < carControllerAI.carDepth * 2f + 3f && ((overTaking && initialIndex - counter >= 0) || (!overTaking && initialIndex + counter < lanes[currentLane].points.Length)) && (lanes[currentLane].points[(!overTaking) ? (initialIndex + counter) : (initialIndex - counter)].reservationID == 0 || lanes[currentLane].points[(!overTaking) ? (initialIndex + counter) : (initialIndex - counter)].reservationID == myID))
			{
				newPoint1.connector = null;
				newPoint1.point = lanes[currentLane].points[(!overTaking) ? (initialIndex + counter) : (initialIndex - counter)];
				if (newPoint1.point.reservationID == 0)
				{
					newPoint1.point.reservationID = myID;
					newPoint1.point.carwhoReserved = carControllerAI;
				}
				reservedChangeLanePoints.Enqueue(newPoint1);
				reservedCLIndex++;
			}
			segDistance += lanes[newLane].points[newCurrentWaypointOnCar + counter].distanceToNextPoint;
			counter++;
		}
		while (reservedChangeLanePoints.Count > reservedCLIndex)
		{
			if (reservedChangeLanePoints.Peek().point.reservationID == myID)
			{
				reservedChangeLanePoints.Peek().point.reservationID = 0;
				reservedChangeLanePoints.Peek().point.carwhoReserved = null;
			}
			reservedChangeLanePoints.Dequeue();
		}
		UnreserveAll(false);
		reservedPoints.Clear();
		for (i = 0; i < counter; i++)
		{
			lanes[newLane].points[newCurrentWaypointOnCar + i].reservationID = myID;
			lanes[newLane].points[newCurrentWaypointOnCar + i].carwhoReserved = carControllerAI;
			newPoint.connector = null;
			newPoint.point = lanes[newLane].points[newCurrentWaypointOnCar + i];
			reservedPoints.Enqueue(newPoint);
		}
		if (counter == 0)
		{
			lanes[newLane].points[newCurrentWaypointOnCar].reservationID = myID;
			lanes[newLane].points[newCurrentWaypointOnCar].carwhoReserved = carControllerAI;
			newPoint.connector = null;
			newPoint.point = lanes[newLane].points[newCurrentWaypointOnCar];
			reservedPoints.Enqueue(newPoint);
		}
		changingLane = true;
		currentLane = newLane;
		if (overTaking)
		{
			overTaking = false;
			carControllerAI.changeLaneTime = Time.time;
		}
		waypoints = lanes[newLane].points;
		wasTravelingOnConnector = false;
		travelingOnConector = false;
		lastLane = 0;
		lastWaypoints = waypoints;
		int num = newCurrentWaypointOnCar + counter - 1;
		if (num >= lastWaypoints.Length)
		{
			num = lastWaypoints.Length - 1;
		}
		if (num < 0)
		{
			num = 0;
		}
		carControllerAI.previousWaypointCurve = num;
		currentWaypoint = num;
		currentWaypointOnCar = newCurrentWaypointOnCar;
		previousWaypointSteer = currentWaypoint;
		RelativeWaypointPositionOnCar = myTransform.InverseTransformPoint(reservedPoints.Peek().point.point);
		relativeWPosMagnitude = 0f;
		carControllerAI.nextPathIndex = 0;
		carControllerAI.segDistance = segDistance - carControllerAI.carDepth - 3f;
		carControllerAI.currenPoint = lastWaypoints[carControllerAI.previousWaypointCurve];
		carControllerAI.currentConnector = null;
		carControllerAI.lane = 0;
		carControllerAI.brakeSpeeds.Clear();
		selectedConnector = 0;
		lastSelectedConnector = null;
		starting = true;
		currentMaxSpeed = Random.Range(lanes[newLane].maxSpeed / 2f, lanes[newLane].maxSpeed);
		AddNextTrackToPath();
	}

	public void OverTaking(bool left)
	{
		if (overTaking || reservedPoints.Count <= 0)
		{
			return;
		}
		newCurrentWaypointOnCar = ((!left) ? reservedPoints.Peek().point.rightParalelLaneIndex : reservedPoints.Peek().point.leftParalelLaneIndex);
		newLane = ((!left) ? lanes[currentLane].laneLinkRight : lanes[currentLane].laneLinkLeft);
		if ((int)((uint)lanes[newLane].vehicleType & (uint)(1 << (int)carControllerAI.myVehicleType)) <= 0 || newCurrentWaypointOnCar < 25)
		{
			return;
		}
		totalDistance = 0f;
		segDistance = (myTransform.position - reservedPoints.Peek().point.point).magnitude;
		counter = 0;
		reservedCLIndex = 0;
		initialIndex = ((!left) ? lanes[newLane].points[newCurrentWaypointOnCar].rightParalelLaneIndex : lanes[newLane].points[newCurrentWaypointOnCar].leftParalelLaneIndex);
		while (segDistance < Mathf.Clamp(carControllerAI.maxLockaheadDistanceFullStop * 5f, carControllerAI.carDepth * 10f + 3f, float.MaxValue))
		{
			if (newCurrentWaypointOnCar - counter < 0 || (lanes[newLane].points[newCurrentWaypointOnCar - counter].reservationID != 0 && lanes[newLane].points[newCurrentWaypointOnCar - counter].reservationID != myID) || initialIndex + counter >= lanes[currentLane].points.Length - 1 || lanes[currentLane].points.Length == 0)
			{
				while (reservedChangeLanePoints.Count > 0)
				{
					if (reservedChangeLanePoints.Peek().point.reservationID == myID)
					{
						reservedChangeLanePoints.Peek().point.reservationID = 0;
						reservedChangeLanePoints.Peek().point.carwhoReserved = null;
					}
					reservedChangeLanePoints.Dequeue();
				}
				return;
			}
			if (segDistance < carControllerAI.carDepth * 3f + 3f && (lanes[currentLane].points[initialIndex + counter].reservationID == 0 || lanes[currentLane].points[initialIndex + counter].reservationID == myID))
			{
				newPoint1.connector = null;
				newPoint1.point = lanes[currentLane].points[initialIndex + counter];
				if (newPoint1.point.reservationID == 0)
				{
					newPoint1.point.reservationID = myID;
					newPoint1.point.carwhoReserved = carControllerAI;
				}
				reservedChangeLanePoints.Enqueue(newPoint1);
				reservedCLIndex++;
			}
			segDistance += lanes[newLane].points[newCurrentWaypointOnCar - counter].distanceToNextPoint;
			counter++;
		}
		if (counter == 0)
		{
			return;
		}
		while (reservedChangeLanePoints.Count > reservedCLIndex)
		{
			if (reservedChangeLanePoints.Peek().point.reservationID == myID)
			{
				reservedChangeLanePoints.Peek().point.reservationID = 0;
				reservedChangeLanePoints.Peek().point.carwhoReserved = null;
			}
			reservedChangeLanePoints.Dequeue();
		}
		UnreserveAll(false);
		if (!wasTravelingOnConnector)
		{
			lanes[lastLane].totalOcupation -= Mathf.Round(carOcupation / lanes[lastLane].totalDistance * 100f);
		}
		reservedPoints.Clear();
		for (i = 0; i < counter; i++)
		{
			lanes[newLane].points[newCurrentWaypointOnCar - i].reservationID = myID;
			lanes[newLane].points[newCurrentWaypointOnCar - i].carwhoReserved = carControllerAI;
			newPoint.connector = null;
			newPoint.point = lanes[newLane].points[newCurrentWaypointOnCar - i];
			reservedPoints.Enqueue(newPoint);
		}
		overTaking = true;
		overtakingFromLeft = !left;
		currentLane = newLane;
		waypoints = lanes[newLane].points;
		lastWaypoints = waypoints;
		int num = newCurrentWaypointOnCar - counter / 2;
		if (num >= lastWaypoints.Length)
		{
			num = lastWaypoints.Length - 1;
		}
		if (num < 0)
		{
			num = 0;
		}
		currentWaypoint = num;
		currentWaypointOnCar = newCurrentWaypointOnCar;
		previousWaypointSteer = currentWaypoint;
		carControllerAI.previousWaypointCurve = newCurrentWaypointOnCar - counter;
		wasTravelingOnConnector = false;
		travelingOnConector = false;
		lastLane = 0;
		RelativeWaypointPositionOnCar = myTransform.InverseTransformPoint(reservedPoints.Peek().point.point);
		relativeWPosMagnitude = 0f;
		carControllerAI.nextPathIndex = 0;
		carControllerAI.segDistance = segDistance - carControllerAI.carDepth - 3f;
		carControllerAI.currenPoint = lastWaypoints[carControllerAI.previousWaypointCurve];
		carControllerAI.currentConnector = null;
		carControllerAI.lane = 0;
		carControllerAI.brakeSpeeds.Clear();
		selectedConnector = 0;
		lastSelectedConnector = null;
		starting = true;
		nextTrackPath.Clear();
		currentMaxSpeed = Random.Range(lanes[newLane].maxSpeed / 2f, lanes[newLane].maxSpeed);
	}

	public void TurnOffTurningLights()
	{
		isTurning = false;
		if (carControllerAI.OnTurnLeft != null)
		{
			carControllerAI.OnTurnLeft(false);
		}
		if (carControllerAI.OnTurnRight != null)
		{
			carControllerAI.OnTurnRight(false);
		}
	}

	public void UnreserveAll(bool all)
	{
		if (nextTrackPath.Count > 0 && !nextTrackPath[0].isConnector)
		{
			lanes[nextTrackPath[0].nextLane].totalOcupation -= Mathf.Round(carOcupation / lanes[nextTrackPath[0].nextLane].totalDistance * 100f);
			if (lanes[nextTrackPath[0].nextLane].totalOcupation < 0f)
			{
				lanes[nextTrackPath[0].nextLane].totalOcupation = 0f;
			}
		}
		else if (!travelingOnConector)
		{
			lanes[currentLane].totalOcupation -= Mathf.Round(carOcupation / lanes[currentLane].totalDistance * 100f);
			if (lanes[currentLane].totalOcupation < 0f)
			{
				lanes[currentLane].totalOcupation = 0f;
			}
		}
		if (changingLane)
		{
			changingLane = false;
		}
		if (all)
		{
			while (reservedChangeLanePoints.Count > 0)
			{
				if (reservedChangeLanePoints.Peek().point.reservationID == myID)
				{
					reservedChangeLanePoints.Peek().point.reservationID = 0;
					reservedChangeLanePoints.Peek().point.carwhoReserved = null;
				}
				reservedChangeLanePoints.Dequeue();
			}
			overTaking = false;
		}
		while (reservedConnectors.Count > 0)
		{
			UnReserveNearConnectorPoints(reservedConnectors.Peek());
			reservedConnectors.Dequeue();
		}
		while (reservedPoints.Count > 0)
		{
			if (reservedPoints.Peek().point.reservationID == myID)
			{
				reservedPoints.Peek().point.reservationID = 0;
				reservedPoints.Peek().point.carwhoReserved = null;
			}
			UnReserveNearConnectorPoints(reservedPoints.Peek().point);
			reservedPoints.Dequeue();
		}
		carControllerAI.nextPathIndex = 0;
		carControllerAI.segDistance = 0f;
		totalDistance = 0f;
		nextTrackIndex = 0;
		nextTrackPath.Clear();
	}

	public void AddNextTrackToPath()
	{
		dist = Mathf.Max(carControllerAI.maxLockaheadDistanceFullStop, carControllerAI.LookAheadDistance) + carControllerAI.carDepth + 2f;
		if (totalDistance < 0f)
		{
			totalDistance = 0f;
		}
		while (totalDistance < dist)
		{
			if (starting)
			{
				wasConnector = true;
				starting = false;
				nextTrack.isConnector = false;
				nextTrack.nextLane = currentLane;
				nextTrackIndex = 1;
				if (currentWaypointOnCar > lanes[currentLane].points.Length)
				{
					currentWaypointOnCar = lanes[currentLane].points.Length - 1;
				}
				totalDistance = lanes[currentLane].totalDistance;
				for (int i = 0; i < currentWaypointOnCar; i++)
				{
					totalDistance -= lanes[currentLane].points[i].distanceToNextPoint;
				}
			}
			else if (!wasConnector)
			{
				if (nextTrackPath.Count > 0)
				{
					wasConnector = true;
					nextTrack.nextLane = lanes[nextTrackPath[nextTrackPath.Count - 1].nextLane].connectors[nextTrackPath[nextTrackPath.Count - 1].nextConnector].nextLane;
					nextTrack.isConnector = false;
				}
				else
				{
					wasConnector = true;
					nextTrack.nextLane = lanes[currentLane].connectors[selectedConnector].nextLane;
					nextTrack.isConnector = false;
				}
				totalDistance += lanes[nextTrack.nextLane].totalDistance;
			}
			else
			{
				wasConnector = false;
				if (nextTrackPath.Count > 0)
				{
					nextTrack.nextLane = nextTrackPath[nextTrackPath.Count - 1].nextLane;
					nextTrack.nextConnector = GetNextConnector(nextTrackPath[nextTrackPath.Count - 1].nextLane);
				}
				else
				{
					nextTrack.nextLane = currentLane;
					nextTrack.nextConnector = GetNextConnector(currentLane);
				}
				nextTrack.isConnector = true;
				totalDistance += lanes[nextTrack.nextLane].connectors[nextTrack.nextConnector].totalDistance;
			}
			nextTrackPath.Add(nextTrack);
		}
	}

	private int GetNextConnector(int lane)
	{
		totalConnectors = lanes[lane].connectors.Length;
		maxInt = float.MaxValue;
		index = 0;
		for (int i = 0; i < totalConnectors; i++)
		{
			if ((int)((uint)lanes[lanes[lane].connectors[i].nextLane].vehicleType & (uint)(1 << (int)carControllerAI.myVehicleType)) <= 0 || (int)((uint)lanes[lane].connectors[i].vehicleType & (uint)(1 << (int)carControllerAI.myVehicleType)) <= 0)
			{
				lanes[lane].connectors[i].randomWeight = float.MaxValue - (float)Random.Range(1, lanes[lane].connectors.Length + 1);
			}
			else
			{
				lanes[lane].connectors[i].randomWeight = lanes[lanes[lane].connectors[i].nextLane].totalOcupation * 100f * Random.Range(1f, (float)lanes[lane].connectors.Length * Random.Range(1f, 100f));
			}
			if (lanes[lane].connectors[i].randomWeight < maxInt && lanes[lanes[lane].connectors[i].nextLane].totalOcupation < 70f)
			{
				maxInt = lanes[lane].connectors[i].randomWeight;
				index = i;
			}
		}
		return index;
	}

	private void OnDrawGizmosSelected()
	{
		if (!Application.isPlaying)
		{
			return;
		}
		Gizmos.color = Color.green;
		foreach (TSReservedPoints reservedPoint in reservedPoints)
		{
			Gizmos.DrawWireSphere(reservedPoint.point.point, 1f);
		}
		Gizmos.color = Color.red;
		foreach (TSLaneConnector reservedConnector in reservedConnectors)
		{
			Gizmos.DrawLine(reservedConnector.conectorA, reservedConnector.conectorB);
		}
		foreach (TSNextLaneSelection item in nextTrackPath)
		{
			if (item.isConnector)
			{
				Gizmos.color = Color.cyan;
				Gizmos.DrawLine(lanes[item.nextLane].connectors[item.nextConnector].conectorA, lanes[item.nextLane].connectors[item.nextConnector].conectorB);
			}
			else
			{
				Gizmos.color = Color.magenta;
				Gizmos.DrawLine(lanes[item.nextLane].conectorA, lanes[item.nextLane].conectorB);
			}
		}
		Gizmos.color = Color.blue;
		Gizmos.DrawSphere(waypoints[currentWaypoint].point, 1f);
	}
}
