using UnityEngine;

#if UNITY_5_3_OR_NEWER
using UnityEngine.SceneManagement;
#endif

/// <summary>
/// Preserves the Unity 4 scene-loading behavior while routing modern editors
/// through SceneManager. Keeping the adapter in firstpass makes it available to
/// both recovered C# assemblies without changing their original dependency order.
/// </summary>
public static class LegacySceneManager
{
#if UNITY_5_3_OR_NEWER
	static LegacySceneManager()
	{
		SceneManager.sceneLoaded += LogDevelopmentSceneRenderState;
	}

	private static void LogDevelopmentSceneRenderState(Scene scene, LoadSceneMode mode)
	{
		if (!Debug.isDebugBuild)
		{
			return;
		}

		int rendererCount = 0;
		int staticBatchRendererCount = 0;
		GameObject[] rootObjects = scene.GetRootGameObjects();
		for (int i = 0; i < rootObjects.Length; i++)
		{
			MeshRenderer[] renderers = rootObjects[i].GetComponentsInChildren<MeshRenderer>(true);
			rendererCount += renderers.Length;
			for (int j = 0; j < renderers.Length; j++)
			{
				if (renderers[j].isPartOfStaticBatch)
				{
					staticBatchRendererCount++;
				}
			}
		}

		Debug.Log(string.Format(
			"[SceneRenderState] scene={0} mode={1} meshRenderers={2} staticBatched={3}",
			scene.name,
			mode,
			rendererCount,
			staticBatchRendererCount));
	}
#endif

    public static int LoadedLevel
    {
        get
        {
#if UNITY_5_3_OR_NEWER
            return SceneManager.GetActiveScene().buildIndex;
#else
            return Application.loadedLevel;
#endif
        }
    }

    public static string LoadedLevelName
    {
        get
        {
#if UNITY_5_3_OR_NEWER
            return SceneManager.GetActiveScene().name;
#else
            return Application.loadedLevelName;
#endif
        }
    }

    public static int LevelCount
    {
        get
        {
#if UNITY_5_3_OR_NEWER
            return SceneManager.sceneCountInBuildSettings;
#else
            return Application.levelCount;
#endif
        }
    }

    public static void LoadLevel(int buildIndex)
    {
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(buildIndex, LoadSceneMode.Single);
#else
        Application.LoadLevel(buildIndex);
#endif
    }

    public static void LoadLevel(string sceneName)
    {
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
#else
        Application.LoadLevel(sceneName);
#endif
    }

    public static AsyncOperation LoadLevelAsync(int buildIndex)
    {
#if UNITY_5_3_OR_NEWER
        return SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Single);
#else
        return Application.LoadLevelAsync(buildIndex);
#endif
    }

    public static AsyncOperation LoadLevelAsync(string sceneName)
    {
#if UNITY_5_3_OR_NEWER
        return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
#else
        return Application.LoadLevelAsync(sceneName);
#endif
    }

    public static void LoadLevelAdditive(int buildIndex)
    {
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(buildIndex, LoadSceneMode.Additive);
#else
        Application.LoadLevelAdditive(buildIndex);
#endif
    }

    public static void LoadLevelAdditive(string sceneName)
    {
#if UNITY_5_3_OR_NEWER
        SceneManager.LoadScene(sceneName, LoadSceneMode.Additive);
#else
        Application.LoadLevelAdditive(sceneName);
#endif
    }

    public static AsyncOperation LoadLevelAdditiveAsync(int buildIndex)
    {
#if UNITY_5_3_OR_NEWER
        return SceneManager.LoadSceneAsync(buildIndex, LoadSceneMode.Additive);
#else
        return Application.LoadLevelAdditiveAsync(buildIndex);
#endif
    }

    public static AsyncOperation LoadLevelAdditiveAsync(string sceneName)
    {
#if UNITY_5_3_OR_NEWER
        return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);
#else
        return Application.LoadLevelAdditiveAsync(sceneName);
#endif
    }
}
