using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using XnaInspector.ToolWindow;
using XnaInspector.Vsx;

namespace XnaInspector.ContentPreview
{
	public class ContentPreviewService : IContentPreviewService
	{
		private readonly XnaInspectorPackage _package;

		public ContentPreviewService(XnaInspectorPackage package)
		{
			_package = package;
		}

		private XnaInspectorToolWindow GetInspectorWindow()
		{
			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			ToolWindowPane window = _package.FindToolWindow(typeof(XnaInspectorToolWindow), 0, true);
			if (window == null || window.Frame == null)
				throw new NotSupportedException(Resources.CanNotCreateWindow);

			return (XnaInspectorToolWindow)window;
		}

		public void ShowPreview(IVsHierarchy hierarchy, string fileName)
		{
			if (FileExtensionUtility.IsInspectableFile(_package, fileName))
			{
				XnaInspectorToolWindow window = ShowPreviewInternal();

				IEnumerable<string> references = VsHelper.GetProjectReferences(hierarchy);
				window.LoadFile(fileName, references);
			}
		}

		public void ShowPreview()
		{
			ShowPreviewInternal();
		}

		private XnaInspectorToolWindow ShowPreviewInternal()
		{
			XnaInspectorToolWindow window = GetInspectorWindow();

			IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

			return window;
		}
	}
}