using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class Affliction
    {
        [HarmonyPatch(typeof(CabinFever), "Update", null)]
        public class CabinFever_Update_Pre
        {
            private static bool Prefix(CabinFever __instance)
            {
                if (!RnGlobal.rnActive) return true;

                if (__instance.HasCabinFever())
                {
                    __instance.CabinFeverEnd();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(DamageTrigger), "OnTriggerEnter")]
        public class DamageTrigger_OnTriggerEnter_Pre
        {
            private static bool Prefix(DamageTrigger __instance)
            {
                if (!RnGlobal.rnActive) return true;

                if (__instance.m_DamageSource == DamageSource.Electrical)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(DamageTrigger), "OnTriggerExit")]
        public class DamageTrigger_OnTriggerExit_Pre
        {
            private static bool Prefix(DamageTrigger __instance)
            {
                if (!RnGlobal.rnActive) return true;

                if (__instance.m_DamageSource == DamageSource.Electrical)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        [HarmonyPatch(typeof(DamageTrigger), "OnTriggerStay")]
        public class DamageTrigger_OnTriggerStay_Pre
        {
            private static bool Prefix(DamageTrigger __instance)
            {
                if (!RnGlobal.rnActive) return true;

                if (__instance.m_DamageSource == DamageSource.Electrical)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
    }    
}