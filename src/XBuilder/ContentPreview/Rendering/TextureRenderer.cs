using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XBuilder.ContentPreview.Rendering
{
	public class TextureRenderer : AssetRenderer
	{
		private SpriteBatch _spriteBatch;
		private Texture2D _texture;
		private Control _parentControl;

		/// <summary>
		/// Gets or sets the current texture.
		/// </summary>
		public Texture2D Texture
		{
			get { return _texture; }

			set
			{
				_texture = value;
				
			}
		}

		public TextureRenderer(Control parentControl, GraphicsDevice graphicsDevice)
		{
			_parentControl = parentControl;
			_spriteBatch = new SpriteBatch(graphicsDevice);
		}

		public override void Draw(GraphicsDevice graphicsDevice)
		{
			_spriteBatch.Begin();
			_spriteBatch.Draw(_texture, new Rectangle(0, 0, _parentControl.Width, _parentControl.Height), Color.White);
			_spriteBatch.End();
		}
	}
}