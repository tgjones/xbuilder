using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace XBuilder.ContentPreview
{
    /// <summary>
    /// This class implements the tool window exposed by this package and hosts a user control.
    ///
    /// In Visual Studio tool windows are composed of a frame (implemented by the shell) and a pane, 
    /// usually implemented by the package implementer.
    ///
    /// This class derives from the ToolWindowPane class provided from the MPF in order to use its 
    /// implementation of the IVsUIElementPane interface.
    /// </summary>
    [Guid("f358ad4b-049b-4aa3-9646-f13e5e5722f9")]
    public class ContentPreviewToolWindow : ToolWindowPane
    {
		private OleMenuCommand _fillModeSolid;
		private OleMenuCommand _fillModeWireframe;
    	private OleMenuCommand _normals;
		private OleMenuCommand _alphaBlend;

		private ShadingMode _shadingMode;

        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ContentPreviewToolWindow() :
            base(null)
        {
        	ToolBar = new CommandID(
        		GuidList.guidModelViewerCmdSet,
        		PkgCmdIDList.cmdidContentPreviewToolbar);

            // Set the window title reading it from the resources.
            this.Caption = Resources.ToolWindowTitle;
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 303;
            this.BitmapIndex = 0;

            // This is the user control hosted by the tool window; Note that, even if this class implements IDisposable,
            // we are not calling Dispose on this object. This is because ToolWindowPane calls Dispose on 
            // the object returned by the Content property.
			base.Content = new ContentPreviewToolWindowControl();
        }

		public override void OnToolWindowCreated()
		{
			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = ((XBuilderPackage) Package).GetService<IMenuCommandService>() as OleMenuCommandService;
			if (mcs != null)
			{
				_fillModeSolid = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarFillModeSolid, ChangeFillModeSolid);
				_fillModeWireframe = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarFillModeWireframe, ChangeFillModeWireframe);
				AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarFillModeSolidAndWireframe, ChangeFillModeSolidAndWireframe);
				_normals = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarNormals, ToggleNormals);
				_alphaBlend = AddCommand(mcs, PkgCmdIDList.cmdidContentPreviewToolbarAlphaBlend, ToggleAlphaBlend);
			}

			((ContentPreviewToolWindowControl)base.Content).Initialize((XBuilderPackage) Package);

			base.OnToolWindowCreated();
		}

		private static OleMenuCommand AddCommand(IMenuCommandService menuCommandService, int commandID, EventHandler eventHandler)
		{
			CommandID menuCommandID = new CommandID(GuidList.guidModelViewerCmdSet, commandID);
			OleMenuCommand menuItem = new OleMenuCommand(eventHandler, menuCommandID);
			menuCommandService.AddCommand(menuItem);
			return menuItem;
		}

		private void ChangeFillModeSolid(object sender, EventArgs e)
		{
			_shadingMode = ShadingMode.Solid;
			ChangeFillMode();
		}

		private void ChangeFillModeWireframe(object sender, EventArgs e)
		{
			_shadingMode = ShadingMode.Wireframe;
			ChangeFillMode();
		}

		private void ChangeFillModeSolidAndWireframe(object sender, EventArgs e)
		{
			_shadingMode = ShadingMode.SolidAndWireframe;
			ChangeFillMode();
		}

		private void ToggleNormals(object sender, EventArgs e)
		{
			_normals.Checked = !_normals.Checked;
			ShowNormals();
		}

		private void ToggleAlphaBlend(object sender, EventArgs e)
		{
			_alphaBlend.Checked = !_alphaBlend.Checked;
			ToggleAlphaBlend();
		}

		public void LoadFile(string fileName, IEnumerable<string> references)
		{
			((ContentPreviewToolWindowControl)base.Content).LoadFile(fileName, references);

			//bool isModelLoaded = ((ContentPreviewToolWindowControl) base.Content).IsModelLoaded;
			//_fillModeSolid.Enabled = _fillModeWireframe.Enabled = isModelLoaded;

			ChangeFillMode();
			ShowNormals();
		}

    	private void ChangeFillMode()
    	{
			((ContentPreviewToolWindowControl)base.Content).ChangeFillMode(_shadingMode);
    	}

		private void ShowNormals()
		{
			((ContentPreviewToolWindowControl)base.Content).ShowNormals(_normals.Checked);
		}

		private void ToggleAlphaBlend()
		{
			((ContentPreviewToolWindowControl)base.Content).ToggleAlphaBlend(_alphaBlend.Checked);
		}
    }

	public enum ShadingMode
	{
		Solid,
		Wireframe,
		SolidAndWireframe
	}
}
