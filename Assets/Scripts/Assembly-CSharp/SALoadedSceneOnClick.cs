public class SALoadedSceneOnClick : SAOnClickAction
{
	public string levelName;

	protected override void OnClick()
	{
		Singleton<SALevelLoader>.instance.LoadLevel(levelName);
	}
}
