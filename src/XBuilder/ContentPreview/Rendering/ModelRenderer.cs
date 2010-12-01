using System;
using System.Linq;
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
		public event EventHandler<ModelChangedEventArgs> ModelChanged;

		protected void OnModelChanged(ModelChangedEventArgs e)
		{
			EventHandler<ModelChangedEventArgs> handler = ModelChanged;
			if (handler != null) handler(this, e);
		}

		// Cache information about the model size and position.
		private Matrix[] _boneTransforms;
		private Vector3 _modelCenter;
		private Vector3 _cameraPosition;
		private Vector3 _viewDirection;
		private float _modelRadius;

		private readonly CameraController _ballController;
		private ModelRendererWidget[] _widgets;

		private NormalsRenderer _normalsRenderer;

		private Model _model;
		private bool _wireframe;

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
				OnModelChanged(new ModelChangedEventArgs(_model));
			}
		}

		public XBuilderPackage Package { get; set; }

		public ModelRenderer(GraphicsDeviceControl parentControl)
		{
			_ballController = new CameraController();

			parentControl.MouseWheelWpf += (sender, e) =>
			{
				_cameraPosition += _viewDirection * e.Delta * _modelRadius * 0.001f;
				parentControl.Invalidate();
			};

			_widgets = new ModelRendererWidget[3];
			_widgets[0] = new GridRenderer(parentControl);
			_widgets[1] = new CubeRenderer(parentControl, _ballController);

			_normalsRenderer = new NormalsRenderer(parentControl, this);
			_widgets[2] = _normalsRenderer;
		}

		public override void Initialize(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
		{
			foreach (ModelRendererWidget widget in _widgets)
				widget.Initialize(serviceProvider, graphicsDevice);
		}

		/// <summary>
		/// Draws the control.
		/// </summary>
		public override void Draw(GraphicsDevice graphicsDevice)
		{
			if (_model != null)
			{
				float aspectRatio = graphicsDevice.Viewport.AspectRatio;

				float nearClip = _modelRadius / 100;
				float farClip = _modelRadius * 100;

				Matrix world = Matrix.Identity;
				Matrix view = _ballController.CurrentOrientation * Matrix.CreateLookAt(_cameraPosition, _cameraPosition + _viewDirection, Vector3.Up);
				Matrix projection = Matrix.CreatePerspectiveFieldOfView(1, aspectRatio,
					nearClip, farClip);

				foreach (ModelRendererWidget widget in _widgets.Where(w => w.RenderPosition == WidgetRenderPosition.BeforeModel))
					widget.Draw(graphicsDevice, _ballController.CurrentOrientation, view, projection);

				graphicsDevice.RasterizerState = new RasterizerState
				{
					FillMode = (_wireframe) ? FillMode.WireFrame : FillMode.Solid
				};

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

					foreach (ModelRendererWidget widget in _widgets)
						widget.OnEndDrawMesh(graphicsDevice, mesh, _boneTransforms[mesh.ParentBone.Index], view, projection);
				}

				foreach (ModelRendererWidget widget in _widgets.Where(w => w.RenderPosition == WidgetRenderPosition.AfterModel))
					widget.Draw(graphicsDevice, _ballController.CurrentOrientation, view, projection);
			}
		}

		public override void ChangeFillMode(bool wireframe)
		{
			_wireframe = wireframe;
		}

		public override void ShowNormals(bool show)
		{
			_normalsRenderer.Active = show;
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

			// Compute camera matrices.
			Vector3 eyePosition = _modelCenter;
			eyePosition.Z += _modelRadius * 2;
			//eyePosition.Y += _modelRadius;

			_cameraPosition = eyePosition;
			_viewDirection = Vector3.Normalize(_modelCenter - _cameraPosition);
		}
	}
}