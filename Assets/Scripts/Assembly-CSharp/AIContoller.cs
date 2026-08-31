using UnityEngine;

public class AIContoller : MonoBehaviour
{
	public float bigWaypointsDetectionArea = 120f;

	public float smallWaypointsDetectionArea = 80f;

	public float stopDistanceParkCars = 8f;

	public float timeForUpdateTrafficInstantiate = 0.25f;

	private Vector3 initialPosition = Vector3.up * -100f;

	[HideInInspector]
	public int numCars;

	public int maxCars;

	private LayerMask invisible;

	private LayerMask visible;

	public float minSideLength;

	public float maxSideLength;

	public GameObject[] aiCarPrefabs;

	[HideInInspector]
	public GameObject[] aiCarsInstantiated;

	private void Awake()
	{
		minSideLength = -0.5f;
		maxSideLength = 0.5f;
		numCars = 0;
		aiCarsInstantiated = new GameObject[maxCars];
	}

	public void InstantiateCars()
	{
		GameObject carsParent = MySingleton<Manager>.Instance.gameLogic.carsParent;
		carsParent.SetActive(true);
		for (int i = 0; i != maxCars; i++)
		{
			GameObject gameObject = Object.Instantiate(aiCarPrefabs[i % aiCarPrefabs.Length], initialPosition, Quaternion.identity) as GameObject;
			gameObject.transform.parent = carsParent.transform;
			NGUITools.SetActive(gameObject, false);
			gameObject.name += i;
			Color colorGroup = base.gameObject.GetComponent<CarsColors>().GetColorGroup(gameObject);
			gameObject.GetComponent<PaintCar>().IniPaintMaterials(colorGroup);
			aiCarsInstantiated[i] = gameObject;
		}
	}

	public GameObject GetCarForActiveTraffic()
	{
		GameObject gameObject = null;
		GameObject[] array = aiCarsInstantiated;
		foreach (GameObject gameObject2 in array)
		{
			if (!gameObject2.activeSelf)
			{
				gameObject = gameObject2;
				break;
			}
		}
		if ((bool)gameObject)
		{
			gameObject.GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
			gameObject.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
		}
		return gameObject;
	}

	public void GetCarForInactiveTraffic(GameObject aiCarActive)
	{
		for (int i = 0; i != maxCars; i++)
		{
			GameObject gameObject = aiCarsInstantiated[i];
			if (gameObject == aiCarActive)
			{
				NGUITools.SetActive(aiCarsInstantiated[i], false);
				break;
			}
		}
	}

	public void SetAllCarForInactiveTraffic()
	{
		GameObject[] array = aiCarsInstantiated;
		foreach (GameObject go in array)
		{
			NGUITools.SetActive(go, false);
		}
	}
}
