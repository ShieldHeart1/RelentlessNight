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

        private static bool PlayerInteractingWithElectricLightSource(PlayerManager __instance)
        {
            GameObject itemUnderCrosshair = __instance.m_InteractiveObjectUnderCrosshair;

            if (itemUnderCrosshair != null && (itemUnderCrosshair.name.ToLowerInvariant().Contains("outlet") || itemUnderCrosshair.name.ToLowerInvariant().Contains("socket") || itemUnderCrosshair.name.ToLowerInvariant().Contains("electricdamage_temp"))) return true;
            return false;
        }

        [HarmonyPatch(typeof(MissionServicesManager), "SceneLoadCompleted")]
        internal class MissionServicesManager_SceneLoadCompleted_Two_Pos
        {
            private static void Postfix(MissionServicesManager __instance)
            {
                Utilities.CreateTorchLightingSceneItem("socket");
                Utilities.CreateTorchLightingSceneItem("outlet");
                Utilities.CreateTorchLightingSceneItem("electricdamage_temp");
            }
        }
        
        [HarmonyPatch(typeof(PlayerManager), "GetInteractiveObjectDisplayText", new Type[] { typeof(GameObject) })]
        internal class PlayerManager_GetObjText
        {
            private static void Postfix(PlayerManager __instance, ref string __result, GameObject interactiveObject)
            {
                if (PlayerInteractingWithElectricLightSource(__instance) && PlayerHoldingUnlitTorch(__instance))
                {
                    __result = "Light Torch";
                }
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "InteractiveObjectsProcessInteraction", null)]
        internal static class PlayerManager_InteractiveObjectsProcessInteraction
        {
            private static void Postfix(PlayerManager __instance)
            {
                if (!RnGl.rnActive || !GameManager.GetAuroraManager().AuroraIsActive() || !PlayerInteractingWithElectricLightSource(__instance) || !PlayerHoldingUnlitTorch(__instance)) return;

                if (InterfaceManager.m_Panel_TorchLight != null) InterfaceManager.m_Panel_TorchLight.StartTorchIgnite(2f, string.Empty, true);
            }
        }
    }
}