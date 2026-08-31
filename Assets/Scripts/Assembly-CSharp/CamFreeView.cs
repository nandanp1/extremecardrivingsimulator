using UnityEngine;

public class CamFreeView : MonoBehaviour
{
	public float sensitivityH = 5f;

	public float sensitivityV = 5f;

	public float dampingH = 4f;

	public float dampingV = 4f;

	public float moveSpeed = 2f;

	public float moveDamping = 5f;

	public float minV = -30f;

	public float maxV = 50f;

	public float minH = -180f;

	public float maxH = 180f;

	public float minFov = 10f;

	public float maxFov = 60f;

	public float fovSensitivity = 20f;

	public float fovDamping = 4f;

	private float m_rotH;

	private float m_rotV;

	private Vector3 m_Pos = new Vector3(0f, 0f, 0f);

	private float m_fov;

	private float m_savedFov;

	private Camera m_Camera;

	private void Start()
	{
		SetLocalEulerAngles(base.transform.localEulerAngles);
		m_Camera = GetComponent<Camera>();
		if ((bool)m_Camera)
		{
			m_fov = m_Camera.fieldOfView;
			m_savedFov = m_Camera.fieldOfView;
		}
	}

	private void OnEnable()
	{
		SetLocalEulerAngles(base.transform.localEulerAngles);
		m_Pos = base.transform.localPosition;
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

	public void SetLocalEulerAngles(Vector3 Angles)
	{
		Vector3 localEulerAngles = Angles;
		m_rotH = localEulerAngles.y;
		m_rotV = localEulerAngles.x;
		base.transform.localEulerAngles = localEulerAngles;
	}

	private void LateUpdate()
	{
		m_rotH += Input.GetAxis("Mouse X") * sensitivityH;
		m_rotV -= Input.GetAxis("Mouse Y") * sensitivityV;
		m_rotH = ClampAngle(m_rotH, minH, maxH);
		m_rotV = ClampAngle(m_rotV, minV, maxV);
		Vector3 localEulerAngles = base.transform.localEulerAngles;
		localEulerAngles.y = Mathf.LerpAngle(base.transform.localEulerAngles.y, m_rotH, dampingH * Time.deltaTime);
		localEulerAngles.x = Mathf.LerpAngle(base.transform.localEulerAngles.x, m_rotV, dampingV * Time.deltaTime);
		base.transform.localEulerAngles = localEulerAngles;
		if ((bool)m_Camera)
		{
			m_fov -= Input.GetAxis("Mouse ScrollWheel") * fovSensitivity;
			m_fov = Mathf.Clamp(m_fov, minFov, maxFov);
			m_Camera.fieldOfView = Mathf.Lerp(m_Camera.fieldOfView, m_fov, fovDamping * Time.deltaTime);
		}
		float num = moveSpeed * Time.deltaTime;
		m_Pos += Input.GetAxis("Sideways") * base.transform.right * num;
		m_Pos += Input.GetAxis("Upwards") * base.transform.up * num;
		Vector3 pos = m_Pos;
		float axis = Input.GetAxis("Forwards");
		Vector3 vector = new Vector3(base.transform.forward.x, 0f, base.transform.forward.z);
		m_Pos = pos + axis * vector.normalized * num;
		base.transform.localPosition = Vector3.Lerp(base.transform.localPosition, m_Pos, moveDamping * Time.deltaTime);
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
