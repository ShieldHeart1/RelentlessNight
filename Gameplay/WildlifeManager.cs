using System;
using Il2CppSystem.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace RelentlessNight
{
    internal class WildlifeManager
    {
        [HarmonyPatch(typeof(SpawnRegion), "Start", null)]
        internal static class SpawnRegion_Start
        {
            private static void Postfix(SpawnRegion __instance)
            {
                if (!MenuManager.modEnabled || Global.minWildlifePercent == 100) return;

                float wildlifePopulationMultiplier = CalculateWildlifePopulationMultiplier();

                __instance.m_ChanceActive *= wildlifePopulationMultiplier;

                __instance.m_MaxRespawnsPerDayPilgrim *= wildlifePopulationMultiplier;
                __instance.m_MaxRespawnsPerDayVoyageur *= wildlifePopulationMultiplier;
                __instance.m_MaxRespawnsPerDayStalker *= wildlifePopulationMultiplier;
                __instance.m_MaxRespawnsPerDayInterloper *= wildlifePopulationMultiplier;

                __instance.m_MaxSimultaneousSpawnsDayPilgrim = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsDayPilgrim * wildlifePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsDayVoyageur = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsDayVoyageur * wildlifePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsDayStalker = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsDayStalker * wildlifePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsDayInterloper = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsDayInterloper * wildlifePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsNightPilgrim = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsNightPilgrim * wildlifePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsNightVoyageur = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsNightVoyageur * wildlifePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsNightStalker = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsNightStalker * wildlifePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsNightInterloper = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsNightInterloper * wildlifePopulationMultiplier));
            }
        }
        [HarmonyPatch(typeof(SnareItem), "Start", null)]
        internal static class SnareItem_Start
        {
            private static void Postfix(SnareItem __instance)
            {
                if (!MenuManager.modEnabled || Global.minWildlifePercent == 100) return;

                __instance.m_ChanceSpawnCarcassOnRoll *= CalculateWildlifePopulationMultiplier();
            }
        }
        [HarmonyPatch(typeof(IceFishingHole), "Start", null)]
        internal static class IceFishingHole_Start
        { 
            private static void Postfix(IceFishingHole __instance)
            {
                if (!MenuManager.modEnabled || Global.minFishPercent == 100) return;

                float fishingCatchTimeRange = __instance.m_MaxGameMinutesCatchFish - __instance.m_MinGameMinutesCatchFish;

                __instance.m_MaxGameMinutesCatchFish += fishingCatchTimeRange * CalculateFishingTimeMultiplier();
            }
        }

        internal static float CalculateWildlifePopulationMultiplier()
        {
            float minWildLifeNormalized = Global.minWildlifePercent / 100f;
            float progressToMinWildlife = Mathf.Clamp((GameManager.GetTimeOfDayComponent().GetDayNumber()) / Global.minWildlifeDay, 0f, 1f);
            return Mathf.Lerp(1f, minWildLifeNormalized, progressToMinWildlife);
        }
        internal static float CalculateFishingTimeMultiplier()
        {
            float maxfisingTimeMultiplier =  100f / (Global.minFishPercent + float.Epsilon);
            float progressToMaxFishingTime = Mathf.Clamp((GameManager.GetTimeOfDayComponent().GetDayNumber()) / Global.minFishDay, 0f, 1f);

            return 1f + Mathf.Lerp(1f, maxfisingTimeMultiplier, progressToMaxFishingTime);
        }
        internal static void ResetWildlifePopulations()
        {
            List<SpawnRegion> spawnRegions = GameManager.GetSpawnRegionManager().m_SpawnRegions;
            foreach(SpawnRegion spawnRegion in spawnRegions)
            {
                spawnRegion.Start();
            }
            List<IceFishingHole> iceFishingHoles = IceFishingHole.m_IceFishingHoles;
            foreach (IceFishingHole iceFishingHole in iceFishingHoles)
            {
                iceFishingHole.Start();
            }
        }
    }
}