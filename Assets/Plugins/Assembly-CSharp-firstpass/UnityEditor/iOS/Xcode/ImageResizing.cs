namespace UnityEditor.iOS.Xcode
{
	internal class ImageResizing
	{
		public enum SlicingType
		{
			Horizontal = 0,
			Vertical = 1,
			HorizontalAndVertical = 2
		}

		public enum ResizeMode
		{
			Stretch = 0,
			Tile = 1
		}

		public SlicingType type = SlicingType.HorizontalAndVertical;

		public int left;

		public int right;

		public int top;

		public int bottom;

		public ResizeMode centerResizeMode;

		public int centerWidth;

		public int centerHeight;
	}
}
