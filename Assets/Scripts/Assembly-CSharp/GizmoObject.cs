using UnityEngine;

public class GizmoObject : MonoBehaviour
{
	public Color GColor = Color.white;

	public float GSize = 1f;

	private void OnDrawGizmos()
	{
		Gizmos.color = GColor;
		Gizmos.DrawCube(base.transform.position, Vector3.one * GSize);
	}
}
