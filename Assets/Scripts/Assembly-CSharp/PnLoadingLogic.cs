using UnityEngine;

public class PnLoadingLogic : MonoBehaviour
{
	private const string UI_ROOT_NAME_GAME_OBJECT = "UI Root";

	public UISprite spLoading;

	private GameObject currentUIRootDisabled;

	private void OnEnable()
	{
		currentUIRootDisabled = GameObject.Find("UI Root");
		if (!currentUIRootDisabled && LegacySceneManager.LoadedLevel != 0)
		{
			Debug.LogError("Revisar que el UI Root de la escena se llama como indica la constante UI_ROOT_NAME_GAME_OBJECT");
		}
		if ((bool)currentUIRootDisabled)
		{
			NGUITools.SetActive(currentUIRootDisabled.gameObject, false);
		}
	}

	private void OnDisable()
	{
		if ((bool)currentUIRootDisabled)
		{
			NGUITools.SetActive(currentUIRootDisabled.gameObject, true);
		}
		currentUIRootDisabled = null;
	}

	private void Start()
	{
		MySingleton<Manager>.Instance.spLoading = spLoading;
	}
}
