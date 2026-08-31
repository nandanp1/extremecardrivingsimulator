#if UNITY_6000_0_OR_NEWER
namespace UnityEngine
{
	/// <summary>
	/// Compatibility marker for the recovered Photon PUN implementation.
	/// Unity 4 supplied this attribute and PUN reflects over it to discover RPC methods.
	/// </summary>
	[System.AttributeUsage(System.AttributeTargets.Method, AllowMultiple = true)]
	public sealed class RPC : System.Attribute
	{
	}
}
#endif
