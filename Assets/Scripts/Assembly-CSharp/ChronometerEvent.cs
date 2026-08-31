using UnityEngine;

public class ChronometerEvent : MonoBehaviour
{
	public event TimeOver OnTimeOver;

	public void TimeOver()
	{
		if (OnTimeOver != null)
		{
			OnTimeOver();
		}
		else
		{
			Debug.Log("OnTimeOver: nadie esta escuchando este evento");
		}
	}
}
