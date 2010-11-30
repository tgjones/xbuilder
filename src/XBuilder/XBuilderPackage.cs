using System.ComponentModel.Design;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using XBuilder.ContentPreview;
using XBuilder.Options;

namespace XBuilder
{
	#region Package attributes

    // This attribute tells the PkgDef creation utility (CreatePkgDef.exe) that this class is
    // a package.
    [PackageRegistration(UseManagedResourcesOnly = true)]
    // This attribute is used to register the informations needed to show the this package
    // in the Help/About dialog of Visual Studio.
    [InstalledProductRegistration("#110", "#112", "0.1", IconResourceID = 400)]
    // This attribute is needed to let the shell know that this package exposes some menus.
    [ProvideMenuResource("Menus.ctmenu", 1)]
    // This attribute registers a tool window exposed by this package.
    [ProvideToolWindow(typeof(ContentPreviewToolWindow))]
	[ProvideOptionPage(typeof(XBuilderOptionsGeneral), "XBuilder", "General", 120, 121, true)]
	[ProvideOptionPage(typeof(XBuilderOptionsContentPreview), "XBuilder", "ContentPreview", 120, 122, true)]
    [Guid(GuidList.guidModelViewerPkgString)]
	[ProvideAutoLoad("{f1536ef8-92ec-443c-9ed7-fdadf150da82}")]

	#endregion
	public sealed class XBuilderPackage : Package
    {
    	private readonly ContentPreviewIntegrationManager _contentPreviewIntegrationManager;

        /// <summary>
        /// Default constructor of the package.
        /// Inside this method you can place any initialization code that does not require 
        /// any Visual Studio service because at this point the package object is created but 
        /// not sited yet inside Visual Studio environment. The place to do all the other 
        /// initialization is the Initialize method.
        /// </summary>
        public XBuilderPackage()
        {
            Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering constructor for: {0}", this.ToString()));

        	AddService<IOptionsService>(new OptionsService(this));
			AddService<IContentPreviewService>(new ContentPreviewService(this));

        	_contentPreviewIntegrationManager = new ContentPreviewIntegrationManager(this);
        }

		public void AddService<TService>(object serviceInstance)
		{
			((IServiceContainer) this).AddService(typeof (TService), serviceInstance);
		}

		public TService GetService<TService>()
		{
			return (TService) GetService(typeof(TService));
		}

		public T GetAutomationObject<T>(string name)
		{
			return (T) GetAutomationObject(name);
		}

        /// <summary>
        /// Initialization of the package; this method is called right after the package is sited, so this is the place
        /// where you can put all the initilaization code that rely on services provided by VisualStudio.
        /// </summary>
		protected override void Initialize()
        {
        	Trace.WriteLine(string.Format(CultureInfo.CurrentCulture, "Entering Initialize() of: {0}", this.ToString()));

			_contentPreviewIntegrationManager.Initialize();

        	base.Initialize();
        }
    }
}
