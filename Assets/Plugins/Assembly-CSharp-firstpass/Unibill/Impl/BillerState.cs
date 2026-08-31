namespace Unibill.Impl
{
	public enum BillerState
	{
		NOT_INITIALISED = 0,
		INITIALISING = 1,
		INITIALISED = 2,
		INITIALISED_WITH_ERROR = 3,
		INITIALISED_WITH_CRITICAL_ERROR = 4
	}
}
