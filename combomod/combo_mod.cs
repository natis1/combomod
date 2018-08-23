using System;
using System.IO;
using System.Linq;
using System.Reflection;
using ModCommon;
using Modding;
using UnityEngine;
using Debug = System.Diagnostics.Debug;
using Logger = Modding.Logger;

namespace combomod
{
    // ReSharper disable once UnusedMember.Global Used implicitly
    // ReSharper disable once ClassNeverInstantiated.Global Used implicitly
    public class combo_mod : Mod, ITogglableMod
    {
        private readonly string settingsFilename;
        private settings globalSettings = Activator.CreateInstance<settings>();

        private void saveGlobalSettings()
        {
            log("Saving combomod settings!");
            if (File.Exists(settingsFilename + ".bak"))
                File.Delete(settingsFilename + ".bak");
            if (File.Exists(settingsFilename))
                File.Move(settingsFilename, settingsFilename + ".bak");
            using (FileStream fileStream = File.Create(settingsFilename))
            {
                using (StreamWriter streamWriter = new StreamWriter((Stream) fileStream))
                {
                    string json = JsonUtility.ToJson((object) globalSettings, true);
                    streamWriter.Write(json);
                }
            }
        }
        
        private void loadGlobalSettings()
        {
            log("Loading combomod settings!");
            if (!File.Exists(settingsFilename))
                return;
            using (FileStream fileStream = File.OpenRead(settingsFilename))
            {
                using (StreamReader streamReader = new StreamReader((Stream) fileStream))
                    globalSettings = JsonUtility.FromJson<settings>(streamReader.ReadToEnd());
            }
        }

        public combo_mod()
        {
            settingsFilename = Application.persistentDataPath + ModHooks.PathSeperator + globals.SETTINGS_FILE_APPEND;

            loadGlobalSettings();
            
            FieldInfo field = typeof(Mod).GetField
                ("Name", BindingFlags.Instance | BindingFlags.Public);
            field?.SetValue(this, "Glorious Combos");
        }


        public override void Initialize()
        {
            setupSettings();
            
            ModHooks.Instance.AfterSavegameLoadHook += saveGame;
            ModHooks.Instance.NewGameHook += addComponent;
            ModHooks.Instance.ApplicationQuitHook += saveGlobalSettings;
        }
        
        public void Unload()
        {
            log("Disabling! If you see any more non-settings messages by this mod please report as an issue.");
            ModHooks.Instance.AfterSavegameLoadHook -= saveGame;
            ModHooks.Instance.NewGameHook -= addComponent;
        }
        
        
        private void saveGame(SaveGameData data)
        {
            addComponent();
        }
        
        private void addComponent()
        {
            log("Adding combos to game!");
            globals.fileSettings = globalSettings;
            GameManager.instance.gameObject.GetOrAddComponent<combos>();
        }
        
        
        private void setupSettings()
        {
            string settingsFilePath = Application.persistentDataPath + ModHooks.PathSeperator + globals.SETTINGS_FILE_APPEND;
            bool forceReloadGlobalSettings = (globalSettings != null && globalSettings.settingsVersion != globals.SETTINGS_VER);
            if (forceReloadGlobalSettings || !File.Exists(settingsFilePath))
            {
                if (forceReloadGlobalSettings)
                {
                    log("Settings outdated! Rebuilding.");
                }
                else
                {
                    log("Settings not found, rebuilding... File will be saved to: " + settingsFilePath);
                }

                globalSettings?.reset();
            }

            Debug.Assert(globalSettings != null, nameof(globalSettings) + " != null");
            bool noCheating = (cheat_detect.verifyString(globalSettings.soulless1, 0) &&
                          cheat_detect.verifyString(globalSettings.soulless2, 1) &&
                          cheat_detect.verifyString(globalSettings.soulless3, 2) &&
                          cheat_detect.verifyString(globalSettings.soulless4, 3) &&
                          cheat_detect.verifyString(globalSettings.soulless5, 4));
            if (!noCheating)
            {
                log("Detected possible cheating in save file. Resetting save.");
                globalSettings?.reset();
            }
            
            saveGlobalSettings();
        }

        public override int LoadPriority()
        {
            return globals.LOAD_PRIORITY;
        }

        public override string GetVersion()
        {
            string ver = globals.MOD_VERSION_STRING;
            const int minApi = 45;
            
            bool apiTooLow = (Convert.ToInt32(ModHooks.Instance.ModVersion.Split('-')[1]) < minApi);
            if (apiTooLow)
                ver += " (Error: ModAPI possibly too old... Minimum version for Godmaster is 45.)";
            
            if (!hasAssembly("ModCommon"))
                ver += " (Error: Glorious Combos requires ModCommon)";

            return ver;
        }
        
        
        private static bool hasAssembly(string assemblyNamespaceName)
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblies)
            {
                try
                {
                    if (assembly.GetTypes().Any(type => type.Namespace == assemblyNamespaceName))
                    {
                        return true;
                    }
                }
                catch
                {
                    log("You have a broken assembly named '" + assembly.FullName + "' You should probably remove it.");
                }
            }

            return false;
        }

        public static void log(string text)
        {
            Logger.Log(text);
        }
    }
}