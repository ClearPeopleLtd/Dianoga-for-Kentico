using CMS;
using CMS.DataEngine;
using CMS.DocumentEngine;
using CMS.EventLog;
using CMS.MediaLibrary;
using Dianoga;

[assembly: RegisterModule(typeof(DianogaKenticoModule))]
namespace Dianoga
{
    public class DianogaKenticoModule : Module
    {
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

            MediaFileInfo.TYPEINFO.Events.Insert.Before += MediaOptimizer.MediaFile_Insert_Before;
            MediaFileInfo.TYPEINFO.Events.Update.Before += MediaOptimizer.MediaFile_Insert_Before;
            AttachmentInfo.TYPEINFO.Events.Insert.Before += MediaOptimizer.MediaFile_Insert_Before;
            AttachmentInfo.TYPEINFO.Events.Update.Before += MediaOptimizer.MediaFile_Insert_Before;

            EventLogProvider.LogInformation("Dianoga.Kentico", "STARTAPP", "Dianoga module loaded");
        }
    }
}
