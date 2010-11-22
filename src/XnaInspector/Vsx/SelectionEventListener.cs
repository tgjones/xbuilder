using System;
using System.Collections.Generic;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using XnaInspector.Xna;

namespace XnaInspector.Vsx
{
	public class SelectionEventListener : IVsSelectionEvents
	{
		private readonly XnaInspectorPackage _package;

		public SelectionEventListener(XnaInspectorPackage package)
		{
			_package = package;
		}

		public int OnSelectionChanged(IVsHierarchy pHierOld, uint itemidOld, IVsMultiItemSelect pMISOld, ISelectionContainer pSCOld, IVsHierarchy pHierNew, uint itemidNew, IVsMultiItemSelect pMISNew, ISelectionContainer pSCNew)
		{
			// If new selection is a file we know how to display, then display it.
			string filename;
			if (VsHelper.TryGetFileName(pHierNew, itemidNew, out filename))
			{
				if (FileExtensionUtility.IsInspectableFile(filename))
				{
					IEnumerable<string> references = VsHelper.GetProjectReferences(pHierNew);
					_package.GetInspectorWindow().LoadFile(filename, references);
				}
			}
			return VSConstants.S_OK;
		}

		public int OnElementValueChanged(uint elementid, object varValueOld, object varValueNew)
		{
			return VSConstants.S_OK;
		}

		public int OnCmdUIContextChanged(uint dwCmdUICookie, int fActive)
		{
			return VSConstants.S_OK;
		}
	}
}