using System.Text;
using UnityEngine;

public class GoogleCloudUseExample : MonoBehaviour
{
	public GameObject[] hideOnConnect;

	public GameObject[] showOnConnect;

	private void Awake()
	{
		Singleton<GoogleCloudManager>.instance.addEventListener("all_states_loaded", OnAllLoaded);
		Singleton<GoogleCloudManager>.instance.addEventListener("state_loaded", OnStateUpdated);
		Singleton<GoogleCloudManager>.instance.addEventListener("state_resolved", OnStateUpdated);
		Singleton<GoogleCloudManager>.instance.addEventListener("state_updated", OnStateUpdated);
		Singleton<GoogleCloudManager>.instance.addEventListener("state_conflict", OnStateConflict);
		Singleton<GooglePlayConnection>.instance.connect();
	}

	private void FixedUpdate()
	{
		if (GooglePlayConnection.state == GPConnectionState.STATE_CONNECTED)
		{
			GameObject[] array = hideOnConnect;
			foreach (GameObject gameObject in array)
			{
				gameObject.SetActive(false);
			}
			GameObject[] array2 = showOnConnect;
			foreach (GameObject gameObject2 in array2)
			{
				gameObject2.SetActive(true);
			}
		}
		else
		{
			GameObject[] array3 = hideOnConnect;
			foreach (GameObject gameObject3 in array3)
			{
				gameObject3.SetActive(true);
			}
			GameObject[] array4 = showOnConnect;
			foreach (GameObject gameObject4 in array4)
			{
				gameObject4.SetActive(false);
			}
		}
	}

	private void LoadAllStates()
	{
		Singleton<GoogleCloudManager>.instance.loadAllStates();
	}

	private void LoadState()
	{
		Singleton<GoogleCloudManager>.instance.loadState(0);
	}

	private void UpdateState()
	{
		string s = "Hello bytes data";
		UTF8Encoding uTF8Encoding = new UTF8Encoding();
		byte[] bytes = uTF8Encoding.GetBytes(s);
		Singleton<GoogleCloudManager>.instance.updateState(0, bytes);
	}

	private void DeleteState()
	{
		Singleton<GoogleCloudManager>.instance.deleteState(0);
		Singleton<GoogleCloudManager>.instance.addEventListener("key_deleted", OnStateDeleted);
	}

	private void OnStateConflict(CEvent e)
	{
		GoogleCloudResult googleCloudResult = e.data as GoogleCloudResult;
		AndroidNative.showMessage("OnStateUpdated", string.Concat(googleCloudResult.message, "\n State ID: ", googleCloudResult.stateKey, "\n State Data: ", googleCloudResult.stateData, "\n State Conflict: ", googleCloudResult.serverConflictData, "\n State resolve: ", googleCloudResult.resolvedVersion));
		Singleton<GoogleCloudManager>.instance.resolveState(googleCloudResult.stateKey, googleCloudResult.stateData, googleCloudResult.resolvedVersion);
	}

	private void OnStateUpdated(CEvent e)
	{
		GoogleCloudResult googleCloudResult = e.data as GoogleCloudResult;
		AndroidNative.showMessage("State Updated", googleCloudResult.message + "\n State ID: " + googleCloudResult.stateKey + "\n State Data: " + googleCloudResult.stateDataString);
	}

	private void OnAllLoaded(CEvent e)
	{
		GoogleCloudResult googleCloudResult = e.data as GoogleCloudResult;
		AndroidNative.showMessage("All States Loaded", googleCloudResult.message + "\nTotal states: " + Singleton<GoogleCloudManager>.instance.states.Count);
	}

	private void OnStateDeleted(CEvent e)
	{
		Singleton<GoogleCloudManager>.instance.removeEventListener("key_deleted", OnStateDeleted);
		GoogleCloudResult googleCloudResult = e.data as GoogleCloudResult;
		AndroidNative.showMessage("KeyDeleted", googleCloudResult.message + "\n for state key: " + googleCloudResult.stateKey);
	}
}
