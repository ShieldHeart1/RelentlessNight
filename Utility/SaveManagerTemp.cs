using System;
using System.IO;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using UnityEngine;
using Newtonsoft.Json;
using Il2CppSystem.Collections.Generic;

namespace RelentlessNight
{
    internal class SaveManager
    {
        // All Relentless Night data being written into save file
        internal class ModSaveData
        {
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
            public int minWildlifeDay;
            public int minWildlifePercent;
            public float fireFuelDurationMultiplier;
            public float lanternFuelDurationMultiplier;
            public float torchBurnDurationMultiplier;
            public int currentDay;
            public int dayOfTidalLocking;
            public bool playerIsCarryingCarcass;
            public string activeCarcass;
        }

        internal static bool gameLoaded = false;
        internal static List<string> listLoadedRnSaves = new List<string>();
        internal static List<SaveSlotInfo> sortedRnSaveSlots = new List<SaveSlotInfo>();
        internal static List<SaveSlotInfo> sortedRnSaveSlotsTemp = new List<SaveSlotInfo>();

        [HarmonyPatch(typeof(Panel_MainMenu), "Initialize", null)]
        internal class Panel_MainMenu_Initialize
        {
            private static void Postfix(Panel_MainMenu __instance)
            {
                if (!gameLoaded)
                {
                    Utilities.ModLog("Relentless Night V" + RnGlobal.RnVersion + " Loaded Successfully");
                    LoadRnSaveFiles();
                }

                sortedRnSaveSlots = SaveGameSlotHelper.GetSaveSlotInfoList(SaveSlotType.SANDBOX);

                foreach (SaveSlotInfo saveSlot in sortedRnSaveSlots)
                {
                    if (saveSlot.m_SaveSlotName.ToLower().Contains())
                }

                //if (sortedRnSaveSlots.Count == 0) return;

                //RemoveDuplicateSaveSlots(__instance);

                string modData = SaveGameSlots.LoadDataFromSlot(sortedRnSaveSlots[0].m_SaveSlotName, "RelentlessNight");

                if (modData != null)
                {
                    ModSaveData data = JsonConvert.DeserializeObject<ModSaveData>(modData);

                    RnGlobal.worldSpinDeclinePercent = data.worldSpinDeclinePercent;
                    RnGlobal.endgameEnabled = data.endgameEnabled;
                    RnGlobal.endgameDay = data.endgameDay;
                    RnGlobal.endgameAuroraEnabled = data.endgameAuroraEnabled;                    
                    RnGlobal.minAirTemperature = data.minAirTemperature;
                    RnGlobal.indoorOutdoorTemperaturePercent = data.indoorOutdoorTemperaturePercent;
                    RnGlobal.carcassMovingEnabled = data.carcassMovingEnabled;
                    RnGlobal.electricTorchLightingEnabled = data.electricTorchLightingEnabled;
                    RnGlobal.heatRetentionEnabled = data.heatRetentionEnabled;
                    RnGlobal.realisticFreezingEnabled = data.realisticFreezingEnabled;
                    RnGlobal.minWildlifeDay = data.minWildlifeDay;
                    RnGlobal.minWildlifePercent = data.minWildlifePercent;
                    RnGlobal.fireFuelDurationMultiplier = data.fireFuelDurationMultiplier;
                    RnGlobal.lanternFuelDurationMultiplier = data.lanternFuelDurationMultiplier;
                    RnGlobal.torchBurnDurationMultiplier = data.torchBurnDurationMultiplier;

                    RnGlobal.currentDay = data.currentDay;

                    #if DEBUG
                    Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " " + sortedRnSaveSlots[0].m_SaveSlotName + " Save Data Loaded.");
                    #endif
                }

                Settings.options.worldSpinDeclinePercent = RnGlobal.worldSpinDeclinePercent;
                Settings.options.endgameEnabled = RnGlobal.endgameEnabled;
                Settings.options.endgameDay = RnGlobal.endgameDay;
                Settings.options.endgameAuroraEnabled = RnGlobal.endgameAuroraEnabled;                
                Settings.options.minAirTemperature = RnGlobal.minAirTemperature;
                Settings.options.indoorOutdoorTemperaturePercent = RnGlobal.indoorOutdoorTemperaturePercent;
                Settings.options.carcassMovingEnabled = RnGlobal.carcassMovingEnabled;
                Settings.options.electricTorchLightingEnabled = RnGlobal.electricTorchLightingEnabled;
                Settings.options.heatRetentionEnabled = RnGlobal.heatRetentionEnabled;
                Settings.options.realisticFreezingEnabled = RnGlobal.realisticFreezingEnabled;
                Settings.options.minWildlifePercent = RnGlobal.minWildlifePercent;
                Settings.options.minWildlifeDay = RnGlobal.minWildlifeDay;
                Settings.options.fireFuelDurationMultiplier = RnGlobal.fireFuelDurationMultiplier;
                Settings.options.lanternFuelDurationMultiplier = RnGlobal.lanternFuelDurationMultiplier;
                Settings.options.torchBurnDurationMultiplier = RnGlobal.torchBurnDurationMultiplier;

                //rnSavesLoaded = true;

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Globals Applied To RN Settings.");
                #endif
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData", null)]
        internal class SaveGameSystem_SaveGlobalData
        {
            private static void Postfix(SaveSlotType gameMode, string name)
            {
                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Entered.");
                #endif

                if (!RnGlobal.rnEnabled) return;

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Conditions Passed.");
                #endif

                ModSaveData data = new ModSaveData
                {
                    worldSpinDeclinePercent = RnGlobal.worldSpinDeclinePercent,
                    endgameEnabled = RnGlobal.endgameEnabled,
                    endgameDay = RnGlobal.endgameDay,
                    endgameAuroraEnabled = RnGlobal.endgameAuroraEnabled,                    
                    minAirTemperature = RnGlobal.minAirTemperature,
                    indoorOutdoorTemperaturePercent = RnGlobal.indoorOutdoorTemperaturePercent,
                    carcassMovingEnabled = RnGlobal.carcassMovingEnabled,
                    electricTorchLightingEnabled = RnGlobal.electricTorchLightingEnabled,
                    heatRetentionEnabled = RnGlobal.heatRetentionEnabled,
                    realisticFreezingEnabled = RnGlobal.realisticFreezingEnabled,
                    minWildlifeDay = RnGlobal.minWildlifeDay,
                    minWildlifePercent = RnGlobal.minWildlifePercent,
                    fireFuelDurationMultiplier = RnGlobal.fireFuelDurationMultiplier,
                    lanternFuelDurationMultiplier = RnGlobal.lanternFuelDurationMultiplier,
                    torchBurnDurationMultiplier = RnGlobal.torchBurnDurationMultiplier,
                    currentDay = RnGlobal.currentDay,
                    dayOfTidalLocking = RnGlobal.dayOfTidalLocking,
                    playerIsCarryingCarcass = RnGlobal.playerIsCarryingCarcass,
                    activeCarcass = RnGlobal.activeCarcass,
                };

                string saveProxyData = JsonConvert.SerializeObject(data);
                SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, "RelentlessNight", saveProxyData);

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " RN Data Saved.");
                #endif
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), "RestoreGlobalData", null)]
        internal class GameManager_RestoreGlobalData
        {
            private static void Postfix()
            {
                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Entered.");
                #endif

                if (!RnGlobal.rnEnabled || GameManager.m_ActiveScene == "MainMenu") return;

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Conditions Passed.");
                #endif

                string modData = SaveGameSlots.LoadDataFromSlot(SaveGameSystem.GetCurrentSaveName(), "RelentlessNight");

                if (modData == null)
                {
                    RnGlobal.worldSpinDeclinePercent = 15;
                    RnGlobal.endgameEnabled = true;
                    RnGlobal.endgameDay = 100;
                    RnGlobal.endgameAuroraEnabled = false;                    
                    RnGlobal.minAirTemperature = -80;
                    RnGlobal.indoorOutdoorTemperaturePercent = 60;
                    RnGlobal.carcassMovingEnabled = true;
                    RnGlobal.electricTorchLightingEnabled = true;
                    RnGlobal.heatRetentionEnabled = true;
                    RnGlobal.realisticFreezingEnabled = false;
                    RnGlobal.minWildlifeDay = 100;
                    RnGlobal.minWildlifePercent = 10;
                    RnGlobal.fireFuelDurationMultiplier = 1.5f;
                    RnGlobal.lanternFuelDurationMultiplier = 1.5f;
                    RnGlobal.torchBurnDurationMultiplier = 1.5f;

                    RnGlobal.currentDay = 0;
                    RnGlobal.playerIsCarryingCarcass = false;
                    RnGlobal.activeCarcass = null;

                    #if DEBUG
                    Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " No Applied RN Settings Found, Loading Defaults.");
                    #endif
                }
                else
                {
                    ModSaveData data = JsonConvert.DeserializeObject<ModSaveData>(modData);

                    RnGlobal.worldSpinDeclinePercent = data.worldSpinDeclinePercent;
                    RnGlobal.endgameEnabled = data.endgameEnabled;
                    RnGlobal.endgameDay = data.endgameDay;
                    RnGlobal.endgameAuroraEnabled = data.endgameAuroraEnabled;                    
                    RnGlobal.minAirTemperature = data.minAirTemperature;
                    RnGlobal.indoorOutdoorTemperaturePercent = data.indoorOutdoorTemperaturePercent;
                    RnGlobal.carcassMovingEnabled = data.carcassMovingEnabled;
                    RnGlobal.electricTorchLightingEnabled = data.electricTorchLightingEnabled;
                    RnGlobal.heatRetentionEnabled = data.heatRetentionEnabled;
                    RnGlobal.realisticFreezingEnabled = data.realisticFreezingEnabled;
                    RnGlobal.minWildlifeDay = data.minWildlifeDay;
                    RnGlobal.minWildlifePercent = data.minWildlifePercent;
                    RnGlobal.fireFuelDurationMultiplier = data.fireFuelDurationMultiplier;
                    RnGlobal.lanternFuelDurationMultiplier = data.lanternFuelDurationMultiplier;
                    RnGlobal.torchBurnDurationMultiplier = data.torchBurnDurationMultiplier;

                    RnGlobal.currentDay = data.currentDay;
                    RnGlobal.dayOfTidalLocking = data.dayOfTidalLocking;
                    RnGlobal.playerIsCarryingCarcass = data.playerIsCarryingCarcass;
                    RnGlobal.activeCarcass = data.activeCarcass;

                    Settings.options.worldSpinDeclinePercent = data.worldSpinDeclinePercent;
                    Settings.options.endgameEnabled = data.endgameEnabled;
                    Settings.options.endgameDay = data.endgameDay;
                    Settings.options.endgameAuroraEnabled = data.endgameAuroraEnabled;                    
                    Settings.options.minAirTemperature = data.minAirTemperature;
                    Settings.options.indoorOutdoorTemperaturePercent = data.indoorOutdoorTemperaturePercent;
                    Settings.options.carcassMovingEnabled = data.carcassMovingEnabled;
                    Settings.options.electricTorchLightingEnabled = data.electricTorchLightingEnabled;
                    Settings.options.heatRetentionEnabled = data.heatRetentionEnabled;
                    Settings.options.realisticFreezingEnabled = data.realisticFreezingEnabled;
                    Settings.options.minWildlifeDay = data.minWildlifeDay;
                    Settings.options.minWildlifePercent = data.minWildlifePercent;
                    Settings.options.fireFuelDurationMultiplier = data.fireFuelDurationMultiplier;
                    Settings.options.lanternFuelDurationMultiplier = data.lanternFuelDurationMultiplier;
                    Settings.options.torchBurnDurationMultiplier = data.torchBurnDurationMultiplier;

                    #if DEBUG
                    Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " RN Settings Applied From Save.");
                    #endif
                }
            }
        }

        [HarmonyPatch(typeof(GameManager), "LaunchSandbox", null)]
        internal static class GameManager_LaunchSandbox
        {
            private static void Postfix()
            {
                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Entered.");
                #endif

                if (!RnGlobal.rnEnabled) return;

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Conditions Passed.");
                #endif

                RnGlobal.worldSpinDeclinePercent = Settings.options.worldSpinDeclinePercent;
                RnGlobal.endgameEnabled = Settings.options.endgameEnabled;
                RnGlobal.endgameDay = Settings.options.endgameDay;
                RnGlobal.endgameAuroraEnabled = Settings.options.endgameAuroraEnabled;                
                RnGlobal.minAirTemperature = Settings.options.minAirTemperature;
                RnGlobal.indoorOutdoorTemperaturePercent = Settings.options.indoorOutdoorTemperaturePercent;
                RnGlobal.carcassMovingEnabled = Settings.options.carcassMovingEnabled;
                RnGlobal.electricTorchLightingEnabled = Settings.options.electricTorchLightingEnabled;
                RnGlobal.heatRetentionEnabled = Settings.options.heatRetentionEnabled;
                RnGlobal.realisticFreezingEnabled = Settings.options.realisticFreezingEnabled;
                RnGlobal.minWildlifeDay = Settings.options.minWildlifeDay;
                RnGlobal.minWildlifePercent = Settings.options.minWildlifePercent;
                RnGlobal.fireFuelDurationMultiplier = Settings.options.fireFuelDurationMultiplier;
                RnGlobal.lanternFuelDurationMultiplier = Settings.options.lanternFuelDurationMultiplier;
                RnGlobal.torchBurnDurationMultiplier = Settings.options.torchBurnDurationMultiplier;
            }
        }

        /*
        [HarmonyPatch(typeof(SaveGameSlots), "GetSortedSaveSlots", null)]
        internal class SaveGameSlots_GetSortedSaveSlots
        {
            private static void Prefix(ref SaveSlotType slotType)
            {
                if (!RnGlobal.rnEnabled) return;

                slotType = RnGlobal.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), "SetCurrentSaveInfo", null)]
        internal class SaveGameSystem_SetCurrentSaveInfo
        {
            private static void Prefix(ref SaveSlotType gameMode)
            {
                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Entered.");
                #endif

                if (!RnGlobal.rnEnabled) return;

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Conditions Passed.");
                #endif

                gameMode = RnGlobal.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "GetSlotPrefix", null)]
        internal class SaveGameSlots_GetSlotPrefix
        {

            private static bool Prefix(ref string __result)
            {
                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Entered.");
                #endif

                if (!RnGlobal.rnEnabled) return true;

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Conditions Passed.");
                #endif

                __result = RnGlobal.RnSlotPrefix;
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "GetSaveSlotTypeFromName", null)]
        internal class SaveGameSlots_GetSaveSlotTypeFromName
        {
            private static bool Prefix(string name, ref SaveSlotType __result)
            {
                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Entered.");
                #endif

                if (name != null && !name.StartsWith(RnGlobal.RnSlotPrefix)) return true;

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Conditions Passed.");
                #endif

                __result = RnGlobal.RnSlotType;
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "SlotsAreLoading", null)]
        internal class SaveGameSlots_SlotsAreLoading
        {
            private static void Prefix(ref SaveSlotType slotType)
            {
                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Entered.");
                #endif

                if (!RnGlobal.rnEnabled) return;

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " Conditions Passed.");
                #endif

                slotType = RnGlobal.RnSlotType;
            }
        }
        */

        internal static FileInfo[] GetAllGameSaveFiles()
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(PersistentDataPath.m_Path);
            return directoryInfo.GetFiles("*", SearchOption.AllDirectories);
        }

        internal static void LoadRnSaveFiles()
        {
            foreach (FileInfo saveFile in GetAllGameSaveFiles())
            {
                if (IsCompatibleRnSave(saveFile.Name))
                {
                    SaveGameDataPC.LoadData(saveFile.Name);

                    #if DEBUG
                    Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " " + saveFile.Name + " Save File Loaded.");
                    #endif
                }
            }
        }

        internal static void RemoveDuplicateSaveSlots(Panel_MainMenu __instance)
        {
            Int32 index = sortedRnSaveSlots.Count - 1;
            while (index > 0)
            {
                if (sortedRnSaveSlots[index].m_SaveSlotName == sortedRnSaveSlots[index - 1].m_SaveSlotName)
                {
                    Utilities.ModLog("Removing Duplicate Slot with name " + sortedRnSaveSlots[index].m_SaveSlotName);

                    if (index < sortedRnSaveSlots.Count - 1)
                    {
                        (sortedRnSaveSlots[index], sortedRnSaveSlots[sortedRnSaveSlots.Count - 1]) = (sortedRnSaveSlots[sortedRnSaveSlots.Count - 1], sortedRnSaveSlots[index]);
                    }
                    sortedRnSaveSlots.RemoveAt(sortedRnSaveSlots.Count - 1);
                    SaveGameSlotHelper.m_SandboxSlots.Remove(sortedRnSaveSlots[index - 1]);
                    index--;
                }
                else
                {
                    index--;
                }
            }
        }

        internal static bool IsIncompatibleRnSave(string fileName)
        {
            return fileName.StartsWith("ep1" + RnGlobal.RnSlotPrefix);
        }

        internal static bool IsCompatibleRnSave(string fileName)
        {
            return fileName.StartsWith(RnGlobal.RnSlotPrefix);
        }

        internal static void LoadSaveSlots(string savePrefix)
        {
            DirectoryInfo directoryInfo = new DirectoryInfo(PersistentDataPath.m_Path);
            FileInfo[] rnSaveFiles = directoryInfo.GetFiles("*" + savePrefix + "*", SearchOption.AllDirectories);

            foreach (FileInfo saveFile in rnSaveFiles)
            {
                SlotData saveSlot = SaveGameSlots.GetSaveSlotFromName(saveFile.Name);

                saveSlot.m_GameMode = RnGlobal.RnSlotType;
                saveSlot.m_Episode = Episode.One;

                SaveGameSlots.AddSlotData(saveSlot);

                #if DEBUG
                Debug.Log(DateTime.Now.TimeOfDay + " - " + MethodBase.GetCurrentMethod().DeclaringType.ToString() + " RN Save Slot Created.");
                #endif
            }
        }
    }
}