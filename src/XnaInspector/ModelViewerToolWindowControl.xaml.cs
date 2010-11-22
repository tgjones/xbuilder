using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using XnaInspector.Xna;

namespace XnaInspector
{
    /// <summary>
    /// Interaction logic for MyControl.xaml
    /// </summary>
    public partial class ModelViewerToolWindowControl : UserControl
    {
    	ContentBuilder contentBuilder;
		ContentManager contentManager;

        public ModelViewerToolWindowControl()
        {
        	InitializeComponent();

			contentBuilder = new ContentBuilder();

			contentManager = new ContentManager(modelViewerControl.Services,
												contentBuilder.OutputDirectory);

        }

		/// <summary>
		/// Loads a new 3D model file into the ModelViewerControl.
		/// </summary>
		public void LoadModel(string fileName, List<string> references)
		{
			Cursor = Cursors.Wait;

			// Unload any existing model.
			modelViewerControl.Model = null;
			contentManager.Unload();

			// Tell the ContentBuilder what to build.
			contentBuilder.Clear();
			contentBuilder.SetReferences(references);
			contentBuilder.Add(fileName, "Model", null, "ModelProcessor");

			// Build this new model data.
			string buildError = contentBuilder.Build();

			if (string.IsNullOrEmpty(buildError))
			{
				// If the build succeeded, use the ContentManager to
				// load the temporary .xnb file that we just created.
				modelViewerControl.Model = contentManager.Load<Model>("Model");
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