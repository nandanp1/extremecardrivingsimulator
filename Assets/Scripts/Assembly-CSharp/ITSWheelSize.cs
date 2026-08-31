using UnityEngine;

public class ITSWheelSize : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawWireSphere(base.transform.position, base.transform.GetComponent<WheelCollider>().radius);
	}
}
