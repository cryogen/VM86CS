using System.Configuration;

namespace x86CS.Configuration
{
    [ConfigurationCollection(typeof(DiskElement), CollectionType = ConfigurationElementCollectionType.BasicMap, AddItemName = "Disk")]
    public class DiskElementCollection : ConfigurationElementCollection
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
            get { return "Disk"; }
        }

        public DiskElement this[int index]
        {
            get { return base.BaseGet(index) as DiskElement; }
            set
            {
                if (base.BaseGet(index) != null)
                {
                    base.BaseRemoveAt(index);
                }
                base.BaseAdd(index, value);
            }
        }

        public new DiskElement this[string name]
        {
            get { return base.BaseGet(name) as DiskElement; }
        }

        static DiskElementCollection()
        {
            properties = new ConfigurationPropertyCollection();
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new DiskElement();
        }

        protected override object GetElementKey(ConfigurationElement element)
        {
            return ((DiskElement)element).Id;
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        public void Add(DiskElement element)
        {
            BaseAdd(element);
        }

        public void Clear()
        {
            BaseClear();
        }

        public void Remove(DiskElement element)
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

    public class DiskElement : ConfigurationElement
    {
        private static ConfigurationPropertyCollection properties;
        private static ConfigurationProperty id;
        private static ConfigurationProperty image;
        private static ConfigurationProperty type;

        [ConfigurationProperty("Id", IsKey = true)]
        public int Id
        {
            get { return (int)base[id]; }
            set { base[id] = value; }
        }

        [ConfigurationProperty("Type")]
        public DriveType Type
        {
            get { return (DriveType)base[type]; }
            set { base[type] = value; }
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

        static DiskElement()
        {
            properties = new ConfigurationPropertyCollection();

            id = new ConfigurationProperty("Id", typeof(int), null, ConfigurationPropertyOptions.IsKey);
            image = new ConfigurationProperty("Image", typeof(string), null, ConfigurationPropertyOptions.None);
            type = new ConfigurationProperty("Type", typeof(DriveType), DriveType.None, ConfigurationPropertyOptions.None);

            properties.Add(id);
            properties.Add(image);
            properties.Add(type);
        }
    }
}
