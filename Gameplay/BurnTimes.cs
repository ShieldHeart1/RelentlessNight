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
                if (!RnGl.rnActive) return;

                __result *= RnGl.glFireFuelFactor;
            }
        }

        [HarmonyPatch(typeof(KeroseneLampItem), "Awake", null)]
        internal static class KeroseneLampItem_Awake_Post
        {
            private static void Postfix(KeroseneLampItem __instance)
            {
                if (!RnGl.rnActive) return;

                __instance.m_FuelBurnLitersPerHour /= RnGl.glLanternFuelFactor;
            }
        }

        [HarmonyPatch(typeof(TorchItem), "Awake")]
        internal static class TorchItem_Awake_Post
        {
            private static void Postfix(TorchItem __instance)
            {
                if (!RnGl.rnActive) return;

                __instance.m_BurnLifetimeMinutes *= RnGl.glTorchFuelFactor;
            }
        }
    }
}
