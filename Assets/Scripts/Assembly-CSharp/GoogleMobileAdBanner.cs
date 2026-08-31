using UnityEngine;

public interface GoogleMobileAdBanner
{
	int id { get; }

	int width { get; }

	int height { get; }

	bool IsLoaded { get; }

	bool IsOnScreen { get; }

	bool ShowOnLoad { get; set; }

	GADBannerSize size { get; }

	TextAnchor anchor { get; }

	void Hide();

	void Show();

	void Refresh();

	void addEventListener(string eventName, EventHandlerFunction handler);

	void addEventListener(string eventName, DataEventHandlerFunction handler);

	void removeEventListener(string eventName, EventHandlerFunction handler);

	void removeEventListener(string eventName, DataEventHandlerFunction handler);
}
