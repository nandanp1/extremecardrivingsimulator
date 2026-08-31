using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class CoppaPanel : MonoBehaviour
{
	public UIWidget widget;

	private void Awake()
	{
		if (ObscuredPrefs.GetBool(Manager.COPPA_ALREADY_SHOWED, false))
		{
			Object.Destroy(base.gameObject);
		}
		else
		{
			NGUITools.SetActive(widget.gameObject, true);
		}
	}

	public void Accept()
	{
		ObscuredPrefs.SetBool(Manager.COPPA_ALREADY_SHOWED, true);
		MySingleton<Manager>.Instance.guiMenuLogic.InitStarterPackAndServices();
		MySingleton<Manager>.Instance.ads.Init();
		ManagerAnalytics.instance.CoppaRequest();
		Object.Destroy(base.gameObject);
	}

	public void ExitApp()
	{
		Application.Quit();
	}

	public void OpenPolicy()
	{
		Application.OpenURL("http://axesinmotion.com/privacy/");
	}

	public void OpenTerms()
	{
		Application.OpenURL("http://axesinmotion.com/terms/");
	}
}
