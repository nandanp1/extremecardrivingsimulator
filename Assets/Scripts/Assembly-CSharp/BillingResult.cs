public class BillingResult
{
	private int _response;

	private string _message;

	private GooglePurchaseTemplate _purchase;

	public GooglePurchaseTemplate purchase
	{
		get
		{
			return _purchase;
		}
	}

	public int response
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
			return _response == 0;
		}
	}

	public bool isFailure
	{
		get
		{
			return !isSuccess;
		}
	}

	public BillingResult(int code, string msg, GooglePurchaseTemplate p)
		: this(code, msg)
	{
		_purchase = p;
	}

	public BillingResult(int code, string msg)
	{
		_response = code;
		_message = msg;
	}
}
