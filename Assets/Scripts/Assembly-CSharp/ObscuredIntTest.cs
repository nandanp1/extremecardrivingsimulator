using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class ObscuredIntTest : MonoBehaviour
{
	internal int cleanLivesCount = 11;

	internal ObscuredInt obscuredLivesCount = 11;

	internal bool useRegular;

	private void Start()
	{
		Debug.Log("===== ObscuredIntTest =====\n");
		cleanLivesCount = 5;
		Debug.Log("Original lives count:\n" + cleanLivesCount);
		obscuredLivesCount = cleanLivesCount;
		Debug.Log("How your lives count is stored in memory when obscured:\n" + obscuredLivesCount.GetEncrypted());
		ObscuredInt.SetNewCryptoKey(666);
		ObscuredInt obscuredInt = 100;
		obscuredInt = (int)obscuredInt - 10;
		obscuredInt = (int)obscuredInt + 100;
		obscuredInt = (int)obscuredInt / 10;
		ObscuredInt.SetNewCryptoKey(888);
		++obscuredInt;
		ObscuredInt.SetNewCryptoKey(999);
		++obscuredInt;
		--obscuredInt;
		Debug.Log(string.Concat("Lives count: ", obscuredInt, " (", obscuredInt.ToString("X"), "h)"));
	}

	public void UseRegular()
	{
		useRegular = true;
		cleanLivesCount += Random.Range(-10, 50);
		obscuredLivesCount = 11;
		Debug.Log("Try to change this int in memory:\n" + cleanLivesCount);
	}

	public void UseObscured()
	{
		useRegular = false;
		obscuredLivesCount = (int)obscuredLivesCount + Random.Range(-10, 50);
		cleanLivesCount = 11;
		Debug.Log("Try to change this int in memory:\n" + obscuredLivesCount);
	}
}
