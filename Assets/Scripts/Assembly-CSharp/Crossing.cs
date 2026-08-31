using UnityEngine;

public class Crossing : MonoBehaviour
{
	public enum WayMode
	{
		Side = 1,
		Center = 2
	}

	private const float TIME_FOR_UPDATE_LOGIC_SEMAPHORE = 0.35f;

	public int ways = 1;

	public bool trafficControl;

	public Transform[] TrafficWays1;

	public Transform[] TrafficWays2;

	public Transform[] TrafficWays3;

	public int traffic1Number;

	public int traffic2Number;

	public int traffic3Number;

	public float TrafficTime;

	public Transform way1;

	public Transform way2;

	public Transform way3;

	public Transform way4;

	public GameObject aiCar;

	public int TrafficNumber = 1;

	public float TrafficTimer;

	public float size = 1f;

	public float stopDistance = 4f;

	public bool show_distance;

	public bool stop;

	private Transform player;

	private void Awake()
	{
		if (player == null)
		{
			player = base.transform.Find("/Player");
		}
	}

	private void Start()
	{
		if (trafficControl)
		{
			TrafficTimer = TrafficTime;
			TrafficController();
		}
	}

	private void OnEnable()
	{
		if (trafficControl)
		{
			InvokeRepeating("UpdateTrafficConttrol", 0f, 0.35f);
		}
	}

	private void OnDisable()
	{
		CancelInvoke("UpdateTrafficConttrol");
	}

	private void UpdateTrafficConttrol()
	{
		if (!trafficControl)
		{
			return;
		}
		TrafficTimer = Mathf.MoveTowards(TrafficTimer, 0f, 0.35f);
		if (TrafficTimer == 0f)
		{
			TrafficNumber++;
			if (TrafficNumber > 2 && TrafficWays3.Length == 0)
			{
				TrafficNumber = 1;
			}
			else if (TrafficNumber > 3 && TrafficWays3.Length != 0)
			{
				TrafficNumber = 1;
			}
			TrafficTimer = TrafficTime;
		}
		TrafficController();
	}

	private void TrafficController()
	{
		Transform[] trafficWays = TrafficWays1;
		foreach (Transform transform in trafficWays)
		{
			transform.GetComponent<TrafficWaypoint>().stop = ((TrafficNumber != 1 || stop) ? true : false);
		}
		Transform[] trafficWays2 = TrafficWays2;
		foreach (Transform transform2 in trafficWays2)
		{
			transform2.GetComponent<TrafficWaypoint>().stop = ((TrafficNumber != 2 || stop) ? true : false);
		}
		Transform[] trafficWays3 = TrafficWays3;
		foreach (Transform transform3 in trafficWays3)
		{
			transform3.GetComponent<TrafficWaypoint>().stop = ((TrafficNumber != 3 || stop) ? true : false);
		}
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawCube(base.transform.position, Vector3.one * size);
		Gizmos.color = Color.blue;
		if ((bool)way1)
		{
			Gizmos.DrawLine(base.transform.position, way1.position);
		}
		if ((bool)way2 && ways >= 2)
		{
			Gizmos.DrawLine(base.transform.position, way2.position);
		}
		if ((bool)way3 && ways >= 3)
		{
			Gizmos.DrawLine(base.transform.position, way3.position);
		}
		if ((bool)way4 && ways == 4)
		{
			Gizmos.DrawLine(base.transform.position, way4.position);
		}
	}

	private void OnDrawGizmosSelected()
	{
		Gizmos.color = new Color(0.5f, 1f, 0.5f);
		Gizmos.DrawWireSphere(base.transform.position, stopDistance);
	}
}
