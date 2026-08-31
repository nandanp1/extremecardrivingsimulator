using System;
using AxesInMotion.Network;
using UnityEngine;

public class NetworkCarRadarMarker : NetworkBehaviour
{
	public GameObject marker;

	public Transform markerTransform;

	public Transform targetTransform;

	public Transform sourceTransform;

	public Camera destiny;

	private Vector3 direction;

	private float angle;

	private float angleOffset = (float)Math.PI / 2f;

	private Vector3 nguiContent = new Vector3(1280f, 720f);

	private Vector3 viewportCenter = new Vector3(0.5f, 0.5f, 0f);

	private Vector3 markerScale = new Vector3(1f, 0.5f, 1f);

	private float angleThreshold = (float)Math.PI / 2f;

	private float radarRadius = 260f;

	private Vector2 _viewportMin;

	private Vector2 _viewportMax;

	private float _offsetAdsY = 0.1f;

	public bool ready;

	protected override void Awake()
	{
		base.Awake();
	}

	private void OnEnable()
	{
		if (MySingleton<Manager>.Instance.gameMode != Manager.GameMode.Multiplayer)
		{
			if ((bool)marker)
			{
				UnityEngine.Object.Destroy(marker);
			}
			UnityEngine.Object.Destroy(this);
		}
	}

	public GameObject Initialize(Color color)
	{
		GameObject pnNetworkRadar = MySingleton<Manager>.Instance.guiGameLogic.pnNetworkRadar;
		destiny = pnNetworkRadar.transform.parent.gameObject.GetComponent<Camera>();
		marker = NGUITools.AddChild(pnNetworkRadar, (GameObject)Resources.Load("Marker", typeof(GameObject)));
		markerTransform = marker.transform;
		targetTransform = base.transform;
		sourceTransform = MySingleton<Manager>.Instance.gameLogic.currentCar;
		marker.name = "Marker#" + base.viewID;
		color.a = 0.4f;
		marker.GetComponent<UITexture>().color = color;
		SetViewportMaxMin(marker.GetComponent<UITexture>().localSize);
		ready = true;
		return marker;
	}

	private void LocateMarkerOnRadar(Transform marker, Transform target, Transform src, Camera dst)
	{
		Vector3 up = src.up;
		Vector3 forward = src.forward;
		Vector3 zero = Vector3.zero;
		Vector3 vector = new Vector3(1f / nguiContent.x, 1f / nguiContent.y, 0f);
		direction = target.position - src.position;
		direction.y = 0f;
		direction.Normalize();
		angle = Mathf.Atan2(Vector3.Dot(up, Vector3.Cross(forward, direction)), Vector3.Dot(forward, direction));
		if (angle < 0f - angleThreshold)
		{
			angle = 0f - angleThreshold;
		}
		else if (angle > angleThreshold)
		{
			angle = angleThreshold;
		}
		angle += angleOffset;
		zero.x = 0f - Mathf.Cos(angle);
		zero.y = Mathf.Sin(angle);
		zero.Normalize();
		zero.x *= radarRadius * vector.x;
		zero.y *= radarRadius * vector.y;
		marker.position = dst.ViewportToWorldPoint(viewportCenter + zero);
		marker.rotation = Quaternion.identity;
		marker.Rotate(Vector3.forward, (0f - (angle - angleOffset)) * 57.29578f, Space.World);
		marker.localScale = markerScale;
	}

	public void SetViewportMaxMin(Vector2 textureSize)
	{
		_viewportMin = Vector2.zero;
		_viewportMin.x = textureSize.x / 2f / (float)Screen.width;
		_viewportMin.y = textureSize.y / 2f / (float)Screen.height + _offsetAdsY;
		_viewportMax = Vector2.zero;
		_viewportMax.x = 1f - _viewportMin.x;
		_viewportMax.y = 1f - _viewportMin.y;
	}

	private void Update()
	{
		if (ready)
		{
			LocateMarkerOnRadar(markerTransform, targetTransform, sourceTransform, destiny);
		}
	}

	private void OnDestroy()
	{
		UnityEngine.Object.Destroy(marker);
	}
}
