using System.ComponentModel;
using Microsoft.VisualStudio.Shell;

namespace XnaInspector
{
	public class XBuilderOptions : DialogPage
	{
		[Category("Content Preview"),
		DisplayName("Model File Extensions"),
		Description("Comma-separated list of model file extensions (prefixed by a '.') that you want to load using Content Preview."),
		DefaultValue(".fbx, .x")]
		public string ModelExtensions { get; set; }

		[Category("Content Preview"),
		DisplayName("Texture File Extensions"),
		Description("Comma-separated list of texture file extensions (prefixed by a '.') that you want to load using Content Preview."),
		DefaultValue(".bmp, .dds, .dib, .hdr, .jpg, .pfm, .png, .ppm, .tga")]
		public string TextureExtensions { get; set; }

		public XBuilderOptions()
		{
			ModelExtensions = ".fbx, .x";
			TextureExtensions = ".bmp, .dds, .dib, .hdr, .jpg, .pfm, .png, .ppm, .tga";
		}
	}
}