using System;
using System.IO;
using Harmony;
using UnityEngine;
using Newtonsoft.Json;
using Il2CppSystem.Collections.Generic;

namespace RelentlessNight
{
    public class SaveSystem
    {
        [HarmonyPatch(typeof(Panel_ChooseSandbox), "AddSavesOfTypeToMenu", null)]
        internal class Panel_MainMenu_AddSavesOfTypeToMenu_Post
        {
            public static bool Prefix(Panel_ChooseSandbox __instance)
            {
                if (!RnGlobal.rnActive) return true;

                string descriptionText = Localization.Get("GAMEPLAY_DescriptionLoadSurvival");
                int numSandboxSaveSlots = InterfaceManager.m_Panel_MainMenu.GetNumSandboxSaveSlots();
                for (int i = 0; i < numSandboxSaveSlots; i++)
                {
                    SaveSlotInfo sandboxSaveSlotInfo = InterfaceManager.m_Panel_MainMenu.GetSandboxSaveSlotInfo(i);

                    if (sandboxSaveSlotInfo.m_GameMode == RnGlobal.RnSlotType)
                    {
                        BasicMenu basicMenu = __instance.m_BasicMenu;
                        string saveSlotName = sandboxSaveSlotInfo.m_SaveSlotName;
                        int value = i;
                        string userDefinedName = sandboxSaveSlotInfo.m_UserDefinedName;
                        basicMenu.AddItem(saveSlotName, value, i, userDefinedName, descriptionText, null, new System.Action(() => __instance.OnSlotClicked()), Color.gray, Color.white);
                    }
                }

                return false;
            }
        }

        public delegate void OnSlotClicked();

        [HarmonyPatch(typeof(Panel_CustomXPSetup), "Start", null)]
        internal class Panel_Panel_CustomXPSetup_Start_Pre
        {
            private static void Prefix(Panel_CustomXPSetup __instance)
            {
                bool rnSaveExists = false;

                DirectoryInfo directoryInfo = new DirectoryInfo(PersistentDataPath.m_Path);

                FileInfo[] saveFiles = directoryInfo.GetFiles("*", SearchOption.AllDirectories);

                foreach (FileInfo fileInfo in saveFiles)
                {
                    if (fileInfo.Name.Contains("relentless"))
                    {
                        SaveGameDataPC.LoadData(fileInfo.Name);
                        rnSaveExists = true;
                    }
                }

                if (rnSaveExists)
                {
                    List<SlotData> m_SaveSlots = SaveGameSlots.m_SaveSlots;

                    foreach (SlotData slotData in m_SaveSlots)
                    {
                        if (slotData.m_Name.Contains("relentless") && slotData.m_GameMode != RnGlobal.RnSlotType)
                        {
                            slotData.m_GameMode = RnGlobal.RnSlotType;
                        }
                    }

                    List<SaveSlotInfo> sortedSaveSlots = SaveGameSystem.GetSortedSaveSlots(Episode.One, RnGlobal.RnSlotType);

                    // Pre 4.0 RN save exists, prevent loading, not compatible
                    if (sortedSaveSlots.Count == 0) return;

                    string text = SaveGameSlots.LoadDataFromSlot(sortedSaveSlots[0].m_SaveSlotName, "RelentlessNight");

                    // 4.0 RN save is found, populate globals from save data
                    if (text != null)
                    {
                        RnSd rnSd = JsonConvert.DeserializeObject<RnSd>(text);
                        RnGlobal.glEndgameActive = rnSd.sdEndgameActive;
                        RnGlobal.glEndgameDay = rnSd.sdEndgameDay;
                        RnGlobal.glEndgameAurora = rnSd.sdEndgameAurora;
                        RnGlobal.glRotationDecline = rnSd.sdRotationDecline;
                        RnGlobal.glMinimumTemperature = rnSd.sdMinimumTemperature;
                        RnGlobal.glTemperatureEffect = rnSd.sdTemperatureEffect;                        
                        RnGlobal.glHeatRetention = rnSd.sdHeatRetention;
                        RnGlobal.glRealisticFreezing = rnSd.sdRealisticFreezing;
                        RnGlobal.glWildlifeFreezing = rnSd.sdWildlifeFreezing;
                        RnGlobal.glMinWildlifeDay = rnSd.sdMinWildlifeDay;
                        RnGlobal.glMinWildlifeAmount = rnSd.sdMinWildlifeAmount;
                        RnGlobal.glFireFuelFactor = rnSd.sdFireFuelFactor;
                        RnGlobal.glLanternFuelFactor = rnSd.sdLanternFuelFactor;
                        RnGlobal.glTorchFuelFactor = rnSd.sdTorchFuelFactor;
                        RnGlobal.glDayTidallyLocked = rnSd.sdDayTidallyLocked;
                        RnGlobal.glDayNum = rnSd.sdDayNum;
                    }

                    Settings.options.coEndgameActive = RnGlobal.glEndgameActive;
                    Settings.options.coEndgameDay = RnGlobal.glEndgameDay;
                    Settings.options.coEndgameAurora = RnGlobal.glEndgameAurora;
                    Settings.options.coRotationDecline = RnGlobal.glRotationDecline;
                    Settings.options.coMinimumTemperature = RnGlobal.glMinimumTemperature;
                    Settings.options.coTemperatureEffect = RnGlobal.glTemperatureEffect;                    
                    Settings.options.coHeatRetention = RnGlobal.glHeatRetention;
                    Settings.options.coRealisticFreezing = RnGlobal.glRealisticFreezing;
                    Settings.options.coWildlifeFreezing = RnGlobal.glWildlifeFreezing;
                    Settings.options.coMinWildlifeAmount = RnGlobal.glMinWildlifeAmount;
                    Settings.options.coMinWildlifeDay = RnGlobal.glMinWildlifeDay;
                    Settings.options.coFireFuelFactor = RnGlobal.glFireFuelFactor;
                    Settings.options.coLanternFuelFactor = RnGlobal.glLanternFuelFactor;
                    Settings.options.coTorchFuelFactor = RnGlobal.glTorchFuelFactor;
                }
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnLoadGame", null)]
        public class Panel_MainMenu_OnLoadGame_Post
        {
            private static void Postfix()
            {
                if (!RnGlobal.rnActive) return;

                string currentSaveName = SaveGameSystem.GetCurrentSaveName();
                string rnSaveData = SaveGameSlots.LoadDataFromSlot(currentSaveName, "RelentlessNight");

                if (rnSaveData == null)
                {
                    RnGlobal.glEndgameActive = true;
                    RnGlobal.glEndgameDay = 100;
                    RnGlobal.glEndgameAurora = false;
                    RnGlobal.glRotationDecline = 15;
                    RnGlobal.glMinimumTemperature = -80;
                    RnGlobal.glTemperatureEffect = 50;                    
                    RnGlobal.glHeatRetention = true;
                    RnGlobal.glRealisticFreezing = false;
                    RnGlobal.glWildlifeFreezing = true;
                    RnGlobal.glMinWildlifeDay = 100;
                    RnGlobal.glMinWildlifeAmount = 10;
                    RnGlobal.glFireFuelFactor = 1f;
                    RnGlobal.glLanternFuelFactor = 1f;
                    RnGlobal.glTorchFuelFactor = 1f;
                    RnGlobal.glIsCarryingCarcass = false;
                    RnGlobal.glSerializedCarcass = null;
                }
                else
                {
                    RnSd rnSd = JsonConvert.DeserializeObject<RnSd>(rnSaveData);

                    RnGlobal.glEndgameActive = rnSd.sdEndgameActive;
                    RnGlobal.glEndgameDay = rnSd.sdEndgameDay;
                    RnGlobal.glEndgameAurora = rnSd.sdEndgameAurora;
                    RnGlobal.glRotationDecline = rnSd.sdRotationDecline;
                    RnGlobal.glMinimumTemperature = rnSd.sdMinimumTemperature;
                    RnGlobal.glTemperatureEffect = rnSd.sdTemperatureEffect;                 
                    RnGlobal.glHeatRetention = rnSd.sdHeatRetention;
                    RnGlobal.glRealisticFreezing = rnSd.sdRealisticFreezing;
                    RnGlobal.glWildlifeFreezing = rnSd.sdWildlifeFreezing;
                    RnGlobal.glMinWildlifeDay = rnSd.sdMinWildlifeDay;
                    RnGlobal.glMinWildlifeAmount = rnSd.sdMinWildlifeAmount;
                    RnGlobal.glFireFuelFactor = rnSd.sdFireFuelFactor;
                    RnGlobal.glLanternFuelFactor = rnSd.sdLanternFuelFactor;
                    RnGlobal.glTorchFuelFactor = rnSd.sdTorchFuelFactor;
                    RnGlobal.glDayTidallyLocked = rnSd.sdDayTidallyLocked;
                    RnGlobal.glDayNum = rnSd.sdDayNum;
                    RnGlobal.glIsCarryingCarcass = rnSd.sdIsCarryingCarcass;
                    RnGlobal.glSerializedCarcass = rnSd.sdSerializedCarcass;

                    Settings.options.coEndgameActive = rnSd.sdEndgameActive;
                    Settings.options.coEndgameDay = rnSd.sdEndgameDay;
                    Settings.options.coEndgameAurora = rnSd.sdEndgameAurora;
                    Settings.options.coRotationDecline = rnSd.sdRotationDecline;
                    Settings.options.coMinimumTemperature = rnSd.sdMinimumTemperature;
                    Settings.options.coTemperatureEffect = rnSd.sdTemperatureEffect;                    
                    Settings.options.coHeatRetention = rnSd.sdHeatRetention;
                    Settings.options.coRealisticFreezing = rnSd.sdRealisticFreezing;
                    Settings.options.coWildlifeFreezing = rnSd.sdWildlifeFreezing;
                    Settings.options.coMinWildlifeDay = rnSd.sdMinWildlifeDay;
                    Settings.options.coMinWildlifeAmount = rnSd.sdMinWildlifeAmount;
                    Settings.options.coFireFuelFactor = rnSd.sdFireFuelFactor;
                    Settings.options.coLanternFuelFactor = rnSd.sdLanternFuelFactor;
                    Settings.options.coTorchFuelFactor = rnSd.sdTorchFuelFactor;
                }
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData", null)]
        public class SaveGameSystem_SaveGlobalData_Pre
        {
            private static void Prefix(SaveSlotType gameMode, string name)
            {
                if (!RnGlobal.rnActive) return;

                RnSd value = new RnSd
                {
                    sdEndgameActive = RnGlobal.glEndgameActive,
                    sdEndgameDay = RnGlobal.glEndgameDay,
                    sdEndgameAurora = RnGlobal.glEndgameAurora,
                    sdRotationDecline = RnGlobal.glRotationDecline,
                    sdMinimumTemperature = RnGlobal.glMinimumTemperature,
                    sdTemperatureEffect = RnGlobal.glTemperatureEffect,                    
                    sdHeatRetention = RnGlobal.glHeatRetention,
                    sdRealisticFreezing = RnGlobal.glRealisticFreezing,
                    sdWildlifeFreezing = RnGlobal.glWildlifeFreezing,
                    sdMinWildlifeDay = RnGlobal.glMinWildlifeDay,
                    sdMinWildlifeAmount = RnGlobal.glMinWildlifeAmount,
                    sdFireFuelFactor = RnGlobal.glFireFuelFactor,
                    sdLanternFuelFactor = RnGlobal.glLanternFuelFactor,
                    sdTorchFuelFactor = RnGlobal.glTorchFuelFactor,
                    sdDayTidallyLocked = RnGlobal.glDayTidallyLocked,
                    sdDayNum = RnGlobal.glDayNum,
                    sdIsCarryingCarcass = RnGlobal.glIsCarryingCarcass,
                    sdSerializedCarcass = RnGlobal.glSerializedCarcass,
                };

                string data = JsonConvert.SerializeObject(value);
                SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, "RelentlessNight", data);
            }
        }

        [HarmonyPatch(typeof(Panel_ChooseSandbox), "ProcessMenu", null)]
        internal class Panel_ChooseSandbox_ProcessMenu_Post
        {
            private static void Postfix()
            {
                if (!RnGlobal.rnActive) return;

                SaveGameSlots.SANDBOX_SLOT_PREFIX = "sandbox";
            }
        }
  
        [HarmonyPatch(typeof(SaveGameSlots), "BuildSlotName", null)]
        internal class SaveGameSlots_BuildSlotName_Pre
        {
            public static bool Prefix(SaveSlotType slotType, int n, ref string __result)
            {
                if (!RnGlobal.rnActive) return true;

                __result = "ep1relentless" + n.ToString();
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "GetSaveSlotTypeFromName", null)]
        internal class SaveGameSlots_GetSaveSlotTypeFromName_Pre
        {
            public static bool Prefix(string name, ref SaveSlotType __result)
            {
                if (name != null && !name.Contains(RnGlobal.RnSlotPrefix)) return true;

                __result = RnGlobal.RnSlotType;
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "GetSlotPrefix", null)]
        internal class SaveGameSlots_GetSlotPrefix_Pre
        {
            public static bool Prefix(SaveSlotType slotType, ref string __result)
            {
                if (slotType != RnGlobal.RnSlotType) return true;

                __result = RnGlobal.RnSlotPrefix;
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "SaveDataToSlot", null)]
        public class SaveGameSlots_SaveDataToSlot_Pre
        {
            private static void Prefix(ref SaveSlotType gameMode)
            {
                if (!RnGlobal.rnActive) return;

                gameMode = RnGlobal.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), "SetCurrentSaveInfo", null)]
        internal class SaveGameSystem_SetCurrentSaveInfo_Pre
        {
            private static void Prefix(ref SaveSlotType gameMode)
            {
                if (!RnGlobal.rnActive) return;

                gameMode = RnGlobal.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "SetLoadingPriority", null)]
        internal class SaveGameSlots_SetLoadingPriority_Pre
        {
            public static void Prefix(ref SaveSlotType slotType)
            {
                if (!RnGlobal.rnActive) return;

                slotType = RnGlobal.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "SlotsAreLoading", null)]
        internal class SaveGameSlots_SlotsAreLoading_Pre
        {
            public static void Prefix(ref SaveSlotType slotType)
            {
                if (!RnGlobal.rnActive) return;

                slotType = RnGlobal.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveSlotViewItem), "LoadSlotData", null)]
        internal class SaveSlotViewItem_LoadSlotData_Pre
        {
            public static void Prefix(ref SaveSlotType slotType)
            {
                if (!RnGlobal.rnActive) return;

                slotType = RnGlobal.RnSlotType;
            }
        }        
    }
}