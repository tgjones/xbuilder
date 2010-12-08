using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XBuilder.Options;
using XBuilder.Util;

namespace XBuilder.ContentPreview.Rendering
{
	/// <summary>
	/// Example control inherits from GraphicsDeviceControl, and displays
	/// a spinning 3D model. The main form class is responsible for loading
	/// the model: this control just displays it.
	/// </summary>
	public class ModelRenderer : AssetRenderer
	{
		private readonly GraphicsDeviceControl _parentControl;
		public event EventHandler<ModelChangedEventArgs> ModelChanged;

		protected void OnModelChanged(ModelChangedEventArgs e)
		{
			EventHandler<ModelChangedEventArgs> handler = ModelChanged;
			if (handler != null) handler(this, e);
		}

		private IOptionsService _optionsService;
		private XBuilderOptionsContentPreview _options;

		// Cache information about the model size and position.
		private Matrix[] _boneTransforms;
		private Vector3 _modelCenter;
		private Vector3 _cameraPosition;
		private Vector3 _viewDirection;
		private float _modelRadius;

		private readonly CameraController _ballController;
		private readonly ModelRendererWidget[] _widgets;

		private readonly NormalsRenderer _normalsRenderer;
		private readonly BoundingBoxRenderer _bboxRenderer;

		private Model _model;
		private ShadingMode _shadingMode;
		private bool _alphaBlend;

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
			_parentControl = parentControl;
			_ballController = new CameraController();

			parentControl.MouseWheelWpf += (sender, e) =>
			{
				_cameraPosition += _viewDirection * e.Delta * _modelRadius * 0.001f;
				parentControl.Invalidate();
			};

			_widgets = new ModelRendererWidget[4];
			_widgets[0] = new GridRenderer(parentControl);
			_widgets[1] = new CubeRenderer(parentControl, _ballController);

			_normalsRenderer = new NormalsRenderer(parentControl, this);
			_widgets[2] = _normalsRenderer;

			_bboxRenderer = new BoundingBoxRenderer(parentControl, this);
			_widgets[3] = _bboxRenderer;
		}

		public override void Initialize(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
		{
			_optionsService = (IOptionsService) serviceProvider.GetService(typeof (IOptionsService));
			_optionsService.OptionsChanged += (sender, e) =>
			{
				SetOptions();
				_parentControl.Invalidate();
			};
			SetOptions();

			foreach (ModelRendererWidget widget in _widgets)
				widget.Initialize(serviceProvider, graphicsDevice);
		}

		private void SetOptions()
		{
			_options = _optionsService.GetContentPreviewOptions();
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

				graphicsDevice.BlendState = _alphaBlend ? BlendState.AlphaBlend : BlendState.Opaque;

				graphicsDevice.RasterizerState = new RasterizerState
				{
					FillMode = (_shadingMode == ShadingMode.Wireframe) ? FillMode.WireFrame : FillMode.Solid
				};

				// Draw the model.
				DrawModel(graphicsDevice, view, projection, world, null);

				if (_shadingMode == ShadingMode.SolidAndWireframe)
				{
					graphicsDevice.RasterizerState = new RasterizerState
					{
						FillMode = FillMode.WireFrame,
						DepthBias = _options.SolidAndWireframeDepthBias
					};

					// Draw the model with a fixed colour.
					graphicsDevice.BlendState = BlendState.AlphaBlend;
					DrawModel(graphicsDevice, view, projection, world,
						ConvertUtility.ToXnaColor(_options.SolidAndWireframeColor).ToVector3());

					graphicsDevice.RasterizerState = new RasterizerState
					{
						FillMode = FillMode.Solid,
						DepthBias = 0.0f
					};
				}

				graphicsDevice.BlendState = BlendState.Opaque;

				foreach (ModelRendererWidget widget in _widgets.Where(w => w.RenderPosition == WidgetRenderPosition.AfterModel))
					widget.Draw(graphicsDevice, _ballController.CurrentOrientation, view, projection);
			}
		}

		private void DrawModel(GraphicsDevice graphicsDevice, Matrix view, Matrix projection, Matrix world, Vector3? color)
		{
			DrawModel(true, world, view, projection, color);

			foreach (ModelMesh mesh in _model.Meshes)
				foreach (ModelRendererWidget widget in _widgets)
					widget.OnEndDrawMesh(graphicsDevice, mesh, _boneTransforms[mesh.ParentBone.Index], view, projection);

			DrawModel(false, world, view, projection, color);
		}

		private void DrawModel(bool opaque, Matrix world, Matrix view, Matrix projection, Vector3? color)
		{
			foreach (ModelMesh mesh in _model.Meshes)
				DrawMesh(mesh, opaque, world, view, projection, color);
		}

		private void DrawMesh(ModelMesh mesh, bool opaque, Matrix world, Matrix view, Matrix projection, Vector3? color)
		{
			int count = mesh.MeshParts.Count;
			for (int i = 0; i < count; i++)
			{
				ModelMeshPart part = mesh.MeshParts[i];
				Effect effect = part.Effect;

				Vector3 diffuseColor = Vector3.One;
				bool textureEnabled = false, lightingEnabled = false, vertexColorEnabled = false;
				float alpha = 1.0f;
				BasicEffect basicEffect = effect as BasicEffect;
				if (basicEffect != null)
				{
					if (basicEffect.Alpha < 1.0f && opaque)
						continue;
					if (basicEffect.Alpha == 1.0f && !opaque)
						continue;

					basicEffect.PreferPerPixelLighting = true;
					basicEffect.SpecularPower = 16;

					if (color != null)
					{
						diffuseColor = basicEffect.DiffuseColor;
						textureEnabled = basicEffect.TextureEnabled;
						lightingEnabled = basicEffect.LightingEnabled;
						vertexColorEnabled = basicEffect.VertexColorEnabled;
						alpha = basicEffect.Alpha;

						basicEffect.DiffuseColor = color.Value;
						basicEffect.TextureEnabled = false;
						basicEffect.LightingEnabled = false;
						basicEffect.VertexColorEnabled = false;
						basicEffect.Alpha = _options.SolidAndWireframeAlpha;
					}
				}

				IEffectMatrices effectMatrices = effect as IEffectMatrices;
				if (effectMatrices != null)
				{
					effectMatrices.World = _boneTransforms[mesh.ParentBone.Index] * world;
					effectMatrices.View = view;
					effectMatrices.Projection = projection;
				}

				IEffectLights effectLights = effect as IEffectLights;
				if (effectLights != null)
					effectLights.EnableDefaultLighting();

				int passes = effect.CurrentTechnique.Passes.Count;
				for (int j = 0; j < passes; j++)
				{
					effect.CurrentTechnique.Passes[j].Apply();
					DrawMeshPart(part);
				}

				if (basicEffect != null && color != null)
				{
					basicEffect.DiffuseColor = diffuseColor;
					basicEffect.Alpha = alpha;
					basicEffect.TextureEnabled = textureEnabled;
					basicEffect.LightingEnabled = lightingEnabled;
					basicEffect.VertexColorEnabled = vertexColorEnabled;
				}
			}
		}

		private static void DrawMeshPart(ModelMeshPart meshPart)
		{
			if (meshPart.NumVertices > 0)
			{
				GraphicsDevice graphicsDevice = meshPart.VertexBuffer.GraphicsDevice;
				graphicsDevice.SetVertexBuffer(meshPart.VertexBuffer, meshPart.VertexOffset);
				graphicsDevice.Indices = meshPart.IndexBuffer;
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, meshPart.NumVertices,
					meshPart.StartIndex, meshPart.PrimitiveCount);
			}
		}

		public override void ChangeFillMode(ShadingMode shadingMode)
		{
			_shadingMode = shadingMode;
		}

		public override void ShowNormals(bool show)
		{
			_normalsRenderer.Active = show;
		}

		public override void ShowBoundingBox(bool show)
		{
			_bboxRenderer.Active = show;
		}

		public override void ToggleAlphaBlend(bool show)
		{
			_alphaBlend = show;
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