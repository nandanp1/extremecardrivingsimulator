using System.Collections;

namespace AxesInMotion.SocialPlugin
{
	public class InviteSocialEventArgs : SocialEventArgs
	{
		public ICollection Recipients;

		public InviteSocialEventArgs()
			: base(SocialEventType.InviteSuccess)
		{
		}
	}
}
