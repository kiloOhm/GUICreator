namespace Oxide.Plugins
{
    public partial class GUICreator
    {
        private static ConfigData config;

        private class ConfigData
        {
            public string steamAPIKey;
            public string gameTipIcon;
        }

        private ConfigData GetDefaultConfig()
        {
            return new ConfigData
            {
                steamAPIKey = "",
                gameTipIcon = "https://i.imgur.com/VFBB8ib.png"
            };
        }

        protected override void LoadConfig()
        {
            base.LoadConfig();

            try
            {
                config = Config.ReadObject<ConfigData>();
            }
            catch
            {
                LoadDefaultConfig();
            }

            SaveConfig();
        }

        protected override void LoadDefaultConfig()
        {
            PrintError("Configuration file is corrupt(or doesn't exist), creating new one!");
            config = GetDefaultConfig();
        }

        protected override void SaveConfig()
        {
            Config.WriteObject(config);
        }
    }
}