using UnityEngine;

public class MoreGames : MonoBehaviour
{
	public enum Game
	{
		None = 0,
		FlightSimulator = 1,
		ZombieSniper = 2,
		Brasil2014 = 3,
		TaxiCity = 4,
		Island = 5
	}

	public const string BUNDLE_IDENTIFIER_ANDROID = "com.aim.racing";

	public const string GOOGLE_PLAY_URL = "market://details?id=";

	public const string GOOGLE_PLAY_DEVELOPER_URL = "market://search?q=pub:AxesInMotion Racing";

	public const string FLIGHT_ANDROID_ID = "com.aim.airplanefight";

	public const string FLIGHT_IOS_ID = "896629812";

	public const string ZOMBIE_ANDROID_ID = "com.aim.zombiesniper";

	public const string ZOMBIE_IOS_ID = "896631751";

	public const string BRASIL_ANDROID_ID = "com.sportgames.world";

	public const string BRASIL_IOS_ID = "834366265";

	public const string TAXI_ANDROID_ID = "com.aim.taxicity";

	public const string TAXI_IOS_ID = "896143636";

	public const string ISLAND_ANDROID_ID = "com.aim.racingisland";

	public const string ISLAND_IOS_ID = "NONE";

	public Game game;

	private void OnClick()
	{
		string text = string.Empty;
		switch (game)
		{
		case Game.FlightSimulator:
			text = "com.aim.airplanefight";
			break;
		case Game.ZombieSniper:
			text = "com.aim.zombiesniper";
			break;
		case Game.Brasil2014:
			text = "com.sportgames.world";
			break;
		case Game.TaxiCity:
			text = "com.aim.taxicity";
			break;
		case Game.Island:
			text = "com.aim.racingisland";
			break;
		}
		text = "market://details?id=" + text;
		ShowGameInStore(text);
	}

	private void ShowGameInStore(string gameURL)
	{
		if (!(gameURL == string.Empty))
		{
			Application.OpenURL(gameURL);
		}
	}
}
