using UnityEngine;

public class CamSmoothLookAtFromPos : MonoBehaviour
{
	public Transform pos;

	public Transform target;

	public float damping = 6f;

	public float positionZ = -10f;

	public float positionY = 2f;

	private void LateUpdate()
	{
		if ((bool)target)
		{
			Vector3 vector = ((!target.GetComponent<Rigidbody>()) ? target.position : target.GetComponent<Rigidbody>().worldCenterOfMass);
			Quaternion to = Quaternion.LookRotation(vector - base.transform.position);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, to, Time.deltaTime * damping);
			if ((bool)pos)
			{
				base.transform.position = pos.transform.position + Vector3.up * positionY + base.transform.forward * positionZ;
			}
		}
	}

	private void Start()
	{
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
	}
}
