using System;
using System.Globalization;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace XBuilder.Vsx
{
	internal static class XBuilderWindowPane
	{
		// Fields
		private static bool failedPaneCreation;
		private static IVsOutputWindowPane windowPane;

		// Methods
		internal static void WriteLine(string format, params object[] args)
		{
			if ((windowPane == null) && !failedPaneCreation)
			{
				IVsOutputWindow globalService = Package.GetGlobalService(typeof(SVsOutputWindow)) as IVsOutputWindow;
				Guid gameStudioWindowPane = GuidList.XBuilderWindowPane;
				if (ErrorHandler.Failed(globalService.GetPane(ref gameStudioWindowPane, out windowPane)))
				{
					try
					{
						ErrorHandler.ThrowOnFailure(globalService.CreatePane(ref gameStudioWindowPane, "XBuilder", 1, 1));
						ErrorHandler.ThrowOnFailure(globalService.GetPane(ref gameStudioWindowPane, out windowPane));
					}
					catch
					{
						failedPaneCreation = true;
						throw;
					}
				}
			}
			if (windowPane != null)
			{
				ErrorHandler.ThrowOnFailure(windowPane.Activate());
				ErrorHandler.ThrowOnFailure(windowPane.OutputString(string.Format(CultureInfo.CurrentUICulture, format + "\n", args)));
			}
		}
	}
}