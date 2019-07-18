using CMS.DataEngine;
using CMS.EventLog;
using CMS.MediaLibrary;
using CMS.SiteProvider;
using Dianoga.Config;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using CMS.DocumentEngine;

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
                            path.SetValue(optimizer, item.Path, null);
                        }
                        optimizers.Add((IImageOptimizer)optimizer);
                    }
                }
            }

            Optimizers = optimizers.ToArray();
        }

        public static void MediaFile_Insert_Before(object sender, ObjectEventArgs e)
        {
            MediaOptimizer optimizer = new MediaOptimizer();
            if (e.Object != null)
            {
                if (e.Object is MediaFileInfo)
                {
                    var image = (MediaFileInfo) e.Object;
                    if (image != null)
                    {
                        IOptimizerResult result = null;
                        result = optimizer.Process(image);

                        if (result != null && result.OptimizedBytes != null && result.OptimizedBytes.Length > 0)
                        {
                            image.FileBinary = result.OptimizedBytes;
                            image.FileSize = result.OptimizedBytes.Length;
                        }
                    }
                    return;
                }

                if (e.Object is AttachmentInfo)
                {
                    var image = (AttachmentInfo)e.Object;
                    if (image != null)
                    {
                        image.AttachmentTitle = image.AttachmentName.ToUpper();
                        IOptimizerResult result = null;
                        result = optimizer.Process(image);

                        if (result != null && result.OptimizedBytes != null && result.OptimizedBytes.Length > 0)
                        {
                            image.AttachmentBinary = result.OptimizedBytes;
                            image.AttachmentSize = result.OptimizedBytes.Length;
                        }
                    }
                    return;
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
            return LogResult(result, stream.FileName, stream.FileExtension, stream.FileImageWidth, stream.FileImageHeight, sw.ElapsedMilliseconds);
        }

        public virtual IOptimizerResult Process(AttachmentInfo stream)
        {
            var optimizer = CreateOptimizer(stream);

            if (optimizer == null) return null;

            var sw = new Stopwatch();
            sw.Start();
            var result = optimizer.Optimize(stream);
            sw.Stop();

            return LogResult(result, stream.AttachmentName, stream.AttachmentExtension, stream.AttachmentImageWidth, stream.AttachmentImageHeight, sw.ElapsedMilliseconds);
        }

        protected virtual IOptimizerResult LogResult(IOptimizerResult result, string name, string extension, int imageWidth, int imageHeight, long ms)
        {
            if (result.Success)
            {
                if (result.SizeAfter < result.SizeBefore)
                    EventLogProvider.LogInformation("Dianoga.Kentico", "IMAGE_OPTIMIZED", String.Format("Dianoga: optimized '{0}{1}' [{2}x{3}] (final size: {4} bytes) - saved {5} bytes / {6:p}. Optimized in {7}ms.",
                                                    name,
                                                    extension,
                                                    imageWidth,
                                                    imageHeight,
                                                    result.SizeAfter,
                                                    result.SizeBefore - result.SizeAfter,
                                                    1 - ((result.SizeAfter / (float)result.SizeBefore)),
                                                    ms));
                else
                    EventLogProvider.LogInformation("Dianoga.Kentico", "IMAGE_OPTIMIZED", String.Format("Dianoga: '{0}{1}' already optimized",
                                                        name,
                                                        extension));

                return result;
            }
            EventLogProvider.LogWarning("Dianoga.Kentico", "IMAGE_OPT_ERR", new Exception(String.Format("Dianoga: unable to optimize {0} because {1}", name, result.ErrorMessage)), SiteContext.CurrentSiteID, "Dianoga Error");
            return null;

        }

        protected virtual IImageOptimizer CreateOptimizer(MediaFileInfo stream)
        {
            return Optimizers.FirstOrDefault(optimizer => optimizer.CanOptimize(stream));
        }

        protected virtual IImageOptimizer CreateOptimizer(AttachmentInfo stream)
        {
            return Optimizers.FirstOrDefault(optimizer => optimizer.CanOptimize(stream));
        }

        public virtual bool CanOptimize(MediaFileInfo stream)
        {
            return CreateOptimizer(stream) != null;
        }
    }
}
