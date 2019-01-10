using Harmony;
using UnityEngine;
using RelentlessNight;

[HarmonyPatch(typeof(CharcoalItem), "StartDetailSurvey", null)]
internal static class CharcoalItem_StartDetailSurvey_Pre
{
    private static bool Prefix(CharcoalItem __instance)
    {
        //Debug.Log("CharcoalItem_StartDetailSurvey_Pre");
        if (!RnGl.rnActive) return true;      
                
        if ((bool)AccessTools.Field(typeof(CharcoalItem), "m_IsActive").GetValue(__instance)) return false;

        if (!GameManager.GetPlayerAnimationComponent().CanTransitionToState(PlayerAnimation.State.Stowing)) return false;
           
        if (GameManager.GetWeatherComponent().IsIndoorScene())
        {
            GameAudioManager.PlayGUIError();
            HUDMessage.AddMessage(Localization.Get("GAMEPLAY_CannotMapLocalAreaIndoors"), false);
            return false;
        }
               
        if (GameManager.GetWeatherComponent().IsDenseFog() || GameManager.GetWeatherComponent().IsBlizzard() || 
            (GameManager.GetTimeOfDayComponent().IsNight() && !GameManager.GetWeatherComponent().IsClear()))
        {
            GameAudioManager.PlayGUIError();
            HUDMessage.AddMessage(Localization.Get("GAMEPLAY_DetailSurveyNoVisibility"), false);
            return false;
        }
                      
        AccessTools.Field(typeof(CharcoalItem), "m_IsActive").SetValue(__instance, true);
        CharcoalItem.m_CharcoalItemInUseForSurvey = __instance;
        AccessTools.Field(typeof(CharcoalItem), "m_TimeSpentSurveying").SetValue(__instance, 0f);
        AccessTools.Method(typeof(CharcoalItem), "AccelerateTimeOfDay", null, null).Invoke(__instance, null);
        AccessTools.Field(typeof(CharcoalItem), "m_SurveyAudioID").SetValue(__instance, GameAudioManager.PlaySound(__instance.m_SurveyLoopAudio, InterfaceManager.GetSoundEmitter()));
        return false;    
    }
}

[HarmonyPatch(typeof(InteriorLightingGroup), "ScrubUpdate", null)]
internal static class InteriorLightingGroup_ScrubUpdate_Pre
{
    private static void Prefix(InteriorLightingGroup __instance, ref float intensityMultiplier)
    {
        //Debug.Log("InteriorLightingGroup_ScrubUpdate_Pre");
        if (!RnGl.rnActive) return;

        int moonPhaseIndex = GameManager.GetTimeOfDayComponent().m_WeatherSystem.GetMoonPhaseIndex();
     
        if (GameManager.GetTimeOfDayComponent().IsNight() && GameManager.GetWeatherComponent().IsClear() && moonPhaseIndex >= 3 && moonPhaseIndex <= 5)
        {
            intensityMultiplier = 1f;
        }
    }
}

[HarmonyPatch(typeof(InteriorLightingManager), "Update", null)]
internal static class InteriorLightingManager_Update_Pos
{
    private static void Postfix(InteriorLightingManager __instance)
    {
        //Debug.Log("InteriorLightingManager_Update_Pos");
        if (!RnGl.rnActive || GameManager.m_IsPaused) return;
        
        float m_AuroraFade = (float)AccessTools.Field(typeof(InteriorLightingManager), "m_AuroraFade").GetValue(__instance);
        float m_ScrubTimer = (float)AccessTools.Field(typeof(InteriorLightingManager), "m_ScrubTimer").GetValue(__instance);  
     
        if (GameManager.GetTimeOfDayComponent().IsNight())
        {       
            if (GameManager.GetWeatherComponent().IsClear())
            {
                int moonPhaseIndex = GameManager.GetTimeOfDayComponent().m_WeatherSystem.GetMoonPhaseIndex();            
                if (moonPhaseIndex >= 3 && moonPhaseIndex <= 5)
                {
                    object[] parameters = new object[] {m_ScrubTimer, 0f, 0.25f, m_AuroraFade};
                    AccessTools.Method(typeof(InteriorLightingManager), "UpdateLights", null, null).Invoke(__instance, parameters);
                }
                else
                {
                    float intensity = GameManager.GetUniStorm().m_MoonLight.intensity;
                    object[] parameters2 = new object[] {m_ScrubTimer, 0f, Mathf.Lerp(0.02f, 0.1f, intensity), m_AuroraFade};
                    AccessTools.Method(typeof(InteriorLightingManager), "UpdateLights", null, null).Invoke(__instance, parameters2);
                }
            }
        }       
    }
}

[HarmonyPatch(typeof(Weather), "IsTooDarkForAction", null)]
internal static class Weather_IsTooDarkForAction_Pre
{
    private static bool Prefix(Weather __instance)
    {
        //Debug.Log("Weather_IsTooDarkForAction_Pre");
        if (!RnGl.rnActive) return true;
      
        int moonPhaseIndex = GameManager.GetTimeOfDayComponent().m_WeatherSystem.GetMoonPhaseIndex();
        bool enoughMoonlightForAction = GameManager.GetTimeOfDayComponent().IsNight() && __instance.IsClear() && moonPhaseIndex >= 3 && moonPhaseIndex <= 5;
        return !enoughMoonlightForAction;
    }
}