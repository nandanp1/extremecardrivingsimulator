using UnityEngine;

public class CheckNicknameFocus : MonoBehaviour
{
	public MultiplayerPanel multiplayerPanel;

	private void OnSelect(bool selected)
	{
		if (!selected)
		{
			multiplayerPanel.CheckNicknameIsEmptyAndFix();
		}
	}
}
