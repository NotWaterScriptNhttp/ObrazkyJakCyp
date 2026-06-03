using System;
using System.IO;
using BepInEx;
using BepInEx.Configuration;

namespace ObrazkyJakCyp
{
    public class PluginConfig
    {
        public string Directory { get; private set; } = "Paintings";
        public int MaxImages { get; private set; } = 500;

        public PluginConfig(ConfigFile cfg)
        {
            var dir = cfg.Bind<string>(
                new ConfigDefinition("General", "Directory"),
                Directory
            );
            var maxImgs = cfg.Bind<int>(
                new ConfigDefinition("General", "Max Images"),
                MaxImages
            );

            if (Path.IsPathRooted(dir.Value))
                Directory = dir.Value;
            else Directory = Path.GetFullPath(Path.Combine(Paths.BepInExRootPath, dir.Value));

            MaxImages = maxImgs.Value;
        }
    }
}
