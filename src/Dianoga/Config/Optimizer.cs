using System.Configuration;
using System.Xml;

namespace Dianoga.Config
{
    public class OptimizerElement : ConfigurationElement
    {
        public OptimizerElement() { }

        [ConfigurationProperty("type", IsKey = true, IsRequired = true)]
        public string Type
        {
            get { return (string)this["type"]; }
            set { this["type"] = value; }
        }
        [ConfigurationProperty("path", IsRequired = true)]
        public string Path
        {
            get { return (string)this["path"]; }
            set { this["path"] = value; }
        }
    }
}
