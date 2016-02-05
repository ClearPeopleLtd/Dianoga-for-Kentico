using System;
using System.Linq;
using CMS.MediaLibrary;

namespace Dianoga
{
	/// <summary>
	/// An optimizer that determines if it can optimize based on file extension
	/// </summary>
	public abstract class ExtensionBasedImageOptimizer : IImageOptimizer
	{
		protected abstract string[] SupportedExtensions { get; }

		public virtual bool CanOptimize(MediaFileInfo stream)
		{
			return SupportedExtensions.Any(ext => ext.Equals(stream.FileExtension, StringComparison.OrdinalIgnoreCase));
		}

		public abstract IOptimizerResult Optimize(MediaFileInfo stream);
	}
}
