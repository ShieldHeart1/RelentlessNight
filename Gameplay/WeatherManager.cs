using HarmonyLib;

namespace RelentlessNight
{
    internal class WeatherManager
    {
        internal static int denseFogEliminationThreshold = 1000;
        internal static int lightFogEliminationThreshold = 2000;
        internal static int windSpeedIncreaseThreshhold = 1000;

        [HarmonyPatch(typeof(WeatherTransition), "ChooseNextWeatherSet", null)]
        internal class WeatherTransition_ChooseNextWeatherSet
        {
            private static void Prefix(WeatherTransition __instance)
            {
                if (!MenuManager.modEnabled) return;

                int dayNumber = GameManager.GetTimeOfDayComponent().GetDayNumber();
                WeatherSet m_CurrentWeatherSet = __instance.m_CurrentWeatherSet;
                MaybeEliminateDenseFog(m_CurrentWeatherSet, dayNumber);
                MaybeEliminateLightFog(m_CurrentWeatherSet, dayNumber);
            }
        }
        [HarmonyPatch(typeof(Wind), "StartRandomPhase", null)]
        internal static class Wind_StartRandomPhase
        {
            private static bool Prefix(Wind __instance, bool forceCalm)
            {
                if (!MenuManager.modEnabled) return true;

                if (Global.worldSpinDeclinePercent * GameManager.GetTimeOfDayComponent().GetDayNumber() < windSpeedIncreaseThreshhold)
                {
                    return true;
                }
                IncreaseWindStrength(__instance, forceCalm);
                return false;
            }
        }
        [HarmonyPatch(typeof(WeatherTransition), "ActivateWeatherSetAtFrac", null)]
        internal static class WeatherTransition_ActivateWeatherSetAtFrac
        {
            private static void Prefix(ref WeatherStage reqType)
            {
                if (!MenuManager.modEnabled || !ShouldSetPermanentAuroraForEndGame()) return;

                reqType = WeatherStage.ClearAurora;
            }
        }

        internal static void MaybeEliminateDenseFog(WeatherSet currentWeatherSet, int dayNumber)
        {
            if (Global.worldSpinDeclinePercent * dayNumber > denseFogEliminationThreshold)
            {
                currentWeatherSet.m_DenseFogAsNextSelectionWeight = 0;
            }
        }
        internal static void MaybeEliminateLightFog(WeatherSet currentWeatherSet, int dayNumber)
        {
            if (Global.worldSpinDeclinePercent * dayNumber > lightFogEliminationThreshold)
            {
                currentWeatherSet.m_LightFogAsNextSelectionWeight = 0;
            }
        }
        internal static bool ShouldSetPermanentAuroraForEndGame()
        {
            if (TimeManager.GameInEndgame() && Global.endgameAuroraEnabled)
            {
                return true;
            }
            return false;
        }
        internal static void IncreaseWindStrength(Wind __instance, bool forceCalm)
        {
            WindStrength strength;

            if (GameManager.GetWeatherComponent().IsBlizzard()) strength = WindStrength.Blizzard;
            else strength = (WindStrength)UnityEngine.Random.Range(1, 4);

            if (forceCalm) __instance.StartPhase(WindStrength.Calm, -1f);
            else __instance.StartPhase(strength, -1f);
        }
    }
}