using UnityEngine;

public class DisableCameraRotation : MonoBehaviour
{
	private void OnPress(bool pressed)
	{
		if (pressed)
		{
			MySingleton<Manager>.Instance.isRollingScreenshotMode = true;
		}
		else
		{
			MySingleton<Manager>.Instance.isRollingScreenshotMode = false;
		}
	}
}
