using Microsoft.Xna.Framework.Content;
using XBuilder.ContentPreview.Rendering;
using XBuilder.Options;
using XBuilder.Vsx;

namespace XBuilder.Xna
{
	public class AssetHandlers
	{
		private readonly GraphicsDeviceControl _graphicsDeviceControl;
		private IOptionsService _optionsService;
		private readonly ModelHandler _modelHandler;
		private readonly EffectHandler _effectHandler;
		private readonly TextureHandler _textureHandler;

		public AssetHandlers(ContentManager contentManager, GraphicsDeviceControl graphicsDeviceControl)
		{
			_graphicsDeviceControl = graphicsDeviceControl;
			_modelHandler = new ModelHandler(contentManager, graphicsDeviceControl);
			_effectHandler = new EffectHandler(contentManager, graphicsDeviceControl);
			_textureHandler = new TextureHandler(contentManager, graphicsDeviceControl);
		}

		public void Initialize(XBuilderPackage package)
		{
			_optionsService = package.GetService<IOptionsService>();
			_modelHandler.Initialize(package);
			_effectHandler.Initialize(_graphicsDeviceControl.Services, package);

			_modelHandler.Renderer.Initialize(_graphicsDeviceControl.Services, _graphicsDeviceControl.GraphicsDevice);
			_effectHandler.Renderer.Initialize(_graphicsDeviceControl.Services, _graphicsDeviceControl.GraphicsDevice);
			_textureHandler.Renderer.Initialize(_graphicsDeviceControl.Services, _graphicsDeviceControl.GraphicsDevice);
		}

		public AssetHandler GetAssetHandler(string fileName)
		{
			AssetType assetType = FileExtensionUtility.GetAssetType(_optionsService, fileName);
			switch (assetType)
			{
				case AssetType.Effect :
					return _effectHandler;
				case AssetType.Model :
					return _modelHandler;
				case AssetType.Texture :
					return _textureHandler;
				default :
					throw new System.NotSupportedException();
			}
		}
	}
}