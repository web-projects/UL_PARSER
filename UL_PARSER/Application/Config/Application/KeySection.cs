using System;

namespace UL_PARSER.Config.Application
{
    [Serializable]
    public class ConfigurationSection
    {
        public ULCommandSettings vipaCommandSettings { get; internal set; } = new ULCommandSettings();
    }
}
