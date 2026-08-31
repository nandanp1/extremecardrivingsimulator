using UnityEngine;

public class CamRotate : MonoBehaviour
{
	public Transform target;

	public float distance = 3f;

	public float speed = 0.5f;

	private void Start()
	{
	}

	private void LateUpdate()
	{
		base.transform.position = new Vector3(target.position.x + Mathf.Sin(Time.time) * distance, base.transform.position.y, target.position.z + Mathf.Cos(Time.time) * distance);
		base.transform.LookAt(target.position);
	}
}
