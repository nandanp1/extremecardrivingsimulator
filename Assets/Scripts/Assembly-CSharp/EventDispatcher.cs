using System.Collections.Generic;
using UnityEngine;

public class EventDispatcher : MonoBehaviour, IDispatcher
{
	private Dictionary<int, List<EventHandlerFunction>> listners = new Dictionary<int, List<EventHandlerFunction>>();

	private Dictionary<int, List<DataEventHandlerFunction>> dataListners = new Dictionary<int, List<DataEventHandlerFunction>>();

	public void addEventListener(string eventName, EventHandlerFunction handler)
	{
		addEventListener(eventName.GetHashCode(), handler, eventName);
	}

	public void addEventListener(int eventID, EventHandlerFunction handler)
	{
		addEventListener(eventID, handler, eventID.ToString());
	}

	private void addEventListener(int eventID, EventHandlerFunction handler, string eventGraphName)
	{
		if (listners.ContainsKey(eventID))
		{
			listners[eventID].Add(handler);
			return;
		}
		List<EventHandlerFunction> list = new List<EventHandlerFunction>();
		list.Add(handler);
		listners.Add(eventID, list);
	}

	public void addEventListener(string eventName, DataEventHandlerFunction handler)
	{
		addEventListener(eventName.GetHashCode(), handler, eventName);
	}

	public void addEventListener(int eventID, DataEventHandlerFunction handler)
	{
		addEventListener(eventID, handler, eventID.ToString());
	}

	private void addEventListener(int eventID, DataEventHandlerFunction handler, string eventGraphName)
	{
		if (dataListners.ContainsKey(eventID))
		{
			dataListners[eventID].Add(handler);
			return;
		}
		List<DataEventHandlerFunction> list = new List<DataEventHandlerFunction>();
		list.Add(handler);
		dataListners.Add(eventID, list);
	}

	public void removeEventListener(string eventName, EventHandlerFunction handler)
	{
		removeEventListener(eventName.GetHashCode(), handler, eventName);
	}

	public void removeEventListener(int eventID, EventHandlerFunction handler)
	{
		removeEventListener(eventID, handler, eventID.ToString());
	}

	public void removeEventListener(int eventID, EventHandlerFunction handler, string eventGraphName)
	{
		if (listners.ContainsKey(eventID))
		{
			List<EventHandlerFunction> list = listners[eventID];
			list.Remove(handler);
			if (list.Count == 0)
			{
				listners.Remove(eventID);
			}
		}
	}

	public void removeEventListener(string eventName, DataEventHandlerFunction handler)
	{
		removeEventListener(eventName.GetHashCode(), handler, eventName);
	}

	public void removeEventListener(int eventID, DataEventHandlerFunction handler)
	{
		removeEventListener(eventID, handler, eventID.ToString());
	}

	public void removeEventListener(int eventID, DataEventHandlerFunction handler, string eventGraphName)
	{
		if (dataListners.ContainsKey(eventID))
		{
			List<DataEventHandlerFunction> list = dataListners[eventID];
			list.Remove(handler);
			if (list.Count == 0)
			{
				dataListners.Remove(eventID);
			}
		}
	}

	public void dispatchEvent(string eventName)
	{
		dispatch(eventName.GetHashCode(), null, eventName);
	}

	public void dispatchEvent(string eventName, object data)
	{
		dispatch(eventName.GetHashCode(), data, eventName);
	}

	public void dispatchEvent(int eventID)
	{
		dispatch(eventID, null, string.Empty);
	}

	public void dispatchEvent(int eventID, object data)
	{
		dispatch(eventID, data, string.Empty);
	}

	public void dispatch(string eventName)
	{
		dispatch(eventName.GetHashCode(), null, eventName);
	}

	public void dispatch(string eventName, object data)
	{
		dispatch(eventName.GetHashCode(), data, eventName);
	}

	public void dispatch(int eventID)
	{
		dispatch(eventID, null, string.Empty);
	}

	public void dispatch(int eventID, object data)
	{
		dispatch(eventID, data, string.Empty);
	}

	private void dispatch(int eventID, object data, string eventName)
	{
		CEvent cEvent = new CEvent(eventID, eventName, data, this);
		if (dataListners.ContainsKey(eventID))
		{
			List<DataEventHandlerFunction> list = cloenArray(dataListners[eventID]);
			int count = list.Count;
			for (int i = 0; i < count; i++)
			{
				if (cEvent.canBeDisptached(list[i].Target))
				{
					list[i](cEvent);
				}
			}
		}
		if (!listners.ContainsKey(eventID))
		{
			return;
		}
		List<EventHandlerFunction> list2 = cloenArray(listners[eventID]);
		int count2 = list2.Count;
		for (int j = 0; j < count2; j++)
		{
			if (cEvent.canBeDisptached(list2[j].Target))
			{
				list2[j]();
			}
		}
	}

	public void clearEvents()
	{
		listners.Clear();
		dataListners.Clear();
	}

	private List<EventHandlerFunction> cloenArray(List<EventHandlerFunction> list)
	{
		List<EventHandlerFunction> list2 = new List<EventHandlerFunction>();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	private List<DataEventHandlerFunction> cloenArray(List<DataEventHandlerFunction> list)
	{
		List<DataEventHandlerFunction> list2 = new List<DataEventHandlerFunction>();
		int count = list.Count;
		for (int i = 0; i < count; i++)
		{
			list2.Add(list[i]);
		}
		return list2;
	}

	protected virtual void OnDestroy()
	{
		clearEvents();
	}
}
