using System;

public class AndroidAppInfoLoader : Singleton<AndroidAppInfoLoader>
{
	public static string PACKAGE_INFO_LOADED = "package_info_loaded";

	public PackageAppInfo PacakgeInfo = new PackageAppInfo();

	public void LoadPackageInfo()
	{
		AndroidNative.LoadPackageInfo();
	}

	private void OnPackageInfoLoaded(string data)
	{
		string[] array = data.Split("|"[0]);
		PacakgeInfo.versionName = array[0];
		PacakgeInfo.versionCode = array[1];
		PacakgeInfo.packageName = array[2];
		PacakgeInfo.lastUpdateTime = Convert.ToInt64(array[3]);
		PacakgeInfo.sharedUserId = array[3];
		PacakgeInfo.sharedUserLabel = array[4];
		dispatch(PACKAGE_INFO_LOADED, PacakgeInfo);
	}
}
