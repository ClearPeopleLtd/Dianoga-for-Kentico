using System.Configuration;

namespace Dianoga.Config
{
    public class DianogaConfig
    {
        public static DianogaSection _Config = ConfigurationManager.GetSection("dianoga") as DianogaSection;
        public static OptimizersCollection GetOptimizers()
        {
            return _Config.Optimizers;
        }
    }

    public class DianogaSection : ConfigurationSection
    {
        [ConfigurationProperty("optimizers")]
        public OptimizersCollection Optimizers
        {
            get { return (OptimizersCollection)this["optimizers"]; }
        }
    }
}
