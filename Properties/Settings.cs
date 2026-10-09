using System.Configuration;
using System.ComponentModel;

namespace SmartMed.Properties
{
    [SettingsProvider(typeof(LocalFileSettingsProvider))]
    internal sealed class Settings : ApplicationSettingsBase
    {
        private static Settings _defaultInstance = (Settings)Synchronized(new Settings());

        public static Settings Default => _defaultInstance;

        [UserScopedSetting]
        [DefaultSettingValue("")]
        public string SavedUsername
        {
            get => (string)this["SavedUsername"];
            set => this["SavedUsername"] = value;
        }
    }
}
