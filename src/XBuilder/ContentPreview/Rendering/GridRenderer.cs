using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using XBuilder.Options;
using XBuilder.Util;

namespace XBuilder.ContentPreview.Rendering
{
	public class GridRenderer : ModelRendererWidget
	{
		private readonly GraphicsDeviceControl _parentControl;
		private XBuilderOptionsContentPreview _options;
		private Effect _lineEffect;
		private VertexBuffer _lineBuffer;
		private int _spacing, _majorLines;
		private Color _minorColor, _majorColor, _axisColor;

		public override WidgetRenderPosition RenderPosition
		{
			get { return WidgetRenderPosition.BeforeModel; }
		}

		public GridRenderer(GraphicsDeviceControl parentControl)
		{
			_parentControl = parentControl;
		}

		public override void Initialize(IServiceProvider serviceProvider, GraphicsDevice graphicsDevice)
		{
			IOptionsService optionsService = (IOptionsService) serviceProvider.GetService(typeof (IOptionsService));
			optionsService.OptionsChanged += (sender, e) =>
			{
				RecreateGrid(graphicsDevice, optionsService);
				_parentControl.Invalidate();
			};

			_lineEffect = new BasicEffect(graphicsDevice);
			((BasicEffect) _lineEffect).VertexColorEnabled = true;
			((BasicEffect) _lineEffect).LightingEnabled = false;
			((BasicEffect) _lineEffect).TextureEnabled = false;

			RecreateGrid(graphicsDevice, optionsService);
		}

		private void RecreateGrid(GraphicsDevice graphicsDevice, IOptionsService optionsService)
		{
			_options = optionsService.GetContentPreviewOptions();
			_spacing = _options.GridSpacing;
			_majorLines = _options.MajorLinesEveryNthGridLine;
			_minorColor = ConvertUtility.ToXnaColor(_options.GridLineMinorColor);
			_majorColor = ConvertUtility.ToXnaColor(_options.GridLineMajorColor);
			_axisColor = ConvertUtility.ToXnaColor(_options.GridLineAxisColor);

			int size = _options.GridSize;

			_lineBuffer = new VertexBuffer(graphicsDevice, typeof(VertexPositionColor), size * 4, BufferUsage.WriteOnly);

			VertexPositionColor[] vertices = new VertexPositionColor[size * 4];

			int end = (size - 1) / 2;
			int start = -end;

			int index = 0;

			// Lines along z.
			for (int x = start; x <= end; ++x)
			{
				Color color = GetColor(x);
				vertices[index++] = new VertexPositionColor(new Vector3(GetCoordinate(x), 0, GetCoordinate(start)), color);
				vertices[index++] = new VertexPositionColor(new Vector3(GetCoordinate(x), 0, GetCoordinate(end)), color);
			}

			// Lines along x.
			for (int z = start; z <= end; ++z)
			{
				Color color = GetColor(z);
				vertices[index++] = new VertexPositionColor(new Vector3(GetCoordinate(start), 0, GetCoordinate(z)), color);
				vertices[index++] = new VertexPositionColor(new Vector3(GetCoordinate(end), 0, GetCoordinate(z)), color);
			}

			_lineBuffer.SetData(vertices);
		}

		private Color GetColor(int value)
		{
			if (value == 0)
				return _axisColor;
			return (value % _majorLines == 0) ? _majorColor : _minorColor;
		}

		private float GetCoordinate(int value)
		{
			return value*_spacing;
		}

		public override void Draw(GraphicsDevice graphicsDevice, Matrix cameraRotation, Matrix view, Matrix projection)
		{
			if (!_options.ShowGrid)
				return;

			if (_lineEffect is IEffectMatrices)
			{
				IEffectMatrices effectMatrices = (IEffectMatrices) _lineEffect;
				effectMatrices.World = Matrix.Identity;
				effectMatrices.View = view;
				effectMatrices.Projection = projection;
			}

			graphicsDevice.SetVertexBuffer(_lineBuffer);
			foreach (EffectPass pass in _lineEffect.CurrentTechnique.Passes)
			{
				pass.Apply();
				graphicsDevice.DrawPrimitives(PrimitiveType.LineList, 0, 
					_lineBuffer.VertexCount / 2);
			}
		}
	}
}