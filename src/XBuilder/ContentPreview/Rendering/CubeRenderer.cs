using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace XBuilder.ContentPreview.Rendering
{
	public class CubeRenderer : ModelRendererWidget
	{
		private readonly int _size;
		private Effect _effect;
		private VertexBuffer _cubeVertices;
		private IndexBuffer _cubeIndices;
		private bool _isCubeActive;

		public CubeRenderer(GraphicsDeviceControl parentControl, CameraController cameraController)
		{
			parentControl.MouseDown += (sender, e) =>
			{
				if (IsWithinCameraCube(parentControl, e.Location))
				{
					cameraController.MouseDown(ToXnaPoint(e.Location));
					_isCubeActive = true;
					parentControl.Invalidate();
				}
			};
			parentControl.MouseMove += (sender, e) =>
			{
				bool isCubeActive = _isCubeActive;
				_isCubeActive = IsWithinCameraCube(parentControl, e.Location) || cameraController.IsMouseButtonDown;
				if (cameraController.MouseMove(ToXnaPoint(e.Location)) || isCubeActive != _isCubeActive)
					parentControl.Invalidate();
			};
			parentControl.MouseUp += (sender, e) =>
			{
				if (!IsWithinCameraCube(parentControl, e.Location))
				{
					_isCubeActive = false;
					parentControl.Invalidate();
				}
				cameraController.MouseUp(ToXnaPoint(e.Location));
			};

			_size = 100; // TODO: Make this a configurable setting.
		}

		private bool IsWithinCameraCube(GraphicsDeviceControl parentControl, System.Drawing.Point p)
		{
			return (p.X > parentControl.ClientSize.Width - _size)
				&& p.Y < _size;
		}

		private static Point ToXnaPoint(System.Drawing.Point p)
		{
			return new Point(p.X, p.Y);
		}

		public override WidgetRenderPosition RenderPosition
		{
			get { return WidgetRenderPosition.AfterModel; }
		}

		public override void Initialize(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
		{
			ResourceContentManager contentManager = new ResourceContentManager(serviceProvider, Resources.ResourceManager);

			BasicEffect basicEffect = new BasicEffect(graphicsDevice);
			basicEffect.VertexColorEnabled = false;
			basicEffect.LightingEnabled = false;
			basicEffect.TextureEnabled = true;
			basicEffect.Texture = contentManager.Load<Texture2D>("CubeTexture");
			basicEffect.Projection = Matrix.CreatePerspectiveFieldOfView(1, 1, 1, 10);
			basicEffect.View = Matrix.CreateLookAt(Vector3.Backward * 2, Vector3.Zero, Vector3.Up);
			_effect = basicEffect;

			CreateCube(graphicsDevice, 1);
		}

		private void CreateCube(GraphicsDevice graphicsDevice, int size)
		{
			List<short> indices = new List<short>();
			List<VertexPositionTexture> vertices = new List<VertexPositionTexture>();

			// A cube has six faces, each one pointing in a different direction.
			Vector3[] normals =
				{
					Vector3.Backward,
					Vector3.Forward,
					Vector3.Right,
					Vector3.Left,
					Vector3.Up,
					Vector3.Down
				};
			Vector2[,] texCoords =
				{
					{ // front
						new Vector2(0.50f, 0.66f),
						new Vector2(0.25f, 0.66f),
						new Vector2(0.25f, 0.33f),
						new Vector2(0.50f, 0.33f)
					},
					{ // back
						new Vector2(0.75f, 0.33f),
						new Vector2(1.00f, 0.33f),
						new Vector2(1.00f, 0.66f),
						new Vector2(0.75f, 0.66f)
					},
					{ // right
						new Vector2(0.75f, 0.33f),
						new Vector2(0.75f, 0.66f),
						new Vector2(0.50f, 0.66f),
						new Vector2(0.50f, 0.33f)
					},
					{ // left
						new Vector2(0.25f, 0.33f),
						new Vector2(0.25f, 0.66f),
						new Vector2(0.00f, 0.66f),
						new Vector2(0.00f, 0.33f)
					},
					{ // top
						new Vector2(0.25f, 0.33f),
						new Vector2(0.25f, 0.00f),
						new Vector2(0.50f, 0.00f),
						new Vector2(0.50f, 0.33f)
					},
					{ // bottom
						new Vector2(0.50f, 0.66f),
						new Vector2(0.50f, 1.00f),
						new Vector2(0.25f, 1.00f),
						new Vector2(0.25f, 0.66f)
					},
				};

			// Create each face in turn.
			for (int i = 0; i < normals.Length; ++i)
			{
				Vector3 normal = normals[i];

				// Get two vectors perpendicular to the face normal and to each other.
				Vector3 side1 = new Vector3(normal.Y, normal.Z, normal.X);
				Vector3 side2 = Vector3.Cross(normal, side1);

				// Six indices (two triangles) per face.
				int currentVertex = vertices.Count;
				indices.Add((short)(currentVertex + 0));
				indices.Add((short)(currentVertex + 1));
				indices.Add((short)(currentVertex + 2));

				indices.Add((short)(currentVertex + 0));
				indices.Add((short)(currentVertex + 2));
				indices.Add((short)(currentVertex + 3));

				// Four vertices per face.
				vertices.Add(new VertexPositionTexture((normal - side1 - side2) * size / 2, texCoords[i, 0])); // TL
				vertices.Add(new VertexPositionTexture((normal - side1 + side2) * size / 2, texCoords[i, 1])); // TR
				vertices.Add(new VertexPositionTexture((normal + side1 + side2) * size / 2, texCoords[i, 2])); // BR
				vertices.Add(new VertexPositionTexture((normal + side1 - side2) * size / 2, texCoords[i, 3])); // BL
			}

			_cubeVertices = new VertexBuffer(graphicsDevice, typeof (VertexPositionTexture), vertices.Count,
				BufferUsage.WriteOnly);
			_cubeVertices.SetData(vertices.ToArray());

			_cubeIndices = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, indices.Count, BufferUsage.WriteOnly);
			_cubeIndices.SetData(indices.ToArray());
		}

		public override void Draw(GraphicsDevice graphicsDevice, Matrix cameraRotation, Matrix view, Matrix projection)
		{
			Viewport savedViewport = graphicsDevice.Viewport;

			graphicsDevice.Viewport = new Viewport(
				graphicsDevice.Viewport.Width - _size,
				0, _size, _size);

			graphicsDevice.BlendState = BlendState.AlphaBlend;
			graphicsDevice.RasterizerState = new RasterizerState
			{
				FillMode = FillMode.Solid
			};

			if (_effect is IEffectMatrices)
			{
				IEffectMatrices effectMatrices = (IEffectMatrices)_effect;
				effectMatrices.World = cameraRotation;
			}

			if (_effect is BasicEffect)
			{
				((BasicEffect) _effect).Alpha = _isCubeActive ? 1.0f : 0.7f; // TODO: Make non-hover alpha configurable.
			}

			graphicsDevice.Indices = _cubeIndices;
			graphicsDevice.SetVertexBuffer(_cubeVertices);
			foreach (EffectPass pass in _effect.CurrentTechnique.Passes)
			{
				pass.Apply();
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0,
					_cubeVertices.VertexCount, 0, _cubeIndices.IndexCount/3);
			}

			graphicsDevice.Indices = null;
			graphicsDevice.Viewport = savedViewport;

			graphicsDevice.BlendState = BlendState.Opaque;
		}
	}
}