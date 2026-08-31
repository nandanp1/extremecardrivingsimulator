using UnityEngine;

public class AsyncTask : EventDispatcher
{
	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}
}
