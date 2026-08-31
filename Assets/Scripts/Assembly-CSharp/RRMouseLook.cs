using UnityEngine;

public class RRMouseLook : MonoBehaviour
{
	public enum RotationAxes
	{
		MouseXAndY = 0,
		MouseX = 1,
		MouseY = 2
	}

	public RotationAxes axes;

	public float sensitivityX = 15f;

	public float sensitivityY = 15f;

	public float minimumX = -360f;

	public float maximumX = 360f;

	public float minimumY = -60f;

	public float maximumY = 60f;

	public float smoothSpeed = 20f;

	private float rotationX;

	private float smoothRotationX;

	private float rotationY;

	private float smoothRotationY;

	private void Update()
	{
		if (axes == RotationAxes.MouseXAndY)
		{
			rotationX = base.transform.localEulerAngles.y + Input.GetAxis("Mouse X") * sensitivityX;
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
			smoothRotationX += (rotationX - smoothRotationX) * smoothSpeed * Time.smoothDeltaTime;
			smoothRotationY += (rotationY - smoothRotationY) * smoothSpeed * Time.smoothDeltaTime;
			base.transform.localEulerAngles = new Vector3(0f - rotationY, rotationX, 0f);
		}
		else if (axes == RotationAxes.MouseX)
		{
			base.transform.Rotate(0f, Input.GetAxis("Mouse X") * sensitivityX, 0f);
		}
		else
		{
			rotationY += Input.GetAxis("Mouse Y") * sensitivityY;
			rotationY = Mathf.Clamp(rotationY, minimumY, maximumY);
			base.transform.localEulerAngles = new Vector3(0f - rotationY, base.transform.localEulerAngles.y, 0f);
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
