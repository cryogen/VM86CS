using System.Configuration;

namespace x86CS.Configuration
{
    public static class SystemConfig
    {
        private static MachineSection machineSection;
        private static System.Configuration.Configuration config;

        public static MachineSection Machine
        {
            get { return machineSection; }
        }

        static SystemConfig()
        {
            config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);

            if (config.Sections["Machine"] == null)
            {
                config.Sections.Add("Machine", new MachineSection());
                machineSection.SectionInformation.ForceSave = true;
                config.Save();
            }
            machineSection = config.GetSection("Machine") as MachineSection;
        }

        public static void Save()
        {
            config.Save();
        }
    }
}
