using HarmonyLib;

namespace RelentlessNight
{
    internal class VanillaBugFixes
    {
        internal class WakeUpWhenFreezing
        {
            [HarmonyPatch(typeof(Panel_Rest), "DoRest", null)]
            internal static class Panel_Rest_DoRest
            {
                private static bool Prefix(Panel_Rest __instance)
                {
                    if (!MenuManager.modEnabled) return true;

                    if (!WarmEnoughToSleep(__instance))
                    {
                        DisallowRest(__instance);
                        return false;
                    }
                    return true;
                }
            }
            [HarmonyPatch(typeof(Rest), "UpdateWhenSleeping", null)]
            internal static class Rest_UpdateWhenSleeping
            {
                private static void Postfix(Rest __instance)
                {
                    if (!MenuManager.modEnabled) return;

                    MaybeWakeUpPlayerDueToFreezing(__instance);
                }
            }
            internal static bool BedProvidesAboveFreezingTemperature(Panel_Rest __instance)
            {
                return __instance.m_Bed.m_WarmthBonusCelsius + GameManager.GetFreezingComponent().CalculateBodyTemperature() > 0;
            }
            internal static bool WarmEnoughToSleep(Panel_Rest __instance)
            {
                return !GameManager.GetFreezingComponent().IsFreezing() || BedProvidesAboveFreezingTemperature(__instance);
            }
            internal static void DisallowRest(Panel_Rest __instance)
            {
                Utilities.DisallowActionWithModMessage("You cannot sleep while freezing");
            }
            internal static void MaybeWakeUpPlayerDueToFreezing(Rest __instance)
            {
                if (__instance.m_Bed != null)
                {
                    bool bodyTemperatureInBedIsPositive = GameManager.GetFreezingComponent().CalculateBodyTemperature() > 0;
                    if (bodyTemperatureInBedIsPositive || !GameManager.GetFreezingComponent().IsFreezing())
                    {
                        return;
                    }
                    __instance.EndSleeping(true);
                    HUDMessage.AddMessage("You woke up due to freezing", false);
                }
            }
        }
        internal class TurnOffRogueHeatSource
        {
            [HarmonyPatch(typeof(FirstPersonLightSource), "TurnOnEffects", null)]
            internal class FirstPersonLightSource_TurnOnEffectst
            {
                private static void Postfix(FirstPersonLightSource __instance)
                {
                    if (!MenuManager.modEnabled) return;

                    HeatSource componentInChildren = __instance.m_FXGameObject.GetComponentInChildren<HeatSource>();
                    if (componentInChildren)
                    {
                        componentInChildren.TurnOff();
                    }
                }
            }
        }        
    }
}