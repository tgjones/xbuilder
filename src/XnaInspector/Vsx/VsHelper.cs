using System;
using System.Diagnostics;
using System.Xml;
using EnvDTE;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using VSLangProj;

namespace FormosatekLtd.ModelViewer.Vsx
{
	public static class VsHelper
	{
		public static IVsHierarchy GetCurrentHierarchy(DTE vs)
		{
			if (vs == null) throw new InvalidOperationException("DTE not found.");

			return ToHierarchy(vs.SelectedItems.Item(1).ProjectItem.ContainingProject);
		}

		public static IVsHierarchy ToHierarchy(EnvDTE.Project project)
		{
			if (project == null) throw new ArgumentNullException("project");

			string projectGuid = null;

			// DTE does not expose the project GUID that exists at in the msbuild project file.
			// Cannot use MSBuild object model because it uses a static instance of the Engine, 
			// and using the Project will cause it to be unloaded from the engine when the 
			// GC collects the variable that we declare.
			using (XmlReader projectReader = XmlReader.Create(project.FileName))
			{
				projectReader.MoveToContent();
				object nodeName = projectReader.NameTable.Add("ProjectGuid");
				while (projectReader.Read())
				{
					if (Object.Equals(projectReader.LocalName, nodeName))
					{
						projectGuid = projectReader.ReadElementContentAsString();
						break;
					}
				}
			}

			Debug.Assert(!String.IsNullOrEmpty(projectGuid));

			IServiceProvider serviceProvider = new ServiceProvider(project.DTE as
				Microsoft.VisualStudio.OLE.Interop.IServiceProvider);

			return VsShellUtilities.GetHierarchy(serviceProvider, new Guid(projectGuid));
		}

		public static IVsProject3 ToVsProject(EnvDTE.Project project)
		{
			if (project == null) throw new ArgumentNullException("project");

			IVsProject3 vsProject = ToHierarchy(project) as IVsProject3;

			if (vsProject == null)
			{
				throw new ArgumentException("Project is not a VS project.");
			}

			return vsProject;
		}

		public static VSProject ToDteProject(IVsHierarchy hierarchy)
		{
			if (hierarchy == null) throw new ArgumentNullException("hierarchy");

			object prjObject = null;
			if (hierarchy.GetProperty(0xfffffffe, -2027, out prjObject) >= 0)
			{
				EnvDTE.Project project = (Project) prjObject;
				return (VSProject)project.Object;
			}
			else
			{
				throw new ArgumentException("Hierarchy is not a project.");
			}
		}

		public static VSProject ToDteProject(IVsProject project)
		{
			if (project == null) throw new ArgumentNullException("project");

			return ToDteProject(project as IVsHierarchy);
		}
	}
}