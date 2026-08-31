using UnityEngine;

public class TrafficITSZoneActivation : MonoBehaviour
{
	public GameObject TSTrafficSpawnerPrefab;

	private GameObject TSTrafficSpawnerInstance;

	private GameObject TestSwicherInstance;

	private void Start()
	{
	}

	private void Update()
	{
	}

	public void CreateTraffic()
	{
		GameObject gameObject = GameObject.Find("TrafficCarsContainer");
		if ((bool)gameObject)
		{
			Object.Destroy(gameObject);
		}
		GameObject gameObject2 = new GameObject("Traffic Detector");
		gameObject2.transform.SetParent(MySingleton<Manager>.Instance.gameLogic.currentCar);
		gameObject2.transform.localPosition = Vector3.zero;
		gameObject2.AddComponent<TestSwitcher>();
		TSTrafficSpawnerInstance = Object.Instantiate(TSTrafficSpawnerPrefab, Vector3.zero, Quaternion.identity) as GameObject;
		TSTrafficSpawnerInstance.gameObject.SetActive(true);
	}

	public void DeleteTraffic()
	{
		if (TSTrafficSpawnerInstance != null)
		{
			Object.DestroyImmediate(TSTrafficSpawnerInstance);
		}
		if (TestSwicherInstance != null)
		{
			Object.DestroyImmediate(TestSwicherInstance);
		}
		GameObject gameObject = GameObject.Find("TrafficCarsContainer");
		if ((bool)gameObject)
		{
			Object.Destroy(gameObject);
		}
	}
}
