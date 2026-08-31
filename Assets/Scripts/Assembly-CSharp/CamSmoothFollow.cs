using UnityEngine;

public class CamSmoothFollow : MonoBehaviour
{
	public Transform target;

	public float distance = 10f;

	public float height = 5f;

	public float targetHeightRatio = 0.5f;

	public float heightDamping = 2f;

	public float rotationDamping = 3f;

	public bool followVelocity = true;

	public float velocityDamping = 5f;

	private Vector3 lastPos = Vector3.zero;

	private Vector3 currentVelocity = Vector3.zero;

	private float wantedRotationAngle;

	[HideInInspector]
	public bool reset = true;

	private void LateUpdate()
	{
		if ((bool)target)
		{
			if (reset)
			{
				lastPos = target.position;
				wantedRotationAngle = target.eulerAngles.y;
				currentVelocity = target.forward * 2f;
				reset = false;
			}
			Vector3 to = (target.position - lastPos) / Time.deltaTime;
			to.y = 0f;
			if (to.sqrMagnitude > 5f)
			{
				currentVelocity = Vector3.Lerp(currentVelocity, to, velocityDamping * Time.deltaTime);
				wantedRotationAngle = Mathf.Atan2(currentVelocity.x, currentVelocity.z) * 57.29578f;
			}
			lastPos = target.position;
			if (!followVelocity)
			{
				wantedRotationAngle = target.transform.eulerAngles.y;
			}
			float to2 = target.position.y + height;
			float y = base.transform.eulerAngles.y;
			float y2 = base.transform.position.y;
			y = Mathf.LerpAngle(y, wantedRotationAngle, 0.1f);
			y2 = Mathf.Lerp(y2, to2, 0.1f);
			Quaternion quaternion = Quaternion.Euler(0f, y, 0f);
			base.transform.position = target.position;
			base.transform.position -= quaternion * Vector3.forward * distance;
			Vector3 position = base.transform.position;
			position.y = y2;
			base.transform.position = position;
			if ((bool)target.GetComponent<Rigidbody>())
			{
				Vector3 position2 = Vector3.Scale(target.GetComponent<Rigidbody>().centerOfMass, new Vector3(1f / target.transform.localScale.x, 1f / target.transform.localScale.y, 1f / target.transform.localScale.z));
				position2 = target.transform.TransformPoint(position2);
				base.transform.LookAt(position2 + Vector3.up * height * targetHeightRatio);
			}
			else
			{
				base.transform.LookAt(target.position + Vector3.up * height * targetHeightRatio);
			}
		}
	}
}
