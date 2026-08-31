using UnityEngine;

public class GooglePlayerTemplate
{
	private string _id;

	private string _name;

	private string _iconImageUrl;

	private string _hiResImageUrl;

	private Texture2D _icon;

	private Texture2D _image;

	public string playerId
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

	public string iconImageUrl
	{
		get
		{
			return _iconImageUrl;
		}
	}

	public string hiResImageUrl
	{
		get
		{
			return _hiResImageUrl;
		}
	}

	public Texture2D icon
	{
		get
		{
			return _icon;
		}
	}

	public Texture2D image
	{
		get
		{
			return _image;
		}
	}

	public GooglePlayerTemplate(string pId, string pName, string iconUrl, string imageUrl)
	{
		_id = pId;
		_name = pName;
		_iconImageUrl = iconUrl;
		_hiResImageUrl = imageUrl;
		if (AndroidNativeSettings.Instance.LoadProfileIcons)
		{
			LoadIcon();
		}
		if (AndroidNativeSettings.Instance.LoadProfileImages)
		{
			LoadImage();
		}
	}

	public void LoadImage()
	{
		if (!(image != null))
		{
			WWWTextureLoader wWWTextureLoader = WWWTextureLoader.Create();
			wWWTextureLoader.addEventListener("loaded", OnProfileImageLoaded);
			wWWTextureLoader.LoadTexture(_hiResImageUrl);
		}
	}

	public void LoadIcon()
	{
		if (!(image != null))
		{
			WWWTextureLoader wWWTextureLoader = WWWTextureLoader.Create();
			wWWTextureLoader.addEventListener("loaded", OnProfileIconLoaded);
			wWWTextureLoader.LoadTexture(_iconImageUrl);
		}
	}

	private void OnProfileImageLoaded(CEvent e)
	{
		e.dispatcher.removeEventListener("loaded", OnProfileImageLoaded);
		_image = e.data as Texture2D;
	}

	private void OnProfileIconLoaded(CEvent e)
	{
		e.dispatcher.removeEventListener("loaded", OnProfileIconLoaded);
		_icon = e.data as Texture2D;
	}
}
