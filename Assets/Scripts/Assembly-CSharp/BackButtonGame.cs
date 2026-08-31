using UnityEngine;

public class BackButtonGame : MonoBehaviour
{
	private GUIGameButton _button;

	public GUIGameButton button
	{
		get
		{
			if (_button == null)
			{
				_button = base.gameObject.AddMissingComponent<GUIGameButton>();
				MySingleton<Manager>.Instance.guiGameLogic.backButton = this;
				function = GUIGameButton.GameButtonFunction.Pause;
			}
			return _button;
		}
	}

	public GUIGameButton.GameButtonFunction function
	{
		get
		{
			return button.function;
		}
		set
		{
			button.function = value;
		}
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape) && MySingleton<Manager>.Instance.guiStoreLogic == null)
		{
			button.OnClick();
		}
	}
}
