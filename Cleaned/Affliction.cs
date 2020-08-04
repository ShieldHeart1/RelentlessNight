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
                if (!RnGl.rnActive) return true;

                if (__instance.HasCabinFever())
                {
                    __instance.CabinFeverEnd();
                }
                return false;
            }
        }
    }    
}