using System.Collections.Generic;
using System.IO;
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

    	private bool _loaded;
    	private AssetHandlers _assetHandlers;

        public ModelViewerToolWindowControl()
        {
        	InitializeComponent();

			_contentBuilder = new ContentBuilder();
			_contentManager = new ContentManager(graphicsDeviceControl.Services, _contentBuilder.OutputDirectory);

        	Loaded += (sender, e) =>
        	{
        		_assetHandlers = new AssetHandlers(_contentManager, graphicsDeviceControl);
				_loaded = true;
        	};
        }

		/// <summary>
		/// Loads a new XNA asset file into the ModelViewerControl.
		/// </summary>
		public void LoadFile(string fileName, IEnumerable<string> references)
		{
			if (!_loaded)
				return;

			Cursor = Cursors.Wait;

			// Unload any existing content.
			graphicsDeviceControl.AssetRenderer = null;
			AssetHandler assetHandler = _assetHandlers.GetAssetHandler(fileName);
			assetHandler.ResetRenderer();
			_contentManager.Unload();

			// Tell the ContentBuilder what to build.
			_contentBuilder.Clear();
			_contentBuilder.SetReferences(references);

			string assetName = fileName;
			foreach (char c in Path.GetInvalidFileNameChars())
				assetName = assetName.Replace(c.ToString(), string.Empty);
			assetName = Path.GetFileNameWithoutExtension(assetName);
			_contentBuilder.Add(fileName, assetName, null, assetHandler.ProcessorName);

			// Build this new model data.
			string buildError = _contentBuilder.Build();

			if (string.IsNullOrEmpty(buildError))
			{
				// If the build succeeded, use the ContentManager to
				// load the temporary .xnb file that we just created.
				assetHandler.LoadContent(assetName);
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