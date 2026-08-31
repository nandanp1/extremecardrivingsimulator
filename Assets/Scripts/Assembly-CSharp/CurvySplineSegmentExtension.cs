using UnityEngine;

[RequireComponent(typeof(CurvySplineSegment))]
[ExecuteInEditMode]
public class CurvySplineSegmentExtension : MonoBehaviour
{
	public bool ignoreTimeToCome = true;

	public float timeToCome;

	public bool ignoreFOV;

	[Range(0f, 179f)]
	public float fovValue = 60f;

	[HideInInspector]
	public float desireSpeed;

	[HideInInspector]
	public float length;

	[HideInInspector]
	public float localTimeToCome;

	[HideInInspector]
	public bool isFirstSegment;

	[HideInInspector]
	public bool isLastSegment;

	[HideInInspector]
	public float lastTimeToCome;
}
