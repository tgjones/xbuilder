using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace XBuilder.ContentPreview.Rendering
{
	/// <summary>
	/// Example control inherits from GraphicsDeviceControl, and displays
	/// a spinning 3D model. The main form class is responsible for loading
	/// the model: this control just displays it.
	/// </summary>
	public class ModelRenderer : AssetRenderer
	{
		// Cache information about the model size and position.
		private Matrix[] _boneTransforms;
		private Vector3 _modelCenter;
		private float _modelRadius;

		private readonly CameraController _ballController;

		private Model _model;

		/// <summary>
		/// Gets or sets the current model.
		/// </summary>
		public Model Model
		{
			get { return _model; }

			set
			{
				_model = value;
				if (_model != null)
					MeasureModel();
				_ballController.ResetOrientation();
			}
		}

		public ModelRenderer(Control parentControl)
		{
			// Camera stuff.
			_ballController = new CameraController();

			parentControl.MouseDown += (sender, e) => _ballController.MouseDown(ToXnaPoint(e.Location));
			parentControl.MouseMove += (sender, e) =>
			{
				_ballController.MouseMove(ToXnaPoint(e.Location));
				parentControl.Invalidate();
			};
			parentControl.MouseUp += (sender, e) => _ballController.MouseUp(ToXnaPoint(e.Location));
		}

		private static Point ToXnaPoint(System.Drawing.Point p)
		{
			return new Point(p.X, p.Y);
		}

		/// <summary>
		/// Draws the control.
		/// </summary>
		public override void Draw(GraphicsDevice graphicsDevice)
		{
			if (_model != null)
			{
				// Compute camera matrices.
				Vector3 eyePosition = _modelCenter;
				eyePosition.Z += _modelRadius * 2;
				eyePosition.Y += _modelRadius;

				float aspectRatio = graphicsDevice.Viewport.AspectRatio;

				float nearClip = _modelRadius / 100;
				float farClip = _modelRadius * 100;

				Matrix world = _ballController.CurrentOrientation;
				Matrix view = Matrix.CreateLookAt(eyePosition, _modelCenter, Vector3.Up);
				Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspectRatio,
					nearClip, farClip);

				// Draw the model.
				foreach (ModelMesh mesh in _model.Meshes)
				{
					foreach (BasicEffect effect in mesh.Effects)
					{
						effect.World = _boneTransforms[mesh.ParentBone.Index] * world;
						effect.View = view;
						effect.Projection = projection;

						effect.EnableDefaultLighting();
						effect.PreferPerPixelLighting = true;
						effect.SpecularPower = 16;
					}

					mesh.Draw();
				}
			}
		}

		/// <summary>
		/// Whenever a new model is selected, we examine it to see how big
		/// it is and where it is centered. This lets us automatically zoom
		/// the display, so we can correctly handle models of any scale.
		/// </summary>
		private void MeasureModel()
		{
			// Look up the absolute bone transforms for this model.
			_boneTransforms = new Matrix[_model.Bones.Count];

			_model.CopyAbsoluteBoneTransformsTo(_boneTransforms);

			// Compute an (approximate) model center position by
			// averaging the center of each mesh bounding sphere.
			_modelCenter = Vector3.Zero;

			foreach (ModelMesh mesh in _model.Meshes)
			{
				BoundingSphere meshBounds = mesh.BoundingSphere;
				Matrix transform = _boneTransforms[mesh.ParentBone.Index];
				Vector3 meshCenter = Vector3.Transform(meshBounds.Center, transform);

				_modelCenter += meshCenter;
			}

			_modelCenter /= _model.Meshes.Count;

			// Now we know the center point, we can compute the model radius
			// by examining the radius of each mesh bounding sphere.
			_modelRadius = 0;

			foreach (ModelMesh mesh in _model.Meshes)
			{
				BoundingSphere meshBounds = mesh.BoundingSphere;
				Matrix transform = _boneTransforms[mesh.ParentBone.Index];
				Vector3 meshCenter = Vector3.Transform(meshBounds.Center, transform);

				float transformScale = transform.Forward.Length();

				float meshRadius = (meshCenter - _modelCenter).Length() +
								   (meshBounds.Radius * transformScale);

				_modelRadius = Math.Max(_modelRadius, meshRadius);
			}
		}
	}
}