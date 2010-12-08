using System;
using EnvDTE;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.Xna.PlatformTools.ContentPipeline.Views;
using VSLangProj;
using XBuilder.Xna.Building;

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

		public static XnaBuildProperties GetXnaBuildProperties(IVsHierarchy hierarchy, uint itemID)
		{
			XnaBuildProperties buildProperties = new XnaBuildProperties();

			// Get project object for specified hierarchy.
			VSProject project = GetProject(hierarchy);
			if (project == null)
				throw new InvalidOperationException("Could not find content project for this item.");

			// Get references from project.
			int referenceCount = project.References.Count;
			for (int i = 1; i <= referenceCount; ++i)
			{
				Reference reference = project.References.Item(i);
				buildProperties.ProjectReferences.Add(reference.Path);
			}

			IVsBuildPropertyStorage buildPropertyStorage = (IVsBuildPropertyStorage) hierarchy;

			string importer;
			buildPropertyStorage.GetItemAttribute(itemID, XnaConstants.Importer, out importer);
			if (!string.IsNullOrEmpty(importer))
				buildProperties.Importer = importer;

			string processor;
			buildPropertyStorage.GetItemAttribute(itemID, XnaConstants.Processor, out processor);
			if (!string.IsNullOrEmpty(processor))
				buildProperties.Processor = processor;

			if (buildProperties.Processor == null)
				return buildProperties;

			// TODO: Look into caching ContentPipelineManager, but then we need to take care of refreshing it
			// when the content project references change.
			ContentPipelineManager contentPipelineManager = new ContentPipelineManager(project.References, XnaConstants.XnaFrameworkVersion);
			var processorParameters = contentPipelineManager.GetProcessorParameters(buildProperties.Processor);

			foreach (IParameterDescriptor processorParameter in processorParameters)
			{
				string propertyValue;
				buildPropertyStorage.GetItemAttribute(itemID, XnaConstants.ProcessorParametersPrefix + processorParameter.PropertyName, out propertyValue);
				buildProperties.ProcessorParameters.Add(XnaConstants.ProcessorParametersPrefix + processorParameter.PropertyName, propertyValue);
			}

			/*
			// Cannot use MSBuild object model because it uses a static instance of the Engine.
			XDocument projectDocument = XDocument.Load(project.Project.FullName);
			string projectFolder = Path.GetDirectoryName(project.Project.FullName);

			var projectItem = projectDocument.Descendants().FirstOrDefault(n =>
			{
				XAttribute attr = n.Attribute("Include");
				if (attr == null)
					return false;

				string includeValue = attr.Value;
				return string.Equals(Path.Combine(projectFolder, includeValue), fileName, StringComparison.InvariantCultureIgnoreCase);
			});
			if (projectItem == null)
					throw new InvalidOperationException("Could not find item in project.");

			if (projectItem.Element(PropertyNames.Importer) != null)
				buildProperties.Importer = projectItem.Element(PropertyNames.Importer).Value;
			if (projectItem.Element(PropertyNames.Processor) != null)
				buildProperties.Processor = projectItem.Element(PropertyNames.Processor).Value;

			foreach (XElement processorParameter in projectItem.Elements().Where(e => e.Name.LocalName.StartsWith(PropertyNames.ProcessorParametersPrefix)))
				buildProperties.ProcessorParameters.Add(processorParameter.Name.LocalName, processorParameter.Value);
			 * */

			return buildProperties;
		}
	}
}