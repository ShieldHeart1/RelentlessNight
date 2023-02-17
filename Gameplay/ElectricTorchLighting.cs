using System;
using System.Collections.Generic;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace RelentlessNight
{
    internal class ElectricTorchLighting
    {
        [HarmonyPatch(typeof(MissionServicesManager), "RegisterAnyMissionObjects")]
        internal class MissionServicesManager_RegisterAnyMissionObjects
        {
            private static void Postfix()
            {
                if (!MenuManager.modEnabled) return;

                MakeTorchLightingItemInteractible("socket");
                MakeTorchLightingItemInteractible("outlet");
                MakeTorchLightingItemInteractible("cableset");
                MakeTorchLightingItemInteractible("electricdamage_temp");
            }
        }
        [HarmonyPatch(typeof(PlayerManager), "GetInteractiveObjectDisplayText")]
        internal class PlayerManager_GetInteractiveObjectDisplayText
        {
            private static void Postfix(PlayerManager __instance, ref string __result)
            {
                if (!MenuManager.modEnabled || !Global.electricTorchLightingEnabled) return;

                if (PlayerInteractingWithElectricLightSource(__instance) && __instance.PlayerHoldingTorchThatCanBeLit())
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
                if (!MenuManager.modEnabled || !Global.electricTorchLightingEnabled) return;

                if (!GameManager.GetAuroraManager().AuroraIsActive() || !PlayerInteractingWithElectricLightSource(__instance) || !__instance.PlayerHoldingTorchThatCanBeLit()) return;

                if (InterfaceManager.GetPanel<Panel_TorchLight>() != null) InterfaceManager.GetPanel<Panel_TorchLight>().StartTorchIgnite(2f, string.Empty, true);
            }
        }
        // Removes burn damage from stepping on wires, ensures player wont get burned trying to light torch from cable
        [HarmonyPatch(typeof(DamageTrigger), "ApplyOneTimeDamage")]
        internal class DamageTrigger_ApplyOneTimeDamage
        {
            private static bool Prefix(DamageTrigger __instance)
            {
                if (!MenuManager.modEnabled) return true;
                
                if (__instance.m_DamageSource != DamageSource.Electrical) return true;
                return false;
            }
        }
        [HarmonyPatch(typeof(DamageTrigger), "ApplyContinuousDamage")]
        internal class DamageTrigger_ApplyContinousDamage
        {
            private static bool Prefix(DamageTrigger __instance)
            {
                if (!MenuManager.modEnabled || __instance.m_DamageSource != DamageSource.Electrical) return true;

                return false;
            }
        }
        // Prevents saving when moving over live wires, as burn damage is removed
        [HarmonyPatch(typeof(DamageTrigger), "OnTriggerExit")]
        internal class DamageTrigger_OnTriggerExit
        {
            private static bool Prefix(DamageTrigger __instance)
            {
                if (!MenuManager.modEnabled) return true;

                return false;
            }
        }

        private static bool PlayerInteractingWithElectricLightSource(PlayerManager __instance)
        {
            GameObject itemUnderCrosshair = __instance.GetInteractiveObjectUnderCrosshairs(0.25f);

            if (itemUnderCrosshair != null && (itemUnderCrosshair.name.ToLowerInvariant().Contains("outlet") || itemUnderCrosshair.name.ToLowerInvariant().Contains("socket") || itemUnderCrosshair.name.ToLowerInvariant().Contains("electricdamage_temp") || itemUnderCrosshair.name.ToLowerInvariant().Contains("cableset"))) return true;

            return false;
        }
        internal static void MakeTorchLightingItemInteractible(string objectName)
        {
            List<GameObject> rObjs = Utilities.GetRootObjects();
            List<GameObject> result = new List<GameObject>();

            foreach (GameObject rootObj in rObjs)
            {
                Utilities.GetChildrenWithName(rootObj, objectName, result);

                if (result.Count > 0)
                {
                    foreach (GameObject child in result)
                    {
                        child.layer = 12;
                    }
                }
            }
        }
    }
}