using UnityEngine;

public class MainCamera : MonoBehaviour
{
	public Transform target;

	public LayerMask lineOfSightMask = 0;

	public float smoothTime = 0.15f;

	public float smoothRotate = 0.1f;

	public float xSpeed = 150f;

	public float ySpeed = 150f;

	public float yMinLimit = -40f;

	public float yMaxLimit = 60f;

	public float runSpeed = 15f;

	public float runClamp = 4f;

	public float camera_Distance = 2.5f;

	public Vector3 targetOffset = Vector3.zero;

	public float FOV = 10f;

	public float FOVSpeed = 1f;

	private float distance = 10f;

	public float x;

	public float y;

	public float z;

	private float xSmooth;

	private float ySmooth;

	private float zSmooth;

	private float xSmooth2;

	private float ySmooth2;

	private float xVelocity;

	private float yVelocity;

	private float zVelocity;

	private float xSmooth2Velocity;

	private float ySmooth2Velocity;

	private Vector3 posVelocity = Vector3.zero;

	private float distanceVelocity;

	private Vector3 targetPos;

	private Quaternion rotation;

	private float curFOV;

	private float shiftFOV;

	public bool visibleMouseCursor = true;

	private void Start()
	{
		if (visibleMouseCursor)
		{
			UnityEngine.Cursor.visible = true;
		}
		else
		{
			UnityEngine.Cursor.visible = false;
		}
		Vector3 eulerAngles = base.transform.eulerAngles;
		x = eulerAngles.y;
		y = eulerAngles.x;
		curFOV = GetComponent<Camera>().fieldOfView;
		shiftFOV = curFOV + FOV;
	}

	private void FixedUpdate()
	{
		CharacterController component = target.GetComponent<CharacterController>();
		if (Mathf.Abs(component.velocity.magnitude) > 3f)
		{
			z = Mathf.PingPong(Time.time * runSpeed, runClamp) - runClamp / 2f;
			GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, shiftFOV, FOVSpeed * 0.05f);
		}
		else if (Mathf.Abs(component.velocity.magnitude) > 0.5f)
		{
			z = Mathf.SmoothStep(z, 0f, 0.2f);
			GetComponent<Camera>().fieldOfView = Mathf.Lerp(GetComponent<Camera>().fieldOfView, shiftFOV / 1.1f, FOVSpeed * 0.05f);
		}
	}

	private void LateUpdate()
	{
		if ((bool)target)
		{
			x += Input.GetAxis("Mouse X") * xSpeed * 0.02f;
		}
		y -= Input.GetAxis("Mouse Y") * ySpeed * 0.02f;
		y = ClampAngle(y, yMinLimit, yMaxLimit);
		distance = Mathf.SmoothDamp(distance, Mathf.Clamp(y / 30f, -100f, 0f) + camera_Distance, ref distanceVelocity, 0.2f);
		xSmooth = Mathf.SmoothDamp(xSmooth, x, ref xVelocity, smoothTime);
		ySmooth = Mathf.SmoothDamp(ySmooth, y, ref yVelocity, smoothTime);
		zSmooth = Mathf.SmoothDamp(zSmooth, z, ref zVelocity, smoothTime);
		xSmooth2 = Mathf.SmoothDamp(xSmooth2, Input.GetAxis("Mouse X") / 5f, ref xSmooth2Velocity, 0.1f);
		ySmooth2 = Mathf.SmoothDamp(ySmooth2, Input.GetAxis("Mouse Y") / 5f, ref ySmooth2Velocity, 0.1f);
		rotation = Quaternion.Euler(ySmooth, xSmooth, zSmooth);
		targetPos = Vector3.SmoothDamp(targetPos, base.transform.TransformDirection(Mathf.Clamp(xSmooth2, -0.4f, 0.4f), 0f, 0f) + new Vector3(0f, targetOffset.y - Mathf.Clamp(ySmooth2, -0.2f, 0.2f)), ref posVelocity, smoothRotate);
		Vector3 vector = rotation * -Vector3.forward;
		float num = AdjustLineOfSight(targetPos + target.position, vector);
		base.transform.rotation = rotation;
		base.transform.position = targetPos + target.position + base.transform.TransformDirection(targetOffset.x, 0f, 0.1f) + vector * num;
	}

	private float AdjustLineOfSight(Vector3 target, Vector3 direction)
	{
		RaycastHit hitInfo;
		if (Physics.Raycast(target, direction, out hitInfo, distance, lineOfSightMask.value))
		{
			return hitInfo.distance;
		}
		return distance;
	}

	private static float ClampAngle(float angle, float min, float max)
	{
		if (angle < -360f)
		{
			angle += 360f;
		}
		if (angle > 360f)
		{
			angle -= 360f;
		}
		return Mathf.Clamp(angle, min, max);
	}
}
