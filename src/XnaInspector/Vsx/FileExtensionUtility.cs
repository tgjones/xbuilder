using System;
using System.IO;
using System.Linq;
using Microsoft.VisualStudio.Shell;
using XnaInspector.Xna;

namespace XnaInspector.Vsx
{
	internal static class FileExtensionUtility
	{
		public static bool IsInspectableFile(XnaInspectorPackage package, string fileName)
		{
			return GetAssetType(package, fileName) != AssetType.None;
		}

		public static AssetType GetAssetType(XnaInspectorPackage package, string fileName)
		{
			string extension = Path.GetExtension(fileName);
			if (extension == null)
				return AssetType.None;

			extension = extension.ToLower();

			XBuilderOptions options = package.GetOptions();
			if (options.ModelExtensions != null)
			{
				string[] modelExtensions = options.ModelExtensions.Split(new[] {','}, StringSplitOptions.RemoveEmptyEntries);
				if (modelExtensions.Any(e => e.Trim() == extension))
					return AssetType.Model;
			}

			if (options.TextureExtensions != null)
			{
				string[] textureExtensions = options.TextureExtensions.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
				if (textureExtensions.Any(e => e.Trim() == extension))
					return AssetType.Texture;
			}

			// TODO: Support effects.

			return AssetType.None;
		}
	}
}