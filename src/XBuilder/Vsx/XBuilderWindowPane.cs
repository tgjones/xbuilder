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
		private static bool _failedPaneCreation;
		private static IVsOutputWindowPane _windowPane;

		// Methods
		internal static void WriteLine(string format, params object[] args)
		{
			if ((_windowPane == null) && !_failedPaneCreation)
			{
				IVsOutputWindow globalService = (IVsOutputWindow) Package.GetGlobalService(typeof(SVsOutputWindow));
				Guid xBuilderWindowPane = GuidList.XBuilderWindowPane;
				if (ErrorHandler.Failed(globalService.GetPane(ref xBuilderWindowPane, out _windowPane)))
				{
					try
					{
						ErrorHandler.ThrowOnFailure(globalService.CreatePane(ref xBuilderWindowPane, "XBuilder", 1, 1));
						ErrorHandler.ThrowOnFailure(globalService.GetPane(ref xBuilderWindowPane, out _windowPane));
					}
					catch
					{
						_failedPaneCreation = true;
						throw;
					}
				}
			}
			if (_windowPane != null)
			{
				ErrorHandler.ThrowOnFailure(_windowPane.Activate());
				ErrorHandler.ThrowOnFailure(_windowPane.OutputString(string.Format(CultureInfo.CurrentUICulture, format + "\n", args)));
			}
		}
	}
}