using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

public class TSMainManager : MonoBehaviour
{
	[Serializable]
	public class VehicleTypePresets
	{
		public TSLaneInfo.VehicleType vehicleType;

		public string name;
	}

	[SerializeField]
	[XmlArrayItem("Lane")]
	[XmlArray("Lanes")]
	public TSLaneInfo[] lanes;

	public int menuSelection;

	public int laneMenuSelection;

	public int connectionsMenuSelection;

	public int settingsMenuSelection;

	public float visualLinesWidth = 5f;

	public float resolution = 4f;

	public float resolutionConnectors = 2.3f;

	public float laneCurveSpeedMultiplier = 0.7f;

	public float connectorsCurveSpeedMultiplier = 0.7f;

	public TSLaneInfo.VehicleType defaultVehicleType = (TSLaneInfo.VehicleType)(-1);

	public bool junctionsProcessed;

	[SerializeField]
	public List<VehicleTypePresets> vehicleTypePresets = new List<VehicleTypePresets>();
}
