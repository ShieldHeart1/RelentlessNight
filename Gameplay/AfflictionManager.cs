using HarmonyLib;
using static RelentlessNight.AfflictionManager;

namespace RelentlessNight
{
	internal class AfflictionManager
	{
        [HarmonyPatch(typeof(CabinFever), nameof(CabinFever.Update))]
        internal class CabinFever_Update
        {
            private static bool Prefix(CabinFever __instance)
            {
                if (!MenuManager.modEnabled) return true;

                if (__instance.HasCabinFever())
                {
                    __instance.CabinFeverEnd();
                }
                return false;
            }
        }
    }    
}