using System;
using Microsoft.Xna.Framework.Graphics;

namespace XBuilder.ContentPreview.Rendering
{
	public abstract class AssetRenderer
	{
		public virtual void Initialize(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
		{
			
		}

		public abstract void Draw(GraphicsDevice graphicsDevice);

		public virtual void ChangeFillMode(bool wireframe)
		{
			
		}
	}
}