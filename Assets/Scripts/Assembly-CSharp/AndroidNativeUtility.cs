public class AndroidNativeUtility
{
	public static void ShowPreloader(string title, string message)
	{
		AndroidNative.ShowPreloader(title, message);
	}

	public static void HidePreloader()
	{
		AndroidNative.HidePreloader();
	}
}
