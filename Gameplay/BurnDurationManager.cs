using HarmonyLib;

namespace RelentlessNight
{
    internal class BurnDurationManager
    {
        [HarmonyPatch(typeof(FuelSourceItem), "GetModifiedBurnDurationHours", null)]
        internal static class FuelSourceItem_GetModifiedBurnDurationHours
        {
            private static void Postfix(ref float __result)
            {
                if (!MenuManager.modEnabled) return;

                __result *= Global.fireFuelDurationMultiplier;
            }
        }
        [HarmonyPatch(typeof(KeroseneLampItem), "Update", null)]
        internal static class KeroseneLampItem_Update
        {
            private static void Postfix(KeroseneLampItem __instance)
            {
                if (!MenuManager.modEnabled) return;

                __instance.m_FuelBurnLitersPerHour = 0.25f / Global.lanternFuelDurationMultiplier;
            }
        }
        [HarmonyPatch(typeof(TorchItem), "Update")]
        internal static class TorchItem_Update
        {
            private static void Postfix(TorchItem __instance)
            {
                if (!MenuManager.modEnabled) return;

                if (TorchBurnDurationChanged(__instance))
                {
                    float oldBurnTime = __instance.m_BurnLifetimeMinutes;
                    float newBurnTime = 90f * Global.torchBurnDurationMultiplier;

                    __instance.m_ElapsedBurnMinutes *= newBurnTime / oldBurnTime;
                }
                __instance.m_BurnLifetimeMinutes = 90f * Global.torchBurnDurationMultiplier;
            }
        }

        internal static bool TorchBurnDurationChanged(TorchItem __instance)
        {
            return __instance.m_BurnLifetimeMinutes != 90f * Global.torchBurnDurationMultiplier;
        }
    }
}