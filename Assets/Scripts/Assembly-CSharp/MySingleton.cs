using UnityEngine;

public class MySingleton<T> : MonoBehaviour where T : MonoBehaviour
{
	private static T _instance;

	private static object _lock = new object();

	private static bool applicationIsQuitting = false;

	public static T Instance
	{
		get
		{
			if (applicationIsQuitting)
			{
				Debug.LogWarning(string.Concat("[Singleton] Instance '", typeof(T), "' already destroyed on application quit. Won't create again - returning null."));
				return (T)null;
			}
			lock (_lock)
			{
				if (_instance == null)
				{
					_instance = (T)Object.FindFirstObjectByType(typeof(T));
					if (Object.FindObjectsByType(typeof(T), FindObjectsSortMode.InstanceID).Length > 1)
					{
						Debug.LogError("[Singleton] Something went really wrong  - there should never be more than 1 singleton! Reopenning the scene might fix it.");
						return _instance;
					}
					if (_instance == null)
					{
						GameObject gameObject = new GameObject();
						_instance = gameObject.AddComponent<T>();
						gameObject.name = "(singleton) " + typeof(T).ToString();
						Object.DontDestroyOnLoad(gameObject);
						Debug.Log(string.Concat("[Singleton] An instance of ", typeof(T), " is needed in the scene, so '", gameObject, "' was created with DontDestroyOnLoad."));
					}
					else
					{
						Debug.Log("[Singleton] Using instance already created: " + _instance.gameObject.name);
					}
				}
				return _instance;
			}
		}
	}

	public void OnDestroy()
	{
		applicationIsQuitting = true;
	}
}
