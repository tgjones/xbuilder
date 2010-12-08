using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XBuilder.Options;
using XBuilder.Util;
using XBuilder.Xna;

namespace XBuilder.ContentPreview.Rendering
{
	public class BoundingBoxRenderer : ModelRendererWidget
	{
		private readonly GraphicsDeviceControl _parentControl;
		private BasicEffect _lineEffect;
		private IOptionsService _optionsService;
		private XBuilderOptionsContentPreview _options;
		private Model _model;

		public override WidgetRenderPosition RenderPosition
		{
			get { return WidgetRenderPosition.AfterModel; }
		}

		public bool Active { get; set; }

		public BoundingBoxRenderer(GraphicsDeviceControl parentControl, ModelRenderer renderer)
		{
			_parentControl = parentControl;
			renderer.ModelChanged += OnRendererModelChanged;
		}

		private void OnRendererModelChanged(object sender, ModelChangedEventArgs e)
		{
			_model = e.Model;
			RecreateBoundingBox();
		}

		private void RecreateBoundingBox()
		{
			_options = _optionsService.GetContentPreviewOptions();

			if (_model == null)
				return;

			foreach (ModelMesh mesh in _model.Meshes)
			{
				BoundingBox boundingBox = new BoundingBox();
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
				{
					BoundingBox? bbox = GetBoundingBox(meshPart);
					if (bbox != null)
						boundingBox = BoundingBox.CreateMerged(boundingBox, bbox.Value);
				}

				mesh.Tag = CreateBoundingBoxBuffers(boundingBox, _parentControl.GraphicsDevice);
			}
		}

		private BoundingBoxBuffers CreateBoundingBoxBuffers(BoundingBox boundingBox, GraphicsDevice graphicsDevice)
		{
			BoundingBoxBuffers boundingBoxBuffers = new BoundingBoxBuffers();

			boundingBoxBuffers.PrimitiveCount = 24;
			boundingBoxBuffers.VertexCount = 48;

			VertexBuffer vertexBuffer = new VertexBuffer(graphicsDevice,
				typeof(VertexPositionColor), boundingBoxBuffers.VertexCount,
				BufferUsage.WriteOnly);
			List<VertexPositionColor> vertices = new List<VertexPositionColor>();

			const float ratio = 5.0f;

			Vector3 xOffset = new Vector3((boundingBox.Max.X - boundingBox.Min.X) / ratio, 0, 0);
			Vector3 yOffset = new Vector3(0, (boundingBox.Max.Y - boundingBox.Min.Y) / ratio, 0);
			Vector3 zOffset = new Vector3(0, 0, (boundingBox.Max.Z - boundingBox.Min.Z) / ratio);
			Vector3[] corners = boundingBox.GetCorners();

			// Corner 1.
			AddVertex(vertices, corners[0]);
			AddVertex(vertices, corners[0] + xOffset);
			AddVertex(vertices, corners[0]);
			AddVertex(vertices, corners[0] - yOffset);
			AddVertex(vertices, corners[0]);
			AddVertex(vertices, corners[0] - zOffset);

			// Corner 2.
			AddVertex(vertices, corners[1]);
			AddVertex(vertices, corners[1] - xOffset);
			AddVertex(vertices, corners[1]);
			AddVertex(vertices, corners[1] - yOffset);
			AddVertex(vertices, corners[1]);
			AddVertex(vertices, corners[1] - zOffset);

			// Corner 3.
			AddVertex(vertices, corners[2]);
			AddVertex(vertices, corners[2] - xOffset);
			AddVertex(vertices, corners[2]);
			AddVertex(vertices, corners[2] + yOffset);
			AddVertex(vertices, corners[2]);
			AddVertex(vertices, corners[2] - zOffset);

			// Corner 4.
			AddVertex(vertices, corners[3]);
			AddVertex(vertices, corners[3] + xOffset);
			AddVertex(vertices, corners[3]);
			AddVertex(vertices, corners[3] + yOffset);
			AddVertex(vertices, corners[3]);
			AddVertex(vertices, corners[3] - zOffset);

			// Corner 5.
			AddVertex(vertices, corners[4]);
			AddVertex(vertices, corners[4] + xOffset);
			AddVertex(vertices, corners[4]);
			AddVertex(vertices, corners[4] - yOffset);
			AddVertex(vertices, corners[4]);
			AddVertex(vertices, corners[4] + zOffset);

			// Corner 6.
			AddVertex(vertices, corners[5]);
			AddVertex(vertices, corners[5] - xOffset);
			AddVertex(vertices, corners[5]);
			AddVertex(vertices, corners[5] - yOffset);
			AddVertex(vertices, corners[5]);
			AddVertex(vertices, corners[5] + zOffset);

			// Corner 7.
			AddVertex(vertices, corners[6]);
			AddVertex(vertices, corners[6] - xOffset);
			AddVertex(vertices, corners[6]);
			AddVertex(vertices, corners[6] + yOffset);
			AddVertex(vertices, corners[6]);
			AddVertex(vertices, corners[6] + zOffset);

			// Corner 8.
			AddVertex(vertices, corners[7]);
			AddVertex(vertices, corners[7] + xOffset);
			AddVertex(vertices, corners[7]);
			AddVertex(vertices, corners[7] + yOffset);
			AddVertex(vertices, corners[7]);
			AddVertex(vertices, corners[7] + zOffset);

			vertexBuffer.SetData(vertices.ToArray());
			boundingBoxBuffers.Vertices = vertexBuffer;

			IndexBuffer indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, boundingBoxBuffers.VertexCount,
				BufferUsage.WriteOnly);
			indexBuffer.SetData(Enumerable.Range(0, boundingBoxBuffers.VertexCount).Select(i => (short)i).ToArray());
			boundingBoxBuffers.Indices = indexBuffer;

			return boundingBoxBuffers;
		}

		private void AddVertex(List<VertexPositionColor> vertices, Vector3 position)
		{
			vertices.Add(new VertexPositionColor(position, ConvertUtility.ToXnaColor(_options.BoundingBoxColor)));
		}

		private static BoundingBox? GetBoundingBox(ModelMeshPart meshPart)
		{
			if (meshPart.VertexBuffer == null)
				return null;

			Vector3[] positions = VertexElementExtractor.GetVertexElement(meshPart, VertexElementUsage.Position);
			if (positions == null)
				return null;

			return BoundingBox.CreateFromPoints(positions);
		}

		public override void Initialize(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
		{
			_optionsService = (IOptionsService)serviceProvider.GetService(typeof(IOptionsService));
			_optionsService.OptionsChanged += (sender, e) =>
			{
				RecreateBoundingBox();
				_parentControl.Invalidate();
			};

			_lineEffect = new BasicEffect(graphicsDevice);
			_lineEffect.LightingEnabled = false;
			_lineEffect.TextureEnabled = false;
			_lineEffect.VertexColorEnabled = true;
		}

		public override void OnEndDrawMesh(GraphicsDevice graphicsDevice, ModelMesh mesh, Matrix bone,
			Matrix view, Matrix projection)
		{
			if (!Active)
				return;

			BoundingBoxBuffers boundingBoxBuffers = mesh.Tag as BoundingBoxBuffers;
			if (boundingBoxBuffers == null)
				return;

			graphicsDevice.SetVertexBuffer(boundingBoxBuffers.Vertices);
			graphicsDevice.Indices = boundingBoxBuffers.Indices;

			_lineEffect.World = bone;
			_lineEffect.View = view;
			_lineEffect.Projection = projection;

			foreach (EffectPass pass in _lineEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0,
					boundingBoxBuffers.VertexCount, 0, boundingBoxBuffers.PrimitiveCount);
			}
		}

		public override void Draw(GraphicsDevice graphicsDevice, Matrix cameraRotation, Matrix view, Matrix projection)
		{
			
		}

		private class BoundingBoxBuffers
		{
			public VertexBuffer Vertices;
			public int VertexCount;
			public IndexBuffer Indices;
			public int PrimitiveCount;
		}
	}
}