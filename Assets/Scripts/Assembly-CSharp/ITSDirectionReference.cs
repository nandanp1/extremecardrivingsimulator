using UnityEngine;

public class ITSDirectionReference : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Gizmos.color = Color.yellow;
		Gizmos.DrawLine(base.transform.position + base.transform.forward, base.transform.position + base.transform.forward + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position + base.transform.forward * 2f, base.transform.position + base.transform.forward * 2f + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position + base.transform.forward * 3f, base.transform.position + base.transform.forward * 3f + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position + base.transform.forward * 4f, base.transform.position + base.transform.forward * 4f + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position + base.transform.forward * 5f, base.transform.position + base.transform.forward * 5f + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position + base.transform.forward * 6f, base.transform.position + base.transform.forward * 6f + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position - base.transform.forward, base.transform.position - base.transform.forward + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position - base.transform.forward * 2f, base.transform.position - base.transform.forward * 2f + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position - base.transform.forward * 3f, base.transform.position - base.transform.forward * 3f + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position - base.transform.forward * 4f, base.transform.position - base.transform.forward * 4f + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position - base.transform.forward * 5f, base.transform.position - base.transform.forward * 5f + base.transform.right * 2f);
		Gizmos.DrawLine(base.transform.position - base.transform.forward * 6f, base.transform.position - base.transform.forward * 6f + base.transform.right * 2f);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(base.transform.position - base.transform.forward * 2f, base.transform.forward * 2f);
		Gizmos.DrawLine(base.transform.forward * 2f, -base.transform.up * 2f + base.transform.forward * 2f);
		Gizmos.DrawLine(-base.transform.up * 2f + base.transform.forward * 2f, base.transform.forward * 5f + base.transform.up);
		Gizmos.DrawLine(base.transform.forward * 5f + base.transform.up, base.transform.up * 4f + base.transform.forward * 2f);
		Gizmos.DrawLine(base.transform.up * 4f + base.transform.forward * 2f, base.transform.up * 2f + base.transform.forward * 2f);
		Gizmos.DrawLine(base.transform.up * 2f + base.transform.forward * 2f, base.transform.position + base.transform.up * 2f - base.transform.forward * 2f);
		Gizmos.DrawLine(base.transform.position + base.transform.up * 2f - base.transform.forward * 2f, base.transform.position - base.transform.forward * 2f);
		Gizmos.color = Color.red;
		Gizmos.DrawLine(base.transform.position, base.transform.right * 15f);
		Gizmos.DrawLine(base.transform.position, -base.transform.right * 15f);
		Gizmos.color = Color.green;
		Gizmos.DrawLine(base.transform.position, base.transform.up * 15f);
		Gizmos.DrawLine(base.transform.position, -base.transform.up * 15f);
		Gizmos.color = Color.blue;
		Gizmos.DrawLine(base.transform.position, base.transform.forward * 15f);
		Gizmos.DrawLine(base.transform.position, -base.transform.forward * 15f);
	}
}
