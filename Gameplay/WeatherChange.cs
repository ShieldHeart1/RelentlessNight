using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class WeatherChange
    {
        [HarmonyPatch(typeof(WeatherTransition), "ChooseNextWeatherSet", null)]
        public class WeatherTransition_ChooseNextWeatherSet_Pre
        {
            public static void Prefix(WeatherTransition __instance)
            {
                if (!RnGl.rnActive) return;

                int dayNumber = GameManager.GetTimeOfDayComponent().GetDayNumber();
                WeatherSet m_CurrentWeatherSet = __instance.m_CurrentWeatherSet;

                if (RnGl.glRotationDecline * dayNumber > 250)
                {
                    m_CurrentWeatherSet.m_DenseFogAsNextSelectionWeight = 0;
                }
                if (RnGl.glRotationDecline * dayNumber > 500)
                {
                    m_CurrentWeatherSet.m_LightFogAsNextSelectionWeight = 0;
                }
            }
        }

        [HarmonyPatch(typeof(Wind), "StartRandomPhase", null)]
        internal static class Wind_StartRandomPhase_Pre
        {
            private static bool Prefix(Wind __instance, bool forceCalm)
            {
                if (!RnGl.rnActive) return true;

                int dayNumber = GameManager.GetTimeOfDayComponent().GetDayNumber();
                if (RnGl.glRotationDecline * dayNumber < 500) return true;

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
                if (!RnGl.rnActive || !RnGl.glEndgameActive || !RnGl.glEndgameAurora) return;

                reqType = WeatherStage.ClearAurora;
            }
        }
    }
}