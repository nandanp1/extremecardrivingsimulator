using CodeStage.AntiCheat.ObscuredTypes;
using UnityEngine;

public class ObscuredPrefsTest : MonoBehaviour
{
	private const string PREFS_NAME = "name";

	private const string PREFS_MONEY = "money";

	private const string PREFS_LIFE_BAR = "lifeBar";

	private const string PREFS_GAME_COMPLETE = "gameComplete";

	private const string PREFS_UINT = "demoUint";

	private const string PREFS_LONG = "demoLong";

	private const string PREFS_DOUBLE = "demoDouble";

	private const string PREFS_VECTOR3 = "demoVector3";

	private const string PREFS_RECT = "demoRect";

	private const string PREFS_BYTE_ARRAY = "demoByteArray";

	public string encryptionKey = "change me!";

	internal string gameData = string.Empty;

	private void OnApplicationQuit()
	{
		PlayerPrefs.DeleteKey("name");
		PlayerPrefs.DeleteKey("money");
		PlayerPrefs.DeleteKey("lifeBar");
		ObscuredPrefs.DeleteKey("name");
		ObscuredPrefs.DeleteKey("money");
		ObscuredPrefs.DeleteKey("lifeBar");
		ObscuredPrefs.DeleteKey("gameComplete");
		ObscuredPrefs.DeleteKey("demoUint");
		ObscuredPrefs.DeleteKey("demoLong");
		ObscuredPrefs.DeleteKey("demoDouble");
		ObscuredPrefs.DeleteKey("demoVector3");
		ObscuredPrefs.DeleteKey("demoRect");
		ObscuredPrefs.DeleteKey("demoByteArray");
	}

	private void Awake()
	{
		ObscuredPrefs.SetNewCryptoKey(encryptionKey);
	}

	public void SaveGame(bool obscured)
	{
		if (obscured)
		{
			ObscuredPrefs.SetString("name", "obscured focus oO");
			ObscuredPrefs.SetInt("money", 1500);
			ObscuredPrefs.SetFloat("lifeBar", 25.9f);
			ObscuredPrefs.SetBool("gameComplete", true);
			ObscuredPrefs.SetUInt("demoUint", 4294967290u);
			ObscuredPrefs.SetLong("demoLong", 3457657543456775432L);
			ObscuredPrefs.SetDouble("demoDouble", 345765.1312315678);
			ObscuredPrefs.SetRect("demoRect", new Rect(1f, 2f, 3f, 4f));
			ObscuredPrefs.SetVector3("demoVector3", new Vector3(123.312f, 453.12344f, 1223f));
			ObscuredPrefs.SetByteArray("demoByteArray", new byte[4] { 44, 104, 43, 32 });
			Debug.Log("Game saved using ObscuredPrefs. Try to find and change saved data now! ;)");
		}
		else
		{
			PlayerPrefs.SetString("name", "focus :D");
			PlayerPrefs.SetInt("money", 2100);
			PlayerPrefs.SetFloat("lifeBar", 88.4f);
			Debug.Log("Game saved with regular PlayerPrefs. Try to find and change saved data now (it's easy)!");
		}
		ObscuredPrefs.Save();
	}

	public void ReadSavedGame(bool obscured)
	{
		if (obscured)
		{
			byte[] byteArray = ObscuredPrefs.GetByteArray("demoByteArray", 0, 4);
			gameData = "Name: " + ObscuredPrefs.GetString("name") + "\n";
			string text = gameData;
			gameData = text + "Money: " + ObscuredPrefs.GetInt("money") + "\n";
			text = gameData;
			gameData = text + "Life bar: " + ObscuredPrefs.GetFloat("lifeBar") + "\n";
			text = gameData;
			gameData = text + "bool: " + ObscuredPrefs.GetBool("gameComplete") + "\n";
			text = gameData;
			gameData = text + "uint: " + ObscuredPrefs.GetUInt("demoUint") + "\n";
			text = gameData;
			gameData = text + "long: " + ObscuredPrefs.GetLong("demoLong") + "\n";
			text = gameData;
			gameData = text + "double: " + ObscuredPrefs.GetDouble("demoDouble") + "\n";
			text = gameData;
			gameData = string.Concat(text, "Vector3: ", ObscuredPrefs.GetVector3("demoVector3"), "\n");
			text = gameData;
			gameData = string.Concat(text, "Rect: ", ObscuredPrefs.GetRect("demoRect"), "\n");
			text = gameData;
			gameData = text + "byte[]: {" + byteArray[0] + "," + byteArray[1] + "," + byteArray[2] + "," + byteArray[3] + "}";
		}
		else
		{
			gameData = "Name: " + PlayerPrefs.GetString("name") + "\n";
			string text = gameData;
			gameData = text + "Money: " + PlayerPrefs.GetInt("money") + "\n";
			gameData = gameData + "Life bar: " + PlayerPrefs.GetFloat("lifeBar");
		}
	}
}
