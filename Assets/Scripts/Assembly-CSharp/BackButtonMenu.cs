using UnityEngine;

public class BackButtonMenu : MonoBehaviour
{
	private GUIMenuButton _backButton;

	public GUIMenuButton backButton
	{
		get
		{
			if (_backButton == null)
			{
				_backButton = base.gameObject.AddMissingComponent<GUIMenuButton>();
				MySingleton<Manager>.Instance.guiMenuLogic.backButton = this;
				function = GUIMenuButton.MenuButtonFunction.ShowExit;
			}
			return _backButton;
		}
	}

	public GUIMenuButton.MenuButtonFunction function
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

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && MySingleton<Manager>.Instance.guiStoreLogic == null)
		{
			backButton.OnClick();
		}
	}
}
