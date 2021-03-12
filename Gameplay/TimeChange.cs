using Harmony;
using RelentlessNight;
using UnityEngine;

namespace RelentlessNight
{
    public class TimeChange
    {
        [HarmonyPatch(typeof(Feat_EfficientMachine), "IncrementElapsedHours", null)]
        internal static class Feat_EfficientMachine_IncrementElapsedHours_Pre
        {
            private static void Prefix(Feat_EfficientMachine __instance, ref float hours)
            {
                if (!RnGlobal.rnActive) return;

                if (GameManager.m_IsPaused) return;

                hours = RnGlobal.rnHours;
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnNewSandbox", null)]
        internal class Panel_MainMenu_OnNewSandbox_Pre
        {
            private static void Prefix(Panel_MainMenu __instance)
            {
                if (!RnGlobal.rnActive) return;

                RnGlobal.glDayTidallyLocked = -1;
                RnGlobal.glDayNum = 1;
                RnGlobal.glIsCarryingCarcass = false;

                HeatRetention.UpdateHeatRetentionFactors();                
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "TeleportPlayerAfterSceneLoad", null)]
        internal static class PlayerManager_TeleportPlayerAfterSceneLoad_Post
        {
            private static void Postfix()
            {
                if (!RnGlobal.rnActive) return;

                if (RnGlobal.glEndgameActive && RnGlobal.glEndgameDay == 0) GameManager.GetTimeOfDayComponent().SetNormalizedTime(0f);
            }
        }

        [HarmonyPatch(typeof(UniStormWeatherSystem), "SetNormalizedTime", new[] { typeof(float), typeof(bool) })]
        internal static class UniStormWeatherSystem_SetNormalizedTime_Pre
        {
            private static bool Prefix(UniStormWeatherSystem __instance, ref float time)
            {
                if (!RnGlobal.rnActive) return true;

                float m_DeltaTime = __instance.m_DeltaTime;
                RnGlobal.rnHours = m_DeltaTime / Mathf.Clamp((7200f * __instance.m_DayLengthScale / RnGlobal.rnTimeAccel) * GameManager.GetExperienceModeManagerComponent().GetTimeOfDayScale(), 1f, float.PositiveInfinity) * 24f;

                if (RnGlobal.glEndgameActive && __instance.m_DayCounter >= RnGlobal.glEndgameDay && GameManager.GetTimeOfDayComponent().IsNight())
                {
                    if (RnGlobal.glDayTidallyLocked == -1) RnGlobal.glDayTidallyLocked = __instance.m_DayCounter;
                    time = __instance.m_NormalizedTime;
                }
                return true;
            }
        }

        [HarmonyPatch(typeof(TimeWidget), "Update", null)]
        internal static class TimeWidget_Start_Post
        {
            private static void Postfix(TimeWidget __instance)
            {
                if (RnGlobal.rnActive && RnGlobal.glEndgameActive && RnGlobal.glDayTidallyLocked != -1 && RnGlobal.glDayTidallyLocked > Settings.options.coEndgameDay)
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
                if (!RnGlobal.rnActive || GameManager.m_IsPaused || !__instance.m_MainCamera) return;

                timeDeltaHours = RnGlobal.rnHours;
            }
        }

        [HarmonyPatch(typeof(UniStormWeatherSystem), "Update", null)]
        internal static class UniStormWeatherSystem_Update_Pre
        {
            private static void Prefix(UniStormWeatherSystem __instance)
            {
                if (!RnGlobal.rnActive || GameManager.m_IsPaused || !__instance.m_MainCamera) return;

                float dayLengthFactor = (float)RnGlobal.glRotationDecline * 72f;
                float curDay = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() / 24f;
                __instance.m_DayLength = ((7200f + curDay * dayLengthFactor) / RnGlobal.rnTimeAccel) * GameManager.GetExperienceModeManagerComponent().GetTimeOfDayScale();

                RnGlobal.rnElapsedHoursAccumulator = __instance.m_ElapsedHoursAccumulator;
                RnGlobal.rnElapsedHours = __instance.m_ElapsedHours;
            }
        }

        [HarmonyPatch(typeof(UniStormWeatherSystem), "Update", null)]
        internal static class UniStormWeatherSystem_Update_Post
        {
            private static void Postfix(UniStormWeatherSystem __instance)
            {
                if (!RnGlobal.rnActive || GameManager.m_IsPaused || !__instance.m_MainCamera) return;

                RnGlobal.rnElapsedHoursAccumulator += RnGlobal.rnHours;

                if (RnGlobal.rnElapsedHoursAccumulator > 0.5f)
                {
                    RnGlobal.rnElapsedHours += RnGlobal.rnElapsedHoursAccumulator;
                    RnGlobal.rnElapsedHoursAccumulator = 0f;
                    __instance.SetMoonPhase();
                }

                __instance.m_ElapsedHoursAccumulator = RnGlobal.rnElapsedHoursAccumulator;
                __instance.m_ElapsedHours = RnGlobal.rnElapsedHours;

                __instance.m_DayCounter = 1 + (int)(GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() / 24f);
                RnGlobal.glDayNum = __instance.m_DayCounter;
                __instance.m_DayLength = (7200f / RnGlobal.rnTimeAccel) * GameManager.GetExperienceModeManagerComponent().GetTimeOfDayScale();
            }
        }
    }
}