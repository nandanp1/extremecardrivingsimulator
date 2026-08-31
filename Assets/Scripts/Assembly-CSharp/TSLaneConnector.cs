using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class TSLaneConnector
{
	public enum Direction
	{
		Left = 0,
		Right = 1,
		Straight = 2
	}

	[XmlElement("a30")]
	public Vector3 conectorA = Vector3.zero;

	[XmlElement("a31")]
	public Vector3 conectorB = Vector3.zero;

	[XmlArrayItem("a33")]
	[XmlArray("a32")]
	public List<Vector3> middlePoints = new List<Vector3>();

	[XmlArray("a34")]
	[SerializeField]
	[XmlArrayItem("a35")]
	public TSPoints[] points;

	[XmlElement("a36")]
	public int nextLane;

	[XmlElement("a37")]
	public int previousLane;

	[XmlElement("a38")]
	public bool forcedStop;

	[XmlElement("a39")]
	public bool isReserved;

	[XmlElement("a40")]
	public bool isRequested;

	[XmlArrayItem("a42")]
	[XmlArray("a41")]
	public HashSet<int> reservedByID = new HashSet<int>();

	[XmlElement("a43")]
	public int priority = 1;

	[XmlElement("a44")]
	public Direction direction = Direction.Straight;

	[XmlElement("a45")]
	public float totalDistance;

	[XmlElement("a46")]
	public float randomWeight;

	[XmlElement("a47", typeof(int))]
	public TSLaneInfo.VehicleType vehicleType = (TSLaneInfo.VehicleType)(-1);

	[XmlElement("a48")]
	public bool connectorReservedByTrafficLight;

	[XmlElement("a49")]
	public float remainingGreenLightTime = -1f;

	[XmlElement("a50")]
	public HashSet<TSLaneConnector> OtherConnectors = new HashSet<TSLaneConnector>();
}
