using System.Collections.Generic;
using Harmony;
using RelentlessNight;
using UnityEngine;
using Newtonsoft.Json;

[HarmonyPatch(typeof(MissionServicesManager), "SceneLoadCompleted", null)]
public class MissionServicesManager_SceneLoadCompleted_Pos
{
    private static void Postfix()
    {
        //Debug.Log("MissionServicesManager_SceneLoadCompleted_Pos");
        if (!RnGl.rnActive || !RnGl.glHeatRetention) return;
      
        RnGl.UpdateRnGlobalsForScene();      
    }
}

[HarmonyPatch(typeof(Panel_ChooseSandbox), "ProcessMenu", null)]
internal class Panel_ChooseSandbox_ProcessMenu_Pos
{
    private static void Postfix()
    {
        //Debug.Log("Panel_ChooseSandbox_ProcessMenu_Pos");
        if (!RnGl.rnActive) return;

        SaveGameSlots.SANDBOX_SLOT_PREFIX = "sandbox"; 
    }
}

/*
[HarmonyPatch(typeof(Panel_ChooseSandbox), "AddSavesOfTypeToMenu", null)]
internal class Panel_MainMenu_AddSavesOfTypeToMenu_Pos
{
    public static bool Prefix(Panel_ChooseSandbox __instance)
    {
        Debug.Log("Panel_MainMenu_AddSavesOfTypeToMenu_Pos");
        if (!RnGl.rnActive) return true;

        string descriptionText = Localization.Get("GAMEPLAY_DescriptionLoadSurvival");
        int numSandboxSaveSlots = InterfaceManager.m_Panel_MainMenu.GetNumSandboxSaveSlots();      
        for (int i = 0; i < numSandboxSaveSlots; i++)
        {
            SaveSlotInfo sandboxSaveSlotInfo = InterfaceManager.m_Panel_MainMenu.GetSandboxSaveSlotInfo(i);          
            if (sandboxSaveSlotInfo == null)
            {
                Debug.LogWarningFormat("Missing save slot {0}", new object[] {i});
            }
            else if (sandboxSaveSlotInfo.m_GameMode == RnGl.RnSlotType)
            {
                BasicMenu basicMenu = __instance.m_BasicMenu;
                string saveSlotName = sandboxSaveSlotInfo.m_SaveSlotName;
                int value = i;
                string userDefinedName = sandboxSaveSlotInfo.m_UserDefinedName;
                m_BasicMenu.AddItem(saveSlotName, value, i, userDefinedName, descriptionText, null, () => { AccessTools.Method(typeof(Panel_ChooseSandbox), "OnSlotClicked", null, null).Invoke(__instance, null); });
            }           
        }
        return false;
    }
}
public delegate void OnSlotClicked();
*/

[HarmonyPatch(typeof(Panel_MainMenu), "Awake", null)]
public class Panel_MainMenu_Awake_Pos
{
    public static void Postfix(Panel_MainMenu __instance)
    {
        //Debug.Log("Panel_MainMenu_Awake_Pos");
        Panel_MainMenu.MainMenuItem mainMenuItem = new Panel_MainMenu.MainMenuItem();
        mainMenuItem.m_LabelLocalizationId = "Relentless Night";
        mainMenuItem.m_Type = (Panel_MainMenu.MainMenuItem.MainMenuItemType)7;
        __instance.m_MenuItems.Insert(1, mainMenuItem);
    }
}

[HarmonyPatch(typeof(Panel_MainMenu), "OnLoadGame", null)]
public class Panel_MainMenu_OnLoadGame_Pos
{
    private static void Postfix()
    {
        //Debug.Log("Panel_MainMenu_OnLoadGame_Pos");
        if (!RnGl.rnActive) return;
        
        string currentSaveName = SaveGameSystem.GetCurrentSaveName();
        string rnSaveData = SaveGameSlots.LoadDataFromSlot(currentSaveName, "RelentlessNight");
 
        if (rnSaveData == null)
        {
            RnGl.glEndgameActive = true;
            RnGl.glEndgameDay = 300;
            RnGl.glRotationDecline = 12;
            RnGl.glTemperatureEffect = 30;
            RnGl.glHeatRetention = true;
            RnGl.glRealisticFreezing = true;
            RnGl.glWildlifeFreezing = true;
            RnGl.glMinWildlifeDay = 300;
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
            RnGl.glIsCarryingCarcass = rnSd.sdIsCarryingCarcass;
            RnGl.glSerializedCarcass = rnSd.sdSerializedCarcass;
        }
    }
}

[HarmonyPatch(typeof(SaveGameSystem), "SaveGlobalData", null)]
public class SaveGameSystem_SaveGlobalData_Pre
{
    private static void Prefix(SaveSlotType gameMode, string name)
    {
        //Debug.Log("SaveGameSystem_SaveGlobalData_Pre");
        if (!RnGl.rnActive) return;

        RnSd value = new RnSd
        {
            sdEndgameActive = RnGl.glEndgameActive,
            sdEndgameDay = RnGl.glEndgameDay,
            sdRotationDecline = RnGl.glRotationDecline,
            sdTemperatureEffect = RnGl.glTemperatureEffect,
            sdHeatRetenion = RnGl.glHeatRetention,
            sdRealisticFreezing = RnGl.glRealisticFreezing,
            sdWildlifeFreezing = RnGl.glWildlifeFreezing,
            sdMinWildlifeDay = RnGl.glMinWildlifeDay,
            sdMinWildlifeAmount = RnGl.glMinWildlifeAmount,
            sdFireFuelFactor = RnGl.glFireFuelFactor,
            sdLanternFuelFactor = RnGl.glLanternFuelFactor,
            sdDayTidallyLocked = RnGl.glDayTidallyLocked,
            sdDayNum = RnGl.glDayNum,
            sdLastOutdoorTempNoBliz = RnGl.glLastOutdoorTempNoBliz,
            sdIsCarryingCarcass = RnGl.glIsCarryingCarcass,
            sdSerializedCarcass = RnGl.glSerializedCarcass,
        };
        string data = JsonConvert.SerializeObject(value);
        SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, "RelentlessNight", data);       
    }
}

[HarmonyPatch(typeof(SaveGameSlots), "BuildSlotName", null)]
internal class SaveGameSlots_BuildSlotName_Pre
{
    public static bool Prefix(SaveSlotType slotType, int n, ref string __result)
    {
        //Debug.Log("SaveGameSlots_BuildSlotName_Pre");
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
        //Debug.Log("SaveGameSlots_GetSaveSlotTypeFromName_Pre");
        if (!name.Contains(RnGl.RnSlotPrefix)) return true;
       
        __result = RnGl.RnSlotType;
        return false;        
    }
}

[HarmonyPatch(typeof(SaveGameSlots), "GetSlotPrefix", null)]
internal class SaveGameSlots_GetSlotPrefix_Pre
{
    public static bool Prefix(SaveSlotType slotType, ref string __result)
    {
        //Debug.Log("SaveGameSlots_GetSlotPrefix_Pre");
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
        //Debug.Log("SaveGameSlots_SaveDataToSlot_Pre");
        if (!RnGl.rnActive) return;
       
        gameMode = RnGl.RnSlotType;       
    }
}

[HarmonyPatch(typeof(SaveGameSystem), "SetCurrentSaveInfo", null)]
internal class SaveGameSystem_SetCurrentSaveInfo_Pre
{
    private static void Prefix(ref SaveSlotType gameMode)
    {
        //Debug.Log("SaveGameSystem_SetCurrentSaveInfo_Pre");
        if (!RnGl.rnActive) return;

        gameMode = RnGl.RnSlotType;
    }
}

[HarmonyPatch(typeof(SaveGameSlots), "SetLoadingPriority", null)]
internal class SaveGameSlots_SetLoadingPriority_Pre
{
    public static void Prefix(ref SaveSlotType slotType)
    {
        //Debug.Log("SaveGameSlots_SetLoadingPriority_Pre");
        if (!RnGl.rnActive) return;

        slotType = RnGl.RnSlotType;
    }
}

[HarmonyPatch(typeof(SaveGameSlots), "SlotsAreLoading", null)]
internal class SaveGameSlots_SlotsAreLoading_Pre
{
    public static void Prefix(ref SaveSlotType slotType)
    {
        //Debug.Log("SaveGameSlots_SlotsAreLoading_Pre");
        if (!RnGl.rnActive) return;

        slotType = RnGl.RnSlotType;
    }
}

[HarmonyPatch(typeof(SaveSlotViewItem), "LoadSlotData", null)]
internal class SaveSlotViewItem_LoadSlotData_Pre
{
    public static void Prefix(ref SaveSlotType slotType)
    {
        //Debug.Log("SaveSlotViewItem_LoadSlotData_Pre");
        if (!RnGl.rnActive) return;

        slotType = RnGl.RnSlotType;
    }
}