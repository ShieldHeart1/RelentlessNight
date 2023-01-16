using System.IO;
using HarmonyLib;
using Newtonsoft.Json;
using Il2CppSystem.Collections.Generic;

namespace RelentlessNight
{
    internal class SaveManager
    {
        internal const string rnSavePrefix = "relentlessnight";

        internal static bool isFirstMenuInitialize = true;

        internal class ModSaveData
        {
            // Mod settings
            public int worldSpinDeclinePercent;
            public bool endgameEnabled;
            public int endgameDay;
            public bool endgameAuroraEnabled;
            public int minAirTemperature;
            public int indoorOutdoorTemperaturePercent;
            public bool carcassMovingEnabled;
            public bool electricTorchLightingEnabled;
            public bool heatRetentionEnabled;
            public bool realisticFreezingEnabled;
            public int minWildlifePercent;
            public int minWildlifeDay;
            public int minFishPercent;
            public int minFishDay;
            public float fireFuelDurationMultiplier;
            public float lanternFuelDurationMultiplier;
            public float torchBurnDurationMultiplier;

            // Other mod data
            public int lastTemperatureOffsetDay;
            public int dayTidalLocked;
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnEnable", null)]
        internal class Panel_MainMenu_OnEnable
        {
            private static void Postfix()
            {
                if (isFirstMenuInitialize)
                {
                    Utilities.ModLog("Relentless Night V" + Global.RnVersion + " Loaded Successfully");
                    LoadRnSaveFiles();
                    isFirstMenuInitialize = false;
                }
                string lastRnSaveData = GetLastRnSaveData();
                if (lastRnSaveData != null)
                {
                    ModSaveData data = JsonConvert.DeserializeObject<ModSaveData>(lastRnSaveData);

                    Global.SetOptionGlobalsFromSave(data);
                    MaybeUpdateOptionGlobalsForNewModVersion(data);
                    Settings.SetModSettingsToOptionGlobals();
                }
            }
        }
        [HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData", null)]
        internal class SaveGameSystem_SaveGlobalData
        {
            private static void Postfix(SaveSlotType gameMode, string name)
            {
                if (!MenuManager.modEnabled) return;

                SaveGlobalsToSaveFile(gameMode, name);
            }
        }
        [HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData", null)]
        internal class SaveGameSystem_RestoreGlobalData
        {
            private static void Postfix()
            {
                if (!MenuManager.modEnabled || GameManager.m_ActiveScene == "MainMenu") return;

                string modData = SaveGameSlots.LoadDataFromSlot(SaveGameSystem.GetCurrentSaveName(), "RelentlessNight");
                if (modData != null)
                {
                    ModSaveData data = JsonConvert.DeserializeObject<ModSaveData>(modData);

                    Global.SetOptionGlobalsFromSave(data);
                    Global.SetGameGlobalsFromSave(data);
                    MaybeUpdateOptionGlobalsForNewModVersion(data);
                    Settings.SetModSettingsToOptionGlobals();
                }
                else
                {
                    Global.SetOptionGlobalsToRnClassic();
                    Global.SetGameGlobalsForNewGame();
                }
            }
        }
        [HarmonyPatch(typeof(GameManager), "LaunchSandbox", null)]
        internal static class GameManager_LaunchSandbox
        {
            private static void Postfix()
            {
                if (!MenuManager.modEnabled) return;

                Global.SetOptionGlobalsFromModOptions();
            }
        }
        [HarmonyPatch(typeof(SaveGameSlots), "GetSlotPrefix", null)]
        internal class SaveGameSlots_GetSlotPrefix
        {

            private static bool Prefix(ref string __result)
            {
                if (!MenuManager.modEnabled) return true;

                __result = rnSavePrefix;
                return false;
            }
        }
        [HarmonyPatch(typeof(SaveGameSlots), "GetSaveSlotTypeFromName", null)]
        internal class SaveGameSlots_GetSaveSlotTypeFromName
        {

            private static bool Prefix(ref SaveSlotType __result)
            {
                if (!MenuManager.modEnabled) return true;

                __result = SaveSlotType.SANDBOX;
                return false;
            }
        }
        [HarmonyPatch(typeof(SaveGameSlotHelper), "GetNumSaveSlots", null)]
        internal class SaveGameSlotHelper_GetNumSaveSlots
        {
            private static bool Prefix(SaveSlotType saveSlotType, ref int __result)
            {
                if (saveSlotType != SaveSlotType.SANDBOX) return true;

                __result = GetNumSortedSlots();
                return false;
            }
        }

        internal static int GetNumSortedSlots()
        {
            SaveGameSlotHelper.RefreshSandboxSaveSlots();
            List<SaveSlotInfo> sandboxAndRnSaveSlots = SaveGameSlotHelper.GetSaveSlotInfoList(SaveSlotType.SANDBOX);
            List<SaveSlotInfo> rnSaveSlots = new List<SaveSlotInfo>();
            List<SaveSlotInfo> sandboxSaveSlots = new List<SaveSlotInfo>();

            for (int i = 0; i < sandboxAndRnSaveSlots.Count; i++)
            {
                if (sandboxAndRnSaveSlots[i].m_SaveSlotName.Contains(rnSavePrefix))
                {
                    rnSaveSlots.Add(sandboxAndRnSaveSlots[i]);
                }
                else
                {
                    sandboxSaveSlots.Add(sandboxAndRnSaveSlots[i]);
                }
            }
            if (MenuManager.modEnabled)
            {
                SaveGameSlotHelper.m_SandboxSlots = rnSaveSlots;
                return rnSaveSlots.Count;
            }
            else
            {
                SaveGameSlotHelper.m_SandboxSlots = sandboxSaveSlots;
                return sandboxSaveSlots.Count;
            }
        }
        internal static string GetLastRnSaveData()
        {
            SaveGameSlotHelper.RefreshSandboxSaveSlots();
            List<SaveSlotInfo> sandboxSaveSlots = SaveGameSlotHelper.GetSaveSlotInfoList(SaveSlotType.SANDBOX);

            foreach (SaveSlotInfo saveSlot in sandboxSaveSlots)
            {
                if (saveSlot.m_SaveSlotName.StartsWith(rnSavePrefix))
                {
                    return SaveGameSlots.LoadDataFromSlot(saveSlot.m_SaveSlotName, "RelentlessNight");
                }
            }
            return null;
        }
        internal static void SaveGlobalsToSaveFile(SaveSlotType gameMode, string name)
        {
            ModSaveData data = new ModSaveData
            {
                worldSpinDeclinePercent = Global.worldSpinDeclinePercent,
                endgameEnabled = Global.endgameEnabled,
                endgameDay = Global.endgameDay,
                endgameAuroraEnabled = Global.endgameAuroraEnabled,
                minAirTemperature = Global.minAirTemperature,
                indoorOutdoorTemperaturePercent = Global.indoorOutdoorTemperaturePercent,
                carcassMovingEnabled = Global.carcassMovingEnabled,
                electricTorchLightingEnabled = Global.electricTorchLightingEnabled,
                heatRetentionEnabled = Global.heatRetentionEnabled,
                realisticFreezingEnabled = Global.realisticFreezingEnabled,
                minWildlifePercent = Global.minWildlifePercent,
                minWildlifeDay = Global.minWildlifeDay,
                minFishPercent = Global.minFishPercent,
                minFishDay = Global.minFishDay,
                fireFuelDurationMultiplier = Global.fireFuelDurationMultiplier,
                lanternFuelDurationMultiplier = Global.lanternFuelDurationMultiplier,
                torchBurnDurationMultiplier = Global.torchBurnDurationMultiplier,

                lastTemperatureOffsetDay = Global.lastTemperatureOffsetDay,
                dayTidalLocked = Global.dayTidalLocked,
            };
            SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, "RelentlessNight", JsonConvert.SerializeObject(data));
        }
        internal static void LoadRnSaveFiles()
        {
            foreach (FileInfo saveFile in GetAllGameSaveFiles())
            {
                if (IsCompatibleRnSave(saveFile.Name))
                {
                    SaveGameDataPC.LoadData(saveFile.Name);
                }
            }
        }
        internal static FileInfo[] GetAllGameSaveFiles()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(PersistentDataPath.m_Path);
            return directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        }
        internal static bool IsCompatibleRnSave(string fileName)
        {
            return fileName.StartsWith(rnSavePrefix);
        }
        internal static bool IsIncompatibleRnSave(string fileName)
        {
            // Pre-4.40 save file name prefix
            return fileName.StartsWith("ep1relentless");
        }
        internal static void MaybeUpdateOptionGlobalsForNewModVersion(ModSaveData data)
        {
            if (data.endgameDay == 0) Global.endgameDay = 1;
            if (data.minWildlifeDay == 0) Global.minWildlifeDay = 1;
            if (data.minFishDay == 0) 
            {
                Global.minFishPercent = 20;
                Global.minFishDay = 120;
            }
        }
    }
}