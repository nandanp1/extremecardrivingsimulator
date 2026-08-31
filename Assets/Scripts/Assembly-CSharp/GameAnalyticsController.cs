using System.Collections.Generic;
using UnityEngine;

public class GameAnalyticsController : MonoBehaviour
{
	public static GameAnalyticsController instance;

	private void Awake()
	{
		instance = this;
	}

	public void SendEvent(string idEvent)
	{
	}

	public void SendEvent(string idEvent, Dictionary<string, object> dictData)
	{
	}

	public void SendRevenueEvent(double amount, string itemId, string receipt, string signature)
	{
	}
}
