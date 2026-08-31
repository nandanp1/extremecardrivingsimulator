using System;

public class GooglePlayResult
{
	private GooglePlayResponceCode _response;

	private string _message;

	public string leaderboardId = string.Empty;

	public string achievementId = string.Empty;

	public GooglePlayResponceCode response
	{
		get
		{
			return _response;
		}
	}

	public string message
	{
		get
		{
			return _message;
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

	public GooglePlayResult(string code)
	{
		_response = PlayServiceUtil.GetGPCodeFromInt(Convert.ToInt32(code));
		_message = _response.ToString();
	}
}
