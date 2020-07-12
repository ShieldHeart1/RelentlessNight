using Harmony;
using RelentlessNight;
using UnityEngine;

[HarmonyPatch(typeof(Feat_EfficientMachine), "IncrementElapsedHours", null)]
internal static class Feat_EfficientMachine_IncrementElapsedHours_Pre
{
    private static void Prefix(Feat_EfficientMachine __instance, ref float hours)
    {
        //Debug.Log("Feat_EfficientMachine_IncrementElapsedHours_Pre");
        if (!RnGl.rnActive) return;

        if (GameManager.m_IsPaused) return;
          
        hours = RnGl.rnNormNum5;
    }
}

[HarmonyPatch(typeof(Panel_MainMenu), "OnNewSandbox", null)]
internal class Panel_MainMenu_OnNewSandbox_Pre
{
    private static void Prefix(Panel_MainMenu __instance) 
    {
        //Debug.Log("Panel_MainMenu_OnNewSandbox_Pre");
        if (!RnGl.rnActive) return;

        RnGl.glDayTidallyLocked = -1;
        RnGl.glDayNum = 1;
        RnGl.glLastOutdoorTempNoBliz = -10f;
        RnGl.rnIndoorTempFactor = 1f;
        RnGl.rnFireShouldHeatWholeScene = false;
        RnGl.rnHeatDissapationFactor = 15f;
        RnGl.glIsCarryingCarcass = false;
    }
}

[HarmonyPatch(typeof(PlayerManager), "TeleportPlayerAfterSceneLoad", null)]
internal static class PlayerManager_TeleportPlayerAfterSceneLoad_Pos
{
    private static void Postfix()
    {
        //Debug.Log("PlayerManager_TeleportPlayerAfterSceneLoad_Pos"); // Hands up, this is a Harmony patch method name list robbery! // god for it if it helps
        if (!RnGl.rnActive) return;

        if (RnGl.glEndgameActive && RnGl.glEndgameDay == 0) GameManager.GetTimeOfDayComponent().SetNormalizedTime(0f);       
    }
}

[HarmonyPatch(typeof(UniStormWeatherSystem), "SetNormalizedTime", new[] { typeof(float) })]
internal static class UniStormWeatherSystem_SetNormalizedTime_Pre
{
    private static bool Prefix(UniStormWeatherSystem __instance, ref float time)
    {
        //Debug.Log("UniStormWeatherSystem_SetNormalizedTime_Pre");
        if (!RnGl.rnActive) return true;

        float m_DeltaTime = __instance.m_DeltaTime;
        RnGl.rnNormNum5 = m_DeltaTime / Mathf.Clamp((7200f * __instance.m_DayLengthScale / RnGl.rnTimeAccel) * GameManager.GetExperienceModeManagerComponent().GetTimeOfDayScale(), 1f, float.PositiveInfinity) * 24f;

        if (RnGl.glEndgameActive && __instance.m_DayCounter >= RnGl.glEndgameDay && GameManager.GetTimeOfDayComponent().IsNight())
        {
            if (RnGl.glDayTidallyLocked == -1) RnGl.glDayTidallyLocked = __instance.m_DayCounter;
            time = __instance.m_NormalizedTime;
        }
        return true;
    }
}

[HarmonyPatch(typeof(TimeWidget), "Update", null)]
internal static class TimeWidget_Start_Pos
{
    private static void Postfix(TimeWidget __instance)
    {
        //Debug.Log("TimeWidget_Start_Pos");
        if (RnGl.rnActive && RnGl.glEndgameActive && RnGl.glDayTidallyLocked != -1)
        {
            __instance.m_MoonSprite.color = new Color(0.2f, 0.2f, 0.6f, 1f);
        }
        else
        {
            __instance.m_MoonSprite.color = Color.white;
        }
    }
}

[HarmonyPatch(typeof(UniStormWeatherSystem), "UpdateTimeStats", null)]
internal static class UniStormWeatherSystem_UpdateTimeStats_Pre
{
    private static void Prefix(UniStormWeatherSystem __instance, ref float timeDeltaHours)
    {
        //Debug.Log("UniStormWeatherSystem_UpdateTimeStats_Pre");
        if (!RnGl.rnActive || GameManager.m_IsPaused || !__instance.m_MainCamera) return; 
     
        timeDeltaHours = RnGl.rnNormNum5;       
    }
}

[HarmonyPatch(typeof(UniStormWeatherSystem), "Update", null)]
internal static class UniStormWeatherSystem_Update_Pre
{
    private static void Prefix(UniStormWeatherSystem __instance)
    {
        //Debug.Log("UniStormWeatherSystem_Update_Pre");
        if (!RnGl.rnActive || GameManager.m_IsPaused || !__instance.m_MainCamera) return;        

        float dayLengthFactor = (float)RnGl.glRotationDecline * 72f;
        float curDay = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() / 24f;
        __instance.m_DayLength = ((7200f + curDay * dayLengthFactor) / RnGl.rnTimeAccel) * GameManager.GetExperienceModeManagerComponent().GetTimeOfDayScale();

        RnGl.rnElapsedHoursAccumulator = __instance.m_ElapsedHoursAccumulator;
        RnGl.rnElapsedHours = __instance.m_ElapsedHours;            
    }
}

[HarmonyPatch(typeof(UniStormWeatherSystem), "Update", null)]
internal static class UniStormWeatherSystem_Update_Pos
{
    private static void Postfix(UniStormWeatherSystem __instance)
    {
        //Debug.Log("UniStormWeatherSystem_Update_Pos");
        if (!RnGl.rnActive || GameManager.m_IsPaused || !__instance.m_MainCamera) return;

        RnGl.rnElapsedHoursAccumulator += RnGl.rnNormNum5;

        if (RnGl.rnElapsedHoursAccumulator > 0.5f)
        {
            RnGl.rnElapsedHours += RnGl.rnElapsedHoursAccumulator;
            RnGl.rnElapsedHoursAccumulator = 0f;
            __instance.SetMoonPhase();
        }

        __instance.m_ElapsedHoursAccumulator = RnGl.rnElapsedHoursAccumulator;
        __instance.m_ElapsedHours = RnGl.rnElapsedHours;

        __instance.m_DayCounter = 1 + (int)(GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() / 24f);
        RnGl.glDayNum = __instance.m_DayCounter;
        __instance.m_DayLength = (7200f / RnGl.rnTimeAccel) * GameManager.GetExperienceModeManagerComponent().GetTimeOfDayScale();           
    }
}