using System.Configuration;
namespace x86CS.Configuration
{
    public class MachineSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection properties;  
        private static ConfigurationProperty memorySize;
        private static ConfigurationProperty floppies;
        private static ConfigurationProperty disks;

        static MachineSection()
        {
            memorySize = new ConfigurationProperty("memorySize", typeof(int), null, ConfigurationPropertyOptions.None);
            floppies = new ConfigurationProperty("Floppies", typeof(FloppyElementCollection), null, ConfigurationPropertyOptions.None);
            disks = new ConfigurationProperty("Disks", typeof(DiskElementCollection), null, ConfigurationPropertyOptions.None);

            properties = new ConfigurationPropertyCollection();
            properties.Add(memorySize);
            properties.Add(floppies);
            properties.Add(disks);
        }

        [ConfigurationProperty("memorysize", DefaultValue = 16)]
        public int MemorySize
        {
            get { return (int)base[memorySize]; }
            set { base[memorySize] = value; }
        }

        [ConfigurationProperty("Floppies")]
        public FloppyElementCollection Floppies
        {
            get { return this["Floppies"] as FloppyElementCollection; }
        }

        [ConfigurationProperty("Disks")]
        public DiskElementCollection Disks
        {
            get { return this["Disks"] as DiskElementCollection; }
        }

        public override bool IsReadOnly()
        {
            return false;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return properties; }
        }
    }
}
