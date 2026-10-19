using System.Collections.Generic;
using System.Linq;
using BepInEx.Configuration;

namespace KongStrong
{
    internal static class ConfigBinder
    {
        /// <summary>
        /// Binds a config entry. If the stored value is still the default of an older version, it is
        /// replaced by the current default, so an update (e.g. through Bifrost Updater) brings new
        /// recipes and balance without editing the config. Values the player changed stay untouched.
        /// </summary>
        public static T Bind<T>(ConfigFile config, string section, string key, T value, ConfigDescription description,
                                IEnumerable<T> oldDefaults = null)
        {
            ConfigEntry<T> entry = config.Bind(section, key, value, description);
            if (oldDefaults != null && !Equals(entry.Value, value) && oldDefaults.Any(old => Equals(entry.Value, old)))
            {
                entry.Value = value;
            }

            return entry.Value;
        }

        public static T Bind<T>(ConfigFile config, string section, string key, T value, string description,
                                IEnumerable<T> oldDefaults = null)
        {
            return Bind(config, section, key, value, new ConfigDescription(description), oldDefaults);
        }
    }
}
