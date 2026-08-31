using UnityEngine;

public class InstantiateAICar : MonoBehaviour
{
	public LayerMask layerMask = -1;

	private AIContoller ACScript;

	private AIContoller aiContoller;

	public void OnEnable()
	{
		ACScript = Object.FindFirstObjectByType(typeof(AIContoller)) as AIContoller;
		if (!ACScript)
		{
			Debug.LogError("La escena no tiene configuradas las rutas para el trafico");
			base.enabled = false;
		}
		else
		{
			InvokeRepeating("UpdateTrafficInstantiate", 0f, ACScript.timeForUpdateTrafficInstantiate);
		}
	}

	public void OnDisable()
	{
		CancelInvoke("UpdateTrafficInstantiate");
		if ((bool)ACScript)
		{
			ACScript.SetAllCarForInactiveTraffic();
		}
		else
		{
			base.enabled = false;
		}
	}

	private void UpdateTrafficInstantiate()
	{
		Collider[] array = Physics.OverlapSphere(base.transform.position, ACScript.bigWaypointsDetectionArea, layerMask);
		if (ACScript.numCars >= ACScript.maxCars)
		{
			return;
		}
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			float num = Vector3.Distance(base.transform.position, collider.transform.position);
			if (num < ACScript.bigWaypointsDetectionArea && num > ACScript.smallWaypointsDetectionArea)
			{
				if ((bool)collider.GetComponent<TrafficWaypoint>())
				{
					collider.GetComponent<TrafficWaypoint>().InstantiateCar();
				}
				else if ((bool)collider.GetComponent<SpawnCarParked>())
				{
					collider.GetComponent<SpawnCarParked>().InstantiateCar();
				}
			}
		}
	}
}
