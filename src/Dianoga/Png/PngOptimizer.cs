using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Web.Hosting;
using CMS.MediaLibrary;

namespace Dianoga.Png
{
	// uses PngOptimizer to crunch PNGs - this is the fastest optimizer by far, and quite effective: http://psydk.org/pngoptimizer
	public class PngOptimizer : ExtensionBasedImageOptimizer
	{
		private readonly object _loaderLock = new object();

		private string _pathToDll;

		public string Path
		{
			get { return _pathToDll; }
			set
			{
				if (value.StartsWith("~") || value.StartsWith("/")) _pathToDll = HostingEnvironment.MapPath(value);
				else _pathToDll = value;
			}
		}

		protected override string[] SupportedExtensions
		{
			get { return new[] { ".png" }; }
		}

		public override IOptimizerResult Optimize(MediaFileInfo file)
		{
            if (file.FileBinary == null)
            {
                return new PngOptimizerResult
                {
                    SizeBefore = 0,
                    SizeAfter = 0,
                    Success = true
                };
            }
            IntPtr pngOptimizer = GetOrLoadPngOptimizer();

            byte[] imageBytes = file.FileBinary;
            byte[] resultBytes = new byte[imageBytes.Length + 400000];
            int resultSize;

            IntPtr addressOfOptimize = NativeMethods.GetProcAddress(pngOptimizer, "PO_OptimizeFileMem");

            if (addressOfOptimize == IntPtr.Zero) throw new Exception("Can't find optimize funtion in PngOptimizerDll.dll!");

            OptimizeBytes optimizeMethod = (OptimizeBytes)Marshal.GetDelegateForFunctionPointer(addressOfOptimize, typeof(OptimizeBytes));
            bool success = optimizeMethod(imageBytes, imageBytes.Length, resultBytes, resultBytes.Length, out resultSize);

		    var result = new PngOptimizerResult
		    {
		        Success = success,
		        SizeBefore = imageBytes.Length,
		        SizeAfter = resultSize,
		    };

		    if (result.SizeBefore > result.SizeAfter)
		        result.OptimizedBytes = resultBytes.Take(resultSize).ToArray();

		    if (result.Success) return result;

		    IntPtr addressOfGetError = NativeMethods.GetProcAddress(pngOptimizer, "PO_GetLastErrorString");
		    if (addressOfGetError == IntPtr.Zero) throw new Exception("Can't find get last error funtion in PngOptimizerDll.dll!");

		    GetLastErrorString errorMethod = (GetLastErrorString)Marshal.GetDelegateForFunctionPointer(addressOfGetError, typeof(GetLastErrorString));
		    result.ErrorMessage = errorMethod();

		    return result;
        }

		private IntPtr _pngo;
		protected virtual IntPtr GetOrLoadPngOptimizer()
		{
			if (_pngo != IntPtr.Zero) return _pngo;

			lock (_loaderLock)
			{
				if (_pngo != IntPtr.Zero) return _pngo;

				if (!File.Exists(Path)) throw new FileNotFoundException("Unable to load PngOptimizerDll.dll from " + Path);

				var tempPath = System.IO.Path.GetTempFileName();

                File.Copy(Path, tempPath, true);

				_pngo = NativeMethods.LoadLibrary(tempPath);

				if (_pngo == IntPtr.Zero) throw new Exception("Unable to load PNGOptimizer from " + Path);

				return _pngo;
			}
		}

		static class NativeMethods
		{
			[DllImport("kernel32.dll")]
			public static extern IntPtr LoadLibrary(string dllToLoad);

			[DllImport("kernel32.dll")]
			public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);
		}

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate bool OptimizeBytes(byte[] image, int imageSize, [Out] byte[] result, int resultCapacity, out int resultSize);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		[return: MarshalAs(UnmanagedType.LPWStr)]
		private delegate string GetLastErrorString();
	}
}
