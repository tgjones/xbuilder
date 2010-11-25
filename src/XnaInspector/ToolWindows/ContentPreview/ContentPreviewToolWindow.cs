using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;

namespace XBuilder.ToolWindows.ContentPreview
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
        /// <summary>
        /// Standard constructor for the tool window.
        /// </summary>
        public ContentPreviewToolWindow() :
            base(null)
        {
            Caption = "XNA Content Preview";
            // Set the image that will appear on the tab of the window frame
            // when docked with an other window
            // The resource ID correspond to the one defined in the resx file
            // while the Index is the offset in the bitmap strip. Each image in
            // the strip being 16x16.
            this.BitmapResourceID = 301;
            this.BitmapIndex = 1;

        	this.ToolBar = new CommandID(
        		GuidList.guidTWToolbarCmdSet,
        		PkgCmdIDList.TWToolbar);

			base.Content = new ContentPreviewToolWindowControl();
        }

		public void LoadFile(string fileName, IEnumerable<string> references)
		{
			((ContentPreviewToolWindowControl)base.Content).LoadFile(fileName, references);
		}
    }
}
