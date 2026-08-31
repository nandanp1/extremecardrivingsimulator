using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
public class TSLaneInfo
{
	public enum VehicleType
	{
		Taxi = 0,
		Bus = 1,
		Light = 2,
		Medium = 3,
		Heavy = 4,
		Train = 5,
		Heavy_Machinery = 6,
		Pedestrians = 7
	}

	[XmlElement("a14")]
	public Vector3 conectorA;

	[XmlElement("a15")]
	public Vector3 conectorB;

	[XmlArrayItem("a17")]
	[XmlArray("a16")]
	public List<Vector3> middlePoints = new List<Vector3>();

	[XmlElement("a18")]
	public float laneWidth = 2.5f;

	[XmlElement("a19", typeof(int))]
	public VehicleType vehicleType = (VehicleType)(-1);

	[XmlElement("a20")]
	public float maxSpeed = 50f;

	[XmlArrayItem("a22")]
	[SerializeField]
	[XmlArray("a21")]
	public TSPoints[] points;

	[XmlArray("a23")]
	[XmlArrayItem("a24")]
	[SerializeField]
	public TSLaneConnector[] connectors;

	[XmlElement("a25")]
	public int laneLinkRight = -1;

	[XmlElement("a26")]
	public int laneLinkLeft = -1;

	[XmlElement("a27")]
	public float trafficDensity = 1f;

	[XmlElement("a28")]
	public float totalOcupation;

	[XmlElement("a29")]
	public float totalDistance;
}
