using UnityEngine;

public class ImmersiveMode : Singleton<ImmersiveMode>
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void EnableImmersiveMode()
	{
		AndroidNative.enableImmersiveMode();
	}
}
