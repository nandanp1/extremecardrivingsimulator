using UnityEngine;

[AddComponentMenu("NGUI/Examples/Follow Target")]
public class UIFollowTarget : MonoBehaviour
{
	public Transform target;

	public Camera uiCamera;

	public bool disableIfInvisible = true;

	private Transform mTrans;

	private bool mIsVisible;

	public UIWidget widget;

	public Camera gameCamera
	{
		get
		{
			return MySingleton<Manager>.Instance.gameLogic.currentCamera;
		}
	}

	private void Awake()
	{
		mTrans = base.transform;
	}

	public void SetTarget(Transform t)
	{
		target = t;
		if (target != null)
		{
			if (gameCamera == null)
			{
				Debug.LogError("UIFollowTarget.SetTarget ha detectado que la camara activa es nula. Si aparecen mas errores comprobar camaras.");
			}
			if (uiCamera == null)
			{
				uiCamera = NGUITools.FindCameraForLayer(base.gameObject.layer);
			}
			SetVisible(false);
		}
		else
		{
			Debug.LogError("Expected to have 'target' set to a valid transform", this);
			base.enabled = false;
		}
	}

	private void SetVisible(bool val)
	{
		mIsVisible = val;
		int i = 0;
		for (int childCount = mTrans.childCount; i < childCount; i++)
		{
			NGUITools.SetActive(mTrans.GetChild(i).gameObject, val);
		}
	}

	private void LateUpdate()
	{
		if (!(target == null) && !(gameCamera == null))
		{
			Vector3 position = gameCamera.WorldToViewportPoint(target.position);
			bool flag = (gameCamera.orthographic || position.z > 0f) && (!disableIfInvisible || (position.x > 0f && position.x < 1f && position.y > 0f && position.y < 1f));
			if (mIsVisible != flag)
			{
				SetVisible(flag);
			}
			if (flag)
			{
				base.transform.position = uiCamera.ViewportToWorldPoint(position);
				position = mTrans.localPosition;
				mTrans.localPosition = position;
			}
			OnUpdate(flag);
		}
	}

	protected virtual void OnUpdate(bool isVisible)
	{
	}
}
