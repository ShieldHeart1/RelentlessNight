﻿using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class VanillaFix
    {
        public class WakeUpWhenFreezing
        {
            [HarmonyPatch(typeof(Panel_Rest), "DoRest", null)]
            internal static class Panel_Rest_DoRest_Pre
            {
                private static bool Prefix(Panel_Rest __instance)
                {
                    bool bedProvidesAboveFreezing = __instance.m_Bed.m_WarmthBonusCelsius + GameManager.GetFreezingComponent().CalculateBodyTemperature() > 0;
                    bool playerIsFreezing = GameManager.GetFreezingComponent().IsFreezing();

                    if (!RnGl.rnActive || !playerIsFreezing || playerIsFreezing && bedProvidesAboveFreezing) return true;

                    GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                    HUDMessage.AddMessage("You cannot sleep while freezing", false);
                    return false;
                }
            }

            [HarmonyPatch(typeof(Rest), "UpdateWhenSleeping", null)]
            internal static class Rest_UpdateWhenSleeping_Pre
            {
                private static void Prefix(Rest __instance)
                {
                    bool bedProvidesAboveFreezing = __instance.m_Bed.m_WarmthBonusCelsius + GameManager.GetFreezingComponent().CalculateBodyTemperature() > 0;
                    bool playerIsFreezing = GameManager.GetFreezingComponent().IsFreezing();

                    if (!RnGl.rnActive || !playerIsFreezing || bedProvidesAboveFreezing) return;

                    __instance.EndSleeping(true);
                    HUDMessage.AddMessage("You woke up due to freezing", false);
                }
            }
        }

        public class TurnOffPhantomHeatSource
        {
            [HarmonyPatch(typeof(FirstPersonLightSource), "TurnOnEffects", null)]
            public class FirstPersonLightSource_TurnOnEffects_Pos
            {
                private static void Postfix(FirstPersonLightSource __instance)
                {
                    if (!RnGl.rnActive || !RnGl.rnFireShouldHeatWholeScene) return;

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