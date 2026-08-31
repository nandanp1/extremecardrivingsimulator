using UnityEngine;

public class RotateAroundAxis : MonoBehaviour
{
	public Transform pivot;

	public float angularSpeed = 1f;

	private float defaultDistance;

	private void Start()
	{
		defaultDistance = (pivot.position - base.transform.position).magnitude;
	}

	private void Update()
	{
		base.transform.position = pivot.position;
		base.transform.Rotate(pivot.up, angularSpeed * Time.deltaTime, Space.World);
		base.transform.position -= base.transform.forward * defaultDistance;
	}
}
