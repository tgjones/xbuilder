using System;
using System.ComponentModel;
using System.Drawing;
using Microsoft.VisualStudio.Shell;

namespace XBuilder.Options
{
	public class XBuilderOptionsGeneral : DialogPage
	{
		[Category("Supported File Extensions"),
		DisplayName("Model File Extensions"),
		Description("Comma-separated list of model file extensions (prefixed by a '.') that you want to load using Content Preview."),
		DefaultValue(".fbx, .x")]
		public string ModelExtensions { get; set; }

		[Category("Supported File Extensions"),
		DisplayName("Texture File Extensions"),
		Description("Comma-separated list of texture file extensions (prefixed by a '.') that you want to load using Content Preview."),
		DefaultValue(".bmp, .dds, .dib, .hdr, .jpg, .pfm, .png, .ppm, .tga")]
		public string TextureExtensions { get; set; }

		public XBuilderOptionsGeneral()
		{
			ModelExtensions = ".fbx, .x";
			TextureExtensions = ".bmp, .dds, .dib, .hdr, .jpg, .pfm, .png, .ppm, .tga";
		}
	}
}