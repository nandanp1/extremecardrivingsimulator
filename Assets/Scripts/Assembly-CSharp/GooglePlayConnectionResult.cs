public class GooglePlayConnectionResult
{
	public GooglePlayConnectionResultCode code;

	public bool HasResolution;

	public bool IsSuccess
	{
		get
		{
			if (code == GooglePlayConnectionResultCode.SUCCESS)
			{
				return true;
			}
			return false;
		}
	}
}
