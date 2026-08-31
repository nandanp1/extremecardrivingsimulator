using UnityEngine;

public class BackButtonWorkshop : MonoBehaviour
{
	public GUIWorkshopButton backButton;

	public GUIWorkshopButton.WorkshopButtonFunction function
	{
		get
		{
			return backButton.function;
		}
		set
		{
			backButton.function = value;
		}
	}

	private void Start()
	{
		if (backButton == null)
		{
			backButton = base.gameObject.AddComponent<GUIWorkshopButton>();
		}
		MySingleton<Manager>.Instance.guiWorkshopLogic.backButton = this;
		function = GUIWorkshopButton.WorkshopButtonFunction.BackToDrivingConfirmation;
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && MySingleton<Manager>.Instance.guiStoreLogic == null)
		{
			backButton.OnClick();
		}
	}
}
