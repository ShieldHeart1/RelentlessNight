using HarmonyLib;
using UnityEngine;

namespace RelentlessNight
{
    internal class NightActivityManager
    {
        [HarmonyPatch(typeof(Weather), "IsTooDarkForAction", null)]
        internal static class Weather_IsTooDarkForAction
        {
            private static bool Prefix()
            {
                if (!MenuManager.modEnabled || !WeatherAllowsNightActivity()) return true;
                return false;
            }
        }
        // Inlined
        [HarmonyPatch(typeof(CharcoalItem), "StartDetailSurvey", null)]
        internal static class CharcoalItem_StartDetailSurvey
        {
            private static bool Prefix(CharcoalItem __instance)
            {
                if (!MenuManager.modEnabled || !WeatherAllowsNightActivity()) return true;

                CharcoalItem_StartDetailSurvey_Rewrite(__instance);
                return false;
            }
        }
        // Ensures aurora-dependent indoor lights are active if in endgame and permemnant aurora is enabled
        [HarmonyPatch(typeof(AuroraManager), "UpdateVisibility", null)]
        internal static class AuroraManager_UpdateVisibility
        {
            private static void Postfix(AuroraManager __instance)
            {
                if (!MenuManager.modEnabled || !Global.endgameAuroraEnabled) return;

                if (TimeManager.GameInEndgame())
                {

                     __instance.m_BoostAuroraElectrolyzer = true;
                }
                else
                {
                    __instance.m_BoostAuroraElectrolyzer = false;
                }
            }
        }
        // Below two patches adjust the indoor light intensity during clear nights and bright moon phases
        [HarmonyPatch(typeof(InteriorLightingGroup), "ScrubUpdate", null)]
        internal static class InteriorLightingGroup_ScrubUpdate
        {
            private static void Prefix(ref float intensityMultiplier)
            {
                if (!MenuManager.modEnabled) return;

                if (WeatherAllowsNightActivity()) intensityMultiplier = 1f;
            }
        }
        [HarmonyPatch(typeof(InteriorLightingManager), "Update", null)]
        internal static class InteriorLightingManager_Update
        {
            private static void Postfix(InteriorLightingManager __instance)
            {
                if (!MenuManager.modEnabled || GameManager.m_IsPaused) return;

                if (GameManager.GetTimeOfDayComponent().IsNight() && GameManager.GetWeatherComponent().IsClear())
                {
                    if (WeatherAllowsNightActivity()) UpdateLightsForNightActivities(__instance);
                    UpdateLightsRelativeToMoonBrightness(__instance);
                }
            }
        }

        internal static bool WeatherAllowsNightActivity()
        {
            int moonPhaseIndex = GameManager.GetTimeOfDayComponent().m_WeatherSystem.GetMoonPhaseIndex();
            return moonPhaseIndex >= 3 && moonPhaseIndex <= 5 && GameManager.GetTimeOfDayComponent().IsNight() && GameManager.GetWeatherComponent().IsClear();
        }
        internal static void CompleteSurveyAction(CharcoalItem __instance)
        {
            __instance.m_IsActive = true;
            CharcoalItem.m_CharcoalItemInUseForSurvey = __instance;
            __instance.m_TimeSpentSurveying = 0f;
            __instance.AccelerateTimeOfDay();
            __instance.m_SurveyAudioID = GameAudioManager.PlaySound(__instance.m_SurveyLoopAudio, InterfaceManager.GetSoundEmitter());
        }
        internal static void CharcoalItem_StartDetailSurvey_Rewrite(CharcoalItem __instance)
        {
            if (__instance.m_IsActive || !GameManager.GetPlayerAnimationComponent().CanTransitionToState(PlayerAnimation.State.Stowing)) return;

            if (GameManager.GetWeatherComponent().IsIndoorScene())
            {
                Utilities.DisallowActionWithGameMessage("GAMEPLAY_CannotMapLocalAreaIndoors");
                return;
            }
            if (GameManager.GetWeatherComponent().IsDenseFog() || GameManager.GetWeatherComponent().IsBlizzard() || (GameManager.GetTimeOfDayComponent().IsNight() && !GameManager.GetWeatherComponent().IsClear()))
            {
                Utilities.DisallowActionWithGameMessage("GAMEPLAY_DetailSurveyNoVisibility");
                return;
            }
            CompleteSurveyAction(__instance);
        }
        internal static void UpdateLightsForNightActivities(InteriorLightingManager __instance)
        {
            __instance.UpdateLights(__instance.m_ScrubTimer, 0f, 0.25f, __instance.m_AuroraFade);
        }
        internal static void UpdateLightsRelativeToMoonBrightness(InteriorLightingManager __instance)
        {
            __instance.UpdateLights(__instance.m_ScrubTimer, 0f, Mathf.Lerp(0.02f, 0.1f, GameManager.GetUniStorm().m_MoonLight.intensity), __instance.m_AuroraFade);
        }        
    }
}