using UnityEngine;

public class SocialPlatfromSettings : ScriptableObject
{
	public const string VERSION_NUMBER = "2.7";

	private const string ISNSettingsAssetName = "SocialSettings";

	private const string ISNSettingsPath = "Extensions/GooglePlayCommon/Resources";

	private const string ISNSettingsAssetExtension = ".asset";

	public string fb_scopes = "email,publish_actions";

	public string TWITTER_CONSUMER_KEY = "REPLACE_WITH_YOUR_CONSUMER_KEY";

	public string TWITTER_CONSUMER_SECRET = "REPLACE_WITH_YOUR_CONSUMER_SECRET";

	private static SocialPlatfromSettings instance;

	public static SocialPlatfromSettings Instance
	{
		get
		{
			if (instance == null)
			{
				instance = Resources.Load("SocialSettings") as SocialPlatfromSettings;
				if (instance == null)
				{
					instance = ScriptableObject.CreateInstance<SocialPlatfromSettings>();
				}
			}
			return instance;
		}
	}
}
