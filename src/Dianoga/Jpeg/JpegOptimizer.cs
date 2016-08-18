using System.Diagnostics;
using System.IO;
using System.Web.Hosting;
using CMS.DocumentEngine;
using CMS.MediaLibrary;

namespace Dianoga.Jpeg
{
    // Squish JPEGs (strip exif, optimize coding) using jpegtran: http://jpegclub.org/jpegtran/
    public class JpegOptimizer : ExtensionBasedImageOptimizer
    {
        private string _pathToExe;

        public string Path
        {
            get { return _pathToExe; }
            set
            {
                if (value.StartsWith("~") || value.StartsWith("/")) _pathToExe = HostingEnvironment.MapPath(value);
                else _pathToExe = value;
            }
        }

        //jpegtran -optimize -progressive -copy none -outfile "<filename>" "<filename>"
        protected override string[] SupportedExtensions
        {
            get { return new[] { ".jpg", ".jpeg", ".jfif", ".jpe" }; }
        }

        public override IOptimizerResult Optimize(MediaFileInfo file)
        {
            return ProcessStream(file.FileBinary);
        }

        public override IOptimizerResult Optimize(AttachmentInfo file)
        {
            return ProcessStream(file.AttachmentBinary);
        }

        private IOptimizerResult ProcessStream(byte[] imageBytes)
        {
            if (imageBytes == null)
            {
                return new JpegOptimizerResult
                {
                    SizeBefore = 0,
                    SizeAfter = 0,
                    Success = true
                };
            }
            var stream = new MemoryStream(imageBytes);

            var tempFilePath = GetTempFilePath();

            using (var fileStream = File.OpenWrite(tempFilePath))
            {
                stream.CopyTo(fileStream);
            }

            var result = new JpegOptimizerResult
            {
                SizeBefore = (int)stream.Length
            };

            var jpegtran = Process.Start(Path, string.Format("-optimize -copy none -progressive -outfile \"{0}\" \"{0}\"", tempFilePath));
            if (jpegtran != null && jpegtran.WaitForExit(ToolTimeout))
            {
                if (jpegtran.ExitCode != 0)
                {
                    result.Success = false;
                    result.ErrorMessage = "jpegtran exited with unexpected exit code " + jpegtran.ExitCode;
                    return result;
                }

                result.Success = true;
                result.SizeAfter = (int)new FileInfo(tempFilePath).Length;

                if (result.SizeBefore > result.SizeAfter)
                {
                    // read the file to memory so we can nuke the temp file
                    using (var fileStream = File.OpenRead(tempFilePath))
                    {
                        result.OptimizedBytes = new byte[fileStream.Length];
                        fileStream.Read(result.OptimizedBytes, 0, result.OptimizedBytes.Length);
                    }
                }
                File.Delete(tempFilePath);

                return result;
            }

            result.Success = false;
            result.ErrorMessage = string.Format("jpegtran took longer than {0} to execute, which we consider a failure.", ToolTimeout);

            return result;
        }

        

        protected virtual string GetTempFilePath()
        {
            return System.IO.Path.GetTempFileName();
        }

        protected virtual int ToolTimeout { get { return 60000; } }
    }
}
