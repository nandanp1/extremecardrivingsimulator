using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class TSPoints
{
	[XmlElement("a4")]
	public Vector3 point;

	[XmlElement("a5")]
	public int reservationID;

	[XmlIgnore]
	public TSTrafficAI carwhoReserved;

	[XmlElement("a6")]
	public int laneReservationID = -1;

	[XmlElement("a7")]
	public int connectorReservationCount;

	[XmlElement("a8")]
	public float distanceToNextPoint;

	[XmlElement("a9")]
	public int rightParalelLaneIndex = -1;

	[XmlElement("a10")]
	public int leftParalelLaneIndex = -1;

	[XmlElement("a11")]
	public float maxSpeedLimit = 1000f;

	[XmlArrayItem("a13")]
	[SerializeField]
	[XmlArray("a12")]
	public TSConnectorOtherPoints[] otherConnectorsPoints;
}
