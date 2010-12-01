using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XBuilder.ContentPreview.Rendering;

namespace XBuilder.Xna
{
	public class ModelHandler : AssetHandler
	{
		private readonly ModelRenderer _modelRenderer;

		public override AssetRenderer Renderer
		{
			get { return _modelRenderer; }
		}

		public override string ProcessorName
		{
			get { return "ModelProcessor"; }
		}

		public ModelHandler(ContentManager contentManager, GraphicsDeviceControl graphicsDeviceControl)
			: base(contentManager, graphicsDeviceControl)
		{
			_modelRenderer = new ModelRenderer(graphicsDeviceControl);
		}

		public override void ResetRenderer()
		{
			_modelRenderer.Model = null;
		}

		public override void LoadContent(string assetName)
		{
			_modelRenderer.Model = ContentManager.Load<Model>(assetName);
		}

		public void Initialize(XBuilderPackage package)
		{
			_modelRenderer.Package = package;
		}
	}
}