using System.IO;
using Harmony;
using Newtonsoft.Json;
using RelentlessNight;
using Il2CppSystem.Collections.Generic;

[HarmonyPatch(typeof(Panel_CustomXPSetup), "Start", null)]
internal class Panel_Panel_CustomXPSetup_Start_Pre
{
    private static void Prefix(Panel_CustomXPSetup __instance)
    {
        //Debug.Log("--- Panel_Panel_CustomXPSetup_Start_Pre");
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
                RnGl.glLastOutdoorTempNoBliz = rnSd.sdLastOutdoorTempNoBliz;
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