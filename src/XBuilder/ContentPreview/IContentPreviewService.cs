using Microsoft.VisualStudio.Shell.Interop;

namespace XBuilder.ContentPreview
{
	public interface IContentPreviewService
	{
		void ShowPreview(IVsHierarchy hierarchy, uint itemID, string fileName);
		void ShowPreview();
	}
}