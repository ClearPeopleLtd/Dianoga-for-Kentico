using CMS.DataEngine;
using CMS.EventLog;
using CMS.MediaLibrary;
using CMS.SiteProvider;
using Dianoga.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Xml;

namespace Dianoga
{
	public class MediaOptimizer
	{
		protected static readonly IImageOptimizer[] Optimizers;

		static MediaOptimizer()
		{
            var config = DianogaConfig.GetOptimizers().OfType<OptimizerElement>();

            List<IImageOptimizer> optimizers = new List<IImageOptimizer>();
            foreach (var item in config)
            {
                var assemblyData = item.Type.Split(',');
                var instance = Activator.CreateInstance(assemblyData[1].Trim(), assemblyData[0].Trim());
                if (instance != null)
                {
                    var optimizer = instance.Unwrap();
                    if (optimizer != null)
                    {
                        Type instanceType = optimizer.GetType();
                        var path = instanceType.GetProperty("Path");
                        if (path != null)
                        {
                            path.SetValue(optimizer, item.Path);
                        }
                        optimizers.Add((IImageOptimizer)optimizer);
                    }
                }
            }

            Optimizers = optimizers.ToArray();
		}

        public void MediaFile_Insert_Before(object sender, ObjectEventArgs e)
        {
            if (e.Object != null)
            {
                var image = (MediaFileInfo)e.Object;
                image.FileTitle = image.FileName.ToUpper();
                IOptimizerResult result = null;
                result = Process(image);

                if (result != null)
                {
                    image.FileBinary = result.OptimizedBytes;
                    image.FileSize = result.OptimizedBytes.Length;
                }
            }
        }


        /// <summary>
        /// Optimizes a media stream and returns the optimized result. The original stream is closed if processing is successful.
        /// </summary>
        public virtual IOptimizerResult Process(MediaFileInfo stream)
		{
			var optimizer = CreateOptimizer(stream);

			if (optimizer == null) return null;

			var sw = new Stopwatch();
			sw.Start();
			var result = optimizer.Optimize(stream);
			sw.Stop();

			if (result.Success)
			{
                //Log.Info("Dianoga: optimized {0}.{1} [{2}] (final size: {3} bytes) - saved {4} bytes / {5:p}. Optimized in {6}ms.".FormatWith(stream.MediaItem.MediaPath, stream.MediaItem.Extension, GetDimensions(options), result.SizeAfter, result.SizeBefore - result.SizeAfter, 1 - ((result.SizeAfter / (float)result.SizeBefore)), sw.ElapsedMilliseconds), this);
                EventLogProvider.LogInformation("Dianoga.Kentico", "IMAGE_OPTIMIZER", String.Format("Dianoga: optimized {0}.{1} [{2}x{3}] (final size: {4} bytes) - saved {5} bytes / {6:p}. Optimized in {7}ms.", 
                                                    stream.FileName, 
                                                    stream.FileExtension, 
                                                    stream.FileImageWidth,
                                                    stream.FileImageHeight,
                                                    result.SizeAfter, 
                                                    result.SizeBefore - result.SizeAfter,
                                                    1 - ((result.SizeAfter / (float)result.SizeBefore)), 
                                                    sw.ElapsedMilliseconds));

                return result;
			}
            EventLogProvider.LogWarning("Dianoga.Kentico", "IMAGE_OPTIMIZER", new Exception(String.Format("Dianoga: unable to optimize {0} because {1}", stream.FileName, result.ErrorMessage)), SiteContext.CurrentSiteID, "Dianoga Error");
			//Log.Error("Dianoga: unable to optimize {0} because {1}".FormatWith(stream.MediaItem.Name, result.ErrorMessage), this);

			return null;
		}

		public virtual bool CanOptimize(MediaFileInfo stream)
		{
			return CreateOptimizer(stream) != null;
		}

		protected virtual IImageOptimizer CreateOptimizer(MediaFileInfo stream)
		{
			return Optimizers.FirstOrDefault(optimizer => optimizer.CanOptimize(stream));
		}
	}
}
