namespace AxesInMotion.Network
{
	public enum DisconnectionCause
	{
		Exception = 0,
		ExceptionOnConnect = 1,
		Inactivity = 2,
		LatencyTooHigh = 3,
		DisconnectByClientTimeout = 4,
		InternalReceiveException = 5,
		DisconnectByServerTimeout = 6,
		DisconnectByServerLogic = 7,
		DisconnectByServerUserLimit = 8,
		InvalidRegion = 9,
		InvalidAuthentication = 10,
		MaxCcuReached = 11,
		DisconnectByClient = 12,
		DisconnectBecauseOfRoomIsFull = 13
	}
}
