using UnityEngine;

public class TwitterPostingTask : AsyncTask
{
	private string _status = string.Empty;

	private Texture2D _texture;

	private TwitterManagerInterface _controller;

	public static TwitterPostingTask Cretae()
	{
		return new GameObject("TwitterPositngTask").AddComponent<TwitterPostingTask>();
	}

	public void Post(string status, Texture2D texture, TwitterManagerInterface controller)
	{
		_status = status;
		_texture = texture;
		_controller = controller;
		if (_controller.IsInited)
		{
			OnTWInited();
			return;
		}
		_controller.addEventListener("twitter_inited", OnTWInited);
		_controller.Init();
	}

	private void OnTWInited()
	{
		_controller.removeEventListener("twitter_inited", OnTWInited);
		if (_controller.IsAuthed)
		{
			OnTWAuth();
			return;
		}
		_controller.addEventListener("authentication_failed", OnTWAuthFailed);
		_controller.addEventListener("authentication_succeeded", OnTWAuth);
		_controller.AuthenticateUser();
	}

	private void OnTWAuth()
	{
		_controller.removeEventListener("authentication_failed", OnTWAuthFailed);
		_controller.removeEventListener("authentication_succeeded", OnTWAuth);
		_controller.addEventListener("post_failed", OnPostFailed);
		_controller.addEventListener("post_succeeded", OnPost);
		if (_texture != null)
		{
			_controller.Post(_status, _texture);
		}
		else
		{
			_controller.Post(_status);
		}
	}

	private void OnTWAuthFailed()
	{
		_controller.removeEventListener("authentication_failed", OnTWAuthFailed);
		_controller.removeEventListener("authentication_succeeded", OnTWAuth);
		TWResult data = new TWResult(false, "Auth failed");
		dispatch("complete", data);
	}

	private void OnPost(CEvent e)
	{
		TWResult data = e.data as TWResult;
		dispatch("complete", data);
	}

	private void OnPostFailed(CEvent e)
	{
		TWResult data = e.data as TWResult;
		dispatch("complete", data);
	}
}
