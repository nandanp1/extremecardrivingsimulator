using System.Collections;
using Uniject;
using UnityEngine;

public class RemoteConfigFetcher : MonoBehaviour
{
	public void Start()
	{
		Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Fetch(IStorage storage, string url, string key)
	{
		StartCoroutine(fetch(storage, url, key));
	}

	private IEnumerator fetch(IStorage storage, string url, string key)
	{
		WWW request = new WWW(url);
		log("Fetching latest Unibill config from " + url);
		while (!request.isDone)
		{
			yield return new WaitForSeconds(1f);
		}
		if (!string.IsNullOrEmpty(request.error))
		{
			log(string.Format("Failed to fetch inventory: {0}", request.error));
			yield break;
		}
		log("Fetched and stored latest inventory");
		storage.SetString(key, request.text);
	}

	private void log(string message)
	{
		Debug.Log("UnibillConfigFetcher:" + message);
	}
}
