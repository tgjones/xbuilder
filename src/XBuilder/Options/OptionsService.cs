using System;

namespace XBuilder.Options
{
	public class OptionsService : IOptionsService
	{
		private readonly XBuilderPackage _package;
		public event EventHandler OptionsChanged;

		public OptionsService(XBuilderPackage package)
		{
			_package = package;
		}

		public void InvokeOptionsChanged(EventArgs e)
		{
			EventHandler handler = OptionsChanged;
			if (handler != null)
				handler(this, e);
		}

		public XBuilderOptionsContentPreview GetContentPreviewOptions()
		{
			return _package.GetAutomationObject<XBuilderOptionsContentPreview>("XBuilder.ContentPreview");
		}

		public XBuilderOptionsGeneral GetGeneralOptions()
		{
			return _package.GetAutomationObject<XBuilderOptionsGeneral>("XBuilder.General");
		}
	}
}