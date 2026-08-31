using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotificationManager : MonoBehaviour
{
	public GameObject notificationOKPrefab;

	public GameObject notificationYesNoPrefab;

	private Queue<NotificationRequest> pendingNotifications;

	private GameObject currentNotification;

	private void Awake()
	{
		pendingNotifications = new Queue<NotificationRequest>();
	}

	private void Start()
	{
		MySingleton<Manager>.Instance.notificationManager = this;
	}

	public void RequestNotificationOK(string message, string okMessage = "OK")
	{
		pendingNotifications.Enqueue(new NotificationOKRequest(message, okMessage, notificationOKPrefab));
		if (!currentNotification)
		{
			ShowNext();
		}
	}

	public void RequestNotificationYesNo(string message, string yesText, string noText, EventDelegate yesFunc, EventDelegate noFunc)
	{
		pendingNotifications.Enqueue(new NotificationYesNoRequest(message, yesText, noText, yesFunc, noFunc, notificationYesNoPrefab));
		if (!currentNotification)
		{
			ShowNext();
		}
	}

	public void RequestNotificationYesNo(string title, string message, string yesText, string noText, Color textColor, Color backgroundColor, bool invertColorsOfYes, bool invertColorsOfNo, EventDelegate yesFunc, EventDelegate noFunc)
	{
		pendingNotifications.Enqueue(new NotificationYesNoRequest(title, message, yesText, noText, textColor, backgroundColor, invertColorsOfYes, invertColorsOfNo, yesFunc, noFunc, notificationYesNoPrefab));
		if (!currentNotification)
		{
			ShowNext();
		}
	}

	public void RequestNewUpdateNotification(EventDelegate yesFunc, EventDelegate noFunc)
	{
		RequestNotificationYesNo(Localization.Get("new_update_title"), Localization.Get("new_update_message"), Localization.Get("update_yes"), Localization.Get("update_no"), Color.white, new Color(0.125f, 117f / 256f, 27f / 128f, 1f), true, false, yesFunc, noFunc);
	}

	public void ShowNext(GameObject uiRoot = null)
	{
		if (!uiRoot && !MySingleton<Manager>.Instance.currentUIRoot)
		{
			return;
		}
		GameObject gameObject = null;
		while (pendingNotifications.Count > 0)
		{
			if (pendingNotifications.Peek().Create(out gameObject))
			{
				if ((bool)uiRoot)
				{
					gameObject.transform.SetParent(uiRoot.transform);
				}
				else
				{
					gameObject.transform.SetParent(MySingleton<Manager>.Instance.currentUIRoot.transform);
				}
				gameObject.transform.localScale = Vector3.one;
				currentNotification = gameObject;
				break;
			}
			pendingNotifications.Dequeue();
		}
	}

	public void CloseCurrent()
	{
		pendingNotifications.Dequeue().Dispose();
		currentNotification = null;
		ShowNext();
	}

	private void OnLevelWasLoaded(int level)
	{
		if (!MySingleton<Manager>.Instance.currentUIRoot)
		{
			StartCoroutine(WaitAndSearchUIRoot());
		}
		else
		{
			ShowNext();
		}
	}

	private IEnumerator WaitAndSearchUIRoot()
	{
		for (int attemptsBeforeSearchingHimself = 15; attemptsBeforeSearchingHimself > 0; attemptsBeforeSearchingHimself--)
		{
			if ((bool)MySingleton<Manager>.Instance.currentUIRoot)
			{
				ShowNext();
				break;
			}
			yield return null;
		}
		if ((bool)MySingleton<Manager>.Instance.currentUIRoot)
		{
			ShowNext();
			yield break;
		}
		UIRoot uiRoot = Object.FindFirstObjectByType<UIRoot>();
		if ((bool)uiRoot)
		{
			ShowNext(uiRoot.gameObject);
		}
		else
		{
			Debug.LogError("No se ha encontrado ningun UI Root. Y mira que lo he buscado!");
		}
	}
}
