using UnityEngine;

namespace AxesInMotion.SocialPlugin
{
	public interface ISocialFeed
	{
		void FeedImage(byte[] imageBytes, string message, WWWForm wwwForm, SocialFeedCallbackDelegate socialFeedCallbackDelegate, string mimetype = "");
	}
}
