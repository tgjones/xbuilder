using System;
using Microsoft.Xna.Framework.Graphics;

namespace XBuilder.ContentPreview.Rendering
{
	public class ModelChangedEventArgs : EventArgs
	{
		public Model Model { get; private set; }

		public ModelChangedEventArgs(Model model)
		{
			Model = model;
		}
	}
}