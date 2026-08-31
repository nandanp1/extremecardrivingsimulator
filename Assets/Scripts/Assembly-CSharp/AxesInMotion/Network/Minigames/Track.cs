using UnityEngine;

namespace AxesInMotion.Network.Minigames
{
	public class Track : MonoBehaviour
	{
		public Transform[] spawns;

		public Waypoint[] waypoints;

		public Goal[] goals;

		public RacingMiniGameLogic raceLogic;

		[ContextMenu("Load Track Data")]
		public void LoadTrackData()
		{
			Transform transform = base.transform.Find("SpawnPoints");
			if ((bool)transform)
			{
				spawns = new Transform[transform.childCount];
				for (int i = 0; i < transform.childCount; i++)
				{
					spawns[i] = transform.GetChild(i);
				}
			}
			transform = base.transform.Find("Waypoints");
			if ((bool)transform)
			{
				foreach (Transform item in transform)
				{
					item.gameObject.SetActive(true);
				}
				waypoints = transform.GetComponentsInChildren<Waypoint>();
				for (int j = 0; j < waypoints.Length; j++)
				{
					waypoints[j].track = this;
					waypoints[j].gameObject.SetActive(false);
					waypoints[j].nextWaypoint = null;
					if (j > 0)
					{
						waypoints[j - 1].nextWaypoint = waypoints[j];
					}
				}
			}
			transform = base.transform.Find("Goals");
			if (!transform)
			{
				return;
			}
			foreach (Transform item2 in transform)
			{
				item2.gameObject.SetActive(true);
			}
			goals = transform.GetComponentsInChildren<Goal>();
			for (int k = 0; k < goals.Length; k++)
			{
				goals[k].track = this;
				goals[k].gameObject.SetActive(false);
				waypoints[waypoints.Length - 1].nextWaypoint = goals[k];
			}
		}

		public void PrepareTrack()
		{
			raceLogic.spawnPoints = spawns;
			Waypoint[] array = waypoints;
			foreach (Waypoint waypoint in array)
			{
				waypoint.gameObject.SetActive(false);
			}
			Goal[] array2 = goals;
			foreach (Goal goal in array2)
			{
				goal.gameObject.SetActive(false);
			}
		}

		public void StartTrack()
		{
			waypoints[0].gameObject.SetActive(true);
			SetArrowTarget(waypoints[0].transform);
		}

		public void DisableTrack()
		{
			Waypoint[] array = waypoints;
			foreach (Waypoint waypoint in array)
			{
				waypoint.gameObject.SetActive(false);
			}
			Goal[] array2 = goals;
			foreach (Goal goal in array2)
			{
				goal.gameObject.SetActive(false);
			}
			SetArrowTarget(null);
		}

		public void SetArrowTarget(Transform target)
		{
			raceLogic.SetTargetForWaypointArrow(target);
		}

		public void GoalReached()
		{
			raceLogic.GoalReached();
		}
	}
}
