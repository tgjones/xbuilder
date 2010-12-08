using System;
using System.ComponentModel.Design;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using XBuilder.Options;
using XBuilder.Vsx;

namespace XBuilder.ContentPreview
{
	public class ContentPreviewIntegrationManager
	{
		private readonly XBuilderPackage _package;
		private readonly IOptionsService _optionsService;

		public ContentPreviewIntegrationManager(XBuilderPackage package)
		{
			_package = package;
			_optionsService = package.GetService<IOptionsService>();
		}

		public void Initialize()
		{
			// Add our command handlers for menu (commands must exist in the .vsct file)
			OleMenuCommandService mcs = _package.GetService<IMenuCommandService>() as OleMenuCommandService;
			if (mcs != null)
			{
				// Create commands for the Content Preview tool window toolbar
				CommandID menuCommandID = new CommandID(GuidList.guidModelViewerCmdSet, PkgCmdIDList.cmdidSolutionContextMenuPreviewContent);
				OleMenuCommand menuItem = new OleMenuCommand(OnContextMenuPreviewContent, menuCommandID);
				menuItem.BeforeQueryStatus += OnBeforeContextMenuPreviewContentQueryStatus;
				mcs.AddCommand(menuItem);

				// Create the command for the tool window
				CommandID toolwndCommandID = new CommandID(GuidList.guidModelViewerCmdSet, (int)PkgCmdIDList.cmdidModelViewer);
				MenuCommand menuToolWin = new MenuCommand(OnShowToolWindow, toolwndCommandID);
				mcs.AddCommand(menuToolWin);
			}
		}

		private void OnBeforeContextMenuPreviewContentQueryStatus(object sender, EventArgs e)
		{
			OleMenuCommand menuCommand = (OleMenuCommand) sender;

			IVsHierarchy hierarchy;
			string fileName;
			uint itemID;
			menuCommand.Visible = GetSelectedFileDetails(menuCommand, out hierarchy, out itemID, out fileName) 
				&& FileExtensionUtility.IsInspectableFile(_optionsService, fileName);
		}

		private void OnContextMenuPreviewContent(object sender, EventArgs e)
		{
			IVsHierarchy hierarchy;
			string fileName;
			uint itemID;

			if (GetSelectedFileDetails(sender as OleMenuCommand, out hierarchy, out itemID, out fileName))
			{
				IContentPreviewService previewService = _package.GetService<IContentPreviewService>();
				previewService.ShowPreview(hierarchy, itemID, fileName);	
			}
		}

		private static bool GetSelectedFileDetails(OleMenuCommand menuCommand, out IVsHierarchy hierarchy, out uint itemID, out string fileName)
		{
			if (menuCommand != null)
			{
				IVsMonitorSelection monitorSelection = (IVsMonitorSelection)ServiceProvider.GlobalProvider.GetService(typeof(SVsShellMonitorSelection));
				IntPtr ppHier;
				IVsMultiItemSelect ppMIS;
				IntPtr ppSC;
				monitorSelection.GetCurrentSelection(out ppHier, out itemID, out ppMIS, out ppSC);
				hierarchy = Marshal.GetTypedObjectForIUnknown(ppHier, typeof(IVsHierarchy)) as IVsHierarchy;

				// If new selection is a file we know how to display, then display it.
				if (VsHelper.TryGetFileName(hierarchy, itemID, out fileName))
				{
					return true;
				}
			}

			hierarchy = null;
			itemID = 0;
			fileName = null;
			return false;
		}

		private void OnShowToolWindow(object sender, EventArgs e)
		{
			IContentPreviewService previewService = _package.GetService<IContentPreviewService>();
			previewService.ShowPreview();
		}
	}
}