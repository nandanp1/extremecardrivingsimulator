using System;

public enum DisconnectCause
{
	Inactivity = 0,
	LatencyTooHigh = 1,
	ExceptionOnConnect = 1023,
	SecurityExceptionOnConnect = 1022,
	[Obsolete("Replaced by clearer: DisconnectByClientTimeout")]
	TimeoutDisconnect = 1040,
	DisconnectByClientTimeout = TimeoutDisconnect,
	InternalReceiveException = 1039,
	[Obsolete("Replaced by clearer: DisconnectByServerTimeout")]
	DisconnectByServer = 1041,
	DisconnectByServerTimeout = DisconnectByServer,
	DisconnectByServerLogic = 1043,
	DisconnectByServerUserLimit = 1042,
	Exception = 1026,
	InvalidRegion = 32756,
	MaxCcuReached = 32757,
	InvalidAuthentication = 32767,
	AuthenticationTicketExpired = 32753
}
