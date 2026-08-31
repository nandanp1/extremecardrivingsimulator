using UnityEngine;

public class SALevelLoader : Singleton<SALevelLoader>
{
	private Texture2D bg;

	private void Awake()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void LoadLevel(string name)
	{
		LegacySceneManager.LoadLevel(name);
	}

	public void Restart()
	{
		LegacySceneManager.LoadLevel(LegacySceneManager.LoadedLevelName);
	}
}
