using System;
using Harmony;
using UnityEngine;
using RelentlessNight;
using IL2CPP = Il2CppSystem.Collections.Generic;

[HarmonyPatch(typeof(ConsoleManager), "RegisterCommands", new Type[] {})]
public class ConsoleManager_RegisterCommands_Pos
{
    private static void Postfix()
    {
        //Debug.Log("ConsoleManager_RegisterCommands_Pos");
        uConsole.RegisterCommand("rn_help", new Action(RnHelp));
        uConsole.RegisterCommand("set_endgame_active", new Action(SetEndGameActive));
        uConsole.RegisterCommand("set_endgame_day", new Action(SetEndGameDay));
        uConsole.RegisterCommand("set_day_length_change_rate", new Action(SetDayLengthChangeRate));
        uConsole.RegisterCommand("set_indoor/outdoor_temp", new Action(SetIndoorOutdoorTemp));
        uConsole.RegisterCommand("set_heat_retention", new Action(SetHeatRetention));
        uConsole.RegisterCommand("set_realistic_freezing", new Action(SetRealisticFreezing));
        uConsole.RegisterCommand("set_temp_affects_wildlife", new Action(SetTempAffectsWildlife));
        uConsole.RegisterCommand("set_wildlife_amount", new Action(SetWildlifeAmount));
        uConsole.RegisterCommand("set_wildlife_day", new Action(SetWildlifeDay));
        uConsole.RegisterCommand("set_fire_fuel_burntime", new Action(SetFireFuelBurntime));
        uConsole.RegisterCommand("set_lantern_fuel_burntime", new Action(SetLanternFuelBurntime));
        uConsole.RegisterCommand("rn_hypertime", new Action(HyperTime));
        uConsole.RegisterCommand("rn_fires", new Action(Fires));
        uConsole.RegisterCommand("rn_save", new Action(SaveGame));
    }
    public static void RnHelp()
    {
        Debug.Log("\nSee example commands below to see how current Relentless Night Settings can be changed.");
        Debug.Log("Enter one of these commands with your own value to update the setting and save the game:");
        Debug.Log("set_endgame_active true");
        Debug.Log("set_endgame_day 200");
        Debug.Log("set_day_length_change_rate 15");
        Debug.Log("set_indoor/outdoor_temp 50");
        Debug.Log("set_heat_retention true");
        Debug.Log("set_realistic_freezing true");
        Debug.Log("set_temp_affects_wildlife true");
        Debug.Log("set_wildlife_amount 10");
        Debug.Log("set_wildlife_day 200");
        Debug.Log("set_fire_fuel_burntime 1.5");
        Debug.Log("set_lantern_fuel_burntime 1.5");
    }
    public static void SetEndGameActive()
    {
        bool par = uConsole.GetBool();
        if (uConsole.GetNumParameters() != 1 ||  par == RnGl.glEndgameActive) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glEndgameActive = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
    public static void SetEndGameDay()
    {
        int par = uConsole.GetInt();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glEndgameDay || par < 0 || par > 500) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glEndgameDay = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
    public static void SetDayLengthChangeRate()
    {
        int par = uConsole.GetInt();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glRotationDecline || par < 0 || par > 100) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glRotationDecline = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
    public static void SetIndoorOutdoorTemp()
    {
        int par = uConsole.GetInt();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glTemperatureEffect || par < 0 || par > 100) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glTemperatureEffect = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
    public static void SetHeatRetention()
    {
        bool par = uConsole.GetBool();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glHeatRetention) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glHeatRetention = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }    
    public static void SetRealisticFreezing()
    {
        bool par = uConsole.GetBool();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glRealisticFreezing) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glRealisticFreezing = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
    public static void SetTempAffectsWildlife()
    {
        bool par = uConsole.GetBool();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glWildlifeFreezing) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glWildlifeFreezing = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
    public static void SetWildlifeAmount()
    {
        int par = uConsole.GetInt();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glMinWildlifeAmount || par < 0 || par > 100) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glMinWildlifeAmount = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
    public static void SetWildlifeDay()
    {
        int par = uConsole.GetInt();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glMinWildlifeDay || par < 0 || par > 500) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glMinWildlifeDay = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
    public static void SetFireFuelBurntime()
    {
        float par = uConsole.GetFloat();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glFireFuelFactor || par < 0f || par > 3f) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glFireFuelFactor = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
    public static void SetLanternFuelBurntime()
    {
        float par = uConsole.GetFloat();
        if (uConsole.GetNumParameters() != 1 || par == RnGl.glLanternFuelFactor || par < 0f || par > 3f) { Debug.Log("Invalid input for setting"); return; }
        RnGl.glLanternFuelFactor = par; GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }

    public static void HyperTime() 
    {
        if (RnGl.rnTimeAccel != 7200) RnGl.rnTimeAccel = 7200;
        else RnGl.rnTimeAccel = 1;
    }
    public static void Fires()
    {
        IL2CPP.List<HeatSource> heatsources = GameManager.GetHeatSourceManagerComponent().m_HeatSources;
        for (int i = 0; i < heatsources.Count; i++)
        {
            Debug.Log("Fire" + i.ToString() + ": " + heatsources[i].GetTempIncrease(GameManager.GetPlayerTransform().position).ToString());
        }
    }
    public static void SaveGame()
    {
        GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
    }
}

[HarmonyPatch(typeof(HUDManager), "UpdateDebugLines", null)]
public class HUDManager_UpdateDebugLines_Pos
{
    private static void Postfix()
    {
        //Debug.Log("HUDManager_UpdateDebugLines_Pos");
        if (!RnGl.rnActive || HUDManager.m_HudDisplayMode != HudDisplayMode.DebugInfo) return;
       
        UILabel label_DebugLines = InterfaceManager.m_Panel_HUD.m_Label_DebugLines;
        label_DebugLines.text += "\n\nRN Diagnostics: " + RnGl.glEndgameActive + " " + RnGl.glEndgameDay + " " + RnGl.glRotationDecline + " " +
            RnGl.glTemperatureEffect + " " + RnGl.glHeatRetention + " " + RnGl.glRealisticFreezing + " " + RnGl.glWildlifeFreezing + " " +
            RnGl.glMinWildlifeAmount + " " + RnGl.glMinWildlifeDay + " " + RnGl.glFireFuelFactor + " " + RnGl.glLanternFuelFactor + " " +
            RnGl.glDayTidallyLocked + " " + RnGl.glDayNum + " " + RnGl.glLastOutdoorTempNoBliz + " " + RnGl.rnFireShouldHeatWholeScene + " " +
            RnGl.rnIndoorTempFactor + " " + RnGl.rnHeatDissapationFactor;      
    }
}