using System;
using Harmony;
using RelentlessNight;
using UnityEngine;

[HarmonyPatch(typeof(Condition), "AddHealth", new Type[] { typeof(float), typeof(DamageSource), typeof(bool) })]
internal static class Condition_AddHealth_Pre
{
    private static void Prefix(Condition __instance, ref float hp, DamageSource cause)
    {
        //Debug.Log("Condition_AddHealth_Pre");
        if (!RnGl.rnActive || !RnGl.glRealisticFreezing) return;
            
        if (cause == DamageSource.Freezing)
        {
            float num = GameManager.GetFreezingComponent().CalculateBodyTemperature();           
            if (num < -40f)
            {               
                hp *= Mathf.Clamp(1f + Math.Abs(num + 40f) / 30f, 0f, 3f);
            }
        }       
    }
}