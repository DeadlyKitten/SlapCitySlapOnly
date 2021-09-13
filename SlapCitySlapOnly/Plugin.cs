using BepInEx;
using BepInEx.Logging;
using HarmonyLib;
using SCMU.Events;
using SCMU.Online;
using SlapCitySlapOnly.Configuration;
using Smash;
using System.Collections;
using System.Reflection;
using UnityEngine;

namespace SlapCitySlapOnly
{
    [BepInPlugin("com.steven.slapcity.slaponly", "Slaps Only", "2.0.0")]
    public class Plugin : BaseUnityPlugin
    {
        internal static Plugin Instance;

        const string harmonyID = "com.steven.slapcity.slaponly";
        Harmony harmony;

        void Awake()
        {
            if (Instance)
            {
                DestroyImmediate(this);
                return;
            }
            Instance = this;

            PluginConfig.Init();

            // set up events
            Events.OnGameStarted += OnGameStart;
            Events.OnGameEnded += OnGameEnd;
            Events.OnJoinedOrCreatedLobby += OnLobbyJoinOrCreate;
            PluginConfig.SettingChanged += OnSettingsChanged;

            ModRegistry.Register(Info.Metadata.Name);

            if (PluginConfig.IsEnabled) Enable();
        }

        void OnGameStart()
        {
            if (routine != null) StopCoroutine(routine);
        }

        void OnGameEnd()
        {
            if (SmashSteam.Lobbies.Joined)
                routine = StartCoroutine(KeepLobbySettingsUpdated());
        }

        void OnLobbyJoinOrCreate()
        {
            UpdateLobbySettings();

            routine = StartCoroutine(KeepLobbySettingsUpdated());
        }

        void OnSettingsChanged(bool enabled)
        {
            if (enabled)
                Enable();
            else
                Disable();
        }

        void Enable()
        {
            if (Harmony.HasAnyPatches(harmonyID)) return;
            if (harmony == null) harmony = new Harmony(harmonyID);
            Logger.LogDebug("Patching...");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
        }

        void Disable()
        {
            if (harmony == null || !Harmony.HasAnyPatches(harmonyID)) return;
            Logger.LogDebug("Unpatching...");
            harmony.UnpatchSelf();
        }

        Coroutine routine;
        IEnumerator KeepLobbySettingsUpdated()
        {
            while (SmashSteam.Lobbies.Joined)
            {
                yield return new WaitForSeconds(0.5f);

                if (PluginConfig.settingsChanged)
                    UpdateLobbySettings();

                var lobbySettings = PluginConfig.GetLobbyModSettings();
                if (lobbySettings == null) yield break;
                PluginConfig.LoadOnlineSettings(lobbySettings);
            }
        }

        void UpdateLobbySettings()
        {
            LogDebug("Attempting to upload mod settings to lobby");

            var myLobbySettings = PluginConfig.GetLobbyModSettings();
            if (myLobbySettings == null) return;
            OnlineSettingsManager.ApplySettingsToLobby(myLobbySettings);
        }

        internal static void LogDebug(string message) => Instance.Log(message, LogLevel.Debug);
        internal static void LogInfo(string message) => Instance.Log(message, LogLevel.Info);
        private void Log(string message, LogLevel logLevel) => Logger.Log(logLevel, message);
    }
}
