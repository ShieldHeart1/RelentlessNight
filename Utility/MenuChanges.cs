using System;
using System.Collections.Generic;
using Harmony;
using RelentlessNight;
using UnityEngine;
using IL2CPP = Il2CppSystem.Collections.Generic;

[HarmonyPatch(typeof(BasicMenu), "InternalClickAction", null)]
internal class BasicMenu_InternalClickAction_Pre
{
    private static void Prefix(int index, BasicMenu __instance)
    {
        //Debug.Log("BasicMenu_InternalClickAction_Pre");
        IL2CPP.List<BasicMenu.BasicMenuItemModel> list = __instance.m_ItemModelList;

        if (list[index].m_LabelText == "Relentless Night")
        {
            RnGl.rnActive = true;
        }
    }
}

[HarmonyPatch(typeof(BasicMenu), "UpdateTitleHeader", null)]
internal class BasicMenu_UpdateTitleHeader_Pos
{
    private static void Postfix(BasicMenu __instance)
    {
        //Debug.Log("BasicMenu_UpdateTitleHeader_Pos");
        if (!RnGl.rnActive || !InterfaceManager.m_Panel_SelectExperience.IsEnabled()) return;
       
        __instance.m_TitleHeaderLabel.capsLock = false;
        __instance.m_TitleHeaderLabel.fontSize = 16;       
    }
}

[HarmonyPatch(typeof(GameManager), "GetVersionString", null)]
internal class GameManager_GetVersionString_Pos
{    
    private static void Postfix(ref string __result)
    {
        //Debug.Log("GameManager_GetVersionString_Pos");
        __result += "Relentless Night " + RnGl.RnVersion;
    }
}

[HarmonyPatch(typeof(Panel_Badges), "OnFeats", null)]
internal class Panel_Badges_OnFeats_Pos
{
    private static void Postfix()
    {
        //Debug.Log("Panel_Badges_OnFeats_Pos");
        if (RnGl.rnFeatsActive)
        {
            RnGl.rnActive = true;
        }
    }
}

[HarmonyPatch(typeof(Panel_ChooseSandbox), "ConfigureMenu", null)]
internal class Panel_ChooseSandbox_ConfigureMenu_Pos
{
    private static void Postfix(Panel_ChooseSandbox __instance)
    {
        //Debug.Log("Panel_ChooseSandbox_ConfigureMenu_Pos");
        if (!RnGl.rnActive) return;

        BasicMenu basicMenu = __instance.m_BasicMenu;
        basicMenu.UpdateTitle("Relentless Night Mode", string.Empty, Vector3.zero);        
    }
}

[HarmonyPatch(typeof(Panel_ChooseSandbox), "ProcessMenu", null)]
internal class Panel_ChooseSandbox_ProcessMenu_Pre
{
    private static void Prefix()
    {
        //Debug.Log("Panel_ChooseSandbox_ProcessMenu_Pre");
        if (!RnGl.rnActive) return;
        
        SaveGameSlots.SANDBOX_SLOT_PREFIX = "relentless";        
    }
}

[HarmonyPatch(typeof(Panel_Confirmation), "ShowRenamePanel", null)]
internal class Panel_Confirmation_ShowRenamePanel_Pre
{
    private static void Prefix(ref string locID, ref string currentName)
    {
        //Debug.Log("Panel_Confirmation_ShowRenamePanel_Pre");
        if (!RnGl.rnActive) return;

        currentName = "Relentless " + (InterfaceManager.m_Panel_OptionsMenu.m_State.m_NumGamesPlayed + 1).ToString();
        locID = "GAMEPLAY_NameSandbox";        
    }
}

[HarmonyPatch(typeof(Panel_MainMenu), "AddMenuItem", null)]
public class Panel_MainMenu_AddMenuItem_Pre
{
    private static bool Prefix(int itemIndex, Panel_MainMenu __instance)
    {
        if (!InterfaceManager.IsMainMenuActive()) return true;

        if (itemIndex == 1)
        {
            string id = __instance.m_MenuItems[itemIndex].m_Type.ToString();
            int type = (int)__instance.m_MenuItems[itemIndex].m_Type;
            string key = "Relentless Night";
            string key2 = "The earth seems to be slowing down. Days and nights are getting longer. Each night is colder and harsher than the last. How long will you survive?";
            string secondaryText = "";
            Action actionFromType = new Action(__instance.OnSandbox);
            BasicMenu basicMenu = __instance.m_BasicMenu;
            BasicMenu.BasicMenuItemModel firstItem = basicMenu.m_ItemModelList[0];
            basicMenu.AddItem(id, type, itemIndex, Localization.Get(key), Localization.Get(key2), secondaryText, actionFromType, firstItem.m_NormalTint, firstItem.m_HighlightTint);
            return false;
        } 
                
        return true; // nvm, go ahead :D
    }
}

[HarmonyPatch(typeof(Panel_MainMenu), "OnSandbox", null)] //inlined everwhere
internal class Panel_MainMenu_OnSandbox_Pre
{
    private static bool Prefix(Panel_MainMenu __instance)
    {
        //Debug.Log("Panel_MainMenu_OnSandbox_Pre");
        if (!RnGl.rnActive) return true;        

        IL2CPP.List<SlotData> m_SaveSlots = SaveGameSlots.m_SaveSlots;
        foreach (SlotData slotData in m_SaveSlots)
        {
            if (slotData.m_Name.Contains("relentless") && slotData.m_GameMode != RnGl.RnSlotType) slotData.m_GameMode = RnGl.RnSlotType;
        }

        bool m_Quit = __instance.m_Quit;
        if (m_Quit) return false;            
     
        if (SaveGameSlots.SlotsAreLoading(RnGl.RnSlotType))
        {
            SaveGameSlots.SetLoadingPriority(RnGl.RnSlotType);
            GameAudioManager.PlayGUIError();
            return false;
        }

        IL2CPP.List<SaveSlotInfo> sortedSaveSlots = SaveGameSystem.GetSortedSaveSlots(Episode.One, RnGl.RnSlotType);
        __instance.m_SandboxSlots = sortedSaveSlots;
        GameAudioManager.PlayGUIButtonClick();
        SaveSlotType saveSlotType = RnGl.RnSlotType;
        __instance.m_SlotTypeSelected = saveSlotType;
        __instance.m_AllObjects.SetActive(false);
        InterfaceManager.m_Panel_Sandbox.Enable(true);
        return false;    
    }
}

[HarmonyPatch(typeof(Panel_MainMenu), "OnSelectSurvivorBack", null)]
internal static class Panel_MainMenu_OnSelectSurvivorBack_Pre
{
    private static bool Prefix(Panel_MainMenu __instance)
    {
        //Debug.Log("Panel_MainMenu_OnSelectSurvivorBack_Pre");
        if (!RnGl.rnActive) return true;
      
        InterfaceManager.m_Panel_SelectExperience.Enable(true);
        return false;
    }
}

[HarmonyPatch(typeof(Panel_MainMenu), "OnSelectSurvivorContinue", null)]
internal static class Panel_MainMenu_OnSelectSurvivorContinue_Pre
{
    private static bool Prefix(Panel_MainMenu __instance)
    {
        //Debug.Log("Panel_MainMenu_OnSelectSurvivorContinue_Pre");
        if (!RnGl.rnActive) return true;
     
        GameAudioManager.PlayGuiConfirm();
        int numUnlockedFeats = __instance.GetNumUnlockedFeats();

        if (numUnlockedFeats > 0)
        {
            __instance.SelectWindow(__instance.m_SelectFeatWindow);
        }
        else
        {
            FeatEnabledTracker.m_FeatsEnabledThisSandbox = new IL2CPP.List<FeatType>();
            __instance.ShowNameSaveSlotPopup();
        }
        return false;
    }
}

[HarmonyPatch(typeof(Panel_MainMenu), "ConfigureMenu", null)]
internal class Panel_PanelMainMenu_ConfigureMenu_Pos
{
    private static void Postfix(Panel_MainMenu __instance)
    {  
        if (InterfaceManager.IsMainMenuActive() && !__instance.IsSubMenuEnabled())
        {
            RnGl.rnActive = false;
        }            
    }
}

[HarmonyPatch(typeof(Panel_PauseMenu), "ConfigureMenu", null)]
internal class Panel_PauseMenu_ConfigureMenu_Pos
{
    private static void Postfix(Panel_PauseMenu __instance)
    {
        //Debug.Log("Panel_PauseMenu_ConfigureMenu_Pos");
        if (!RnGl.rnActive || !InterfaceManager.m_Panel_PauseMenu.IsEnabled()) return;

        string[] array = new string[]
        {
            "ENDGAME ACTIVE: " + RnGl.glEndgameActive.ToString(),
            "ENDGAME DAY: " + RnGl.glEndgameDay.ToString(),
            "DAY LENGTH CHANGE RATE: " + RnGl.glRotationDecline.ToString() + "%",
            "INDOOR/OUTDOOR TEMP: " + RnGl.glTemperatureEffect.ToString() + "%",
            "HEAT RETENTION: " + RnGl.glHeatRetention.ToString(),
            "REALISTIC FREEZING: " + RnGl.glRealisticFreezing.ToString(),
            "TEMP AFFECTS WILDLIFE: " + RnGl.glWildlifeFreezing.ToString(),
            "MIN WILDLIFE AMOUNT: " + RnGl.glMinWildlifeAmount.ToString() + "%",
            "MIN WILDLIFE DAY: " + RnGl.glMinWildlifeDay.ToString(),
            "FIRE FUEL BURNTIME: " + RnGl.glFireFuelFactor.ToString() + "X",
            "LANTERN FUEL BURNTIME: " + RnGl.glLanternFuelFactor.ToString() + "X"
        };

        string rnSettings = "";
        for (int i = 0; i < array.Length; i++)
        {
            rnSettings = rnSettings + array[i] + "\n";
        }
        
        BasicMenu basicMenu = __instance.m_BasicMenu;
        basicMenu.UpdateTitle("Relentless Night Mode", rnSettings, new Vector3(0f, -145f, 0f));
        basicMenu.m_TitleHeaderLabel.fontSize = 14;
        basicMenu.m_TitleHeaderLabel.capsLock = true;
        basicMenu.m_TitleHeaderLabel.useFloatSpacing = true;
        basicMenu.m_TitleHeaderLabel.floatSpacingY = 1.5f;
    }
}

[HarmonyPatch(typeof(Panel_Sandbox), "ConfigureMenu", null)]
internal class Panel_Sandbox_ConfigureMenu_Pos
{
    private static void Postfix(Panel_Sandbox __instance)
    {
        //Debug.Log("Panel_Sandbox_ConfigureMenu_Pos");
        if (!RnGl.rnActive) return;

        BasicMenu basicMenu = __instance.m_BasicMenu;
        basicMenu.UpdateTitle("Relentless Night Mode", string.Empty, Vector3.zero);       
    }
}

[HarmonyPatch(typeof(Panel_Sandbox), "OnClickFeats", null)] // got inlined everywhere
internal class Panel_Sandbox_OnClickFeats_Pre
{    
    private static void Prefix()
    {
        //Debug.Log("Panel_Sandbox_OnClickFeats_Pre");
        RnGl.rnFeatsActive = RnGl.rnActive;
    }
}

[HarmonyPatch(typeof(Panel_SelectExperience), "ConfigureMenu", null)]
internal class Panel_Sandbox_Panel_SelectExperience_Pos
{    
    private static void Postfix(Panel_SelectExperience __instance)
    {
        //Debug.Log("Panel_Sandbox_Panel_SelectExperience_Pos");
        if (!RnGl.rnActive) return;

        BasicMenu basicMenu = __instance.m_BasicMenu;
        basicMenu.UpdateTitle("Relentless Night Mode", "Choose custom to start a Relentless Night game\nwhere you can customize the mod features.\n\nChoosing one of the standard difficulties " +
            "will\nstart a game with the settings of your last\nRelentless Night save. If no previous saves\nexist, the default Relentless Night settings\nwill be used.\n\nYou can pause your " +
            "game any time to see the\nRelentless Night settings for the current run.", new Vector3(0f, -150f, 0f));      
    }
}

