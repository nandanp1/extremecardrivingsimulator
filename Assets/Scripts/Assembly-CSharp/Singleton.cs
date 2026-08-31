using UnityEngine;

public abstract class Singleton<T> : EventDispatcher where T : MonoBehaviour
{
	private static T _instance;

	public static T instance
	{
		get
		{
			if (_instance == null)
			{
				_instance = Object.FindFirstObjectByType(typeof(T)) as T;
				if (_instance == null)
				{
					_instance = new GameObject().AddComponent<T>();
					_instance.gameObject.name = _instance.GetType().Name;
				}
			}
			return _instance;
		}
	}

	public static bool IsDestroyed
	{
		get
		{
			if (_instance == null)
			{
				return true;
			}
			return false;
		}
	}
}
