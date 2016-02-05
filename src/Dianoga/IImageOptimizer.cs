using CMS.MediaLibrary;

namespace Dianoga
{
	public interface IImageOptimizer
	{
		bool CanOptimize(MediaFileInfo stream);
		IOptimizerResult Optimize(MediaFileInfo stream);
	}
}
