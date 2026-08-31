using UnityEngine;

public class GameServicesEnableDisableButton : MonoBehaviour
{
	private string SPRITE_NAME = "play_games";

	private string DISABLE_SPRITE_NAME = "play_games";

	public UISprite sp;

	public GUIGameButton button;

	public GUIGameButton.GameButtonFunction disabledFunction;

	public UIButton uiButton;

	private GUIGameButton.GameButtonFunction _function;

	private void Awake()
	{
		sp.spriteName = DISABLE_SPRITE_NAME;
		if (button != null)
		{
			_function = button.function;
			button.function = disabledFunction;
		}
		else if (uiButton == null)
		{
			Debug.LogError("NULL REFERENCE - GUiGame Button or UIButton needed");
		}
		if (uiButton != null)
		{
			uiButton.isEnabled = false;
		}
	}
}
