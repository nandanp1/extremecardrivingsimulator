public class NotificationOKObject : NotificationObject
{
	public UILabel lbMessage;

	public UIButton btOK;

	public UILabel lbOK;

	public void Awake()
	{
		EventDelegate ev = new EventDelegate(OK);
		EventDelegate.Add(btOK.onClick, ev);
	}

	private void OK()
	{
		MySingleton<Manager>.Instance.notificationManager.CloseCurrent();
	}
}
