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
        [HarmonyPatch(typeof(TorchItem), "Awake")]
        internal static class TorchItem_Awake_Post
        {
            private static void Postfix(TorchItem __instance)
            {
                if (!MenuManager.modEnabled) return;

                __instance.m_BurnLifetimeMinutes *= Global.torchBurnDurationMultiplier;
            }
        }
    }
}