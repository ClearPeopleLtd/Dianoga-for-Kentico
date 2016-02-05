using CMS;
using CMS.DataEngine;
using CMS.MediaLibrary;
using Dianoga;

[assembly: RegisterModule(typeof(DianogaKenticoModule))]
namespace Dianoga
{
    public class DianogaKenticoModule : Module
    {
        private MediaOptimizer _optimizer;

        // Module class constructor, inherits from the base constructor with the code name of the module as the parameter
        public DianogaKenticoModule() : base("DianogaKentico")
        {
        }

        /// <summary>
        /// Initializes the module. Called when the application starts.
        /// </summary>
        protected override void OnInit()
        {
            base.OnInit();
            _optimizer = new MediaOptimizer();

            MediaFileInfo.TYPEINFO.Events.Insert.Before += MediaFile_Insert_Before;
            MediaFileInfo.TYPEINFO.Events.Update.Before += MediaFile_Insert_Before;
        }

        private void MediaFile_Insert_Before(object sender, ObjectEventArgs e)
        {
            if (e.Object != null)
            {
                var image = (MediaFileInfo)e.Object;
                IOptimizerResult result = _optimizer.Process(image);

                if (result != null)
                {
                    image.FileBinary = result.OptimizedBytes;
                    image.FileSize = result.OptimizedBytes.Length;
                }
            }
        }
    }
}
