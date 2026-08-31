using UnityEngine;

namespace AxesInMotion.Network.Minigames
{
	[RequireComponent(typeof(Collider))]
	public class Waypoint : MonoBehaviour
	{
		public Waypoint nextWaypoint;

		public Track track;

		public bool isGoal
		{
			get
			{
				return this is Goal;
			}
		}

		private void OnTriggerEnter(Collider c)
		{
			if ((bool)nextWaypoint)
			{
				nextWaypoint.gameObject.SetActive(true);
				track.SetArrowTarget(nextWaypoint.transform);
			}
			base.gameObject.SetActive(false);
			if (isGoal)
			{
				track.GoalReached();
				track.SetArrowTarget(null);
			}
		}
	}
}
