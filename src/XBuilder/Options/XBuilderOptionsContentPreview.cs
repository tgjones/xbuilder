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
					throw new ArgumentException("Size must be an odd-value number greater than 1.", "size");
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

		public XBuilderOptionsContentPreview()
		{
			GridSize = 101;
			GridSpacing = 10;
			MajorLinesEveryNthGridLine = 10;
			GridLineMinorColor = Color.Gray;
			GridLineMajorColor = Color.Black;
			GridLineAxisColor = Color.CornflowerBlue;
			ShowGrid = true;
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