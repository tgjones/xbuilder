using Microsoft.Xna.Framework.Content;
using XnaInspector.Xna.Rendering;

namespace XnaInspector.Xna
{
	public abstract class AssetHandler
	{
		protected ContentManager ContentManager {get; private set;}
		protected GraphicsDeviceControl GraphicsDeviceControl {get; private set;}

		public abstract AssetRenderer Renderer { get; }
		public abstract string ProcessorName { get; }

		public AssetHandler(ContentManager contentManager, GraphicsDeviceControl graphicsDeviceControl)
		{
			ContentManager = contentManager;
			GraphicsDeviceControl = graphicsDeviceControl;
		}

		public abstract void ResetRenderer();
		public abstract void LoadContent(string assetName);
	}
}