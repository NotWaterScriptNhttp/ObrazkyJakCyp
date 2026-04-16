using System;
using BepInEx.Configuration;

namespace ObrazkyJakCyp
{
    public class PluginConfig
    {
        public ConfigEntry<string> Directory;

        public PluginConfig(ConfigFile cfg)
        {
            Directory = cfg.Bind<string>(
                new ConfigDefinition("General", "Directory"),
                "Paintings"
            );
        }
    }
}
