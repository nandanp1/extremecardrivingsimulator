using System;
using System.Text;

public class GoogleCloudResult
{
	private GooglePlayResponceCode _response;

	private string _message;

	private int _stateKey;

	public byte[] stateData;

	public byte[] serverConflictData;

	public string resolvedVersion;

	public GooglePlayResponceCode response
	{
		get
		{
			return _response;
		}
	}

	public string stateDataString
	{
		get
		{
			if (stateData == null)
			{
				return string.Empty;
			}
			return Encoding.UTF8.GetString(stateData);
		}
	}

	public string serverConflictDataString
	{
		get
		{
			if (serverConflictData == null)
			{
				return string.Empty;
			}
			return Encoding.UTF8.GetString(stateData);
		}
	}

	public string message
	{
		get
		{
			return _message;
		}
	}

	public int stateKey
	{
		get
		{
			return _stateKey;
		}
	}

	public bool isSuccess
	{
		get
		{
			return _response == GooglePlayResponceCode.STATUS_OK;
		}
	}

	public bool isFailure
	{
		get
		{
			return !isSuccess;
		}
	}

	public GoogleCloudResult(string code)
	{
		_response = PlayServiceUtil.GetGPCodeFromInt(Convert.ToInt32(code));
		_message = _response.ToString();
	}

	public GoogleCloudResult(string code, string key)
	{
		_response = PlayServiceUtil.GetGPCodeFromInt(Convert.ToInt32(code));
		_message = _response.ToString();
		_stateKey = Convert.ToInt32(key);
	}
}
