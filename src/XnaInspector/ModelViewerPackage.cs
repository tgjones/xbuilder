using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Runtime.InteropServices;
using System.ComponentModel.Design;
using EnvDTE;
using FormosatekLtd.ModelViewer.Vsx;
using Microsoft.Win32;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using VSLangProj;

namespace FormosatekLtd.ModelViewer
{
    /// <summary>
    /// This is the class that implements the package exposed by this assembly.
    ///
    /// The minimum requirement for a class to be considered a valid package for Visual Studio
    /// is to implement the IVsPackage interface and register itself with the shell.
    /// This package uses the helper classes defined inside the Managed Package Framework (MPF)
    /// to do it: it derives from the Package class that provides the implementation of the 
    /// IVsPackage interface and uses the registration attributes defined in the framework to 
    /// register itself and its components with the shell.
    /// </summary>
    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "1.0", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(ModelViewerToolWindow))]
    [Guid(GuidList.guidModelViewerPkgString)]
	[ProvideAutoLoad(VSConstants.UICONTEXT.SolutionExists_string)]
    public sealed class ModelViewerPackage : Package
    {
        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public ModelViewerPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));
        }

        /// <summary>
        /// This function is called when the user clicks the menu item that shows the 
        /// tool window. See the Initialize method to see how the menu item is associated to 
        /// this function using the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void ShowToolWindow(object sender, EventArgs e)
        {
            // Get the instance number 0 of this tool window. This window is single instance so this instance
            // is actually the only one.
            // The last flag is set to true so that if the tool window does not exists it will be created.
            ToolWindowPane window = this.FindToolWindow(typeof(ModelViewerToolWindow), 0, true);
            if ((null == window) || (null == window.Frame))
            {
                throw new NotSupportedException(Resources.CanNotCreateWindow);
            }

            IVsWindowFrame windowFrame = (IVsWindowFrame)window.Frame;
            Microsoft.VisualStudio.ErrorHandler.ThrowOnFailure(windowFrame.Show());

			OleMenuCommand menuCommand = sender as OleMenuCommand;
			if (menuCommand != null)
			{
				List<string> references;
				string fileName = GetCurrentFileName(out references);
				if (fileName != null && IsRecognisedModelFile(fileName))
				{
					ModelViewerToolWindow myToolWindow = (ModelViewerToolWindow)window;
					myToolWindow.LoadModel(fileName, references);
				}
			}
        }


        /////////////////////////////////////////////////////////////////////////////
        // Overriden Package Implementation
        #region Package Members

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
        protected override void Initialize()
        {
            Trace.WriteLine (string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));
            base.Initialize();

            // Add our command handlers for menu (commands must exist in the .vsct file)
            OleMenuCommandService mcs = GetService(typeof(IMenuCommandService)) as OleMenuCommandService;
            if ( null != mcs )
            {
                // Create the command for the menu item.
                CommandID menuCommandID = new CommandID(GuidList.guidModelViewerCmdSet, (int)PkgCmdIDList.cmdidViewModel);
				OleMenuCommand menuItem = new OleMenuCommand(MenuItemCallback, menuCommandID);
            	menuItem.BeforeQueryStatus += queryStatusMenuCommand_BeforeQueryStatus;
                mcs.AddCommand( menuItem );
                // Create the command for the tool window
                CommandID toolwndCommandID = new CommandID(GuidList.guidModelViewerCmdSet, (int)PkgCmdIDList.cmdidModelViewer);
                MenuCommand menuToolWin = new MenuCommand(ShowToolWindow, toolwndCommandID);
                mcs.AddCommand( menuToolWin );
            }
        }
        #endregion

		private void queryStatusMenuCommand_BeforeQueryStatus(object sender, EventArgs e)
		{
			OleMenuCommand menuCommand = sender as OleMenuCommand;
			if (menuCommand != null)
			{
				List<string> references;
				string fileName = GetCurrentFileName(out references);
				if (fileName != null && IsRecognisedModelFile(fileName))
					menuCommand.Visible = true;
				else
					menuCommand.Visible = false;
			}
		}

		private static string GetCurrentFileName(out List<string> references)
		{
			references = null;

			IntPtr hierarchyPtr, selectionContainerPtr;
			uint projectItemId;
			IVsMultiItemSelect mis;
			IVsMonitorSelection monitorSelection =
				(IVsMonitorSelection) Package.GetGlobalService(typeof (SVsShellMonitorSelection));
			monitorSelection.GetCurrentSelection(out hierarchyPtr, out projectItemId, out mis, out selectionContainerPtr);
			if (hierarchyPtr == IntPtr.Zero)
				return null;

			IVsHierarchy hierarchy = Marshal.GetTypedObjectForIUnknown(hierarchyPtr, typeof (IVsHierarchy)) as IVsHierarchy;
			if (hierarchy != null)
			{
				string value;
				hierarchy.GetCanonicalName(projectItemId, out value);

				IVsHierarchy projectHierarchy = VsHelper.GetCurrentHierarchy((DTE) GetGlobalService(typeof (DTE)));
				VSProject project = VsHelper.ToDteProject(projectHierarchy);
				int referenceCount = project.References.Count;
				references = new List<string>();
				for (int i = 1; i <= referenceCount; ++i)
				{
					Reference reference = project.References.Item(i);
					references.Add(reference.Path);
				}

				return value;
			}
			return null;
		}

    	private static bool IsRecognisedModelFile(string fileName)
		{
			if (fileName == null)
				return false;

			string extension = Path.GetExtension(fileName);
			if (extension == null)
				return false;

			switch (extension.ToLower())
			{
				case ".fbx" :
				case ".x":
				case ".3ds":
				case ".obj":
				case ".nff":
					return true;
				default :
					return false;
			}
		}


    	/// <summary>
        /// This function is the callback used to execute a command when the a menu item is clicked.
        /// See the Initialize method to see how the menu item is associated to this function using
        /// the OleMenuCommandService service and the MenuCommand class.
        /// </summary>
        private void MenuItemCallback(object sender, EventArgs e)
        {
        	ShowToolWindow(sender, e);
        }
    }
}
