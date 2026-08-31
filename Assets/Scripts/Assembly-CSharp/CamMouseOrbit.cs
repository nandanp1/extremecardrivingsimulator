using UnityEngine;

public class CamMouseOrbit : MonoBehaviour
{
	public Transform target;

	public float distance = 10f;

	public float xSpeed = 5f;

	public float ySpeed = 2.5f;

	public float distSpeed = 10f;

	public float yMinLimit = -20f;

	public float yMaxLimit = 80f;

	public float distMinLimit = 5f;

	public float distMaxLimit = 50f;

	public float orbitDamping = 4f;

	public float distDamping = 4f;

	private float x;

	private float y;

	private float dist;

	private void Start()
	{
		dist = distance;
		Vector3 eulerAngles = base.transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
	}

	private void LateUpdate()
	{
		if ((bool)target)
		{
			x += Input.GetAxis("Mouse X") * xSpeed;
			y -= Input.GetAxis("Mouse Y") * ySpeed;
			distance -= Input.GetAxis("Mouse ScrollWheel") * distSpeed;
			y = ClampAngle(y, yMinLimit, yMaxLimit);
			distance = Mathf.Clamp(distance, distMinLimit, distMaxLimit);
			dist = Mathf.Lerp(dist, distance, distDamping * Time.deltaTime);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, Quaternion.Euler(y, x, 0f), Time.deltaTime * orbitDamping);
			base.transform.position = base.transform.rotation * new Vector3(0f, 0f, 0f - dist) + target.position;
		}
	}

	private float ClampAngle(float a, float min, float max)
	{
		while (max < min)
		{
			max += 360f;
		}
		while (a > max)
		{
			a -= 360f;
		}
		while (a < min)
		{
			a += 360f;
		}
		if (a > max)
		{
			if (a - (max + min) * 0.5f < 180f)
			{
				return max;
			}
			return min;
		}
		return a;
	}
}
