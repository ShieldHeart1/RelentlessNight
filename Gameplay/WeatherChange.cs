using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class WeatherChange
    {
        public static void MaybeEliminateDenseFog(WeatherSet currentWeatherSet, int dayNumber)
        {
            if (RnGlobal.glRotationDecline * dayNumber > 1000) currentWeatherSet.m_DenseFogAsNextSelectionWeight = 0;
        }

        public static void MaybeEliminateLightFog(WeatherSet currentWeatherSet, int dayNumber)
        {
            if (RnGlobal.glRotationDecline * dayNumber > 2000) currentWeatherSet.m_LightFogAsNextSelectionWeight = 0;
        }

        public static bool ShouldSetPermanentAuroraForEndGame()
        {
            if (RnGlobal.glEndgameActive && RnGlobal.glEndgameAurora) return true;

            return false;
        }

        [HarmonyPatch(typeof(WeatherTransition), "ChooseNextWeatherSet", null)]
        public class WeatherTransition_ChooseNextWeatherSet_Pre
        {
            public static void Prefix(WeatherTransition __instance)
            {
                if (!RnGlobal.rnActive) return;

                int dayNumber = GameManager.GetTimeOfDayComponent().GetDayNumber();
                WeatherSet m_CurrentWeatherSet = __instance.m_CurrentWeatherSet;

                MaybeEliminateDenseFog(m_CurrentWeatherSet, dayNumber);

                MaybeEliminateLightFog(m_CurrentWeatherSet, dayNumber);
            }
        }

        [HarmonyPatch(typeof(Wind), "StartRandomPhase", null)]
        internal static class Wind_StartRandomPhase_Pre
        {
            private static bool Prefix(Wind __instance, bool forceCalm)
            {
                if (!RnGlobal.rnActive) return true;

                int dayNumber = GameManager.GetTimeOfDayComponent().GetDayNumber();
                if (RnGlobal.glRotationDecline * dayNumber < 500) return true;

                WindStrength strength;
                if (GameManager.GetWeatherComponent().IsBlizzard())
                {
                    strength = WindStrength.Blizzard;
                }
                else
                {
                    strength = (WindStrength)UnityEngine.Random.Range(1, 4);
                }
                if (forceCalm)
                {
                    __instance.StartPhase(WindStrength.Calm, -1f);
                }
                else
                {
                    __instance.StartPhase(strength, -1f);
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(WeatherTransition), "ActivateWeatherSetAtFrac", null)]
        internal static class WeatherTransition_ActivateWeatherSetAtFrac_Pre
        {
            private static void Prefix(ref WeatherStage reqType)
            {
                if (!RnGlobal.rnActive || !ShouldSetPermanentAuroraForEndGame()) return;

                reqType = WeatherStage.ClearAurora;
            }
        }
    }
}