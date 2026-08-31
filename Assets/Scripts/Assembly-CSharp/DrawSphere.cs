using UnityEngine;

public class DrawSphere : MonoBehaviour
{
	private void OnDrawGizmos()
	{
		Gizmos.DrawSphere(base.transform.position, 0.05f);
	}
}
