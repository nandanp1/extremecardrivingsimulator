using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
	public Transform Target;

	public Transform Target2;

	public Camera MapCamera;

	public int DefaultCamera;

	public bool showMirrors = true;

	public RawImage MirrorLeftTex;

	public RawImage MirrorRightTex;

	public RawImage MirrorRearTex;

	public bool enableImageEffects = true;

	public MonoBehaviour[] ImageEffects;

	private MonoBehaviour[] m_cameraScript;

	private int m_currCamera;

	private int m_numCameras = 6;

	private CamFixTo m_scrFixTo;

	private CamSmoothFollow m_scrSmoothFollow;

	private CamMouseOrbit m_scrMouseOrbit;

	private CamSmoothLookAt m_scrSmoothLookAt;

	private CamFreeView m_scrFreeView;

	private CamSmoothLookAtFromPos m_scrSmoothLookAtFromPos;

	private CamFreeView m_scrDriverFreeView;

	private CamSmoothFollow m_scrMapFollow;

	private Transform m_lastTarget;

	private bool m_lastShowMirrors;

	private bool m_lastImageEffects;

	private CarCameras m_targetCams;

	private void UpdateTarget()
	{
		if ((bool)m_targetCams)
		{
			if ((bool)m_targetCams.MirrorLeft)
			{
				m_targetCams.MirrorLeft.enabled = false;
			}
			if ((bool)m_targetCams.MirrorRight)
			{
				m_targetCams.MirrorRight.enabled = false;
			}
			if ((bool)m_targetCams.MirrorRear)
			{
				m_targetCams.MirrorRear.enabled = false;
			}
			if ((bool)m_scrDriverFreeView)
			{
				m_scrDriverFreeView.enabled = false;
			}
		}
		m_targetCams = Target.GetComponent<CarCameras>();
		if ((bool)m_targetCams.DriverFront)
		{
			m_scrDriverFreeView = m_targetCams.DriverFront.GetComponent<CamFreeView>();
		}
		else
		{
			m_scrDriverFreeView = null;
		}
		m_scrFixTo.Pos = m_targetCams.DriverFront;
		m_scrSmoothFollow.target = m_targetCams.CameraLookAtPoint;
		m_scrSmoothFollow.distance = m_targetCams.viewDistance;
		m_scrSmoothFollow.height = m_targetCams.viewHeight;
		m_scrSmoothFollow.rotationDamping = m_targetCams.viewDamping;
		m_scrSmoothFollow.reset = true;
		m_scrMouseOrbit.target = m_targetCams.CameraLookAtPoint;
		m_scrMouseOrbit.distance = m_targetCams.viewDistance;
		m_scrMouseOrbit.distMinLimit = m_targetCams.viewMinDistance;
		m_scrMouseOrbit.yMinLimit = m_targetCams.viewMinHeight;
		m_scrSmoothLookAt.target = m_targetCams.CameraLookAtPoint;
		if ((bool)m_scrMapFollow)
		{
			m_scrMapFollow.target = m_targetCams.CameraLookAtPoint;
		}
		if ((bool)Target2)
		{
			m_scrSmoothLookAtFromPos.pos = m_targetCams.CameraLookAtPoint;
			m_scrSmoothLookAtFromPos.positionZ = 0f - m_targetCams.viewDistance;
			m_scrSmoothLookAtFromPos.positionY = m_targetCams.viewHeight / 2f;
			CarCameras component = Target2.GetComponent<CarCameras>();
			if ((bool)component)
			{
				m_scrSmoothLookAtFromPos.target = component.CameraLookAtPoint;
			}
			else
			{
				m_scrSmoothLookAtFromPos.target = Target2;
			}
		}
		else if (m_currCamera == m_numCameras - 1)
		{
			SwitchTo(DefaultCamera);
		}
		UpdateMirrors();
	}

	private void ClearMirrorTexture(Camera Cam)
	{
		CameraClearFlags clearFlags = Cam.clearFlags;
		Color backgroundColor = Cam.backgroundColor;
		Rect rect = Cam.rect;
		int cullingMask = Cam.cullingMask;
		Cam.clearFlags = CameraClearFlags.Color;
		Cam.backgroundColor = new Color(1f, 1f, 1f, 1f);
		Cam.rect = new Rect(0f, 0f, 1f, 1f);
		Cam.cullingMask = 0;
		Cam.Render();
		Cam.clearFlags = clearFlags;
		Cam.backgroundColor = backgroundColor;
		Cam.rect = rect;
		Cam.cullingMask = cullingMask;
	}

	private void UpdateMirrors()
	{
		if ((bool)m_scrDriverFreeView)
		{
			m_scrDriverFreeView.enabled = m_currCamera == 0;
		}
		if (m_currCamera == 0 && showMirrors)
		{
			if ((bool)MirrorLeftTex)
			{
				if ((bool)m_targetCams.MirrorLeft)
				{
					m_targetCams.MirrorLeft.targetTexture = MirrorLeftTex.texture as RenderTexture;
					m_targetCams.MirrorLeft.enabled = true;
					MirrorLeftTex.enabled = true;
					ClearMirrorTexture(m_targetCams.MirrorLeft);
				}
				else
				{
					MirrorLeftTex.enabled = false;
				}
			}
			if ((bool)MirrorRightTex)
			{
				if ((bool)m_targetCams.MirrorRight)
				{
					m_targetCams.MirrorRight.targetTexture = MirrorRightTex.texture as RenderTexture;
					m_targetCams.MirrorRight.enabled = true;
					MirrorRightTex.enabled = true;
					ClearMirrorTexture(m_targetCams.MirrorRight);
				}
				else
				{
					MirrorRightTex.enabled = false;
				}
			}
			if ((bool)MirrorRearTex)
			{
				if ((bool)m_targetCams.MirrorRear)
				{
					m_targetCams.MirrorRear.targetTexture = MirrorRearTex.texture as RenderTexture;
					m_targetCams.MirrorRear.enabled = true;
					MirrorRearTex.enabled = true;
					ClearMirrorTexture(m_targetCams.MirrorRear);
				}
				else
				{
					MirrorRearTex.enabled = false;
				}
			}
		}
		else
		{
			if ((bool)m_targetCams.MirrorLeft)
			{
				m_targetCams.MirrorLeft.enabled = false;
			}
			if ((bool)m_targetCams.MirrorRight)
			{
				m_targetCams.MirrorRight.enabled = false;
			}
			if ((bool)m_targetCams.MirrorRear)
			{
				m_targetCams.MirrorRear.enabled = false;
			}
			if ((bool)MirrorLeftTex)
			{
				MirrorLeftTex.enabled = false;
			}
			if ((bool)MirrorRightTex)
			{
				MirrorRightTex.enabled = false;
			}
			if ((bool)MirrorRearTex)
			{
				MirrorRearTex.enabled = false;
			}
		}
	}

	private void Start()
	{
		m_currCamera = DefaultCamera;
		m_lastShowMirrors = showMirrors;
		m_lastImageEffects = enableImageEffects;
		m_currCamera = DefaultCamera;
		m_lastTarget = Target;
		m_scrFixTo = GetComponent<CamFixTo>();
		m_scrSmoothFollow = GetComponent<CamSmoothFollow>();
		m_scrMouseOrbit = GetComponent<CamMouseOrbit>();
		m_scrSmoothLookAt = GetComponent<CamSmoothLookAt>();
		m_scrFreeView = GetComponent<CamFreeView>();
		m_scrSmoothLookAtFromPos = GetComponent<CamSmoothLookAtFromPos>();
		m_cameraScript = new MonoBehaviour[m_numCameras];
		m_cameraScript[0] = m_scrFixTo;
		m_cameraScript[1] = m_scrSmoothFollow;
		m_cameraScript[2] = m_scrMouseOrbit;
		m_cameraScript[3] = m_scrSmoothLookAt;
		m_cameraScript[4] = m_scrFreeView;
		m_cameraScript[5] = m_scrSmoothLookAtFromPos;
		if ((bool)MapCamera)
		{
			m_scrMapFollow = MapCamera.GetComponent<CamSmoothFollow>();
		}
		if ((bool)MirrorLeftTex)
		{
			MirrorLeftTex.enabled = false;
		}
		if ((bool)MirrorRightTex)
		{
			MirrorRightTex.enabled = false;
		}
		if ((bool)MirrorRearTex)
		{
			MirrorRearTex.enabled = false;
		}
		if ((bool)Target)
		{
			UpdateTarget();
		}
		for (int i = 0; i < ImageEffects.Length; i++)
		{
			ImageEffects[i].enabled = enableImageEffects;
		}
		for (int j = 0; j < m_numCameras; j++)
		{
			m_cameraScript[j].enabled = false;
		}
		m_cameraScript[m_currCamera].enabled = true;
	}

	private void Update()
	{
		if (m_lastTarget != Target)
		{
			UpdateTarget();
			m_lastTarget = Target;
		}
		if (m_lastShowMirrors != showMirrors)
		{
			UpdateMirrors();
			m_lastShowMirrors = showMirrors;
		}
		if (m_lastImageEffects != enableImageEffects)
		{
			for (int i = 0; i < ImageEffects.Length; i++)
			{
				ImageEffects[i].enabled = enableImageEffects;
			}
			m_lastImageEffects = enableImageEffects;
		}
	}

	public void Next()
	{
		m_cameraScript[m_currCamera++].enabled = false;
		if (m_currCamera >= m_numCameras || (m_currCamera == m_numCameras - 1 && !Target2))
		{
			m_currCamera = 0;
		}
		m_cameraScript[m_currCamera].enabled = true;
		m_scrSmoothFollow.reset = true;
		UpdateMirrors();
	}

	public void SwitchTo(int Cam)
	{
		if (Cam >= m_numCameras)
		{
			return;
		}
		if (Cam == 0 && Cam == m_currCamera)
		{
			CamFreeView component = m_targetCams.DriverFront.GetComponent<CamFreeView>();
			if ((bool)component)
			{
				component.SetLocalEulerAngles(m_targetCams.getDriverViewAngles());
			}
		}
		m_cameraScript[m_currCamera].enabled = false;
		m_cameraScript[Cam].enabled = true;
		m_currCamera = Cam;
		m_scrSmoothFollow.reset = true;
		UpdateMirrors();
	}

	public void ToggleMap()
	{
		if ((bool)MapCamera)
		{
			MapCamera.enabled = !MapCamera.enabled;
		}
	}
}
