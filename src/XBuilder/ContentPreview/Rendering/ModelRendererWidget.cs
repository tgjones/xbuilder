using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XBuilder.ContentPreview.Rendering
{
	public abstract class ModelRendererWidget
	{
		public abstract WidgetRenderPosition RenderPosition { get; }
		public abstract void Initialize(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice);
		public abstract void Draw(GraphicsDevice graphicsDevice, Matrix cameraRotation, Matrix view, Matrix projection);
	}

	public enum WidgetRenderPosition
	{
		BeforeModel,
		AfterModel
	}
}