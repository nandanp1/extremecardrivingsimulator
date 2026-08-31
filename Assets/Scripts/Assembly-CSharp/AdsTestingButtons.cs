using UnityEngine;

public class AdsTestingButtons : MonoBehaviour
{
	public enum TestingButtonFunction
	{
		AdColonyNormal = 1,
		Vungle = 2,
		UnityAds = 3,
		AdColonyRewards = 4
	}

	public TestingButtonFunction function;

	public void OnClick()
	{
	}
}
