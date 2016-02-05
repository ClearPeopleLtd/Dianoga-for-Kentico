using System.Configuration;

namespace Dianoga.Config
{
    [ConfigurationCollection(typeof(OptimizerElement))]
    public class OptimizersCollection : ConfigurationElementCollection
    {
        public OptimizerElement this[int index]
        {
            get { return (OptimizerElement)BaseGet(index); }
            set
            {
                if (BaseGet(index) != null)
                    BaseRemoveAt(index);

                BaseAdd(index, value);
            }
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new OptimizerElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((OptimizerElement)element).Type;
        }
    }
}
