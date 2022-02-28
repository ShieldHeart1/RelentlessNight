using HarmonyLib;
using UnityEngine;

namespace RelentlessNight
{
    internal class TimeManager
    {
        // solarHours represents time in terms of position to the sun
        internal static float solarHours = 0f;
        internal static float ticksInNormalDay = 7200f;
        internal static float elapsedHoursAccumulator = 0f;
        internal static float elapsedHours = 0f;

        [HarmonyPatch(typeof(PlayerManager), "TeleportPlayerAfterSceneLoad", null)]
        internal static class PlayerManager_TeleportPlayerAfterSceneLoad
        {
            private static void Postfix()
            {
                if (!MenuManager.modEnabled) return;

                if (GameStartedAtEndgame())
                {
                    SetTimeToMidnight();
                    if (Global.endgameAuroraEnabled && GameManager.GetWeatherComponent().m_CurrentWeatherStage != WeatherStage.ClearAurora)
                    {
                        GameManager.GetWeatherTransitionComponent().ActivateWeatherSet(WeatherStage.ClearAurora);
                    }
                }
            }
        }
        [HarmonyPatch(typeof(Feat_EfficientMachine), "IncrementElapsedHours", null)]
        internal static class Feat_EfficientMachine_IncrementElapsedHours
        {
            private static void Prefix(ref float hours)
            {
                if (!MenuManager.modEnabled || GameManager.m_IsPaused) return;

                hours = solarHours;
            }
        }
        [HarmonyPatch(typeof(UniStormWeatherSystem), "SetNormalizedTime", new[] { typeof(float), typeof(bool) })]
        internal static class UniStormWeatherSystem_SetNormalizedTime
        {
            private static bool Prefix(UniStormWeatherSystem __instance, ref float time)
            {
                if (!MenuManager.modEnabled) return true;

                solarHours = __instance.m_DeltaTime / Mathf.Clamp((7200f * __instance.m_DayLengthScale / Utilities.devTimeSpeedMultiplier) * GameManager.GetExperienceModeManagerComponent().GetTimeOfDayScale(), 1f, float.PositiveInfinity) * 24f;
                if (GameInEndgame())
                {
                    time = __instance.m_NormalizedTime;

                    if (Global.dayTidalLocked == -1)
                    {
                        Global.dayTidalLocked = GameManager.GetTimeOfDayComponent().GetDayNumber();
                    }
                }
                else
                {
                    if (Global.dayTidalLocked != -1)
                    {
                        Global.dayTidalLocked = -1;
                    }
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(TimeWidget), "Update", null)]
        internal static class TimeWidget_Update
        {
            private static void Postfix(TimeWidget __instance)
            {
                if (!MenuManager.modEnabled) return;

                MaybeChangeMoonColorInTimeWidget(__instance);
            }
        }
        [HarmonyPatch(typeof(UniStormWeatherSystem), "UpdateTimeStats", null)]
        internal static class UniStormWeatherSystem_UpdateTimeStats
        {
            private static void Prefix(UniStormWeatherSystem __instance, ref float timeDeltaHours)
            {
                if (!MenuManager.modEnabled || GameManager.m_IsPaused || !__instance.m_MainCamera) return;

                timeDeltaHours = solarHours;
            }
        }
        [HarmonyPatch(typeof(UniStormWeatherSystem), "Update", null)]
        internal static class UniStormWeatherSystem_Update
        {
            private static void Prefix(UniStormWeatherSystem __instance)
            {
                if (!MenuManager.modEnabled || GameManager.m_IsPaused || !__instance.m_MainCamera) return;

                __instance.m_DayLength = GetCurrentRnDayLength();
                CaptureVanillaHours(__instance.m_ElapsedHoursAccumulator, __instance.m_ElapsedHours);
            }
            private static void Postfix(UniStormWeatherSystem __instance)
            {
                if (!MenuManager.modEnabled || GameManager.m_IsPaused || !__instance.m_MainCamera) return;

                elapsedHoursAccumulator += solarHours;
                if (elapsedHoursAccumulator > 0.5f)
                {
                    elapsedHours += elapsedHoursAccumulator;
                    elapsedHoursAccumulator = 0f;

                    // Ensures moon phase is still set at approperiate times.
                    __instance.SetMoonPhase();
                }
                SetHoursBackToRealTime(__instance);
                SetDaysBackToRealTime(__instance);
            }
        }
        internal static float GetCurrentRnDayLength()
        {
            float currentDay = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() / 24f;
            float ticksAddedPerDay = Global.worldSpinDeclinePercent * 72f;

            return ((ticksInNormalDay + currentDay * ticksAddedPerDay) / Utilities.devTimeSpeedMultiplier) * GameManager.GetExperienceModeManagerComponent().GetTimeOfDayScale();
        }
        internal static void SetTimeToMidnight()
        {
            GameManager.GetTimeOfDayComponent().SetNormalizedTime(0f);
        }
        internal static bool GameInEndgame()
        {
            return Global.endgameEnabled && GameManager.GetTimeOfDayComponent().IsNight() && GameManager.GetTimeOfDayComponent().GetDayNumber() >= Global.endgameDay;
        }
        internal static bool GameStartedAtEndgame()
        {
            return Global.endgameEnabled && Global.endgameDay == 1;
        }
        // Indicates to player that they are now in the endgame through viewing the time widget, white is the default game color
        internal static void MaybeChangeMoonColorInTimeWidget(TimeWidget __instance)
        {
            if (GameInEndgame())
            {
                __instance.m_MoonSprite.color = Color.blue;
            }
            else
            {
                __instance.m_MoonSprite.color = Color.white;
            }
        }
        internal static void CaptureVanillaHours(float m_ElapsedHoursAccumulator, float m_ElapsedHours)
        {
            elapsedHoursAccumulator = m_ElapsedHoursAccumulator;
            elapsedHours = m_ElapsedHours;
        }
        internal static void SetHoursBackToRealTime(UniStormWeatherSystem __instance)
        {
            __instance.m_ElapsedHoursAccumulator = elapsedHoursAccumulator;
            __instance.m_ElapsedHours = elapsedHours;
        }
        internal static void SetDaysBackToRealTime(UniStormWeatherSystem __instance)
        {
            __instance.m_DayCounter = 1 + (int)(GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() / 24f);
            __instance.m_DayLength = (7200f / Utilities.devTimeSpeedMultiplier) * GameManager.GetExperienceModeManagerComponent().GetTimeOfDayScale();
        }
    }
}