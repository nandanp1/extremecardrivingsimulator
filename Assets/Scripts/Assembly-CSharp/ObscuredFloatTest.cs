using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class ObscuredFloatTest : MonoBehaviour
{
	internal float healthBar = 11.4f;

	internal ObscuredFloat obscuredHealthBar = 11.4f;

	internal bool useRegular = true;

	private void Start()
	{
		Debug.Log("===== ObscuredFloatTest =====\n");
		ObscuredFloat.SetNewCryptoKey(404);
		healthBar = 99.9f;
		Debug.Log("Original health bar:\n" + healthBar);
		obscuredHealthBar = healthBar;
		Debug.Log("How your health bar is stored in memory when obscured:\n" + obscuredHealthBar.GetEncrypted());
		float num = 100f;
		ObscuredFloat obscuredFloat = 60.3f;
		ObscuredFloat.SetNewCryptoKey(666);
		++obscuredFloat;
		obscuredFloat = (float)obscuredFloat - 2f;
		--obscuredFloat;
		obscuredFloat = num - (float)obscuredFloat;
		obscuredHealthBar = (healthBar = (obscuredFloat = 0f));
	}

	public void UseRegular()
	{
		useRegular = true;
		healthBar += Random.Range(-10f, 50f);
		obscuredHealthBar = 11f;
		Debug.Log("Try to change this float in memory:\n" + healthBar);
	}

	public void UseObscured()
	{
		useRegular = false;
		obscuredHealthBar = (float)obscuredHealthBar + Random.Range(-10f, 50f);
		healthBar = 11f;
		Debug.Log("Try to change this float in memory:\n" + obscuredHealthBar);
	}
}
