using System;
using System.AddIn.Hosting;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Win32;
using Microsoft.Xna.PlatformTools.ContentPipeline.Views;
using VSLangProj;
using XBuilder.Vsx;

namespace XBuilder.Xna.Building
{
	/// <summary>
	/// It would be great if Microsoft.Xna.GameStudio.ContentProject.ContentPipelineManager in
	/// Microsoft.Xna.GameStudio.Core was public, but it's not. I don't want to use reflection,
	/// so I've reproduced as much as I can using only public interfaces and members.
	/// </summary>
	public class ContentPipelineManager
	{
		private string[] _referencedAssemblies;
		private readonly IComponentScanner _pipelineScanner;
		private bool _needsRefresh;

		public ContentPipelineManager(References references, string xnaFrameworkVersion)
		{
			if (references == null)
				throw new ArgumentNullException("references");
			_referencedAssemblies = new string[0];
			OnReferencesChanged(references);

			_pipelineScanner = GetPipelineScanner(xnaFrameworkVersion);	
		}

		internal IList<IParameterDescriptor> GetProcessorParameters(string processorName)
		{
			if (processorName == null)
				throw new ArgumentNullException("processorName");

			if (_pipelineScanner != null)
			{
				UpdatePipelineComponentScanner();
				return _pipelineScanner.Processors
					.Where(descriptor => processorName == descriptor.Name)
					.Select(descriptor => descriptor.Parameters)
					.FirstOrDefault();
			}
			return null;
		}

		private static IComponentScanner GetPipelineScanner(string xnaFrameworkVersion)
		{
			if (string.IsNullOrEmpty(xnaFrameworkVersion))
			{
				XBuilderWindowPane.WriteLine(Resources.PipelineAddInMissingVersionProperty, new object[0]);
			}
			else
			{
				string pipelineAddInRootPath = GetPipelineAddInRootPath();
				if (!string.IsNullOrEmpty(pipelineAddInRootPath))
				{
					try
					{
						foreach (AddInToken token in AddInStore.FindAddIns(typeof (IComponentScanner), pipelineAddInRootPath, new string[0]))
						{
							string addInFrameworkVersion;
							if (token.QualificationData[AddInSegmentType.AddIn].TryGetValue("XnaFrameworkVersion", out addInFrameworkVersion)
								&& addInFrameworkVersion != null && addInFrameworkVersion == xnaFrameworkVersion)
							{
								try
								{
									return token.Activate<IComponentScanner>(AppDomain.CurrentDomain);
								}
								catch (TargetInvocationException exception)
								{
									string message = string.Empty;
									if (exception.InnerException != null)
										message = exception.InnerException.Message;
									XBuilderWindowPane.WriteLine(
										"An error occurred while instantiating the content pipeline component scanner for version '{0}': {1}",
										new object[] {xnaFrameworkVersion, message});
									continue;
								}
							}
						}
						XBuilderWindowPane.WriteLine(
							"XNA Framework version '{0}' is not supported by this installation of XNA Game Studio, or a required component is missing.",
							new object[] {xnaFrameworkVersion});
					}
					catch (InvalidOperationException ex)
					{
						XBuilderWindowPane.WriteLine(Resources.PipelineAddInInvalidPipeline, new object[] {ex.Message});
					}
					catch (DirectoryNotFoundException ex)
					{
						XBuilderWindowPane.WriteLine(Resources.PipelineAddInInvalidPipeline, new object[] {ex.Message});
					}
					catch (InvalidPipelineStoreException ex)
					{
						XBuilderWindowPane.WriteLine(Resources.PipelineAddInInvalidPipeline, new object[] {ex.Message});
					}
				}
			}
			return null;
		}

		private static string GetPipelineAddInRootPath()
		{
			string result = null;
			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\XNA\Game Studio\");
			if (key != null)
			{
				string sharedComponentsPath = key.GetValue("SharedComponentsPath", string.Empty) as string;
				result = Path.Combine(sharedComponentsPath, "PipelineHosting");
			}
			if (result == "PipelineHosting")
				result = null;
			return result;
		}

		private void UpdatePipelineComponentScanner()
		{
			if (!_needsRefresh || (_pipelineScanner == null))
				return;

			_pipelineScanner.Scan(_referencedAssemblies, new string[0]);
			foreach (string str in _pipelineScanner.Errors)
				XBuilderWindowPane.WriteLine(str, new object[0]);
			_needsRefresh = false;
		}

		internal void OnReferencesChanged(References references)
		{
			_needsRefresh = true;
			if (references == null)
			{
				_referencedAssemblies = new string[0];
			}
			else
			{
				_referencedAssemblies = new string[references.Count];
				int num = 0;
				try
				{
					foreach (Reference reference in references)
					{
						_referencedAssemblies[num++] = reference.Path;
					}
				}
				catch
				{
					_referencedAssemblies = new string[0];
					throw;
				}
			}
		}
	}
}