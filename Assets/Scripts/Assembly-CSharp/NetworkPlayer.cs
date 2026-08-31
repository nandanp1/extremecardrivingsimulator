using ExitGames.Client.Photon;
using UnityEngine;

public class NetworkPlayer
{
	public const string DEFAULT_PLAYER_NAME = "New Player";

	public const string PLAYERPROP_VERSION = "version";

	public const string PLAYERPROP_DATE_VERSION = "dateVersion";

	public const string PLAYERPROP_COLOR = "color";

	public const string PLAYERPROP_OBSOLETE = "obsolete";

	public const string PLAYERPROP_PUBLIC_NAME = "pName";

	public const string PLAYERPROP_UPID = "upid";

	public const string PLAYERPROP_SPECTATOR = "spectator";

	public const string PLAYERPROP_READY = "ready";

	public PhotonPlayer player;

	public string name
	{
		get
		{
			return GetPublicName();
		}
		set
		{
			SetPublicName(value);
		}
	}

	public int ID
	{
		get
		{
			return player.ID;
		}
	}

	public string UPID
	{
		get
		{
			return GetUPID();
		}
		set
		{
			SetUPID(value);
			player.name = GetUPID();
			if (isLocal)
			{
				PhotonNetwork.playerName = GetUPID();
			}
		}
	}

	public bool isIgnored
	{
		get
		{
			string ignorePlayers = SettingsManager.GetIgnorePlayers();
			return ignorePlayers.Contains(name) || ignorePlayers.Contains(UPID);
		}
	}

	public bool isLocal
	{
		get
		{
			return player.isLocal;
		}
	}

	public bool isMaster
	{
		get
		{
			return player.isMasterClient;
		}
	}

	public Hashtable properties
	{
		get
		{
			return player.customProperties;
		}
	}

	public bool isSpectator
	{
		get
		{
			return GetSpectator();
		}
		set
		{
			SetSpectator(value);
		}
	}

	public bool isReady
	{
		get
		{
			return GetIsReady();
		}
		set
		{
			SetIsReady(value);
		}
	}

	public NetworkPlayer(PhotonPlayer player)
	{
		this.player = player;
	}

	public override int GetHashCode()
	{
		return player.GetHashCode();
	}

	public override bool Equals(object obj)
	{
		NetworkPlayer networkPlayer = (NetworkPlayer)obj;
		if (networkPlayer != null)
		{
			return player.Equals(networkPlayer.player);
		}
		return false;
	}

	public string ToStringFull()
	{
		return player.ToStringFull();
	}

	public V GetProperty<V>(string key, V defaultValue)
	{
		object obj = player.customProperties[key];
		if (obj != null && obj is V)
		{
			return (V)obj;
		}
		return defaultValue;
	}

	public void SetProperty<V>(string key, V value)
	{
		Hashtable hashtable = new Hashtable();
		hashtable.Add(key, value);
		player.SetCustomProperties(hashtable);
	}

	public Color GetColor()
	{
		float[] property = GetProperty("color", new float[4] { 0f, 0f, 1f, 1f });
		return new Color(property[0], property[1], property[2], property[3]);
	}

	public void SetColor(Color color)
	{
		SetProperty("color", new float[4] { color.r, color.g, color.b, color.a });
	}

	public int GetVersion()
	{
		return GetProperty("version", SettingsManager.GetMultiplayerVersion());
	}

	public void SetVersion(int version)
	{
		SetProperty("version", version);
	}

	public string GetDateVersion()
	{
		return GetProperty("dateVersion", SettingsManager.GetMultiplayerDateVersion());
	}

	public void SetDateVersion(string version)
	{
		SetProperty("dateVersion", version);
	}

	public bool GetIsObsolete()
	{
		return GetProperty("obsolete", false);
	}

	public void SetIsObsolete(bool obsolete)
	{
		SetProperty("obsolete", obsolete);
	}

	private string GetUPID()
	{
		return GetProperty("upid", "none");
	}

	private void SetUPID(string upid)
	{
		SetProperty("upid", upid);
	}

	private string GetPublicName()
	{
		return GetProperty("pName", "New Player");
	}

	private void SetPublicName(string name)
	{
		SetProperty("pName", name);
	}

	private bool GetSpectator()
	{
		return GetProperty("spectator", false);
	}

	private void SetSpectator(bool spectator)
	{
		SetProperty("spectator", spectator);
	}

	private bool GetIsReady()
	{
		return GetProperty("ready", false);
	}

	private void SetIsReady(bool ready)
	{
		SetProperty("ready", ready);
	}
}
