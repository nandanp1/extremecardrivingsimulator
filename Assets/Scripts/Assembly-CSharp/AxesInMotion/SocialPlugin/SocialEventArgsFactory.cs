using System;

namespace AxesInMotion.SocialPlugin
{
	public class SocialEventArgsFactory
	{
		public static ISocialEventArgs Create(SocialEventType type)
		{
			switch (type)
			{
			case SocialEventType.Initialized:
				return new SocialEventArgs(SocialEventType.Initialized);
			case SocialEventType.LoginSuccess:
				return new SocialEventArgs(SocialEventType.LoginSuccess);
			case SocialEventType.LoginFailed:
				return new SocialEventArgs(SocialEventType.LoginSuccess);
			case SocialEventType.InviteSuccess:
				return new InviteSocialEventArgs();
			case SocialEventType.FeedSuccess:
				return new SocialEventArgs(SocialEventType.FeedSuccess);
			case SocialEventType.FeedFailed:
				return new SocialEventArgs(SocialEventType.FeedFailed);
			case SocialEventType.LoginCanceled:
				return new SocialEventArgs(SocialEventType.LoginCanceled);
			default:
				throw new NotImplementedException();
			}
		}
	}
}
