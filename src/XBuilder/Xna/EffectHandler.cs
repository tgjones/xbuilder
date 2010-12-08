using System;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XBuilder.ContentPreview.Rendering;

namespace XBuilder.Xna
{
	public class EffectHandler : AssetHandler
	{
		private readonly ModelRenderer _modelRenderer;
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
			_modelRenderer = new ModelRenderer(graphicsDeviceControl);
		}

		public override void ResetRenderer()
		{
			_modelRenderer.Model = null;
		}

		public override void LoadContent(string assetName)
		{
			Effect effect = ContentManager.Load<Effect>(assetName);

			// Load sphere and apply effect to sphere.
			ResourceContentManager resourceContentManager = new ResourceContentManager(_serviceProvider, Resources.ResourceManager);
			Model primitive = resourceContentManager.Load<Model>("Sphere");
			foreach (ModelMesh mesh in primitive.Meshes)
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
					meshPart.Effect = effect;

			_modelRenderer.Model = primitive;
		}

		public void Initialize(IServiceProvider serviceProvider, XBuilderPackage package)
		{
			_serviceProvider = serviceProvider;
			_modelRenderer.Package = package;
		}
	}
}