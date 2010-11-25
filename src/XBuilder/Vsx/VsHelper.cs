using System.Collections.Generic;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

namespace XBuilder.Vsx
{
	internal static class VsHelper
	{
		public static bool TryGetFileName(IVsHierarchy hierarchy, uint itemID, out string fileName)
		{
			if (hierarchy != null)
			{
				string value;
				if (ErrorHandler.Succeeded(hierarchy.GetCanonicalName(itemID, out value)))
				{
					fileName = value;
					return true;
				}
			}
			fileName = null;
			return false;
		}

		private static VSProject GetProject(IVsHierarchy hierarchy)
		{
			object projectObject;
			if (ErrorHandler.Succeeded(hierarchy.GetProperty((uint)VSConstants.VSITEMID.Root, (int)__VSHPROPID.VSHPROPID_ExtObject, out projectObject)))
			{
				Project project = (Project) projectObject;
				return (VSProject) project.Object;
			}
			return null;
		}

		public static IEnumerable<string> GetProjectReferences(IVsHierarchy hierarchy)
		{
			List<string> references = new List<string>();

			// Get project object for specified hierarchy.
			VSProject project = GetProject(hierarchy);
			if (project == null)
				return references;

			// Get references from project.
			int referenceCount = project.References.Count;
			for (int i = 1; i <= referenceCount; ++i)
			{
				Reference reference = project.References.Item(i);
				references.Add(reference.Path);
			}
			return references;
		}
	}
}