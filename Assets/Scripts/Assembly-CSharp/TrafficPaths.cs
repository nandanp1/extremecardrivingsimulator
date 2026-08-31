using UnityEngine;

public class TrafficPaths : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		int num = 1;
		Gizmos.color = new Color(1f, 0.5f, 0f);
		foreach (Transform item in base.transform)
		{
			if (!item.GetComponent<TrafficWaypoint>())
			{
				item.gameObject.AddComponent<TrafficWaypoint>();
			}
			if (item.name != num.ToString())
			{
				item.name = num.ToString();
			}
			num++;
			Transform transform2 = base.transform.Find(num.ToString());
			Transform transform3 = base.transform.Find((num - 2).ToString());
			if ((bool)transform2)
			{
				Gizmos.DrawLine(item.position, transform2.position);
				item.GetComponent<TrafficWaypoint>().way_next = transform2;
			}
			if ((bool)transform3)
			{
				item.GetComponent<TrafficWaypoint>().way_before = transform3;
			}
		}
	}
}
