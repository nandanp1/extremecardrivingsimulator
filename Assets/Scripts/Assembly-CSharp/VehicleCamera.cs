using UnityEngine;

public class VehicleCamera : MonoBehaviour
{
	public Transform target;

	public float smooth = 0.3f;

	public float xSmooth = 0.5f;

	public float distance = 5f;

	public float height = 1.2f;

	public float xAngle = 10f;

	private float yVelocity;

	private float xVelocity;

	private Rigidbody targetRigidbody;

	private void Start()
	{
		ChangeTarget(MySingleton<Manager>.Instance.gameLogic.currentCar);
	}

	private void Update()
	{
		if (Time.timeScale == 1f)
		{
			float num = target.eulerAngles.y;
			if (targetRigidbody != null && targetRigidbody.GetComponent<Rigidbody>().linearVelocity.sqrMagnitude > 1f)
			{
				Vector3 velocity = target.GetComponent<Rigidbody>().linearVelocity;
				velocity.y = 0f;
				float num2 = Vector3.Angle(target.forward, velocity) * Mathf.Sign(Vector3.Dot(target.forward, velocity));
				num += num2;
			}
			float y = Mathf.SmoothDampAngle(base.transform.eulerAngles.y, num, ref yVelocity, smooth);
			float x = xAngle;
			if (xSmooth > 0f)
			{
				x = Mathf.SmoothDampAngle(base.transform.eulerAngles.x, target.eulerAngles.x + xAngle, ref xVelocity, xSmooth);
				x = Mathf.Clamp(x, xAngle / 2f, xAngle * 1.5f);
			}
			Vector3 position = target.position;
			position += Quaternion.Euler(x, y, 0f) * new Vector3(0f, 0f, 0f - distance);
			base.transform.eulerAngles = new Vector3(x, y, 0f);
			Vector3 vector = base.transform.rotation * -Vector3.forward;
			Vector3 vector2 = target.position + Vector3.up * height;
			base.transform.position = vector2 + vector * distance;
		}
	}

	private void ApplyValuesFromCar(VehicleCameraValues camValues)
	{
		if (camValues != null)
		{
			height = camValues.height;
			distance = camValues.distance;
			xAngle = camValues.xAngle;
		}
	}

	public void ChangeTarget(Transform t)
	{
		target = t;
		if ((bool)target)
		{
			targetRigidbody = target.GetComponent<Rigidbody>();
			VehicleCameraValues component = target.GetComponent<VehicleCameraValues>();
			ApplyValuesFromCar(component);
		}
	}
}
