using System;
using System.Collections.Generic;

[Serializable]
public class CurvyConnection
{
	public enum HeadingMode
	{
		Minus = -1,
		Sharp = 0,
		Plus = 1,
		Auto = 2
	}

	public enum SyncMode
	{
		NoSync = 0,
		SyncPos = 1,
		SyncRot = 2,
		SyncPosAndRot = 3
	}

	public CurvySplineSegment Owner;

	public CurvySplineSegment Other;

	public HeadingMode Heading;

	public SyncMode Sync;

	public string Tags;

	public bool Active
	{
		get
		{
			return Owner != null && Other != null;
		}
	}

	public CurvyConnection()
	{
		Owner = null;
		Other = null;
	}

	public void Disconnect()
	{
		if (Active)
		{
			Other.ConnectedBy.Remove(Owner);
		}
		Owner = null;
		Other = null;
	}

	public void Validate()
	{
		if (Active && !Other.ConnectedBy.Contains(Owner))
		{
			Owner = null;
			Other = null;
		}
	}

	public void RefreshSplines()
	{
		if (Active)
		{
			Owner.Spline.Refresh();
			Other.Spline.Refresh();
		}
	}

	public void Set(CurvySplineSegment me, CurvySplineSegment other, HeadingMode heading, SyncMode sync, params string[] tags)
	{
		Owner = me;
		Other = other;
		Other.ConnectedBy.Add(me);
		Heading = heading;
		Sync = sync;
		SetTags(tags);
	}

	public CurvySplineSegment GetCounterpart(CurvySplineSegment cp)
	{
		if (Owner == cp)
		{
			return Other;
		}
		if (Other == cp)
		{
			return Owner;
		}
		return null;
	}

	public CurvySplineSegment GetFromSpline(CurvySpline spline)
	{
		if ((bool)Owner && Owner.Spline == spline)
		{
			return Owner;
		}
		if ((bool)Other && Other.Spline == spline)
		{
			return Other;
		}
		return null;
	}

	public string[] GetTags()
	{
		return Tags.Split(' ');
	}

	public void SetTags(params string[] tags)
	{
		Tags = string.Join(" ", tags).Trim(' ');
	}

	public void AddTags(params string[] tags)
	{
		string[] tags2 = GetTags();
		string[] array = new string[tags2.Length + tags.Length];
		tags2.CopyTo(array, 0);
		tags.CopyTo(array, tags2.Length);
		SetTags(array);
	}

	public void RemoveTags(params string[] tags)
	{
		List<string> list = new List<string>(GetTags());
		for (int i = 0; i < tags.Length; i++)
		{
			if (list.Contains(tags[i]))
			{
				list.Remove(tags[i]);
			}
		}
		SetTags(list.ToArray());
	}

	public bool Matches(params string[] tags)
	{
		if (tags.Length == 0)
		{
			return true;
		}
		string text = " " + Tags + " ";
		for (int i = 0; i < tags.Length; i++)
		{
			if (text.Contains(" " + tags[i] + " "))
			{
				return true;
			}
		}
		return false;
	}

	public List<string> MatchingTags(params string[] tags)
	{
		List<string> list = new List<string>();
		if (tags.Length > 0)
		{
			string text = " " + Tags + " ";
			for (int i = 0; i < tags.Length; i++)
			{
				if (text.Contains(" " + tags[i] + " "))
				{
					list.Add(tags[i]);
				}
			}
		}
		return list;
	}

	public override string ToString()
	{
		if (Owner.Spline == Other.Spline)
		{
			return Owner.name + "->" + Other.name;
		}
		return Owner.ToString() + "->" + Other.ToString();
	}

	public string ToStringOwnerSplineOrCP()
	{
		if (Owner.Spline == Other.Spline)
		{
			return Owner.name;
		}
		return Owner.Spline.name;
	}

	public string ToStringOtherSplineOrCP()
	{
		if (Owner.Spline == Other.Spline)
		{
			return Other.name;
		}
		return Other.Spline.name;
	}

	public static List<string> GetTags(bool uniqueTagsOnly, params CurvyConnection[] connections)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < connections.Length; i++)
		{
			string[] array = connections[i].Tags.Split(' ');
			if (uniqueTagsOnly)
			{
				for (int j = 0; j < array.Length; j++)
				{
					if (!list.Contains(array[j]))
					{
						list.Add(array[j]);
					}
				}
			}
			else
			{
				list.AddRange(array);
			}
		}
		return list;
	}

	public static CurvyConnection GetBestMatchingConnection(List<CurvyConnection> connections, params string[] tags)
	{
		CurvyConnection result = null;
		int num = 0;
		for (int i = 0; i < connections.Count; i++)
		{
			int count = connections[i].MatchingTags(tags).Count;
			if (count > num)
			{
				num = count;
				result = connections[i];
			}
		}
		return result;
	}
}
