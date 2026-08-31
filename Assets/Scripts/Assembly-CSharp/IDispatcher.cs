public interface IDispatcher
{
	void addEventListener(string eventName, EventHandlerFunction handler);

	void addEventListener(int eventID, EventHandlerFunction handler);

	void addEventListener(string eventName, DataEventHandlerFunction handler);

	void addEventListener(int eventID, DataEventHandlerFunction handler);

	void removeEventListener(string eventName, EventHandlerFunction handler);

	void removeEventListener(int eventID, EventHandlerFunction handler);

	void removeEventListener(string eventName, DataEventHandlerFunction handler);

	void removeEventListener(int eventID, DataEventHandlerFunction handler);

	void dispatchEvent(int eventID);

	void dispatchEvent(int eventID, object data);

	void dispatchEvent(string eventName);

	void dispatchEvent(string eventName, object data);

	void dispatch(int eventID);

	void dispatch(int eventID, object data);

	void dispatch(string eventName);

	void dispatch(string eventName, object data);

	void clearEvents();
}
