using UnityEngine;

public class CamFixTo : MonoBehaviour
{
	public Transform Pos;

	private void LateUpdate()
	{
		base.transform.position = Pos.transform.position;
		base.transform.rotation = Pos.transform.rotation;
	}
}
