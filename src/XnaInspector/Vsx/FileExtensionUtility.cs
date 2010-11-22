using System.IO;
using XnaInspector.Xna;

namespace XnaInspector.Vsx
{
	internal static class FileExtensionUtility
	{
		public static bool IsInspectableFile(string fileName)
		{
			// TODO: Add an options page, to let the user configure this.
			if (fileName == null)
				return false;

			string extension = Path.GetExtension(fileName);
			if (extension == null)
				return false;

			switch (extension.ToLower())
			{
				case ".fbx":
				case ".x":
				case ".3ds":
				case ".obj":
				case ".nff":
				case ".tga":
				case ".bmp":
				case ".jpg":
				case ".jpeg":
				case ".fx":
				case ".stitchedeffect":
					return true;
				default:
					return false;
			}
		}

		public static AssetType GetAssetType(string fileName)
		{
			// TODO: Add an options page, to let the user configure this.
			string extension = Path.GetExtension(fileName);
			switch (extension.ToLower())
			{
				case ".fbx":
				case ".x":
				case ".3ds":
				case ".obj":
				case ".nff":
					return AssetType.Model;
				case ".tga":
				case ".bmp":
				case ".jpg":
				case ".jpeg":
					return AssetType.Texture;
				case ".fx":
				case ".stitchedeffect":
					return AssetType.Effect;
				default :
					return AssetType.None;
			}
		}
	}
}