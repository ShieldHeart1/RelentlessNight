using System.Collections.Generic;
using System.IO;
using Harmony;
using Newtonsoft.Json;
using RelentlessNight;
using UnityEngine;

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
            List<SlotData> m_SaveSlots = (List<SlotData>)AccessTools.Field(typeof(SaveGameSlots), "m_SaveSlots").GetValue(__instance);
            foreach (SlotData slotData in m_SaveSlots)
            {
                if (slotData.m_Name.Contains("relentless") && slotData.m_GameMode != RnGl.RnSlotType)
                {
                    slotData.m_GameMode = RnGl.RnSlotType;
                }
            }
            List<SaveSlotInfo> sortedSaveSlots = SaveGameSystem.GetSortedSaveSlots(Episode.One, RnGl.RnSlotType);
            string text = SaveGameSlots.LoadDataFromSlot(sortedSaveSlots[0].m_SaveSlotName, "RelentlessNight");

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
            /*
            BuildcustomizationList.customOptionsProxy.coEndgameActive = RnGl.glEndgameActive;
            BuildcustomizationList.customOptionsProxy.coEndgameDay = RnGl.glEndgameDay;
            BuildcustomizationList.customOptionsProxy.coRotationDecline = RnGl.glRotationDecline;
            BuildcustomizationList.customOptionsProxy.coTemperatureEffect = RnGl.glTemperatureEffect;
            BuildcustomizationList.customOptionsProxy.coHeatRetention = RnGl.glHeatRetention;
            BuildcustomizationList.customOptionsProxy.coExtraFreezing = RnGl.glRealisticFreezing;
            BuildcustomizationList.customOptionsProxy.coWildlifeFreezing = RnGl.glWildlifeFreezing;
            BuildcustomizationList.customOptionsProxy.coMinWildlifeAmount = RnGl.glMinWildlifeAmount;
            BuildcustomizationList.customOptionsProxy.coMinWildlifeDay = RnGl.glMinWildlifeDay;
            BuildcustomizationList.customOptionsProxy.coFireFuelFactor = RnGl.glFireFuelFactor;
            BuildcustomizationList.customOptionsProxy.coLanternFuelFactor = RnGl.glLanternFuelFactor;
            */
        }
        //CustomMode.RegisterOptionsObject(BuildcustomizationList.customOptionsProxy, Position.BeforeAll);
    }
}