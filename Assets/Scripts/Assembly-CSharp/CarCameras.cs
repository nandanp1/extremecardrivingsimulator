using UnityEngine;

public class CarCameras : MonoBehaviour
{
	public bool showFixedCams;

	public int startupFixedCam;

	public Camera[] FixedCameras;

	public Camera MirrorLeft;

	public Camera MirrorRight;

	public Camera MirrorRear;

	public Transform CameraLookAtPoint;

	public Transform DriverFront;

	public float viewDistance = 10f;

	public float viewHeight = 3.5f;

	public float viewDamping = 3f;

	public float viewMinDistance = 3.8f;

	public float viewMinHeight;

	private int m_currentFixedCam;

	private Vector3 m_DriverViewAngles;

	public Vector3 getDriverViewAngles()
	{
		return m_DriverViewAngles;
	}

	private void Start()
	{
		m_currentFixedCam = startupFixedCam;
		if (m_currentFixedCam >= FixedCameras.Length)
		{
			m_currentFixedCam = -1;
		}
		for (int i = 0; i < FixedCameras.Length; i++)
		{
			FixedCameras[i].farClipPlane = MySingleton<Manager>.Instance.gameLogic.farCamera;
			FixedCameras[i].gameObject.GetComponent<InfoArrowPointer>().childArrowPointer.SetActive(false);
			FixedCameras[i].enabled = false;
		}
		if ((bool)MirrorLeft)
		{
			MirrorLeft.enabled = false;
		}
		if ((bool)MirrorRight)
		{
			MirrorRight.enabled = false;
		}
		if ((bool)MirrorRear)
		{
			MirrorRear.enabled = false;
		}
		if ((bool)DriverFront)
		{
			CamFreeView component = DriverFront.GetComponent<CamFreeView>();
			if ((bool)component)
			{
				component.enabled = false;
			}
			m_DriverViewAngles = DriverFront.localEulerAngles;
		}
	}

	public void Next()
	{
		if (FixedCameras.Length == 0)
		{
			return;
		}
		if (m_currentFixedCam >= 0)
		{
			FixedCameras[m_currentFixedCam].gameObject.GetComponent<InfoArrowPointer>().childArrowPointer.SetActive(false);
			FixedCameras[m_currentFixedCam].enabled = false;
			m_currentFixedCam++;
			if (m_currentFixedCam < FixedCameras.Length)
			{
				if (MySingleton<Manager>.Instance.gameLogic.challangeInProgress && MySingleton<Manager>.Instance.challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Radar)
				{
					FixedCameras[m_currentFixedCam].gameObject.GetComponent<InfoArrowPointer>().childArrowPointer.SetActive(true && showFixedCams);
				}
				FixedCameras[m_currentFixedCam].enabled = true && showFixedCams;
			}
			else
			{
				m_currentFixedCam = 0;
			}
		}
		else
		{
			m_currentFixedCam = 0;
			if (MySingleton<Manager>.Instance.gameLogic.challangeInProgress && MySingleton<Manager>.Instance.challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Radar)
			{
				FixedCameras[m_currentFixedCam].gameObject.GetComponent<InfoArrowPointer>().childArrowPointer.SetActive(true && showFixedCams);
			}
			FixedCameras[m_currentFixedCam].enabled = true && showFixedCams;
		}
		MySingleton<Manager>.Instance.gameLogic.currentCamera = FixedCameras[m_currentFixedCam];
	}

	private void Update()
	{
		if (m_currentFixedCam >= 0)
		{
			if (showFixedCams && !FixedCameras[m_currentFixedCam].enabled && MySingleton<Manager>.Instance.gameLogic.challangeInProgress && MySingleton<Manager>.Instance.challangeModeManager.GetChallangeInActivationZone().key == ChallangeKey.Radar)
			{
				FixedCameras[m_currentFixedCam].gameObject.GetComponent<InfoArrowPointer>().childArrowPointer.SetActive(true);
			}
			FixedCameras[m_currentFixedCam].enabled = true;
			if (!showFixedCams && FixedCameras[m_currentFixedCam].enabled)
			{
				FixedCameras[m_currentFixedCam].enabled = false;
			}
		}
	}

	public Camera Current()
	{
		if (FixedCameras.Length == 0)
		{
			return null;
		}
		return FixedCameras[m_currentFixedCam];
	}
}
