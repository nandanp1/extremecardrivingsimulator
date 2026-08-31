using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace PrefabEvolution
{
	[Serializable]
	public class PELinkage
	{
		[Serializable]
		public class Link
		{
			public int LIIF;

			public UnityEngine.Object InstanceTarget;

			public override string ToString()
			{
				return string.Format("[Link]{0}:{1}", LIIF, InstanceTarget);
			}
		}

		public List<Link> Links = new List<Link>();

		public Link this[int liif]
		{
			get
			{
				return Links.FirstOrDefault((Link link) => link.LIIF == liif);
			}
		}

		public Link this[Link link]
		{
			get
			{
				if (link == null)
				{
					return null;
				}
				return this[link.LIIF];
			}
		}

		public Link this[UnityEngine.Object obj]
		{
			get
			{
				return Links.FirstOrDefault((Link link) => link.InstanceTarget == obj);
			}
		}

		public UnityEngine.Object GetPrefabObject(GameObject prefab, UnityEngine.Object instanceObject)
		{
			PEPrefabScript component = prefab.GetComponent<PEPrefabScript>();
			Link link = this[instanceObject];
			if (link == null)
			{
				return null;
			}
			Link link2 = component.Links[link.LIIF];
			if (link2 == null)
			{
				return null;
			}
			return link2.InstanceTarget;
		}
	}
}
