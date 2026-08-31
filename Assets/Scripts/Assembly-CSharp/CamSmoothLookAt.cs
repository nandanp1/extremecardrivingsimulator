using UnityEngine;

public class CamSmoothLookAt : MonoBehaviour
{
	public Transform target;

	public float damping = 6f;

	public float minFov = 10f;

	public float maxFov = 60f;

	public float fovSensitivity = 20f;

	public float fovDamping = 4f;

	public float moveSpeed = 2f;

	public float moveDamping = 5f;

	private Vector3 m_Pos;

	private float m_fov;

	private float m_savedFov;

	private Camera m_Camera;

	private void Start()
	{
		m_Pos = base.transform.position;
		m_Camera = GetComponent<Camera>();
		if ((bool)m_Camera)
		{
			m_fov = m_Camera.fieldOfView;
			m_savedFov = m_Camera.fieldOfView;
		}
		if ((bool)GetComponent<Rigidbody>())
		{
			GetComponent<Rigidbody>().freezeRotation = true;
		}
	}

	private void OnEnable()
	{
		m_Pos = base.transform.position;
		if ((bool)m_Camera)
		{
			m_fov = m_Camera.fieldOfView;
		}
	}

	private void OnDisable()
	{
		if ((bool)m_Camera)
		{
			m_Camera.fieldOfView = m_savedFov;
		}
	}

	private void LateUpdate()
	{
		if ((bool)target)
		{
			Vector3 vector = ((!target.GetComponent<Rigidbody>()) ? target.position : target.GetComponent<Rigidbody>().worldCenterOfMass);
			float num = moveSpeed * Time.deltaTime;
			m_Pos += Input.GetAxis("Sideways") * base.transform.right * num;
			m_Pos += Input.GetAxis("Upwards") * base.transform.up * num;
			Vector3 pos = m_Pos;
			float axis = Input.GetAxis("Forwards");
			Vector3 vector2 = new Vector3(base.transform.forward.x, 0f, base.transform.forward.z);
			m_Pos = pos + axis * vector2.normalized * num;
			base.transform.position = Vector3.Lerp(base.transform.position, m_Pos, moveDamping * Time.deltaTime);
			Quaternion to = Quaternion.LookRotation(vector - base.transform.position);
			base.transform.rotation = Quaternion.Slerp(base.transform.rotation, to, Time.deltaTime * damping);
			if ((bool)m_Camera)
			{
				m_fov -= Input.GetAxis("Mouse ScrollWheel") * fovSensitivity;
				m_fov = Mathf.Clamp(m_fov, minFov, maxFov);
				m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, m_fov, fovDamping * Time.deltaTime);
			}
		}
	}
}
