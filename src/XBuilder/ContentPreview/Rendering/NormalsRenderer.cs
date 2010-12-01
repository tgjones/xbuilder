using System;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XBuilder.Options;

namespace XBuilder.ContentPreview.Rendering
{
	public class NormalsRenderer : ModelRendererWidget
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

		public NormalsRenderer(GraphicsDeviceControl parentControl, ModelRenderer renderer)
		{
			_parentControl = parentControl;
			renderer.ModelChanged += OnRendererModelChanged;
		}

		private void OnRendererModelChanged(object sender, ModelChangedEventArgs e)
		{
			_model = e.Model;
			RecreateNormals();
		}

		private void RecreateNormals()
		{
			_options = _optionsService.GetContentPreviewOptions();

			if (_model == null)
				return;

			// Extract normals.
			foreach (ModelMesh mesh in _model.Meshes)
				foreach (ModelMeshPart meshPart in mesh.MeshParts)
					meshPart.Tag = GetNormalBuffers(mesh, meshPart, _parentControl.GraphicsDevice, _options.NormalLengthPercentage);
		}

		private static NormalBuffers GetNormalBuffers(ModelMesh mesh, ModelMeshPart meshPart, GraphicsDevice graphicsDevice, float normalLengthPercentage)
		{
			if (meshPart.VertexBuffer == null)
				return null;

			Vector3[] positions = GetVertexElement(meshPart, VertexElementUsage.Position);
			if (positions == null)
				return null;
			Vector3[] normals = GetVertexElement(meshPart, VertexElementUsage.Normal);
			if (normals == null)
				return null;

			NormalBuffers normalBuffers = new NormalBuffers();

			normalBuffers.PrimitiveCount = normals.Length;
			normalBuffers.VertexCount = normals.Length * 2;

			float size = mesh.BoundingSphere.Radius * normalLengthPercentage;

			VertexBuffer vertexBuffer = new VertexBuffer(graphicsDevice,
				typeof (VertexPositionColor), normalBuffers.VertexCount,
				BufferUsage.WriteOnly);
			VertexPositionColor[] vertices = new VertexPositionColor[normalBuffers.VertexCount];
			int counter = 0;
			for (int i = 0; i < normals.Length; ++i)
			{
				Vector3 normalColorTemp = Vector3.Normalize(normals[i]);
				normalColorTemp += Vector3.One;
				normalColorTemp *= 0.5f;
				Color normalColor = new Color(normalColorTemp);
				vertices[counter++] = new VertexPositionColor(positions[i], normalColor);
				vertices[counter++] = new VertexPositionColor(positions[i] + (normals[i] * size), normalColor);
			}
			vertexBuffer.SetData(vertices);
			normalBuffers.Vertices = vertexBuffer;

			IndexBuffer indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, normalBuffers.VertexCount,
				BufferUsage.WriteOnly);
			indexBuffer.SetData(Enumerable.Range(0, normalBuffers.VertexCount).Select(i => (short) i).ToArray());
			normalBuffers.Indices = indexBuffer;

			return normalBuffers;
		}

		private static Vector3[] GetVertexElement(ModelMeshPart meshPart, VertexElementUsage usage)
		{
			VertexDeclaration vd = meshPart.VertexBuffer.VertexDeclaration;
			VertexElement[] elements = vd.GetVertexElements();

			Func<VertexElement, bool> elementPredicate = ve => ve.VertexElementUsage == usage && ve.VertexElementFormat == VertexElementFormat.Vector3;
			if (!elements.Any(elementPredicate))
				return null;

			VertexElement element = elements.First(elementPredicate);

			Vector3[] vertexData = new Vector3[meshPart.NumVertices];
			meshPart.VertexBuffer.GetData((meshPart.VertexOffset * vd.VertexStride) + element.Offset,
				vertexData, 0, vertexData.Length, vd.VertexStride);

			return vertexData;
		}

		public override void Initialize(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
		{
			_optionsService = (IOptionsService)serviceProvider.GetService(typeof(IOptionsService));
			_optionsService.OptionsChanged += (sender, e) =>
			{
				RecreateNormals();
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

			foreach (ModelMeshPart meshPart in mesh.MeshParts)
			{
				NormalBuffers normalBuffers = meshPart.Tag as NormalBuffers;
				if (normalBuffers == null)
					continue;

				graphicsDevice.SetVertexBuffer(normalBuffers.Vertices);
				graphicsDevice.Indices = normalBuffers.Indices;

				_lineEffect.World = bone;
				_lineEffect.View = view;
				_lineEffect.Projection = projection;

				foreach (EffectPass pass in _lineEffect.CurrentTechnique.Passes)
				{
					pass.Apply();
					graphicsDevice.DrawIndexedPrimitives(PrimitiveType.LineList, 0, 0,
						normalBuffers.VertexCount, 0, normalBuffers.PrimitiveCount);
				}
			}
		}

		public override void Draw(GraphicsDevice graphicsDevice, Matrix cameraRotation, Matrix view, Matrix projection)
		{
			
		}

		private class NormalBuffers
		{
			public VertexBuffer Vertices;
			public int VertexCount;
			public IndexBuffer Indices;
			public int PrimitiveCount;
		}
	}
}