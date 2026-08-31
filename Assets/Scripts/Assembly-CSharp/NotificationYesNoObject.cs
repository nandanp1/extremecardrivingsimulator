public class NotificationYesNoObject : NotificationObject
{
	public UILabel lbTitle;

	public UILabel lbMessage;

	public UIButton btYes;

	public UILabel lbYes;

	public UIButton btNo;

	public UILabel lbNo;

	public UISprite background;

	public UISprite backgroundNo;

	public UISprite backgroundYes;

	public EventDelegate yesFunction;

	public EventDelegate noFunction;

	public void Start()
	{
		EventDelegate ev = new EventDelegate(Close);
		EventDelegate.Set(btYes.onClick, yesFunction);
		if (noFunction != null)
		{
			EventDelegate.Set(btNo.onClick, noFunction);
		}
		EventDelegate.Add(btYes.onClick, ev);
		EventDelegate.Add(btNo.onClick, ev);
	}

	private void Close()
	{
		MySingleton<Manager>.Instance.notificationManager.CloseCurrent();
	}
}
