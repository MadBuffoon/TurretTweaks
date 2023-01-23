using System.IO;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using HarmonyLib;
using ServerSync;
using UnityEngine;

namespace TurretTweaks
{
    [BepInPlugin(ModGUID, ModName, ModVersion)]
    public class TurretTweaksPlugin : BaseUnityPlugin
    {
        internal const string ModName = "TurretTweaks";
        internal const string ModVersion = "1.0.0";
        internal const string Author = "MadBuffoon";
        private const string ModGUID = Author + "." + ModName;
        private static string ConfigFileName = ModGUID + ".cfg";
        private static string ConfigFileFullPath = Paths.ConfigPath + Path.DirectorySeparatorChar + ConfigFileName;

        internal static string ConnectionError = "";

        private readonly Harmony _harmony = new(ModGUID);

        public static readonly ManualLogSource TurretTweaksLogger =
            BepInEx.Logging.Logger.CreateLogSource(ModName);

        private static readonly ConfigSync ConfigSync = new(ModGUID)
            { DisplayName = ModName, CurrentVersion = ModVersion, MinimumRequiredVersion = ModVersion };

        public enum Toggle
        {
            On = 1,
            Off = 0
        }

        public void Awake()
        {
            _serverConfigLocked = config("1 - General", "Lock Configuration", Toggle.On,
                "If on, the configuration is locked and can be changed by server admins only.");
            _ = ConfigSync.AddLockingConfigEntry(_serverConfigLocked);
            EnableConfig = config("1 - General", "Mod Enabled", true, "Enables the mod.");
            EnableFriendlyConfig = config("2 - Basic", "2.1 Friendly", true, "Doesn't attack friendlies only mobs and pvp.");
            MaxAmmoConfig = config("2 - Basic", "2.2 Max Ammo", 20, "The max ammo you load.");
            ViewDistanceConfig = config("2 - Basic", "2.3 View Distance", (float)30, "Attack Distance.");
            TurnRateConfig = config("2 - Basic", "2.4 Turn Rate", (float)45, "How fast the turret turns.");
            EnableNoAmmoCostConfigEntry = config("2 - Basic", "2.5 No Ammo Use", false, "Have to load in 1 ammo for this to work.\nThis adds 1 of the same ammo last use back in.");
            AngleHorizontalConfig = config("3 - Advance", "3.1 Angle Horizontal", (float)50, "Attack Angle.");
            AngleVerticalConfig = config("3 - Advance", "3.2 Angle Vertical", (float)50, "Attack Angle.");
            AttackCoolDownConfig = config("3 - Advance", "3.3 Attack Cool Down", 2f, "After shot how long before next one.");
            AttackWarmUpConfig = config("3 - Advance", "3.4 Attack Warm Up", 1f, "How long it take before it shots the first shot.");


            Assembly assembly = Assembly.GetExecutingAssembly();
            _harmony.PatchAll(assembly);
            SetupWatcher();
        }

        private void OnDestroy()
        {
            Config.Save();
        }
        
        private void SetupWatcher()
        {
            FileSystemWatcher watcher = new(Paths.ConfigPath, ConfigFileName);
            watcher.Changed += ReadConfigValues;
            watcher.Created += ReadConfigValues;
            watcher.Renamed += ReadConfigValues;
            watcher.IncludeSubdirectories = true;
            watcher.SynchronizingObject = ThreadingHelper.SynchronizingObject;
            watcher.EnableRaisingEvents = true;
        }

        private void ReadConfigValues(object sender, FileSystemEventArgs e)
        {
            if (!File.Exists(ConfigFileFullPath)) return;
            try
            {
                TurretTweaksLogger.LogDebug("ReadConfigValues called");
                Config.Reload();
            }
            catch
            {
                TurretTweaksLogger.LogError($"There was an issue loading your {ConfigFileName}");
                TurretTweaksLogger.LogError("Please check your config entries for spelling and format!");
            }
        }


        #region ConfigOptions

        private static ConfigEntry<Toggle> _serverConfigLocked = null!;
        internal static ConfigEntry<bool> EnableConfig = null!;
        internal static ConfigEntry<bool> EnableFriendlyConfig = null!;
        internal static ConfigEntry<bool> EnableNoAmmoCostConfigEntry = null!;
        internal static ConfigEntry<float> TurnRateConfig = null!;
        internal static ConfigEntry<float> AngleHorizontalConfig = null!;
        internal static ConfigEntry<float> AngleVerticalConfig = null!;
        internal static ConfigEntry<float> ViewDistanceConfig = null!;
        internal static ConfigEntry<float> AttackCoolDownConfig = null!;
        internal static ConfigEntry<float> AttackWarmUpConfig = null!;
        internal static ConfigEntry<int> MaxAmmoConfig = null!;

        private ConfigEntry<T> config<T>(string group, string name, T value, ConfigDescription description,
            bool synchronizedSetting = true)
        {
            ConfigDescription extendedDescription =
                new(
                    description.Description +
                    (synchronizedSetting ? " [Synced with Server]" : " [Not Synced with Server]"),
                    description.AcceptableValues, description.Tags);
            ConfigEntry<T> configEntry = Config.Bind(group, name, value, extendedDescription);
            //var configEntry = Config.Bind(group, name, value, description);

            SyncedConfigEntry<T> syncedConfigEntry = ConfigSync.AddConfigEntry(configEntry);
            syncedConfigEntry.SynchronizedConfig = synchronizedSetting;

            return configEntry;
        }

        private ConfigEntry<T> config<T>(string group, string name, T value, string description,
            bool synchronizedSetting = true)
        {
            return config(group, name, value, new ConfigDescription(description), synchronizedSetting);
        }

        private class ConfigurationManagerAttributes
        {
            public bool? Browsable = false;
        }

        class AcceptableShortcuts : AcceptableValueBase
        {
            public AcceptableShortcuts() : base(typeof(KeyboardShortcut))
            {
            }

            public override object Clamp(object value) => value;
            public override bool IsValid(object value) => true;

            public override string ToDescriptionString() =>
                "# Acceptable values: " + string.Join(", ", KeyboardShortcut.AllKeyCodes);
        }

        #endregion
    }
}