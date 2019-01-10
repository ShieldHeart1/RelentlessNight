using System;
using Harmony;
using RelentlessNight; 
using UnityEngine;

[HarmonyPatch(typeof(SpawnRegion), "Start", new Type[] {})]
internal static class SpawnRegion_Start_Pre
{
    private static void Prefix(SpawnRegion __instance)
    {
        //Debug.Log("SpawnRegion_Start_Pre");
        if (!RnGl.rnActive || GameManager.IsStoryMode()) return;

        bool m_StartHasBeenCalled = (bool)AccessTools.Field(typeof(SpawnRegion), "m_StartHasBeenCalled").GetValue(__instance);
        if (m_StartHasBeenCalled) return;
        
        int glDayNum = RnGl.glDayNum;
        float glLastOutdoorTempNoBliz = RnGl.glLastOutdoorTempNoBliz;
        float curDayFactor = 1f;
        float curTempFactor = 1f;
        int minPopulation = 1;
     
        if (RnGl.glMinWildlifeAmount < 100)
        {
            curDayFactor = 1f - Mathf.Clamp((float)glDayNum / (float)RnGl.glMinWildlifeDay, 0f, 0.98f) * ((float)(100 - RnGl.glMinWildlifeAmount) * 0.01f);
        }
        if (RnGl.glWildlifeFreezing)
        {
            curTempFactor = 1f + Mathf.Clamp((RnGl.glLastOutdoorTempNoBliz + 30f) * 0.01f, -0.75f, 0.5f);
        }       
        if (RnGl.glMinWildlifeAmount == 0)
        {
            minPopulation = 0;
        }

        __instance.m_ChanceActive *= curDayFactor * curTempFactor;
        __instance.m_MaxRespawnsPerDayPilgrim *= curDayFactor * curTempFactor;
        __instance.m_MaxRespawnsPerDayVoyageur *= curDayFactor * curTempFactor;
        __instance.m_MaxRespawnsPerDayStalker *= curDayFactor * curTempFactor;
        __instance.m_MaxRespawnsPerDayInterloper *= curDayFactor * curTempFactor;
        __instance.m_MaxSimultaneousSpawnsDayPilgrim = Math.Max(minPopulation, Mathf.RoundToInt((float)__instance.m_MaxSimultaneousSpawnsDayPilgrim * curDayFactor * curTempFactor));
        __instance.m_MaxSimultaneousSpawnsDayVoyageur = Math.Max(minPopulation, Mathf.RoundToInt((float)__instance.m_MaxSimultaneousSpawnsDayVoyageur * curDayFactor * curTempFactor));
        __instance.m_MaxSimultaneousSpawnsDayStalker = Math.Max(minPopulation, Mathf.RoundToInt((float)__instance.m_MaxSimultaneousSpawnsDayStalker * curDayFactor * curTempFactor));
        __instance.m_MaxSimultaneousSpawnsDayInterloper = Math.Max(minPopulation, Mathf.RoundToInt((float)__instance.m_MaxSimultaneousSpawnsDayInterloper * curDayFactor * curTempFactor));
        __instance.m_MaxSimultaneousSpawnsNightPilgrim = Math.Max(minPopulation, Mathf.RoundToInt((float)__instance.m_MaxSimultaneousSpawnsNightPilgrim * curDayFactor * curTempFactor));
        __instance.m_MaxSimultaneousSpawnsNightVoyageur = Math.Max(minPopulation, Mathf.RoundToInt((float)__instance.m_MaxSimultaneousSpawnsNightVoyageur * curDayFactor * curTempFactor));
        __instance.m_MaxSimultaneousSpawnsNightStalker = Math.Max(minPopulation, Mathf.RoundToInt((float)__instance.m_MaxSimultaneousSpawnsNightStalker * curDayFactor * curTempFactor));
        __instance.m_MaxSimultaneousSpawnsNightInterloper = Math.Max(minPopulation, Mathf.RoundToInt((float)__instance.m_MaxSimultaneousSpawnsNightInterloper * curDayFactor * curTempFactor));
        
    }
}
