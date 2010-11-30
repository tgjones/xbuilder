using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using XBuilder.Options;
using XBuilder.Vsx;

namespace XBuilder.ContentPreview
{
	public class ContentPreviewService : IContentPreviewService
	{
		private readonly XBuilderPackage _package;
		private readonly IOptionsService _optionsService;

		public ContentPreviewService(XBuilderPackage package)
		{
			_package = package;
			_optionsService = package.GetService<IOptionsService>();
		}

		private ContentPreviewToolWindow GetInspectorWindow()
		{
			// Get the instance number 0 of this tool window. This window is single instance so this instance
			// is actually the only one.
			// The last flag is set to true so that if the tool window does not exists it will be created.
			ToolWindowPane window = _package.FindToolWindow(typeof(ContentPreviewToolWindow), 0, true);
			if (window == null || window.Frame == null)
				throw new NotSupportedException(Resources.CanNotCreateWindow);

			return (ContentPreviewToolWindow)window;
		}

		public void ShowPreview(IVsHierarchy hierarchy, string fileName)
		{
			if (FileExtensionUtility.IsInspectableFile(_optionsService, fileName))
			{
				ContentPreviewToolWindow window = ShowPreviewInternal();

				IEnumerable<string> references = VsHelper.GetProjectReferences(hierarchy);
				window.LoadFile(fileName, references);
			}
		}

		public void ShowPreview()
		{
			ShowPreviewInternal();
		}

		private ContentPreviewToolWindow ShowPreviewInternal()
		{
			ContentPreviewToolWindow window = GetInspectorWindow();

			IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
			Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

			return window;
		}
	}
}