using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class BurnTimes
    {
        [HarmonyPatch(typeof(FuelSourceItem), "GetModifiedBurnDurationHours", null)]
        internal static class FuelSourceItem_GetModifiedBurnDurationHours_Pos
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGl.rnActive) return;

                __result *= RnGl.glFireFuelFactor;
            }
        }

        [HarmonyPatch(typeof(KeroseneLampItem), "GetModifiedFuelBurnLitersPerHour", null)]
        internal static class KeroseneLampItem_GetModifiedFuelBurnLitersPerHour_Pos
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGl.rnActive) return;

                __result /= RnGl.glLanternFuelFactor;
            }
        }
    }
}
