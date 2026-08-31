using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace Crosstales.BadWord
{
	public abstract class Provider : MonoBehaviour
	{
		public string Name = string.Empty;

		public RegexOptions RegexOption1 = RegexOptions.IgnoreCase;

		public RegexOptions RegexOption2 = RegexOptions.CultureInvariant;

		public RegexOptions RegexOption3;

		public RegexOptions RegexOption4;

		public RegexOptions RegexOption5;

		private bool ready;

		private List<Source> sources = new List<Source>();

		public bool Ready
		{
			get
			{
				return ready;
			}
			protected set
			{
				ready = value;
			}
		}

		public List<Source> Sources
		{
			get
			{
				return sources;
			}
			protected set
			{
				sources = value;
			}
		}

		public abstract void Load();

		public abstract void Save();

		protected abstract void init();

		public void Awake()
		{
			Load();
		}

		protected void logNoResourcesAdded()
		{
			Debug.LogWarning("No 'Resources' for " + base.name + " added!" + Environment.NewLine + "If you want to use this functionality, please add your desired 'Resources' in the editor.");
		}
	}
}
