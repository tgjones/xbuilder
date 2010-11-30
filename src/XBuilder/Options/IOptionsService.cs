using System;

namespace XBuilder.Options
{
	public interface IOptionsService
	{
		event EventHandler OptionsChanged;

		void InvokeOptionsChanged(EventArgs e);

		XBuilderOptionsContentPreview GetContentPreviewOptions();
		XBuilderOptionsGeneral GetGeneralOptions();
	}
}