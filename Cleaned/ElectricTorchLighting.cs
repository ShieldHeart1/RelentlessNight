using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class ElectricTorchLighting
    {
        private static bool PlayerHoldingUnlitTorch(PlayerManager __instance)
        {
            return __instance.PlayerHoldingTorchThatCanBeLit();
        }

        private static bool PlayerInteractingWithToaster(PlayerManager __instance)
        {
            GameObject itemUnderCrosshair = __instance.m_InteractiveObjectUnderCrosshair;

            if (itemUnderCrosshair && itemUnderCrosshair.name.ToLowerInvariant().Contains("toaster")) return true;
            return false;
        }

        [HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction", null)]
        internal static class PlayerManager_InteractiveObjectsProcessInteraction
        {
            private static void Postfix(PlayerManager __instance)
            {
                if (!RnGl.rnActive || !GameManager.GetAuroraManager().AuroraIsActive() || !PlayerInteractingWithToaster(__instance) || !PlayerHoldingUnlitTorch(__instance)) return;

                InterfaceManager.m_Panel_TorchLight.Enable(true);
            }
        }
    }
}