using System;
using System.ComponentModel;
using System.Drawing;
using Microsoft.VisualStudio.Shell;

namespace XBuilder.Options
{
	public class XBuilderOptionsContentPreview : DialogPage
	{
		[Category("Grid"),
		DisplayName("Show Grid"),
		Description("Whether the grid is visible in Content Preview."),
		DefaultValue(true)]
		public bool ShowGrid { get; set; }

		private int _gridSize;

		[Category("Grid"),
		DisplayName("Grid Size"),
		Description("Must be an odd number."),
		DefaultValue(101)]
		public int GridSize
		{
			get { return _gridSize; }
			set
			{
				if (value <= 1 || value%2 == 0)
					throw new ArgumentException("Size must be an odd-value number greater than 1.", "value");
				_gridSize = value;
			}
		}

		[Category("Grid"),
		DisplayName("Grid Spacing"),
		Description("Spacing of grid lines in Content Preview."),
		DefaultValue(10)]
		public int GridSpacing { get; set; }

		[Category("Grid"),
		DisplayName("Major Lines every Nth Grid Line"),
		Description("Spacing of major lines in the grid in Content Preview."),
		DefaultValue(10)]
		public int MajorLinesEveryNthGridLine { get; set; }

		[Category("Grid"),
		DisplayName("Grid Line Minor Color"),
		DefaultValue(typeof(Color), "Gray")]
		public Color GridLineMinorColor { get; set; }

		[Category("Grid"),
		DisplayName("Grid Line Major Color"),
		DefaultValue(typeof(Color), "Black")]
		public Color GridLineMajorColor { get; set; }

		[Category("Grid"),
		DisplayName("Grid Line Axis Color"),
		DefaultValue(typeof(Color), "CornflowerBlue")]
		public Color GridLineAxisColor { get; set; }

		private float _normalLengthPercentage;

		[Category("Normals"),
		DisplayName("Normal Length"),
		Description("Specify normal length as a percentage of the model radius, i.e. 0.1 would mean rendered normal length = model radius / 10."),
		DefaultValue(0.05f)]
		public float NormalLengthPercentage
		{
			get { return _normalLengthPercentage; }
			set
			{
				if (value <= 0.0f)
					throw new ArgumentException("Percentage must be greater than 0.", "value");
				_normalLengthPercentage = value;
			}
		}

		private float _solidAndWireframeAlpha;

		[Category("Solid + Wireframe Shading Mode"),
		DisplayName("Wireframe Transparency"),
		Description("Specify transparency for the wireframe part of the Solid + Wireframe shading mode. 0 is totally transparent, 1 is totally opaque."),
		DefaultValue(0.5f)]
		public float SolidAndWireframeAlpha
		{
			get { return _solidAndWireframeAlpha; }
			set
			{
				if (value <= 0.0f || value > 1.0f)
					throw new ArgumentException("Transparency must be greater than 0 and less than or equal to 1.", "value");
				_solidAndWireframeAlpha = value;
			}
		}

		[Category("Solid + Wireframe Shading Mode"),
		DisplayName("Wireframe Color"),
		DefaultValue(typeof(Color), "Yellow")]
		public Color SolidAndWireframeColor { get; set; }

		private float _solidAndWireframeDepthBias;

		[Category("Solid + Wireframe Shading Mode"),
		DisplayName("Depth Bias"),
		Description("Specify depth bias for the wireframe part of the Solid + Wireframe shading mode. You might need to adjust this, depending on the size of your models, and the distance from the camera to the model. It will usually be a very small number. It must be negative, otherwise the wireframe would be behind the solid part."),
		DefaultValue(-0.00001f)]
		public float SolidAndWireframeDepthBias
		{
			get { return _solidAndWireframeDepthBias; }
			set
			{
				if (value >= 0.0f)
					throw new ArgumentException("Depth bias must be less than 0.", "value");
				_solidAndWireframeDepthBias = value;
			}
		}

		[Category("Bounding Boxes"),
		DisplayName("Bounding Box Color"),
		DefaultValue(typeof(Color), "White")]
		public Color BoundingBoxColor { get; set; }

		public XBuilderOptionsContentPreview()
		{
			GridSize = 101;
			GridSpacing = 10;
			MajorLinesEveryNthGridLine = 10;
			GridLineMinorColor = Color.Gray;
			GridLineMajorColor = Color.Black;
			GridLineAxisColor = Color.CornflowerBlue;
			ShowGrid = true;
			NormalLengthPercentage = 0.05f;
			SolidAndWireframeAlpha = 0.5f;
			SolidAndWireframeColor = Color.Yellow;
			SolidAndWireframeDepthBias = -0.00001f;
			BoundingBoxColor = Color.White;
		}

		protected override void OnApply(PageApplyEventArgs e)
		{
			base.OnApply(e);

			// Make sure we broadcast changes to these options.
			IOptionsService optionsService = (IOptionsService) GetService(typeof (IOptionsService));
			optionsService.InvokeOptionsChanged(EventArgs.Empty);
		}
	}
}