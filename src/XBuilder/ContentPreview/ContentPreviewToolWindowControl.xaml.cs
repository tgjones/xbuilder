using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Xna.Framework.Content;
using XBuilder.ContentPreview.Rendering;
using XBuilder.Xna;
using XBuilder.Xna.Building;

namespace XBuilder.ContentPreview
{
    public partial class ContentPreviewToolWindowControl : UserControl
    {
    	private ContentBuilder _contentBuilder;
		private ContentManager _contentManager;
    	private XBuilderPackage _package;

    	private bool _loaded;
    	private AssetHandlers _assetHandlers;

        public ContentPreviewToolWindowControl()
        {
        	InitializeComponent();

        	IVsUIShell2 shell = (IVsUIShell2) Package.GetGlobalService(typeof(SVsUIShell));
        	uint vsBackColor;
        	shell.GetVSSysColorEx((int) __VSSYSCOLOREX.VSCOLOR_TOOLWINDOW_BACKGROUND,
        		out vsBackColor);
        	System.Drawing.Color backColor = System.Drawing.ColorTranslator.FromWin32((int) vsBackColor);
			graphicsDeviceControl.BackColor = backColor;
        }

		public void Initialize(XBuilderPackage package)
		{
			_package = package;
			graphicsDeviceControl.Initialize(_package);
			_contentBuilder = new ContentBuilder();
			_contentManager = new ContentManager(graphicsDeviceControl.Services, _contentBuilder.OutputDirectory);

			Loaded += (sender, e) =>
			{
				if (!_loaded) // Can't find a WPF event which only fires once when the window is FIRST loaded.
				{
					_assetHandlers = new AssetHandlers(_contentManager, graphicsDeviceControl);
					windowsFormsHost.Visibility = Visibility.Collapsed;
					_loaded = true;

					_assetHandlers.Initialize(package);
				}
			};
		}

		/// <summary>
		/// Loads a new XNA asset file into the ModelViewerControl.
		/// </summary>
		public void LoadFile(string fileName, IEnumerable<string> references)
		{
			if (!_loaded)
				return;

			// Unload any existing content.
			graphicsDeviceControl.AssetRenderer = null;
			AssetHandler assetHandler = _assetHandlers.GetAssetHandler(fileName);
			assetHandler.ResetRenderer();

			windowsFormsHost.Visibility = Visibility.Collapsed;
			txtInfo.Text = "Loading...";
			txtInfo.Visibility = Visibility.Visible;

			// Load asynchronously.
			var ui = TaskScheduler.FromCurrentSynchronizationContext();
			Task<string> loadTask = new Task<string>(() =>
			{
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
				string buildErrorInternal = _contentBuilder.Build();

				if (string.IsNullOrEmpty(buildErrorInternal))
				{
					// If the build succeeded, use the ContentManager to
					// load the temporary .xnb file that we just created.
					assetHandler.LoadContent(assetName);

					graphicsDeviceControl.AssetRenderer = assetHandler.Renderer;
				}

				return buildErrorInternal;
			});

			loadTask.ContinueWith(t =>
			{
				string buildError = t.Result;
				if (!string.IsNullOrEmpty(buildError))
				{
					// If the build failed, display an error message.
					txtInfo.Text = "Uh-oh. Something went wrong. Check the Output window for details.";
					AddToOutputWindow(buildError);
				}
				else
				{
					windowsFormsHost.Visibility = Visibility.Visible;
					txtInfo.Visibility = Visibility.Hidden;
				}
			}, ui);

			loadTask.Start();
		}

		private static void AddToOutputWindow(string message)
		{
			var outputWindow = (IVsOutputWindow) ServiceProvider.GlobalProvider.GetService(typeof(SVsOutputWindow));
			Guid guidGeneralPane = VSConstants.OutputWindowPaneGuid.GeneralPane_guid;

			ErrorHandler.ThrowOnFailure(outputWindow.CreatePane(guidGeneralPane, "General", 1, 0));

			IVsOutputWindowPane pane;
			ErrorHandler.ThrowOnFailure(outputWindow.GetPane(guidGeneralPane, out pane));
			pane.Activate();

			pane.OutputString(message);
		}

    	public bool IsModelLoaded
    	{
			get { return graphicsDeviceControl.AssetRenderer != null && graphicsDeviceControl.AssetRenderer is ModelRenderer; }
    	}

    	public void ChangeFillMode(bool wireframe)
    	{
    		graphicsDeviceControl.ChangeFillMode(wireframe);
    	}

		public void ShowNormals(bool show)
		{
			graphicsDeviceControl.ShowNormals(show);
		}

		protected override void OnMouseWheel(System.Windows.Input.MouseWheelEventArgs e)
		{
			graphicsDeviceControl.RaiseMouseWheel(e);
		}
    }
}