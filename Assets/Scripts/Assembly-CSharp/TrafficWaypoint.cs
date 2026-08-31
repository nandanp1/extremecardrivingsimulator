using UnityEngine;

public class TrafficWaypoint : MonoBehaviour
{
	public enum TrafficDensity
	{
		Normal = 0,
		None = 2,
		Low = 1,
		High = 3,
		trafficJam = 4
	}

	private const float TWO_WAYS_CENTER_DISTANCE = 4f;

	public TrafficDensity trafficDensity;

	private LayerMask layerMask;

	public Transform way_next;

	public Transform way_before;

	public bool onlyOneWay;

	[HideInInspector]
	public bool stop;

	[HideInInspector]
	public string current;

	[HideInInspector]
	public float Timer;

	private Transform AIController;

	private float stopDistance = 15f;

	private float random;

	private AIContoller ACScript;

	private float InstantiateTime;

	private void Awake()
	{
		layerMask = LayerMask.GetMask("Traffic");
		if ((bool)way_next.GetComponent<Crossing>())
		{
			current = "next";
		}
		else
		{
			current = "before";
		}
		InstanceTraffic instanceTraffic = Object.FindFirstObjectByType(typeof(InstanceTraffic)) as InstanceTraffic;
		ACScript = instanceTraffic.trafficPrefab.GetComponent<AIContoller>();
	}

	private int DensityCarController(TrafficDensity trafficDensity)
	{
		switch (trafficDensity)
		{
		case TrafficDensity.Low:
			return 50;
		case TrafficDensity.Normal:
			return 25;
		case TrafficDensity.High:
			return 15;
		case TrafficDensity.trafficJam:
			return 5;
		default:
			return 1000;
		}
	}

	public void InstantiateCar()
	{
		stopDistance = DensityCarController(trafficDensity);
		GameObject carForActiveTraffic = ACScript.GetCarForActiveTraffic();
		if (!carForActiveTraffic)
		{
			return;
		}
		InstantiateTime = Mathf.MoveTowards(InstantiateTime, 0f, Time.deltaTime);
		Collider[] array = Physics.OverlapSphere(base.transform.position, stopDistance, layerMask);
		bool flag = false;
		Collider[] array2 = array;
		foreach (Collider collider in array2)
		{
			if (collider.CompareTag("carcollider"))
			{
				flag = true;
				return;
			}
		}
		if (!flag && ACScript.numCars < ACScript.maxCars - 1)
		{
			GameObject gameObject = carForActiveTraffic;
			gameObject.transform.position = base.transform.position;
			gameObject.transform.rotation = Quaternion.identity;
			CarController component = gameObject.GetComponent<CarController>();
			component.carMode = CarMode.AICar;
			if (!onlyOneWay)
			{
				string text = "NO DISPONIBLE";
				random = Random.Range(-1f, 1f);
				if (random > 0f)
				{
					text = "NEXT";
					component.randomStart = 1;
					component.myStatue = "next";
					component.currentPoint = base.transform;
					component.next_point = base.transform.GetComponent<TrafficWaypoint>().way_before;
				}
				else
				{
					text = "BEFORE";
					component.randomStart = 2;
					component.myStatue = "before";
					component.currentPoint = base.transform;
					component.next_point = base.transform.GetComponent<TrafficWaypoint>().way_before;
				}
			}
			else
			{
				component.randomStart = 0;
				component.myStatue = "before";
				component.currentPoint = base.transform;
				component.next_point = base.transform.GetComponent<TrafficWaypoint>().way_next;
			}
			NGUITools.SetActive(gameObject, true);
		}
		InstantiateTime = Random.Range(1f, 2f);
	}

	private void OnDrawGizmos()
	{
		if (stop)
		{
			Gizmos.color = new Color(1f, 0f, 0f);
		}
		else if ((bool)way_before && (bool)way_next)
		{
			if ((bool)way_before.GetComponent<Crossing>() || (bool)way_next.GetComponent<Crossing>())
			{
				Gizmos.color = Color.green;
			}
			else
			{
				Gizmos.color = Color.yellow;
			}
		}
		else
		{
			Gizmos.color = Color.red;
		}
		Gizmos.DrawSphere(base.transform.position, 2.5f);
	}
}
