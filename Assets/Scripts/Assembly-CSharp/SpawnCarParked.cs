using UnityEngine;

public class SpawnCarParked : MonoBehaviour
{
	private LayerMask layerMask;

	private Transform AIController;

	private GameObject aiCar;

	private AIContoller ACScript;

	private float InstantiateTime;

	private int random;

	private void Awake()
	{
		layerMask = LayerMask.GetMask("Traffic");
		AIController = base.transform.root;
		ACScript = AIController.GetComponent<AIContoller>();
	}

	public void InstantiateCar()
	{
		GameObject carForActiveTraffic = ACScript.GetCarForActiveTraffic();
		if (!carForActiveTraffic)
		{
			return;
		}
		InstantiateTime = Mathf.MoveTowards(InstantiateTime, 0f, Time.deltaTime);
		Collider[] array = Physics.OverlapSphere(base.transform.position, ACScript.stopDistanceParkCars, layerMask);
		bool flag = false;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.CompareTag("carcollider"))
			{
				flag = true;
			}
		}
		if (!flag)
		{
			GameObject gameObject = carForActiveTraffic;
			gameObject.transform.position = base.transform.position;
			gameObject.transform.rotation = base.transform.rotation;
			CarController component = gameObject.GetComponent<CarController>();
			component.carMode = CarMode.Empty;
			gameObject.GetComponent<CarController>().carParked = true;
			NGUITools.SetActive(gameObject, true);
		}
		InstantiateTime = Random.Range(1f, 2f);
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.cyan;
		Gizmos.DrawRay(base.transform.position, base.transform.forward);
		Gizmos.DrawSphere(base.transform.position, 1.5f);
	}
}
