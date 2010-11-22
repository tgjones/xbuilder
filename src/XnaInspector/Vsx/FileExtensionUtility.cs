using System.IO;

namespace XnaInspector.Vsx
{
	public static class FileExtensionUtility
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
					return true;
				default:
					return false;
			}
		}
	}
}