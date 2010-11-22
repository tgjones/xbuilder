using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XnaInspector.Xna.Rendering;

namespace XnaInspector.Xna
{
	public class TextureHandler : AssetHandler
	{
		private readonly TextureRenderer _textureRenderer;

		public override AssetRenderer Renderer
		{
			get { return _textureRenderer; }
		}

		public override string ProcessorName
		{
			get { return "TextureProcessor"; }
		}

		public TextureHandler(ContentManager contentManager, GraphicsDeviceControl graphicsDeviceControl)
			: base(contentManager, graphicsDeviceControl)
		{
			_textureRenderer = new TextureRenderer(graphicsDeviceControl, graphicsDeviceControl.GraphicsDevice);
		}

		public override void ResetRenderer()
		{
			_textureRenderer.Texture = null;
		}

		public override void LoadContent(string assetName)
		{
			_textureRenderer.Texture = ContentManager.Load<Texture2D>(assetName);
		}
	}
}