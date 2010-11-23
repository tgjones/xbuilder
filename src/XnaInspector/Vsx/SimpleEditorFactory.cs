using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using IServiceProvider = Microsoft.VisualStudio.OLE.Interop.IServiceProvider;

namespace XnaInspector.Vsx
{
	public abstract class SimpleEditorFactory<TEditorPane> : IVsEditorFactory
		where TEditorPane: WindowPane, IOleCommandTarget, IVsPersistDocData, IPersistFileFormat, new()
	{
		private IServiceProvider _serviceProvider;

		public int CreateEditorInstance(uint grfCreateDoc, string pszMkDocument, string pszPhysicalView, IVsHierarchy pvHier, uint itemid, IntPtr punkDocDataExisting, out IntPtr ppunkDocView, out IntPtr ppunkDocData, out string pbstrEditorCaption, out Guid pguidCmdUI, out int pgrfCDW)
		{
			ppunkDocView = IntPtr.Zero;
			ppunkDocData = IntPtr.Zero;
			pguidCmdUI = GetType().GUID;
			pgrfCDW = 0;
			pbstrEditorCaption = null;

			// Validate inputs
			if ((grfCreateDoc & (VSConstants.CEF_OPENFILE | VSConstants.CEF_SILENT)) == 0)
				return VSConstants.E_INVALIDARG;

			if (punkDocDataExisting != IntPtr.Zero)
				return VSConstants.VS_E_INCOMPATIBLEDOCDATA;

			// Create the Document (editor)
			TEditorPane newEditor = new TEditorPane();
			ppunkDocView = Marshal.GetIUnknownForObject(newEditor);
			ppunkDocData = Marshal.GetIUnknownForObject(newEditor);
			pbstrEditorCaption = "";
			return VSConstants.S_OK;
		}

		public int SetSite(IServiceProvider serviceProvider)
		{
			_serviceProvider = serviceProvider;
			return VSConstants.S_OK;
		}

		public int Close()
		{
			return VSConstants.S_OK;
		}

		public int MapLogicalView(ref Guid rguidLogicalView, out string pbstrPhysicalView)
		{
			pbstrPhysicalView = null;
			if (rguidLogicalView == VSConstants.LOGVIEWID_Primary)
				return VSConstants.S_OK;
			return VSConstants.E_NOTIMPL;
		}
	}
}