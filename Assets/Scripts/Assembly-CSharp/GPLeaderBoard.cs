using System.Collections.Generic;

public class GPLeaderBoard
{
	private string _id;

	private string _name;

	public GPScoreCollection SocsialCollection = new GPScoreCollection();

	public GPScoreCollection GlobalCollection = new GPScoreCollection();

	public Dictionary<string, int> currentPlayerRank = new Dictionary<string, int>();

	public string id
	{
		get
		{
			return _id;
		}
	}

	public string name
	{
		get
		{
			return _name;
		}
	}

	public GPLeaderBoard(string lId, string lName)
	{
		_id = lId;
		_name = lName;
	}

	public List<GPScore> GetScoresList(GPBoardTimeSpan timeSpan, GPCollectionType collection)
	{
		GPScoreCollection gPScoreCollection = GlobalCollection;
		switch (collection)
		{
		case GPCollectionType.GLOBAL:
			gPScoreCollection = GlobalCollection;
			break;
		case GPCollectionType.FRIENDS:
			gPScoreCollection = SocsialCollection;
			break;
		}
		Dictionary<int, GPScore> dictionary = gPScoreCollection.AllTimeScores;
		switch (timeSpan)
		{
		case GPBoardTimeSpan.ALL_TIME:
			dictionary = gPScoreCollection.AllTimeScores;
			break;
		case GPBoardTimeSpan.TODAY:
			dictionary = gPScoreCollection.TodayScores;
			break;
		case GPBoardTimeSpan.WEEK:
			dictionary = gPScoreCollection.WeekScores;
			break;
		}
		List<GPScore> list = new List<GPScore>();
		list.AddRange(dictionary.Values);
		return list;
	}

	public GPScore GetScoreByPlayerId(string playerId, GPBoardTimeSpan timeSpan, GPCollectionType collection)
	{
		List<GPScore> scoresList = GetScoresList(timeSpan, collection);
		foreach (GPScore item in scoresList)
		{
			if (item.playerId.Equals(playerId))
			{
				return item;
			}
		}
		return null;
	}

	public GPScore GetScore(int rank, GPBoardTimeSpan timeSpan, GPCollectionType collection)
	{
		GPScoreCollection gPScoreCollection = GlobalCollection;
		switch (collection)
		{
		case GPCollectionType.GLOBAL:
			gPScoreCollection = GlobalCollection;
			break;
		case GPCollectionType.FRIENDS:
			gPScoreCollection = SocsialCollection;
			break;
		}
		Dictionary<int, GPScore> dictionary = gPScoreCollection.AllTimeScores;
		switch (timeSpan)
		{
		case GPBoardTimeSpan.ALL_TIME:
			dictionary = gPScoreCollection.AllTimeScores;
			break;
		case GPBoardTimeSpan.TODAY:
			dictionary = gPScoreCollection.TodayScores;
			break;
		case GPBoardTimeSpan.WEEK:
			dictionary = gPScoreCollection.WeekScores;
			break;
		}
		if (dictionary.ContainsKey(rank))
		{
			return dictionary[rank];
		}
		return null;
	}

	public GPScore GetCurrentPlayerScore(GPBoardTimeSpan timeSpan, GPCollectionType collection)
	{
		string key = timeSpan.ToString() + "_" + collection;
		if (currentPlayerRank.ContainsKey(key))
		{
			int rank = currentPlayerRank[key];
			return GetScore(rank, timeSpan, collection);
		}
		return null;
	}

	public void UpdateCurrentPlayerRank(int rank, GPBoardTimeSpan timeSpan, GPCollectionType collection)
	{
		string key = timeSpan.ToString() + "_" + collection;
		if (currentPlayerRank.ContainsKey(key))
		{
			currentPlayerRank[key] = rank;
		}
		else
		{
			currentPlayerRank.Add(key, rank);
		}
	}

	public void UpdateScore(GPScore score)
	{
		GPScoreCollection gPScoreCollection = GlobalCollection;
		switch (score.collection)
		{
		case GPCollectionType.GLOBAL:
			gPScoreCollection = GlobalCollection;
			break;
		case GPCollectionType.FRIENDS:
			gPScoreCollection = SocsialCollection;
			break;
		}
		Dictionary<int, GPScore> dictionary = gPScoreCollection.AllTimeScores;
		switch (score.timeSpan)
		{
		case GPBoardTimeSpan.ALL_TIME:
			dictionary = gPScoreCollection.AllTimeScores;
			break;
		case GPBoardTimeSpan.TODAY:
			dictionary = gPScoreCollection.TodayScores;
			break;
		case GPBoardTimeSpan.WEEK:
			dictionary = gPScoreCollection.WeekScores;
			break;
		}
		if (dictionary.ContainsKey(score.rank))
		{
			dictionary[score.rank] = score;
		}
		else
		{
			dictionary.Add(score.rank, score);
		}
	}
}
