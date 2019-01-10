using Harmony;
using UnityEngine;
using RelentlessNight;

[HarmonyPatch(typeof(CabinFever), "Update", null)]
internal static class CabinFever_Update_Pre
{
    private static bool Prefix(CabinFever __instance)
    {
        //Debug.Log("CabinFever_Update_Pre");
        if (!RnGl.rnActive) return true;
       
        if (__instance.HasCabinFever())
        {
            __instance.CabinFeverEnd();
        }
        return false;
    }
}