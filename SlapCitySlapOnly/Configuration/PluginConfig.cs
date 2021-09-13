using BepInEx.Configuration;
using SCMU.Online;
using Smash;
using System;

namespace SlapCitySlapOnly.Configuration
{
    class PluginConfig
    {
        static bool initialized = false;
        internal static bool settingsChanged = false;
        internal static event Action<bool> SettingChanged;

        public static bool IsEnabled
        {
            get => _isEnabled;
            private set
            {
                if (_isEnabled == value) return;
                _isEnabled = value;
                SettingChanged?.Invoke(_isEnabled);
            }
        }
        static bool _isEnabled;
        static ConfigEntry<bool> enabled;

        internal static void Init()
        {
            var config = Plugin.Instance.Config;
            config.SettingChanged += OnConfigSettingChanged;

            enabled = config.Bind("Settings", "Enabled", false);

            Save();
            Reload();
            initialized = true;
        }

        static void OnConfigSettingChanged(object sender, EventArgs args)
        {
            settingsChanged = true;
            if (OnlineSettingsManager.InLobby)
            {
                if (SmashSteam.Lobbies.Joined && !SmashSteam.Lobbies.OwnerIsMe)
                    return;
            } 
            Reload();
        }

        internal static void LoadOnlineSettings(LobbyModSettings lobbysettings)
        {
            // Check if all players have the mod installed
            if (!ModRegistry.CheckLobbyModInstalled(Plugin.Instance.Info.Metadata.Name))
            {
                Plugin.LogDebug("Not all players have the mod. Disabling...");
                IsEnabled = false;
                return;
            }

            IsEnabled = lobbysettings.TryGetSetting(nameof(IsEnabled), out var setting) && Convert.ToBoolean(setting.value);
        }

        internal static LobbyModSettings GetLobbyModSettings()
        {
            settingsChanged = false;
            Reload();

            var lobbySettings = new LobbyModSettings(
                Plugin.Instance.Info.Metadata.Name,
                new Setting(nameof(IsEnabled), IsEnabled)
            );

            return lobbySettings;
        }

        static void Save()
        {
            if (initialized) enabled.Value = IsEnabled;
            Plugin.Instance.Config.Save();
        }

        static void Reload()
        {
            Plugin.Instance.Config.Reload();
            IsEnabled = enabled.Value;
        }
    }
}
