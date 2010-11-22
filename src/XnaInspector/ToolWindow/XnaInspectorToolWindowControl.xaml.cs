using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework.Content;
using XnaInspector.Xna;
using XnaInspector.Xna.Building;

namespace XnaInspector.ToolWindow
{
    public partial class ModelViewerToolWindowControl : UserControl
    {
    	private readonly ContentBuilder _contentBuilder;
		private readonly ContentManager _contentManager;

    	private AssetHandlers _assetHandlers;

        public ModelViewerToolWindowControl()
        {
        	InitializeComponent();

			_contentBuilder = new ContentBuilder();
			_contentManager = new ContentManager(graphicsDeviceControl.Services, _contentBuilder.OutputDirectory);

        	Loaded += (sender, e) =>
        	{
        		_assetHandlers = new AssetHandlers(_contentManager, graphicsDeviceControl);
        	};
        }

		/// <summary>
		/// Loads a new XNA asset file into the ModelViewerControl.
		/// </summary>
		public void LoadFile(string fileName, IEnumerable<string> references)
		{
			Cursor = Cursors.Wait;

			// Unload any existing content.
			graphicsDeviceControl.AssetRenderer = null;
			AssetHandler assetHandler = _assetHandlers.GetAssetHandler(fileName);
			assetHandler.ResetRenderer();
			_contentManager.Unload();

			// Tell the ContentBuilder what to build.
			_contentBuilder.Clear();
			_contentBuilder.SetReferences(references);
			_contentBuilder.Add(fileName, "Asset", null, assetHandler.ProcessorName);

			// Build this new model data.
			string buildError = _contentBuilder.Build();

			if (string.IsNullOrEmpty(buildError))
			{
				// If the build succeeded, use the ContentManager to
				// load the temporary .xnb file that we just created.
				assetHandler.LoadContent("Asset");
				graphicsDeviceControl.AssetRenderer = assetHandler.Renderer;
			}
			else
			{
				// If the build failed, display an error message.
				MessageBox.Show(buildError, "Error");
			}

			Cursor = Cursors.Arrow;
		}
    }
}