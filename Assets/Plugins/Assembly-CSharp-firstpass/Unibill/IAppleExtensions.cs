using System;

namespace Unibill
{
	public interface IAppleExtensions
	{
		event Action<string> onAppReceiptRefreshed;

		event Action onAppReceiptRefreshFailed;

		void refreshAppReceipt();
	}
}
