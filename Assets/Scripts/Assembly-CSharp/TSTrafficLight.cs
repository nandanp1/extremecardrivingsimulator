using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TSTrafficLight : MonoBehaviour
{
	[Serializable]
	public class TSLight
	{
		public float lightTimer;

		public LightType lightType;

		public float lightTime = 20f;

		public Texture2D lightTexture;

		public MeshRenderer lightMeshRenderer;

		public bool enableDisableRenderer;

		public GameObject lightGameObject;

		public string shaderTexturePropertyName = "_MainTex";
	}

	[Serializable]
	public class TSPointReference
	{
		public int lane;

		public int connector;

		public int point;
	}

	[Serializable]
	public enum LightType
	{
		Green = 0,
		Red = 1,
		Yellow = 2,
		NoLights = 3
	}

	[SerializeField]
	public List<TSLight> lights = new List<TSLight>();

	[SerializeField]
	public TSMainManager manager;

	private int lastLane;

	private int lastPoint;

	private int lastConnector;

	private int trafficLightID;

	[SerializeField]
	public List<TSPointReference> pointsNormalLight = new List<TSPointReference>();

	public bool enableLight = true;

	private bool weHaveManager;

	public bool yellowLightsStopTraffic = true;

	public float lightRange = 500f;

	public int lightToPlay;

	private void Start()
	{
		trafficLightID = GetInstanceID();
		if (manager == null)
		{
			manager = UnityEngine.Object.FindFirstObjectByType(typeof(TSMainManager)) as TSMainManager;
		}
		if (manager == null)
		{
			weHaveManager = false;
		}
		else
		{
			weHaveManager = true;
		}
		foreach (TSLight light in lights)
		{
			if (light.enableDisableRenderer && light.lightMeshRenderer != null)
			{
				light.lightMeshRenderer.enabled = false;
			}
			if (light.lightGameObject != null)
			{
				light.lightGameObject.SetActive(false);
			}
		}
		if (lights.Count > 0 && weHaveManager)
		{
			StartCoroutine(PlayLights());
		}
	}

	private IEnumerator PlayLights()
	{
		while (true)
		{
			if (lightToPlay >= lights.Count)
			{
				lightToPlay = 0;
			}
			if (lights[lightToPlay] != null)
			{
				if (!lights[lightToPlay].enableDisableRenderer)
				{
					if (lights[lightToPlay].lightMeshRenderer != null && lights[lightToPlay].lightMeshRenderer.material != null)
					{
						lights[lightToPlay].lightMeshRenderer.material.SetTexture(lights[lightToPlay].shaderTexturePropertyName, lights[lightToPlay].lightTexture);
					}
				}
				else if (lights[lightToPlay].lightMeshRenderer != null)
				{
					lights[lightToPlay].lightMeshRenderer.enabled = true;
				}
				if (lights[lightToPlay].lightGameObject != null)
				{
					lights[lightToPlay].lightGameObject.SetActive(true);
				}
				switch (lights[lightToPlay].lightType)
				{
				case LightType.Yellow:
					if (yellowLightsStopTraffic)
					{
						for (int j = 0; j < pointsNormalLight.Count; j++)
						{
							ChangeReservation(pointsNormalLight[j], trafficLightID, trafficLightID);
						}
					}
					break;
				case LightType.Red:
				{
					for (int k = 0; k < pointsNormalLight.Count; k++)
					{
						ChangeReservation(pointsNormalLight[k], trafficLightID, trafficLightID);
					}
					break;
				}
				case LightType.Green:
				{
					for (int i = 0; i < pointsNormalLight.Count; i++)
					{
						ChangeReservation(pointsNormalLight[i], 0, -1);
					}
					break;
				}
				}
				yield return new WaitForSeconds(lights[lightToPlay].lightTime);
				if (lights[lightToPlay].enableDisableRenderer && lights[lightToPlay].lightMeshRenderer != null)
				{
					lights[lightToPlay].lightMeshRenderer.enabled = false;
				}
				if (lights[lightToPlay].lightGameObject != null)
				{
					lights[lightToPlay].lightGameObject.SetActive(false);
				}
				lightToPlay++;
			}
			else
			{
				lightToPlay++;
			}
		}
	}

	private IEnumerator UpdateRemaningGreenLightTime(List<TSPointReference> point, float time)
	{
		for (int i = 0; i < point.Count; i++)
		{
			manager.lanes[point[i].lane].connectors[point[i].connector].remainingGreenLightTime = time;
		}
		while (manager.lanes[point[0].lane].connectors[point[0].connector].remainingGreenLightTime > 0f)
		{
			for (int j = 0; j < point.Count; j++)
			{
				manager.lanes[point[j].lane].connectors[point[j].connector].remainingGreenLightTime -= Time.deltaTime;
				if (manager.lanes[point[j].lane].connectors[point[j].connector].remainingGreenLightTime < 0f)
				{
					manager.lanes[point[j].lane].connectors[point[j].connector].remainingGreenLightTime = 0f;
				}
			}
			yield return null;
		}
	}

	private void ChangeReservation(TSPointReference point, int ID, int lID)
	{
		if (point.connector == -1)
		{
			manager.lanes[point.lane].points[point.point].reservationID = ID;
			return;
		}
		manager.lanes[point.lane].connectors[point.connector].points[point.point].reservationID = ID;
		if (ID == 0 && lID == -1)
		{
			manager.lanes[point.lane].connectors[point.connector].connectorReservedByTrafficLight = false;
			manager.lanes[point.lane].connectors[point.connector].remainingGreenLightTime = -1f;
		}
		else
		{
			manager.lanes[point.lane].connectors[point.connector].connectorReservedByTrafficLight = true;
			manager.lanes[point.lane].connectors[point.connector].remainingGreenLightTime = -1f;
			manager.lanes[point.lane].connectors[point.connector].points[point.point].carwhoReserved = null;
		}
	}
}
