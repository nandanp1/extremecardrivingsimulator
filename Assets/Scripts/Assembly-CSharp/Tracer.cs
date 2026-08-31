using System;
using UnityEngine;

public class Tracer : MonoBehaviour
{
	private void OnEnable()
	{
		string stackTrace = Environment.StackTrace;
		Debug.LogError(stackTrace);
	}

	private void Update()
	{
	}
}
