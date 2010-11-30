using System;
using System.IO;
using System.Linq;
using XBuilder.Options;
using XBuilder.Xna;

namespace XBuilder.Vsx
{
	internal static class FileExtensionUtility
	{
		public static bool IsInspectableFile(IOptionsService optionsService, string fileName)
		{
			return GetAssetType(optionsService, fileName) != AssetType.None;
		}

		public static AssetType GetAssetType(IOptionsService optionsService, string fileName)
		{
			string extension = Path.GetExtension(fileName);
			if (extension == null)
				return AssetType.None;

			extension = extension.ToLower();

			XBuilderOptionsGeneral options = optionsService.GetGeneralOptions();
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