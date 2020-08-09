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
        internal class Panel_MainMenu_AddSavesOfTypeToMenu_Pos
        {
            public static bool Prefix(Panel_ChooseSandbox __instance)
            {
                if (!RnGl.rnActive) return true;

                string descriptionText = Localization.Get("GAMEPLAY_DescriptionLoadSurvival");
                int numSandboxSaveSlots = InterfaceManager.m_Panel_MainMenu.GetNumSandboxSaveSlots();
                for (int i = 0; i < numSandboxSaveSlots; i++)
                {
                    SaveSlotInfo sandboxSaveSlotInfo = InterfaceManager.m_Panel_MainMenu.GetSandboxSaveSlotInfo(i);

                    if (sandboxSaveSlotInfo.m_GameMode == RnGl.RnSlotType)
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
                        if (slotData.m_Name.Contains("relentless") && slotData.m_GameMode != RnGl.RnSlotType)
                        {
                            slotData.m_GameMode = RnGl.RnSlotType;
                        }
                    }
                    List<SaveSlotInfo> sortedSaveSlots = SaveGameSystem.GetSortedSaveSlots(Episode.One, RnGl.RnSlotType);
                    string text = SaveGameSlots.LoadDataFromSlot(sortedSaveSlots[0].m_SaveSlotName, "RelentlessNight");

                    // RN save is found, populate globals from save data
                    if (text != null)
                    {
                        RnSd rnSd = JsonConvert.DeserializeObject<RnSd>(text);
                        RnGl.glEndgameActive = rnSd.sdEndgameActive;
                        RnGl.glEndgameDay = rnSd.sdEndgameDay;
                        RnGl.glRotationDecline = rnSd.sdRotationDecline;
                        RnGl.glTemperatureEffect = rnSd.sdTemperatureEffect;
                        RnGl.glHeatRetention = rnSd.sdHeatRetenion;
                        RnGl.glRealisticFreezing = rnSd.sdRealisticFreezing;
                        RnGl.glWildlifeFreezing = rnSd.sdWildlifeFreezing;
                        RnGl.glMinWildlifeDay = rnSd.sdMinWildlifeDay;
                        RnGl.glMinWildlifeAmount = rnSd.sdMinWildlifeAmount;
                        RnGl.glFireFuelFactor = rnSd.sdFireFuelFactor;
                        RnGl.glLanternFuelFactor = rnSd.sdLanternFuelFactor;
                        RnGl.glDayTidallyLocked = rnSd.sdDayTidallyLocked;
                        RnGl.glDayNum = rnSd.sdDayNum;
                    }

                    Settings.options.coEndgameActive = RnGl.glEndgameActive;
                    Settings.options.coEndgameDay = RnGl.glEndgameDay;
                    Settings.options.coRotationDecline = RnGl.glRotationDecline;
                    Settings.options.coTemperatureEffect = RnGl.glTemperatureEffect;
                    Settings.options.coHeatRetention = RnGl.glHeatRetention;
                    Settings.options.coExtraFreezing = RnGl.glRealisticFreezing;
                    Settings.options.coWildlifeFreezing = RnGl.glWildlifeFreezing;
                    Settings.options.coMinWildlifeAmount = RnGl.glMinWildlifeAmount;
                    Settings.options.coMinWildlifeDay = RnGl.glMinWildlifeDay;
                    Settings.options.coFireFuelFactor = RnGl.glFireFuelFactor;
                    Settings.options.coLanternFuelFactor = RnGl.glLanternFuelFactor;
                }
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnLoadGame", null)]
        public class Panel_MainMenu_OnLoadGame_Pos
        {
            private static void Postfix()
            {
                if (!RnGl.rnActive) return;

                string currentSaveName = SaveGameSystem.GetCurrentSaveName();
                string rnSaveData = SaveGameSlots.LoadDataFromSlot(currentSaveName, "RelentlessNight");

                if (rnSaveData == null)
                {
                    RnGl.glEndgameActive = true;
                    RnGl.glEndgameDay = 100;
                    RnGl.glEndgameAurora = false;
                    RnGl.glRotationDecline = 15;
                    RnGl.glTemperatureEffect = 50;
                    RnGl.glMinimumTemperature = -80;
                    RnGl.glHeatRetention = true;
                    RnGl.glRealisticFreezing = false;
                    RnGl.glWildlifeFreezing = true;
                    RnGl.glMinWildlifeDay = 100;
                    RnGl.glMinWildlifeAmount = 10;
                    RnGl.glFireFuelFactor = 1f;
                    RnGl.glLanternFuelFactor = 1f;
                    RnGl.glIsCarryingCarcass = false;
                    RnGl.glSerializedCarcass = null;
                }
                else
                {
                    RnSd rnSd = JsonConvert.DeserializeObject<RnSd>(rnSaveData);

                    RnGl.glEndgameActive = rnSd.sdEndgameActive;
                    RnGl.glEndgameDay = rnSd.sdEndgameDay;
                    RnGl.glEndgameAurora = rnSd.sdEndgameAurora;
                    RnGl.glRotationDecline = rnSd.sdRotationDecline;
                    RnGl.glTemperatureEffect = rnSd.sdTemperatureEffect;
                    RnGl.glMinimumTemperature = rnSd.sdMinimumTemperature;
                    RnGl.glHeatRetention = rnSd.sdHeatRetenion;
                    RnGl.glRealisticFreezing = rnSd.sdRealisticFreezing;
                    RnGl.glWildlifeFreezing = rnSd.sdWildlifeFreezing;
                    RnGl.glMinWildlifeDay = rnSd.sdMinWildlifeDay;
                    RnGl.glMinWildlifeAmount = rnSd.sdMinWildlifeAmount;
                    RnGl.glFireFuelFactor = rnSd.sdFireFuelFactor;
                    RnGl.glLanternFuelFactor = rnSd.sdLanternFuelFactor;
                    RnGl.glDayTidallyLocked = rnSd.sdDayTidallyLocked;
                    RnGl.glDayNum = rnSd.sdDayNum;
                    RnGl.glIsCarryingCarcass = rnSd.sdIsCarryingCarcass;
                    RnGl.glSerializedCarcass = rnSd.sdSerializedCarcass;

                    Settings.options.coEndgameActive = rnSd.sdEndgameActive;
                    Settings.options.coEndgameDay = rnSd.sdEndgameDay;
                    Settings.options.coEndgameAurora = rnSd.sdEndgameAurora;
                    Settings.options.coRotationDecline = rnSd.sdRotationDecline;
                    Settings.options.coTemperatureEffect = rnSd.sdTemperatureEffect;
                    Settings.options.coMinimumTemperature = rnSd.sdMinimumTemperature;
                    Settings.options.coHeatRetention = rnSd.sdHeatRetenion;
                    Settings.options.coExtraFreezing = rnSd.sdRealisticFreezing;
                    Settings.options.coWildlifeFreezing = rnSd.sdWildlifeFreezing;
                    Settings.options.coMinWildlifeDay = rnSd.sdMinWildlifeDay;
                    Settings.options.coMinWildlifeAmount = rnSd.sdMinWildlifeAmount;
                    Settings.options.coFireFuelFactor = rnSd.sdFireFuelFactor;
                    Settings.options.coLanternFuelFactor = rnSd.sdLanternFuelFactor;
                }
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData", null)]
        public class SaveGameSystem_SaveGlobalData_Pre
        {
            private static void Prefix(SaveSlotType gameMode, string name)
            {
                if (!RnGl.rnActive) return;

                RnSd value = new RnSd
                {
                    sdEndgameActive = RnGl.glEndgameActive,
                    sdEndgameDay = RnGl.glEndgameDay,
                    sdEndgameAurora = RnGl.glEndgameAurora,
                    sdRotationDecline = RnGl.glRotationDecline,
                    sdTemperatureEffect = RnGl.glTemperatureEffect,
                    sdMinimumTemperature = RnGl.glMinimumTemperature,
                    sdHeatRetenion = RnGl.glHeatRetention,
                    sdRealisticFreezing = RnGl.glRealisticFreezing,
                    sdWildlifeFreezing = RnGl.glWildlifeFreezing,
                    sdMinWildlifeDay = RnGl.glMinWildlifeDay,
                    sdMinWildlifeAmount = RnGl.glMinWildlifeAmount,
                    sdFireFuelFactor = RnGl.glFireFuelFactor,
                    sdLanternFuelFactor = RnGl.glLanternFuelFactor,
                    sdDayTidallyLocked = RnGl.glDayTidallyLocked,
                    sdDayNum = RnGl.glDayNum,
                    sdIsCarryingCarcass = RnGl.glIsCarryingCarcass,
                    sdSerializedCarcass = RnGl.glSerializedCarcass,
                };
                string data = JsonConvert.SerializeObject(value);
                SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, "RelentlessNight", data);
            }
        }

        [HarmonyPatch(typeof(MissionServicesManager), "SceneLoadCompleted", null)]
        public class MissionServicesManager_SceneLoadCompleted_Pos
        {
            private static void Postfix()
            {
                if (!RnGl.rnActive || !RnGl.glHeatRetention) return;

                RnGl.UpdateRnGlobalsForScene();
            }
        }

        [HarmonyPatch(typeof(Panel_ChooseSandbox), "ProcessMenu", null)]
        internal class Panel_ChooseSandbox_ProcessMenu_Pos
        {
            private static void Postfix()
            {
                if (!RnGl.rnActive) return;

                SaveGameSlots.SANDBOX_SLOT_PREFIX = "sandbox";
            }
        }
  
        [HarmonyPatch(typeof(SaveGameSlots), "BuildSlotName", null)]
        internal class SaveGameSlots_BuildSlotName_Pre
        {
            public static bool Prefix(SaveSlotType slotType, int n, ref string __result)
            {
                if (!RnGl.rnActive) return true;

                __result = "ep1relentless" + n.ToString();
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "GetSaveSlotTypeFromName", null)]
        internal class SaveGameSlots_GetSaveSlotTypeFromName_Pre
        {
            public static bool Prefix(string name, ref SaveSlotType __result)
            {
                if (name != null && !name.Contains(RnGl.RnSlotPrefix)) return true;

                __result = RnGl.RnSlotType;
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "GetSlotPrefix", null)]
        internal class SaveGameSlots_GetSlotPrefix_Pre
        {
            public static bool Prefix(SaveSlotType slotType, ref string __result)
            {
                if (slotType != RnGl.RnSlotType) return true;

                __result = RnGl.RnSlotPrefix;
                return false;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "SaveDataToSlot", null)]
        public class SaveGameSlots_SaveDataToSlot_Pre
        {
            private static void Prefix(ref SaveSlotType gameMode)
            {
                if (!RnGl.rnActive) return;

                gameMode = RnGl.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveGameSystem), "SetCurrentSaveInfo", null)]
        internal class SaveGameSystem_SetCurrentSaveInfo_Pre
        {
            private static void Prefix(ref SaveSlotType gameMode)
            {
                if (!RnGl.rnActive) return;

                gameMode = RnGl.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "SetLoadingPriority", null)]
        internal class SaveGameSlots_SetLoadingPriority_Pre
        {
            public static void Prefix(ref SaveSlotType slotType)
            {
                if (!RnGl.rnActive) return;

                slotType = RnGl.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveGameSlots), "SlotsAreLoading", null)]
        internal class SaveGameSlots_SlotsAreLoading_Pre
        {
            public static void Prefix(ref SaveSlotType slotType)
            {
                if (!RnGl.rnActive) return;

                slotType = RnGl.RnSlotType;
            }
        }

        [HarmonyPatch(typeof(SaveSlotViewItem), "LoadSlotData", null)]
        internal class SaveSlotViewItem_LoadSlotData_Pre
        {
            public static void Prefix(ref SaveSlotType slotType)
            {
                if (!RnGl.rnActive) return;

                slotType = RnGl.RnSlotType;
            }
        }        
    }
}