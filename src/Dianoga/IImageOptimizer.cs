using CMS.DocumentEngine;
using CMS.MediaLibrary;

namespace Dianoga
{
	public interface IImageOptimizer
	{
		bool CanOptimize(MediaFileInfo stream);
		bool CanOptimize(AttachmentInfo stream);
		IOptimizerResult Optimize(MediaFileInfo stream);
		IOptimizerResult Optimize(AttachmentInfo stream);
	}
}
