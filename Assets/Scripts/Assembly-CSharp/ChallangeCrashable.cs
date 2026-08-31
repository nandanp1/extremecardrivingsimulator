using UnityEngine;

public class ChallangeCrashable : MonoBehaviour
{
	private const float DESTROY_TIME = 0.5f;

	public GameObject[] parts;

	public GameObject lod;

	public void OnCrash()
	{
		MySingleton<Manager>.Instance.guiChallangeModeLogic.crashableChallengeManager.SaveScore();
		NGUITools.SetActive(lod, false);
		Invoke("DestroyChallangeCrashable", 0.5f);
	}

	private void DestroyChallangeCrashable()
	{
		for (int i = 0; i < parts.Length; i++)
		{
			if (parts[i] != null && parts[i].GetComponent<MeshCollider>() != null)
			{
				parts[i].GetComponent<MeshCollider>().enabled = false;
			}
		}
	}
}
