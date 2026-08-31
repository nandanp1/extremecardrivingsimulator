using System;
using UnityEngine;

namespace Uniject.Impl
{
	public class UnityLevelLoadListener : MonoBehaviour, ILevelLoadListener
	{
		private Action listener;

		public void registerListener(Action action)
		{
			listener = action;
		}

		private void Start()
		{
			UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
		}

		private void OnLevelWasLoaded(int level)
		{
			if (listener != null)
			{
				listener();
			}
		}
	}
}
