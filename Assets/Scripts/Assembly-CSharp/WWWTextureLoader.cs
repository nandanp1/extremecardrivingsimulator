using System.Collections;
using UnityEngine;

public class WWWTextureLoader : EventDispatcher
{
	private string _url;

	public static WWWTextureLoader Create()
	{
		return new GameObject("WWWTextureLoader").AddComponent<WWWTextureLoader>();
	}

	public void LoadTexture(string url)
	{
		_url = url;
		StartCoroutine(LoadCoroutin());
	}

	private IEnumerator LoadCoroutin()
	{
		WWW www = new WWW(_url);
		yield return www;
		dispatch("loaded", www.texture);
	}
}
