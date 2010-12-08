using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace XBuilder.ContentPreview.Rendering
{
	public class EffectRenderer : ModelRenderer
	{
		public EffectRenderer(GraphicsDeviceControl parentControl)
			: base(parentControl)
		{
		}

		private Effect _effect;
		public Effect Effect
		{
			get { return _effect; }
			set
			{
				_effect = value;
				ChangePrimitive(Primitive.Sphere);
			}
		}

		public override void ChangePrimitive(Primitive primitive)
		{
			// Load sphere and apply effect to sphere.
			ResourceContentManager resourceContentManager = new ResourceContentManager(_serviceProvider, Resources.ResourceManager);
			Model model = resourceContentManager.Load<Model>(primitive.ToString());
			foreach (ModelMesh mesh in model.Meshes)
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
					meshPart.Effect = _effect;
			Model = model;
		}
	}
}