using UnityEngine;

public class AndroidADBanner : EventDispatcherBase, GoogleMobileAdBanner
{
	private int _id;

	private GADBannerSize _size;

	private TextAnchor _anchor;

	private bool _IsLoaded;

	private bool _IsOnScreen;

	private bool firstLoad = true;

	private bool destroyOnLoad;

	private bool _ShowOnLoad = true;

	private int _width;

	private int _height;

	public int id
	{
		get
		{
			return _id;
		}
	}

	public int width
	{
		get
		{
			return _width;
		}
	}

	public int height
	{
		get
		{
			return _height;
		}
	}

	public GADBannerSize size
	{
		get
		{
			return _size;
		}
	}

	public bool IsLoaded
	{
		get
		{
			return _IsLoaded;
		}
	}

	public bool IsOnScreen
	{
		get
		{
			return _IsOnScreen;
		}
	}

	public bool ShowOnLoad
	{
		get
		{
			return _ShowOnLoad;
		}
		set
		{
			_ShowOnLoad = value;
		}
	}

	public TextAnchor anchor
	{
		get
		{
			return _anchor;
		}
	}

	public int gravity
	{
		get
		{
			switch (_anchor)
			{
			case TextAnchor.LowerCenter:
				return 81;
			case TextAnchor.LowerLeft:
				return 83;
			case TextAnchor.LowerRight:
				return 85;
			case TextAnchor.MiddleCenter:
				return 17;
			case TextAnchor.MiddleLeft:
				return 19;
			case TextAnchor.MiddleRight:
				return 21;
			case TextAnchor.UpperCenter:
				return 49;
			case TextAnchor.UpperLeft:
				return 51;
			case TextAnchor.UpperRight:
				return 53;
			default:
				return 48;
			}
		}
	}

	public AndroidADBanner(TextAnchor anchor, GADBannerSize size, int id)
	{
		_id = id;
		_size = size;
		_anchor = anchor;
		AndroidNative.CreateBannerAd(gravity, (int)_size, _id);
	}

	public AndroidADBanner(int x, int y, GADBannerSize size, int id)
	{
		_id = id;
		_size = size;
		AndroidNative.CreateBannerAdPos(x, y, (int)_size, _id);
	}

	public void Hide()
	{
		if (_IsOnScreen)
		{
			_IsOnScreen = false;
			AndroidNative.HideAd(_id);
		}
	}

	public void Show()
	{
		if (!_IsOnScreen)
		{
			_IsOnScreen = true;
			AndroidNative.ShowAd(_id);
		}
	}

	public void Refresh()
	{
		if (_IsLoaded)
		{
			AndroidNative.RefreshAd(_id);
		}
	}

	public void DestroyAfterLoad()
	{
		destroyOnLoad = true;
		ShowOnLoad = false;
	}

	public void SetDimentions(int w, int h)
	{
		_width = w;
		_height = h;
	}

	public void OnBannerAdLoaded()
	{
		if (destroyOnLoad)
		{
			AndroidNative.DestroyBanner(id);
			return;
		}
		_IsLoaded = true;
		if (ShowOnLoad && firstLoad)
		{
			Show();
			firstLoad = false;
		}
		dispatch("on_banner_ad_loaded");
	}

	public void OnBannerAdFailedToLoad()
	{
		dispatch("on_banner_ad_failed_loading");
	}

	public void OnBannerAdOpened()
	{
		dispatch("on_banner_ad_opened");
	}

	public void OnBannerAdClosed()
	{
		dispatch("on_banner_ad_closed");
	}

	public void OnBannerAdLeftApplication()
	{
		dispatch("on_banner_ad_left_application");
	}
}
