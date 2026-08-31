namespace AxesInMotion.SocialPlugin
{
	public interface ISocialLeaderboard
	{
		void ReportScore(string leaderboard, long score);

		void ShowLeaderboard(string leaderboard);

		void ShowAllLeaderboard();

		void ReportAchievement(string achievement, float progress);

		void IncrementAchievement(string achievement, int steps);

		void ShowAchievement();
	}
}
