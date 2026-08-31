using UnityEngine;

public class BackButtonStore : MonoBehaviour
{
	private GUIStoreButton _backButton;

	public GUIStoreButton backButton
	{
		get
		{
			if (_backButton == null)
			{
				_backButton = base.gameObject.AddMissingComponent<GUIStoreButton>();
				if ((bool)MySingleton<Manager>.Instance.guiStoreLogic)
				{
					MySingleton<Manager>.Instance.guiStoreLogic.backButton = this;
				}
				if (!MySingleton<Manager>.Instance.storeInitData.storeFromCarPacks)
				{
					if (MySingleton<Manager>.Instance.starterPackPanelIsActive)
					{
						function = GUIStoreButton.StoreButtonFunction.CloseStarterPack;
					}
					else
					{
						function = GUIStoreButton.StoreButtonFunction.ExitStore;
					}
				}
				else
				{
					function = GUIStoreButton.StoreButtonFunction.BackToPackStore;
				}
			}
			return _backButton;
		}
	}

	public GUIStoreButton.StoreButtonFunction function
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
		if (Input.GetKeyDown(KeyCode.Escape))
		{
			backButton.OnClick();
		}
	}
}
