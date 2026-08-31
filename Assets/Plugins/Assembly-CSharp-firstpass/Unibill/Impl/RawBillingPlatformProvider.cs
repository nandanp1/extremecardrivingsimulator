using System;
using Uniject;
using Uniject.Impl;
using UnityEngine;

namespace Unibill.Impl
{
	internal class RawBillingPlatformProvider : IRawBillingPlatformProvider
	{
		private UnibillConfiguration config;

		private GameObject gameObject;

		private ILevelLoadListener listener;

		private IHTTPClient client;

		public RawBillingPlatformProvider(UnibillConfiguration config, GameObject o)
		{
			this.config = config;
			gameObject = o;
		}

		public IRawGooglePlayInterface getGooglePlay()
		{
			return new RawGooglePlayInterface();
		}

		public IRawAmazonAppStoreBillingInterface getAmazon()
		{
			return new RawAmazonAppStoreBillingInterface(config);
		}

		public IStoreKitPlugin getStorekit()
		{
			throw new NotImplementedException();
		}

		public IRawSamsungAppsBillingService getSamsung()
		{
			return new RawSamsungAppsBillingInterface();
		}

		public ILevelLoadListener getLevelLoadListener()
		{
			if (listener == null)
			{
				listener = gameObject.AddComponent<UnityLevelLoadListener>();
			}
			return listener;
		}

		public IHTTPClient getHTTPClient(IUtil util)
		{
			if (client == null)
			{
				client = new HTTPClient(util);
			}
			return client;
		}
	}
}
