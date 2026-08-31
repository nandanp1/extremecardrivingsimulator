public class CarEntryInfo
{
	public string carIconName;

	public int carID;

	public InfoCar infoCar;

	public CarEntryView entryView;

	public UnlockInfo[] unlockInfo;

	public CarEntryInfo(int UnlockInfoArrayTam = 1)
	{
		infoCar = new InfoCar();
		unlockInfo = new UnlockInfo[UnlockInfoArrayTam];
		for (int i = 0; i < UnlockInfoArrayTam; i++)
		{
			unlockInfo[i] = new UnlockInfo();
			unlockInfo[i].carPrice = 0;
			unlockInfo[i].challengeKind = Challenge.None;
			unlockInfo[i].challengePogress = 0f;
			unlockInfo[i].challengesCompleted = 0;
			unlockInfo[i].challengesTotal = 0;
			unlockInfo[i].challengeTarget = 0f;
			unlockInfo[i].collectablesGot = 0;
			unlockInfo[i].collectablesTotal = 0;
			unlockInfo[i].completed = false;
			unlockInfo[i].iconUnlockOptionName = string.Empty;
			unlockInfo[i].playerMoney = 0;
		}
	}
}
