using System.Collections.Generic;
using System.IO;
using System.Linq;
using BepInEx.Configuration;
using BepInEx.Logging;

namespace KongStrong
{
    /// <summary>
    /// Binds config entries and moves values that are still the default of an older version to the new default,
    /// so an update brings new recipes and balance without editing the config. This runs once per config version
    /// ([General] ConfigVersion): a value the player sets later, even to an old default, is kept.
    /// </summary>
    internal static class ConfigBinder
    {
        /// <summary>Raise this whenever a default that has old values listed changes.</summary>
        public const int CurrentVersion = 1;
        private const string VersionKey = "ConfigVersion";

        private static bool _migrate;
        private static int _changed;

        /// <summary>Call before the first Bind. A new file needs no migration; a file without the key is from 1.2.2 or older.</summary>
        public static void Begin(ConfigFile config)
        {
            _changed = 0;
            _migrate = File.Exists(config.ConfigFilePath) && StoredVersion(config.ConfigFilePath) < CurrentVersion;
        }

        /// <summary>Call after the last Bind.</summary>
        public static void Finish(ConfigFile config, ManualLogSource log)
        {
            ConfigEntry<int> version = config.Bind("General", VersionKey, CurrentVersion,
                "Internal. When it is lower than the mod's version, old default values in this file are replaced by the new defaults.");
            if (_changed > 0)
            {
                log.LogInfo($"Config updated: {_changed} old default value(s) replaced by the new ones. Your own changes were kept.");
            }

            version.Value = CurrentVersion;
            config.Save();
            _migrate = false;
        }

        public static T Bind<T>(ConfigFile config, string section, string key, T value, ConfigDescription description,
                                IEnumerable<T> oldDefaults = null)
        {
            ConfigEntry<T> entry = config.Bind(section, key, value, description);
            if (_migrate && oldDefaults != null && !Equals(entry.Value, value) && oldDefaults.Any(old => Equals(entry.Value, old)))
            {
                entry.Value = value;
                _changed++;
            }

            return entry.Value;
        }

        public static T Bind<T>(ConfigFile config, string section, string key, T value, string description,
                                IEnumerable<T> oldDefaults = null)
        {
            return Bind(config, section, key, value, new ConfigDescription(description), oldDefaults);
        }

        private static int StoredVersion(string path)
        {
            foreach (string line in File.ReadAllLines(path))
            {
                string[] parts = line.Split(new[] { '=' }, 2);
                if (parts.Length == 2 && parts[0].Trim() == VersionKey && int.TryParse(parts[1].Trim(), out int version))
                {
                    return version;
                }
            }

            return 0;
        }
    }
}
