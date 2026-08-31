using System;
using UnityEngine;

namespace Uniject.Impl
{
	public class UnityLogger : ILogger
	{
		public string prefix { get; set; }

		public void LogWarning(string message, params object[] formatArgs)
		{
			Debug.LogWarning(string.Format(message, formatArgs));
		}

		public void Log(string message)
		{
			Debug.Log(formatMessageWithPrefix(message));
		}

		public void Log(string message, object[] args)
		{
			Log(safeFormat(message, args));
		}

		public void LogError(string message, params object[] formatArgs)
		{
			Debug.LogError(formatMessageWithPrefix(safeFormat(message, formatArgs)));
		}

		private string safeFormat(string message, params object[] formatArgs)
		{
			try
			{
				return string.Format(message, formatArgs);
			}
			catch (FormatException ex)
			{
				Log(ex.Data.ToString());
				return message;
			}
		}

		private string formatMessageWithPrefix(string message)
		{
			if (prefix == null)
			{
				return message;
			}
			return safeFormat("{0}: {1}", prefix, message);
		}
	}
}
