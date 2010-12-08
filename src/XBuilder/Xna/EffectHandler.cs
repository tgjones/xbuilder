using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XBuilder.ContentPreview.Rendering;

namespace XBuilder.Xna
{
	public class EffectHandler : AssetHandler
	{
		private readonly EffectRenderer _modelRenderer;
		private IServiceProvider _serviceProvider;

		public override AssetRenderer Renderer
		{
			get { return _modelRenderer; }
		}

		public override string ProcessorName
		{
			get { return "EffectProcessor"; }
		}

		public EffectHandler(ContentManager contentManager, GraphicsDeviceControl graphicsDeviceControl)
			: base(contentManager, graphicsDeviceControl)
		{
			_modelRenderer = new EffectRenderer(graphicsDeviceControl);
		}

		public override void ResetRenderer()
		{
			_modelRenderer.Model = null;
		}

		public override void LoadContent(string assetName)
		{
			_modelRenderer.Effect = ContentManager.Load<Effect>(assetName);
		}

		public void Initialize(IServiceProvider serviceProvider, XBuilderPackage package)
		{
			_serviceProvider = serviceProvider;
			_modelRenderer.Package = package;
		}
	}
}