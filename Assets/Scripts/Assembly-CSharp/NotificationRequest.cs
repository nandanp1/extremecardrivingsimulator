using UnityEngine;

public class NotificationRequest
{
	protected GameObject notificationPrefab;

	protected GameObject notificationObject;

	public NotificationRequest(GameObject notificationPrefab)
	{
		this.notificationPrefab = notificationPrefab;
		notificationObject = null;
	}

	public virtual bool Create(out GameObject gameObject)
	{
		notificationObject = Object.Instantiate(notificationPrefab) as GameObject;
		gameObject = notificationObject;
		return true;
	}

	public virtual void Dispose()
	{
		Object.DestroyImmediate(notificationObject);
	}
}
