using System;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace XnaInspector.Vsx
{
	public abstract class SimpleEditorPane : WindowPane, IVsPersistDocData, IPersistFileFormat
	{
		int IVsPersistDocData.GetGuidEditorType(out Guid pClassID)
		{
			throw new NotImplementedException();
		}

		int IVsPersistDocData.IsDocDataDirty(out int pfDirty)
		{
			throw new NotImplementedException();
		}

		int IVsPersistDocData.SetUntitledDocPath(string pszDocDataPath)
		{
			throw new NotImplementedException();
		}

		int IVsPersistDocData.LoadDocData(string pszMkDocument)
		{
			throw new NotImplementedException();
		}

		int IVsPersistDocData.SaveDocData(VSSAVEFLAGS dwSave, out string pbstrMkDocumentNew, out int pfSaveCanceled)
		{
			throw new NotImplementedException();
		}

		int IVsPersistDocData.Close()
		{
			throw new NotImplementedException();
		}

		int IVsPersistDocData.OnRegisterDocData(uint docCookie, IVsHierarchy pHierNew, uint itemidNew)
		{
			throw new NotImplementedException();
		}

		int IVsPersistDocData.RenameDocData(uint grfAttribs, IVsHierarchy pHierNew, uint itemidNew, string pszMkDocumentNew)
		{
			throw new NotImplementedException();
		}

		int IVsPersistDocData.IsDocDataReloadable(out int pfReloadable)
		{
			throw new NotImplementedException();
		}

		int IVsPersistDocData.ReloadDocData(uint grfFlags)
		{
			throw new NotImplementedException();
		}

		private int GetClassID(out Guid pClassID)
		{
			throw new NotImplementedException();
		}

		int IPersist.GetClassID(out Guid pClassID)
		{
			return GetClassID(out pClassID);
		}

		int IPersistFileFormat.IsDirty(out int pfIsDirty)
		{
			throw new NotImplementedException();
		}

		int IPersistFileFormat.InitNew(uint nFormatIndex)
		{
			throw new NotImplementedException();
		}

		int IPersistFileFormat.Load(string pszFilename, uint grfMode, int fReadOnly)
		{
			throw new NotImplementedException();
		}

		int IPersistFileFormat.Save(string pszFilename, int fRemember, uint nFormatIndex)
		{
			throw new NotImplementedException();
		}

		int IPersistFileFormat.SaveCompleted(string pszFilename)
		{
			throw new NotImplementedException();
		}

		int IPersistFileFormat.GetCurFile(out string ppszFilename, out uint pnFormatIndex)
		{
			throw new NotImplementedException();
		}

		int IPersistFileFormat.GetFormatList(out string ppszFormatList)
		{
			throw new NotImplementedException();
		}

		int IPersistFileFormat.GetClassID(out Guid pClassID)
		{
			return GetClassID(out pClassID);
		}
	}
}