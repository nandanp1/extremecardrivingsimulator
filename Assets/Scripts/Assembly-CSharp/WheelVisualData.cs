using System;
using UnityEngine;

[Serializable]
public class WheelVisualData
{
	public float colliderOffset;

	public float skidmarkOffset;

	public Vector3 wheelVelocity = Vector3.zero;

	public Vector3 groundSpeed = Vector3.zero;

	public float angularVelocity;

	public float lastSuspensionForce;

	public float suspensionStress;

	public int lastSkidmark = -1;

	public float skidmarkTime;

	public float skidSmokeTime = Time.time;

	public Vector3 skidSmokePos = Vector3.zero;

	public float skidSmokeIntensity;

	public float skidValue;
}
