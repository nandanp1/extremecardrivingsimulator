using System;
using System.Collections;
using System.Globalization;
using ANMiniJSON;
using UnityEngine;

public class TwitterUserInfo : EventDispatcherBase
{
	public const string PROFILE_IMAGE_LOADED = "profile_image_loaded";

	public const string PROFILE_BACKGROUND_LOADED = "profile_background_loaded";

	private string _id;

	private string _description;

	private string _name;

	private string _screen_name;

	private string _location;

	private string _lang;

	private string _rawJSON;

	private string _profile_image_url;

	private string _profile_image_url_https;

	private string _profile_background_image_url;

	private string _profile_background_image_url_https;

	private Texture2D _profile_image;

	private Texture2D _profile_background;

	private Color _profile_background_color = Color.clear;

	private Color _profile_text_color = Color.clear;

	private int _friends_count;

	private int _statuses_count;

	private TwitterStatus _status;

	public string rawJSON
	{
		get
		{
			return _rawJSON;
		}
	}

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

	public string description
	{
		get
		{
			return _description;
		}
	}

	public string screen_name
	{
		get
		{
			return _screen_name;
		}
	}

	public string location
	{
		get
		{
			return _location;
		}
	}

	public string lang
	{
		get
		{
			return _lang;
		}
	}

	public string profile_image_url
	{
		get
		{
			return _profile_image_url;
		}
	}

	public string profile_image_url_https
	{
		get
		{
			return _profile_image_url_https;
		}
	}

	public string profile_background_image_url
	{
		get
		{
			return _profile_background_image_url;
		}
	}

	public string profile_background_image_url_https
	{
		get
		{
			return _profile_background_image_url_https;
		}
	}

	public int friends_count
	{
		get
		{
			return _friends_count;
		}
	}

	public int statuses_count
	{
		get
		{
			return _statuses_count;
		}
	}

	public TwitterStatus status
	{
		get
		{
			return _status;
		}
	}

	public Texture2D profile_image
	{
		get
		{
			return _profile_image;
		}
	}

	public Texture2D profile_background
	{
		get
		{
			return _profile_background;
		}
	}

	public Color profile_background_color
	{
		get
		{
			return _profile_background_color;
		}
	}

	public Color profile_text_color
	{
		get
		{
			return _profile_text_color;
		}
	}

	public TwitterUserInfo(string data)
	{
		_rawJSON = data;
		IDictionary dictionary = Json.Deserialize(_rawJSON) as IDictionary;
		_id = Convert.ToString(dictionary["id"]);
		_name = Convert.ToString(dictionary["name"]);
		_description = Convert.ToString(dictionary["description"]);
		_screen_name = Convert.ToString(dictionary["screen_name"]);
		_lang = Convert.ToString(dictionary["lang"]);
		_location = Convert.ToString(dictionary["location"]);
		_profile_image_url = Convert.ToString(dictionary["profile_image_url"]);
		_profile_image_url_https = Convert.ToString(dictionary["profile_image_url_https"]);
		_profile_background_image_url = Convert.ToString(dictionary["profile_background_image_url"]);
		_profile_background_image_url_https = Convert.ToString(dictionary["profile_background_image_url_https"]);
		_friends_count = Convert.ToInt32(dictionary["friends_count"]);
		_statuses_count = Convert.ToInt32(dictionary["statuses_count"]);
		_profile_text_color = HexToColor(Convert.ToString(dictionary["profile_text_color"]));
		_profile_background_color = HexToColor(Convert.ToString(dictionary["profile_background_color"]));
		_status = new TwitterStatus(dictionary["status"] as IDictionary);
	}

	public void LoadProfileImage()
	{
		if (_profile_image != null)
		{
			dispatch("profile_image_loaded");
			return;
		}
		WWWTextureLoader wWWTextureLoader = WWWTextureLoader.Create();
		wWWTextureLoader.addEventListener("loaded", OnProfileImageLoaded);
		wWWTextureLoader.LoadTexture(_profile_image_url_https);
	}

	public void LoadBackgroundImage()
	{
		if (_profile_background != null)
		{
			dispatch("profile_background_loaded");
			return;
		}
		WWWTextureLoader wWWTextureLoader = WWWTextureLoader.Create();
		wWWTextureLoader.addEventListener("loaded", OnProfileBackgroundLoaded);
		wWWTextureLoader.LoadTexture(_profile_background_image_url_https);
	}

	private void OnProfileImageLoaded(CEvent e)
	{
		e.dispatcher.removeEventListener("loaded", OnProfileImageLoaded);
		_profile_image = e.data as Texture2D;
		dispatch("profile_image_loaded");
	}

	private void OnProfileBackgroundLoaded(CEvent e)
	{
		e.dispatcher.removeEventListener("loaded", OnProfileImageLoaded);
		_profile_background = e.data as Texture2D;
		dispatch("profile_background_loaded");
	}

	private Color HexToColor(string hex)
	{
		byte r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
		byte g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
		byte b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);
		return new Color32(r, g, b, byte.MaxValue);
	}
}
