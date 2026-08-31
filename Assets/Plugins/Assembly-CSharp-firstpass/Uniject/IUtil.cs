using System;
using System.Collections;
using UnityEngine;

namespace Uniject
{
	public interface IUtil
	{
		RuntimePlatform Platform { get; }

		bool IsEditor { get; }

		string persistentDataPath { get; }

		DateTime currentTime { get; }

		string DeviceModel { get; }

		string DeviceName { get; }

		DeviceType DeviceType { get; }

		string OperatingSystem { get; }

		T[] getAnyComponentsOfType<T>() where T : class;

		string loadedLevelName();

		object InitiateCoroutine(IEnumerator start);

		object getWaitForSeconds(int seconds);

		void InitiateCoroutine(IEnumerator start, int delayInSeconds);

		void RunOnThreadPool(Action runnable);

		void RunOnMainThread(Action runnable);
	}
}
