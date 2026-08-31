using FluffyUnderware.Curvy.Utils;
using UnityEngine;

namespace FluffyUnderware.Curvy
{
	[ExecuteInEditMode]
	public class CurvyComponent : MonoBehaviour
	{
		public delegate void CurvyComponentEvent(CurvyComponent sender);

		[Label(Tooltip = "Determines when to update")]
		public CurvyUpdateMethod UpdateIn;

		private Transform mTransform;

		public Transform Transform
		{
			get
			{
				if (!mTransform)
				{
					mTransform = base.transform;
				}
				return mTransform;
			}
		}

		public float DeltaTime
		{
			get
			{
				return CurvyUtility.DeltaTime;
			}
		}

		public virtual void EditorUpdate()
		{
			CurvyUtility.SetEditorTiming();
		}

		public virtual bool Initialize()
		{
			return false;
		}
	}
}
