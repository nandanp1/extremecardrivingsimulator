using System.Collections.Generic;

namespace UnityEditor.iOS.Xcode
{
	internal class DeviceRequirement
	{
		internal Dictionary<string, string> values = new Dictionary<string, string>();

		public DeviceRequirement()
		{
			values.Add("idiom", DeviceTypeRequirement.Any);
		}

		public DeviceRequirement AddDevice(string device)
		{
			AddCustom(DeviceTypeRequirement.Key, device);
			return this;
		}

		public DeviceRequirement AddMemory(string memory)
		{
			AddCustom(MemoryRequirement.Key, memory);
			return this;
		}

		public DeviceRequirement AddGraphics(string graphics)
		{
			AddCustom(GraphicsRequirement.Key, graphics);
			return this;
		}

		public DeviceRequirement AddWidthClass(string sizeClass)
		{
			AddCustom(SizeClassRequirement.WidthKey, sizeClass);
			return this;
		}

		public DeviceRequirement AddHeightClass(string sizeClass)
		{
			AddCustom(SizeClassRequirement.HeightKey, sizeClass);
			return this;
		}

		public DeviceRequirement AddScale(string scale)
		{
			AddCustom(ScaleRequirement.Key, scale);
			return this;
		}

		public DeviceRequirement AddCustom(string key, string value)
		{
			if (values.ContainsKey(key))
			{
				values.Remove(key);
			}
			values.Add(key, value);
			return this;
		}
	}
}
