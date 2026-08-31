using System;
using System.Threading;
using UnityEngine;

namespace Amazon
{
	/// <summary>
	/// GUID-stable offline replacement for the legacy AWS Unity initializer.
	/// It preserves the scene contract without initializing AWS or native SQLite.
	/// </summary>
	public class UnityInitializer : MonoBehaviour
	{
		private static UnityInitializer _instance;

		private static readonly object SyncRoot = new object();

		private static Thread _mainThread;

		public static UnityInitializer Instance
		{
			get { return _instance; }
		}

		public void Awake()
		{
			lock (SyncRoot)
			{
				if (_instance == null)
				{
					_instance = this;
					_mainThread = Thread.CurrentThread;
					DontDestroyOnLoad(gameObject);
				}
				else if (_instance != this)
				{
					Destroy(gameObject);
				}
			}
		}

		public static bool IsMainThread()
		{
			if (_mainThread == null)
			{
				throw new InvalidOperationException(
					"Main thread has not been set; the offline AWS facade is not initialized.");
			}

			return Thread.CurrentThread.Equals(_mainThread);
		}
	}
}
