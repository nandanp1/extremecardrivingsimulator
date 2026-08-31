using System.IO;

namespace UnityEditor.iOS.Xcode
{
	internal class AssetImageSet : AssetCatalogItemWithVariants
	{
		private class ImageSetVariant : VariantData
		{
			public ImageAlignment alignment;

			public ImageResizing resizing;

			public ImageSetVariant(DeviceRequirement requirement, string path)
				: base(requirement, path)
			{
			}
		}

		internal AssetImageSet(string assetCatalogPath, string name, string authorId)
			: base(name, authorId)
		{
			m_Path = Path.Combine(assetCatalogPath, name + ".imageset");
		}

		public void AddVariant(DeviceRequirement requirement, string path)
		{
			AddVariant(new ImageSetVariant(requirement, path));
		}

		public void AddVariant(DeviceRequirement requirement, string path, ImageAlignment alignment, ImageResizing resizing)
		{
			ImageSetVariant imageSetVariant = new ImageSetVariant(requirement, path);
			imageSetVariant.alignment = alignment;
			imageSetVariant.resizing = resizing;
			AddVariant(imageSetVariant);
		}

		private void WriteAlignmentToJson(JsonElementDict item, ImageAlignment alignment)
		{
			JsonElementDict jsonElementDict = item.CreateDict("alignment-insets");
			jsonElementDict.SetInteger("top", alignment.top);
			jsonElementDict.SetInteger("bottom", alignment.bottom);
			jsonElementDict.SetInteger("left", alignment.left);
			jsonElementDict.SetInteger("right", alignment.right);
		}

		private static string GetSlicingMode(ImageResizing.SlicingType mode)
		{
			switch (mode)
			{
			case ImageResizing.SlicingType.Horizontal:
				return "3-part-horizontal";
			case ImageResizing.SlicingType.Vertical:
				return "3-part-vertical";
			case ImageResizing.SlicingType.HorizontalAndVertical:
				return "9-part";
			default:
				return string.Empty;
			}
		}

		private static string GetCenterResizeMode(ImageResizing.ResizeMode mode)
		{
			switch (mode)
			{
			case ImageResizing.ResizeMode.Stretch:
				return "stretch";
			case ImageResizing.ResizeMode.Tile:
				return "tile";
			default:
				return string.Empty;
			}
		}

		private void WriteResizingToJson(JsonElementDict item, ImageResizing resizing)
		{
			JsonElementDict jsonElementDict = item.CreateDict("resizing");
			jsonElementDict.SetString("mode", GetSlicingMode(resizing.type));
			JsonElementDict jsonElementDict2 = jsonElementDict.CreateDict("center");
			jsonElementDict2.SetString("mode", GetCenterResizeMode(resizing.centerResizeMode));
			jsonElementDict2.SetInteger("width", resizing.centerWidth);
			jsonElementDict2.SetInteger("height", resizing.centerHeight);
			JsonElementDict jsonElementDict3 = jsonElementDict.CreateDict("cap-insets");
			jsonElementDict3.SetInteger("top", resizing.top);
			jsonElementDict3.SetInteger("bottom", resizing.bottom);
			jsonElementDict3.SetInteger("left", resizing.left);
			jsonElementDict3.SetInteger("right", resizing.right);
		}

		public override void Write()
		{
			Directory.CreateDirectory(m_Path);
			JsonDocument jsonDocument = new JsonDocument();
			JsonElementDict info = WriteInfoToJson(jsonDocument);
			WriteODRTagsToJson(info);
			JsonElementArray jsonElementArray = jsonDocument.root.CreateArray("images");
			foreach (ImageSetVariant variant in m_Variants)
			{
				string fileName = Path.GetFileName(variant.path);
				File.Copy(variant.path, Path.Combine(m_Path, fileName));
				JsonElementDict jsonElementDict = jsonElementArray.AddDict();
				jsonElementDict.SetString("filename", fileName);
				WriteRequirementsToJson(jsonElementDict, variant.requirement);
				if (variant.alignment != null)
				{
					WriteAlignmentToJson(jsonElementDict, variant.alignment);
				}
				if (variant.resizing != null)
				{
					WriteResizingToJson(jsonElementDict, variant.resizing);
				}
			}
			jsonDocument.WriteToFile(Path.Combine(m_Path, "Contents.json"));
		}
	}
}
