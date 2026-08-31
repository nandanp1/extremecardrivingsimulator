using UnityEngine;

[RequireComponent(typeof(BoxCollider))]
public class DrawGizmo : MonoBehaviour
{
	private Color color = Color.white;

	public bool wiredgizmo;

	private BoxCollider _boxCollider;

	private void OnDrawGizmos()
	{
		Gizmos.matrix = base.transform.localToWorldMatrix;
		Gizmos.color = color;
		if (wiredgizmo)
		{
			Gizmos.DrawWireCube(Vector3.zero, Vector3.one);
		}
		else
		{
			Gizmos.DrawCube(Vector3.zero, Vector3.one);
		}
	}
}
