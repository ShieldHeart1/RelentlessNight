using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class NightChange
    {
        public static bool StartDetailSurveyRewrite(CharcoalItem __instance)
        {
            if (__instance.m_IsActive) return false;

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

            __instance.m_IsActive = true;

            CharcoalItem.m_CharcoalItemInUseForSurvey = __instance;

            __instance.m_TimeSpentSurveying = 0f;
            __instance.AccelerateTimeOfDay();
            __instance.m_SurveyAudioID = GameAudioManager.PlaySound(__instance.m_SurveyLoopAudio, InterfaceManager.GetSoundEmitter());

            return false;
        }

        [HarmonyPatch(typeof(CharcoalItem), "StartDetailSurvey", null)]
        internal static class CharcoalItem_StartDetailSurvey_Pre
        {
            private static bool Prefix(CharcoalItem __instance)
            {
                if (!RnGl.rnActive) return true;

                return StartDetailSurveyRewrite(__instance);
            }
        }

        [HarmonyPatch(typeof(InteriorLightingGroup), "ScrubUpdate", null)]
        internal static class InteriorLightingGroup_ScrubUpdate_Pre
        {
            private static void Prefix(InteriorLightingGroup __instance, ref float intensityMultiplier)
            {
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
                if (!RnGl.rnActive || GameManager.m_IsPaused) return;

                float m_AuroraFade = __instance.m_AuroraFade;
                float m_ScrubTimer = __instance.m_ScrubTimer;

                if (GameManager.GetTimeOfDayComponent().IsNight())
                {
                    if (GameManager.GetWeatherComponent().IsClear())
                    {
                        int moonPhaseIndex = GameManager.GetTimeOfDayComponent().m_WeatherSystem.GetMoonPhaseIndex();
                        if (moonPhaseIndex >= 3 && moonPhaseIndex <= 5)
                        {
                            __instance.UpdateLights(m_ScrubTimer, 0f, 0.25f, m_AuroraFade);
                        }
                        else
                        {
                            float intensity = GameManager.GetUniStorm().m_MoonLight.intensity;

                            __instance.UpdateLights(m_ScrubTimer, 0f, Mathf.Lerp(0.02f, 0.1f, intensity), m_AuroraFade);
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
                if (!RnGl.rnActive) return true;

                int moonPhaseIndex = GameManager.GetTimeOfDayComponent().m_WeatherSystem.GetMoonPhaseIndex();
                bool enoughMoonlightForAction = GameManager.GetTimeOfDayComponent().IsNight() && __instance.IsClear() && moonPhaseIndex >= 3 && moonPhaseIndex <= 5;
                return !enoughMoonlightForAction;
            }
        }
    }
}





