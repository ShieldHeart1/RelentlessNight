using Harmony;
using UnityEngine;
using RelentlessNight;

[HarmonyPatch(typeof(FuelSourceItem), "GetModifiedBurnDurationHours", null)]
internal static class FuelSourceItem_GetModifiedBurnDurationHours_Pos
{
    private static void Postfix(ref float __result)
    {
        //Debug.Log("FuelSourceItem_GetModifiedBurnDurationHours_Pos");
        if (!RnGl.rnActive) return;
        
        __result *= RnGl.glFireFuelFactor;
    }
}

[HarmonyPatch(typeof(KeroseneLampItem), "GetModifiedFuelBurnLitersPerHour", null)] //May Not Work
internal static class KeroseneLampItem_GetModifiedFuelBurnLitersPerHour_Pos
{
    private static void Postfix(ref float __result)
    {
        //Debug.Log("KeroseneLampItem_GetModifiedFuelBurnLitersPerHour_Pos");
        if (!RnGl.rnActive) return;
      
        __result /= RnGl.glLanternFuelFactor;       
    }
}