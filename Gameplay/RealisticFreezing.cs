using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class RealisticFreezing
    {
        private const float accelFreezingTempThreshold = -40f;      //Player starts taking extra damage when body temp goes below this number
        private const float addedDamageRatePerDegree = 1 / 30f;     //Severity of the damage increase per degreeC below threshold
        private const int maximumDamageMultiplier = 3;              //Maximum possible multiplier for freezing damage rate

        private static float MaybeApplyFreezingDamageMultiplier()
        {
            float bodyTemp = GameManager.GetFreezingComponent().CalculateBodyTemperature();
            if (bodyTemp < accelFreezingTempThreshold)
            {
                return Mathf.Clamp(1f + (accelFreezingTempThreshold - bodyTemp) * addedDamageRatePerDegree, 1f, maximumDamageMultiplier);
            }
            return 1f;
        }

        [HarmonyPatch(typeof(Condition), "AddHealth", new Type[] { typeof(float), typeof(DamageSource), typeof(bool) })]
        public class Condition_AddHealth_Pre
        {
            private static void Prefix(ref float hp, DamageSource cause)
            {
                if (!RnGl.rnActive || !RnGl.glRealisticFreezing || cause != DamageSource.Freezing) return;

                hp *= MaybeApplyFreezingDamageMultiplier();
            }
        }
    }
}