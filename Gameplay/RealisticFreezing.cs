using System;
using HarmonyLib;
using UnityEngine;

namespace RelentlessNight
{
    internal class RealisticFreezing
    {
        internal const float damageRateIncreaseTemperature = -40f;
        internal const float addedDamageRatePerDegree = 1 / 30f;
        internal const int maximumDamageMultiplier = 3;

        [HarmonyPatch(typeof(Condition), "AddHealth", new Type[] { typeof(float), typeof(DamageSource), typeof(bool) })]
        internal class Condition_AddHealth
        {
            private static void Prefix(ref float hp, DamageSource cause)
            {
                if (!MenuManager.modEnabled || !Global.realisticFreezingEnabled || cause != DamageSource.Freezing) return;

                hp *= MaybeApplyFreezingDamageMultiplier();
            }
        }

        internal static float MaybeApplyFreezingDamageMultiplier()
        {
            float bodyTemp = GameManager.GetFreezingComponent().CalculateBodyTemperature();

            if (bodyTemp < damageRateIncreaseTemperature)
            {
                return Mathf.Clamp(1f + (damageRateIncreaseTemperature - bodyTemp) * addedDamageRatePerDegree, 1f, maximumDamageMultiplier);
            }
            return 1f;
        }
    }
}