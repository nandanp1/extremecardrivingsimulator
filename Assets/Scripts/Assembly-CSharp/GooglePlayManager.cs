using System;
using System.Collections.Generic;
using UnityEngine;

public class GooglePlayManager : Singleton<GooglePlayManager>
{
	public const string SCORE_SUBMITED = "score_submited";

	public const string LEADERBOARDS_LOEADED = "leaderboards_loeaded";

	public const string FRIENDS_LOADED = "players_loaded";

	public const string ACHIEVEMENT_UPDATED = "achievement_updated";

	public const string ACHIEVEMENTS_LOADED = "achievements_loaded";

	public const string SCORE_REQUEST_RECEIVED = "score_request_received";

	private GooglePlayerTemplate _player;

	private Dictionary<string, GPLeaderBoard> _leaderBoards = new Dictionary<string, GPLeaderBoard>();

	private Dictionary<string, GPAchievement> _achievements = new Dictionary<string, GPAchievement>();

	private Dictionary<string, GooglePlayerTemplate> _players = new Dictionary<string, GooglePlayerTemplate>();

	private List<string> _friendsList = new List<string>();

	public GooglePlayerTemplate player
	{
		get
		{
			return _player;
		}
	}

	public Dictionary<string, GooglePlayerTemplate> players
	{
		get
		{
			return _players;
		}
	}

	public Dictionary<string, GPLeaderBoard> leaderBoards
	{
		get
		{
			return _leaderBoards;
		}
	}

	public Dictionary<string, GPAchievement> achievements
	{
		get
		{
			return _achievements;
		}
	}

	public List<string> friendsList
	{
		get
		{
			return _friendsList;
		}
	}

	private void Awake()
	{
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public void Create()
	{
		Debug.Log("GooglePlayManager was created");
	}

	public void showAchivmentsUI()
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.showAchivmentsUI();
		}
	}

	public void showLeaderBoardsUI()
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.showLeaderBoardsUI();
		}
	}

	public void showLeaderBoard(string leaderboardName)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.showLeaderBoard(leaderboardName);
		}
	}

	public void showLeaderBoardById(string leaderboardId)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.showLeaderBoardById(leaderboardId);
		}
	}

	public void submitScore(string leaderboardName, int score)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.submitScore(leaderboardName, score);
		}
	}

	public void submitScoreById(string leaderboardId, int score)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.submitScoreById(leaderboardId, score);
		}
	}

	public void loadLeaderBoards()
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.loadLeaderBoards();
		}
	}

	public void loadPlayerCenteredScores(string leaderboardId, GPBoardTimeSpan span, GPCollectionType collection, int maxResults)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.loadPlayerCenteredScores(leaderboardId, (int)span, (int)collection, maxResults);
		}
	}

	public void loadTopScores(string leaderboardId, GPBoardTimeSpan span, GPCollectionType collection, int maxResults)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.loadTopScores(leaderboardId, (int)span, (int)collection, maxResults);
		}
	}

	public void reportAchievement(string achievementName)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.reportAchievement(achievementName);
		}
	}

	public void reportAchievementById(string achievementId)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.reportAchievementById(achievementId);
		}
	}

	public void revealAchievement(string achievementName)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.revealAchievement(achievementName);
		}
	}

	public void revealAchievementById(string achievementId)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.revealAchievementById(achievementId);
		}
	}

	public void incrementAchievement(string achievementName, int numsteps)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.incrementAchievement(achievementName, numsteps.ToString());
		}
	}

	public void incrementAchievementById(string achievementId, int numsteps)
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.incrementAchievementById(achievementId, numsteps.ToString());
		}
	}

	public void loadAchievements()
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.loadAchivments();
		}
	}

	public void loadConnectedPlayers()
	{
		if (GooglePlayConnection.CheckState())
		{
			AndroidNative.loadConnectedPlayers();
		}
	}

	public GPLeaderBoard GetLeaderBoard(string leaderboardId)
	{
		if (_leaderBoards.ContainsKey(leaderboardId))
		{
			return _leaderBoards[leaderboardId];
		}
		return null;
	}

	public GPAchievement GetAchievementd(string achievementId)
	{
		if (_achievements.ContainsKey(achievementId))
		{
			return _achievements[achievementId];
		}
		return null;
	}

	public GooglePlayerTemplate GetPlayerById(string playerId)
	{
		if (players.ContainsKey(playerId))
		{
			return players[playerId];
		}
		return null;
	}

	private void OnAchievementsLoaded(string data)
	{
		string[] array = data.Split("|"[0]);
		GooglePlayResult googlePlayResult = new GooglePlayResult(array[0]);
		if (googlePlayResult.isSuccess)
		{
			_achievements.Clear();
			for (int i = 1; i < array.Length && !(array[i] == "endofline"); i += 7)
			{
				GPAchievement gPAchievement = new GPAchievement(array[i], array[i + 1], array[i + 2], array[i + 3], array[i + 4], array[i + 5], array[i + 6]);
				Debug.Log(gPAchievement.name);
				Debug.Log(gPAchievement.type);
				_achievements.Add(gPAchievement.id, gPAchievement);
			}
			Debug.Log("Loaded: " + _achievements.Count + " Achievements");
		}
		dispatch("achievements_loaded", googlePlayResult);
	}

	private void OnAchievementUpdated(string data)
	{
		string[] array = data.Split("|"[0]);
		GooglePlayResult googlePlayResult = new GooglePlayResult(array[0]);
		googlePlayResult.achievementId = array[1];
		dispatch("achievement_updated", googlePlayResult);
	}

	private void OnScoreDataRecevide(string data)
	{
		Debug.Log("OnScoreDataRecevide");
		string[] array = data.Split("|"[0]);
		GooglePlayResult googlePlayResult = new GooglePlayResult(array[0]);
		if (googlePlayResult.isSuccess)
		{
			GPBoardTimeSpan gPBoardTimeSpan = (GPBoardTimeSpan)Convert.ToInt32(array[1]);
			GPCollectionType gPCollectionType = (GPCollectionType)Convert.ToInt32(array[2]);
			string text = array[3];
			string lName = array[4];
			GPLeaderBoard gPLeaderBoard;
			if (_leaderBoards.ContainsKey(text))
			{
				gPLeaderBoard = _leaderBoards[text];
			}
			else
			{
				gPLeaderBoard = new GPLeaderBoard(text, lName);
				Debug.Log("Added: " + text);
				_leaderBoards.Add(text, gPLeaderBoard);
			}
			for (int i = 5; i < array.Length && !(array[i] == "endofline"); i += 6)
			{
				int vScore = Convert.ToInt32(array[i]);
				int num = Convert.ToInt32(array[i + 1]);
				string text2 = array[i + 2];
				if (!players.ContainsKey(text2))
				{
					GooglePlayerTemplate p = new GooglePlayerTemplate(text2, array[i + 3], array[i + 4], array[i + 5]);
					AddPlayer(p);
				}
				GPScore score = new GPScore(vScore, num, gPBoardTimeSpan, gPCollectionType, gPLeaderBoard.id, text2);
				gPLeaderBoard.UpdateScore(score);
				if (text2.Equals(player.playerId))
				{
					gPLeaderBoard.UpdateCurrentPlayerRank(num, gPBoardTimeSpan, gPCollectionType);
				}
			}
		}
		dispatch("score_request_received", googlePlayResult);
	}

	private void OnLeaderboardDataLoaded(string data)
	{
		string[] array = data.Split("|"[0]);
		GooglePlayResult googlePlayResult = new GooglePlayResult(array[0]);
		if (googlePlayResult.isSuccess)
		{
			for (int i = 1; i < array.Length && !(array[i] == "endofline"); i += 26)
			{
				string text = array[i];
				string lName = array[i + 1];
				GPLeaderBoard gPLeaderBoard;
				if (_leaderBoards.ContainsKey(text))
				{
					gPLeaderBoard = _leaderBoards[text];
				}
				else
				{
					gPLeaderBoard = new GPLeaderBoard(text, lName);
					_leaderBoards.Add(text, gPLeaderBoard);
				}
				int num = i + 2;
				for (int j = 0; j < 6; j++)
				{
					int vScore = Convert.ToInt32(array[num]);
					int num2 = Convert.ToInt32(array[num + 1]);
					GPBoardTimeSpan gPBoardTimeSpan = (GPBoardTimeSpan)Convert.ToInt32(array[num + 2]);
					GPCollectionType gPCollectionType = (GPCollectionType)Convert.ToInt32(array[num + 3]);
					GPScore score = new GPScore(vScore, num2, gPBoardTimeSpan, gPCollectionType, gPLeaderBoard.id, player.playerId);
					num += 4;
					gPLeaderBoard.UpdateScore(score);
					gPLeaderBoard.UpdateCurrentPlayerRank(num2, gPBoardTimeSpan, gPCollectionType);
				}
			}
			Debug.Log("Loaded: " + _leaderBoards.Count + " Leaderboards");
		}
		dispatch("leaderboards_loeaded", googlePlayResult);
	}

	private void OnScoreSubmitted(string data)
	{
		if (data.Equals(string.Empty))
		{
			Debug.Log("GooglePlayManager OnScoreSubmitted, no data avaiable");
			return;
		}
		string[] array = data.Split("|"[0]);
		GooglePlayResult googlePlayResult = new GooglePlayResult(array[0]);
		googlePlayResult.leaderboardId = array[1];
		dispatch("score_submited", googlePlayResult);
	}

	private void OnPlayerDataLoaded(string data)
	{
		Debug.Log("OnPlayerDataLoaded");
		if (data.Equals(string.Empty))
		{
			Debug.Log("GooglePlayManager OnPlayerLoaded, no data avaiable");
			return;
		}
		string[] array = data.Split("|"[0]);
		_player = new GooglePlayerTemplate(array[0], array[1], array[2], array[3]);
		AddPlayer(_player);
	}

	private void OnPlayersLoaded(string data)
	{
		string[] array = data.Split("|"[0]);
		GooglePlayResult googlePlayResult = new GooglePlayResult(array[0]);
		if (googlePlayResult.isSuccess)
		{
			for (int i = 1; i < array.Length && !(array[i] == "endofline"); i += 4)
			{
				GooglePlayerTemplate googlePlayerTemplate = new GooglePlayerTemplate(array[i], array[i + 1], array[i + 2], array[i + 3]);
				AddPlayer(googlePlayerTemplate);
				_friendsList.Add(googlePlayerTemplate.playerId);
			}
		}
		Debug.Log("OnPlayersLoaded, total:" + players.Count);
		dispatch("players_loaded", googlePlayResult);
	}

	private void AddPlayer(GooglePlayerTemplate p)
	{
		if (!_players.ContainsKey(p.playerId))
		{
			_players.Add(p.playerId, p);
		}
	}
}
