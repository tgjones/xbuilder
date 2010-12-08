using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace XBuilder.Xna.Building
{
	/// <summary>
	/// This class wraps the MSBuild functionality needed to build XNA Framework
	/// content dynamically at runtime. It creates a temporary MSBuild project
	/// in memory, and adds whatever content files you choose to this project.
	/// It then builds the project, which will create compiled .xnb content files
	/// in a temporary directory. After the build finishes, you can use a regular
	/// ContentManager to load these temporary .xnb files in the usual way.
	/// </summary>
	internal class ContentBuilder : IDisposable
	{
		#region Fields

		// MSBuild objects used to dynamically build content.
		private Project _buildProject;
		private ProjectRootElement _projectRootElement;
		private BuildParameters _buildParameters;
		private readonly List<ProjectItem> _projectItems = new List<ProjectItem>();
		private readonly List<ProjectItem> _references = new List<ProjectItem>();
		private ErrorLogger _errorLogger;

		// Temporary directories used by the content build.
		private string _buildDirectory;
		private string _processDirectory;
		private string _baseDirectory;

		// Generate unique directory names if there is more than one ContentBuilder.
		private static int _directorySalt;

		// Have we been disposed?
		private bool _isDisposed;

		#endregion

		#region Properties

		/// <summary>
		/// Gets the output directory, which will contain the generated .xnb files.
		/// </summary>
		public string OutputDirectory
		{
			get { return Path.Combine(_buildDirectory, "bin/Content"); }
		}

		#endregion

		#region Initialization

		/// <summary>
		/// Creates a new content builder.
		/// </summary>
		public ContentBuilder()
		{
			CreateTempDirectory();
			CreateBuildProject();
		}


		/// <summary>
		/// Finalizes the content builder.
		/// </summary>
		~ContentBuilder()
		{
			Dispose(false);
		}


		/// <summary>
		/// Disposes the content builder when it is no longer required.
		/// </summary>
		public void Dispose()
		{
			Dispose(true);

			GC.SuppressFinalize(this);
		}


		/// <summary>
		/// Implements the standard .NET IDisposable pattern.
		/// </summary>
		protected virtual void Dispose(bool disposing)
		{
			if (!_isDisposed)
			{
				_isDisposed = true;

				DeleteTempDirectory();
			}
		}

		#endregion

		#region MSBuild

		/// <summary>
		/// Creates a temporary MSBuild content project in memory.
		/// </summary>
		private void CreateBuildProject()
		{
			string projectPath = Path.Combine(_buildDirectory, "content.contentproj");
			string outputPath = Path.Combine(_buildDirectory, "bin");

			// Create the build project.
			_projectRootElement = ProjectRootElement.Create(projectPath);

			// Include the standard targets file that defines how to build XNA Framework content.
			_projectRootElement.AddImport("$(MSBuildExtensionsPath)\\Microsoft\\XNA Game Studio\\"
				+ "v4.0\\Microsoft.Xna.GameStudio.ContentPipeline.targets");

			_buildProject = new Project(_projectRootElement);

			_buildProject.SetProperty("XnaPlatform", "Windows");
			_buildProject.SetProperty("XnaProfile", "Reach");
			_buildProject.SetProperty("XnaFrameworkVersion", XnaConstants.XnaFrameworkVersion);
			_buildProject.SetProperty("Configuration", "Release");
			_buildProject.SetProperty("OutputPath", outputPath);

			// Hook up our custom error logger.
			_errorLogger = new ErrorLogger();

			_buildParameters = new BuildParameters(ProjectCollection.GlobalProjectCollection);
			_buildParameters.Loggers = new ILogger[] { _errorLogger };
		}

		public void SetReferences(IEnumerable<string> pipelineReferences)
		{
			// Register any custom importers or processors.
			foreach (string pipelineAssembly in pipelineReferences)
				_references.Add(_buildProject.AddItem("Reference", pipelineAssembly)[0]);
		}

		/// <summary>
		/// Adds a new content file to the MSBuild project. The importer and
		/// processor are optional: if you leave the importer null, it will
		/// be autodetected based on the file extension, and if you leave the
		/// processor null, data will be passed through without any processing.
		/// </summary>
		public void Add(string fileName, string name, string importer, string processor, Dictionary<string, string> processorParameters)
		{
			// If item has already been added, don't need to add it again, but we should update build settings.
			ProjectItem item = _projectItems.FirstOrDefault(pi => pi.UnevaluatedInclude == fileName);
			if (item == null)
			{
				item = _buildProject.AddItem("Compile", fileName)[0];

				item.SetMetadataValue("Link", Path.GetFileName(fileName));
				item.SetMetadataValue("Name", name);

				_projectItems.Add(item);
			}

			if (!string.IsNullOrEmpty(importer))
				item.SetMetadataValue(XnaConstants.Importer, importer);

			if (!string.IsNullOrEmpty(processor))
				item.SetMetadataValue(XnaConstants.Processor, processor);

			foreach (var kvp in processorParameters.Where(kvp => !string.IsNullOrEmpty(kvp.Value)))
				item.SetMetadataValue(kvp.Key, kvp.Value);
		}

		/// <summary>
		/// Removes all references from the MSBuild project so that we can start again.
		/// </summary>
		public void Clear()
		{
			_buildProject.RemoveItems(_references);
			_references.Clear();
		}

		/// <summary>
		/// Builds all the content files which have been added to the project,
		/// dynamically creating .xnb files in the OutputDirectory.
		/// Returns an error message if the build fails.
		/// </summary>
		public string Build()
		{
			// Clear any previous errors.
			_errorLogger.Errors.Clear();

			// Create and submit a new asynchronous build request.
			BuildManager.DefaultBuildManager.BeginBuild(_buildParameters);

			BuildRequestData request = new BuildRequestData(_buildProject.CreateProjectInstance(), new string[0]);
			BuildSubmission submission = BuildManager.DefaultBuildManager.PendBuildRequest(request);

			submission.ExecuteAsync(null, null);

			// Wait for the build to finish.
			submission.WaitHandle.WaitOne();

			BuildManager.DefaultBuildManager.EndBuild();

			// If the build failed, return an error string.
			if (submission.BuildResult.OverallResult == BuildResultCode.Failure)
				return string.Join("\n", _errorLogger.Errors.ToArray());

			return null;
		}

		#endregion

		#region Temp Directories

		/// <summary>
		/// Creates a temporary directory in which to build content.
		/// </summary>
		private void CreateTempDirectory()
		{
			// Start with a standard base name:
			//
			//  %temp%\WinFormsContentLoading.ContentBuilder

			_baseDirectory = Path.Combine(Path.GetTempPath(), GetType().FullName);

			// Include our process ID, in case there is more than
			// one copy of the program running at the same time:
			//
			//  %temp%\WinFormsContentLoading.ContentBuilder\<ProcessId>

			int processId = Process.GetCurrentProcess().Id;

			_processDirectory = Path.Combine(_baseDirectory, processId.ToString());

			// Include a salt value, in case the program
			// creates more than one ContentBuilder instance:
			//
			//  %temp%\WinFormsContentLoading.ContentBuilder\<ProcessId>\<Salt>

			_directorySalt++;

			_buildDirectory = Path.Combine(_processDirectory, _directorySalt.ToString());

			// Create our temporary directory.
			Directory.CreateDirectory(_buildDirectory);

			PurgeStaleTempDirectories();
		}

		/// <summary>
		/// Deletes our temporary directory when we are finished with it.
		/// </summary>
		private void DeleteTempDirectory()
		{
			Directory.Delete(_buildDirectory, true);

			// If there are no other instances of ContentBuilder still using their
			// own temp directories, we can delete the process directory as well.
			if (Directory.GetDirectories(_processDirectory).Length == 0)
			{
				Directory.Delete(_processDirectory);

				// If there are no other copies of the program still using their
				// own temp directories, we can delete the base directory as well.
				if (Directory.GetDirectories(_baseDirectory).Length == 0)
					Directory.Delete(_baseDirectory);
			}
		}

		/// <summary>
		/// Ideally, we want to delete our temp directory when we are finished using
		/// it. The DeleteTempDirectory method (called by whichever happens first out
		/// of Dispose or our finalizer) does exactly that. Trouble is, sometimes
		/// these cleanup methods may never execute. For instance if the program
		/// crashes, or is halted using the debugger, we never get a chance to do
		/// our deleting. The next time we start up, this method checks for any temp
		/// directories that were left over by previous runs which failed to shut
		/// down cleanly. This makes sure these orphaned directories will not just
		/// be left lying around forever.
		/// </summary>
		private void PurgeStaleTempDirectories()
		{
			// Check all subdirectories of our base location.
			foreach (string directory in Directory.GetDirectories(_baseDirectory))
			{
				// The subdirectory name is the ID of the process which created it.
				int processId;

				if (int.TryParse(Path.GetFileName(directory), out processId))
				{
					try
					{
						// Is the creator process still running?
						Process.GetProcessById(processId);
					}
					catch (ArgumentException)
					{
						// If the process is gone, we can delete its temp directory.
						Directory.Delete(directory, true);
					}
				}
			}
		}

		#endregion
	}
}