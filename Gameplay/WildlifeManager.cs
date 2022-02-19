using System;
using HarmonyLib;
using UnityEngine;

namespace RelentlessNight
{
    internal class WildlifeManager
    {
        [HarmonyPatch(typeof(SpawnRegion), "Start", new Type[] { })]
        internal static class SpawnRegion_Start
        {
            private static void Postfix(SpawnRegion __instance)
            {
                if (!MenuManager.modEnabled || Global.minWildlifePercent == 100) return;

                float minWildLifeNormalized = Global.minWildlifePercent / 100f;
                float progressToMinWildlife = Mathf.Clamp((GameManager.GetTimeOfDayComponent().GetDayNumber() - 1f) / Global.minWildlifeDay, 0f, 1f);
                float wildlifeDeclinePopulationMultiplier = Mathf.Lerp(1f, minWildLifeNormalized, progressToMinWildlife);

                __instance.m_ChanceActive *= wildlifeDeclinePopulationMultiplier;
                __instance.m_MaxRespawnsPerDayPilgrim *= wildlifeDeclinePopulationMultiplier;
                __instance.m_MaxRespawnsPerDayVoyageur *= wildlifeDeclinePopulationMultiplier;
                __instance.m_MaxRespawnsPerDayStalker *= wildlifeDeclinePopulationMultiplier;
                __instance.m_MaxRespawnsPerDayInterloper *= wildlifeDeclinePopulationMultiplier;
                __instance.m_MaxSimultaneousSpawnsDayPilgrim = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsDayPilgrim * wildlifeDeclinePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsDayVoyageur = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsDayVoyageur * wildlifeDeclinePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsDayStalker = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsDayStalker * wildlifeDeclinePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsDayInterloper = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsDayInterloper * wildlifeDeclinePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsNightPilgrim = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsNightPilgrim * wildlifeDeclinePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsNightVoyageur = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsNightVoyageur * wildlifeDeclinePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsNightStalker = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsNightStalker * wildlifeDeclinePopulationMultiplier));
                __instance.m_MaxSimultaneousSpawnsNightInterloper = Math.Max(0, Mathf.RoundToInt(__instance.m_MaxSimultaneousSpawnsNightInterloper * wildlifeDeclinePopulationMultiplier));
            }
        }
    }
}