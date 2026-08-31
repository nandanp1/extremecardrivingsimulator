using UnityEngine;

public class CircuitSplineMaster : MonoBehaviour
{
	public CurvyFollowSplineController cameraSplineController;

	public CurvyFollowSplineController cameraTargetSplineController;

	private Camera _camera;

	private Camera _cameraToSwitch;

	public event FinishCallback finishCallbackEvent;

	private void OnEnable()
	{
		_camera = cameraSplineController.GetComponent<Camera>();
		_camera.enabled = false;
	}

	private void Update()
	{
		if (cameraSplineController.IsFinalized() && cameraTargetSplineController.IsFinalized() && finishCallbackEvent != null)
		{
			finishCallbackEvent();
		}
	}

	public void Play(Camera cameraToSwitch)
	{
		_cameraToSwitch = cameraToSwitch;
		cameraSplineController.Play();
		cameraTargetSplineController.Play();
		_cameraToSwitch.enabled = false;
		_camera.enabled = true;
	}

	public void Stop()
	{
		_cameraToSwitch.enabled = true;
		_camera.enabled = false;
		cameraSplineController.Stop();
		cameraTargetSplineController.Stop();
	}

	public void DestroyAll()
	{
		Object.Destroy(_camera.gameObject);
		Object.Destroy(cameraSplineController.gameObject);
		Object.Destroy(cameraTargetSplineController.gameObject);
	}

	public void SetEnableAll(bool enable)
	{
		cameraSplineController.Restart();
		cameraTargetSplineController.Restart();
		_camera.gameObject.SetActive(enable);
		cameraSplineController.gameObject.SetActive(enable);
		cameraTargetSplineController.gameObject.SetActive(enable);
	}
}
