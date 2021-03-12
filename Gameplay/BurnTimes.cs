using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class BurnTimes
    {
        [HarmonyPatch(typeof(FuelSourceItem), "GetModifiedBurnDurationHours", null)]
        internal static class FuelSourceItem_GetModifiedBurnDurationHours_Post
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGlobal.rnActive) return;

                __result *= RnGlobal.glFireFuelFactor;
            }
        }

        [HarmonyPatch(typeof(KeroseneLampItem), "Awake", null)]
        internal static class KeroseneLampItem_Awake_Post
        {
            private static void Postfix(KeroseneLampItem __instance)
            {
                if (!RnGlobal.rnActive) return;

                __instance.m_FuelBurnLitersPerHour /= RnGlobal.glLanternFuelFactor;
            }
        }

        [HarmonyPatch(typeof(TorchItem), "Awake")]
        internal static class TorchItem_Awake_Post
        {
            private static void Postfix(TorchItem __instance)
            {
                if (!RnGlobal.rnActive) return;

                __instance.m_BurnLifetimeMinutes *= RnGlobal.glTorchFuelFactor;
            }
        }
    }
}
