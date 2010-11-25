using Microsoft.VisualStudio.Shell.Interop;

namespace XnaInspector.ContentPreview
{
	public interface IContentPreviewService
	{
		void ShowPreview(IVsHierarchy hierarchy, string fileName);
		void ShowPreview();
	}
}