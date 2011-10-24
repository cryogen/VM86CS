
using System.Configuration;
namespace x86CS.Configuration
{
    [ConfigurationCollection(typeof(FloppyElement), CollectionType=ConfigurationElementCollectionType.BasicMap, AddItemName="Floppy")]
    public class FloppyElementCollection : ConfigurationElementCollection
    {
        private static ConfigurationPropertyCollection properties;

        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        public override ConfigurationElementCollectionType CollectionType
        {
            get { return ConfigurationElementCollectionType.BasicMap; }
        }

        protected override string ElementName
        {
            get { return "Floppy"; }
        }

        public FloppyElement this[int index]
        {
            get { return base.BaseGet(index) as FloppyElement; }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new FloppyElement this[string name]
        {
            get { return base.BaseGet(name) as FloppyElement; }
        }

        static FloppyElementCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }
        
        protected override ConfigurationElement CreateNewElement()
        {
            return new FloppyElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((FloppyElement)element).Id;
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        public void Add(FloppyElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(FloppyElement element)
        {
            BaseRemove(GetElementKey(element));
        }

        public void Remove(int id)
        {
            BaseRemove(id);
        }

        public void RemoveAt(int index)
        {
            base.BaseRemoveAt(index);
        }

        public int GetKey(int index)
        {
            return (int)base.BaseGetKey(index);
        }
    }

    public class FloppyElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection properties;
        private static ConfigurationProperty id;
        private static ConfigurationProperty image;

        [ConfigurationProperty("Id", IsKey=true)]
        public int Id
        {
            get { return (int)base[id]; }
            set { base[id] = value; }
        }

        [ConfigurationProperty("Image")]
        public string Image
        {
            get { return (string)base[image]; }
            set { base[image] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }

        static FloppyElement()
        {
            properties = new ConfigurationPropertyCollection();

            id = new ConfigurationProperty("Id", typeof(int), null, ConfigurationPropertyOptions.IsKey);
            image = new ConfigurationProperty("Image", typeof(string), null, ConfigurationPropertyOptions.None);

            properties.Add(id);
            properties.Add(image);
        }
    }
}
