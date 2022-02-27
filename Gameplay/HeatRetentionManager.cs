using HarmonyLib;
using System;
using UnityEngine;

namespace RelentlessNight
{
    // This is a rather large and complicated class, feel free to reach out to Shieldheart for any questions
    internal class HeatRetentionManager
    {
        internal const float rnMaxNonForgeFireTemperature = 120f;
        internal const float cabinDegreesHeatLossPerHour = 7f;
        internal const float caveDegreesHeatLossPerHour = 10f;
        internal const float warehouseDegreesHeatLossPerHour = 14f;
        internal const float outdoorDegreesHeatLossPerHour = 25f;

        internal static float indoorOutdoorTemperatureReliance = 1f;
        internal static float currentDegreesHeatLossPerHour = 20f;
        internal static float currentRetainedHeatInDegrees = 0f;
        internal static bool fireShouldHeatWholeScene = false;    

        [HarmonyPatch(typeof(MissionServicesManager), "PostSceneLoadCo", null)]
        internal class MissionServicesManager_PostSceneLoadCo
        {
            private static void Postfix()
            {
                if (!MenuManager.modEnabled || !Global.heatRetentionEnabled) return;

                SetHeatRetentionFactorsForScene();
            }
        }
        [HarmonyPatch(typeof(GameManager), "CacheComponents", null)]
        internal static class GameManager_CacheComponents
        {
            private static void Postfix()
            {
                if (!MenuManager.modEnabled) return;

                // Max temperatures of non-forge fires increased in order to better combat endgame temperatures
                GameManager.GetFireManagerComponent().m_MaxHeatIncreaseOfFire = rnMaxNonForgeFireTemperature;

                // With configurable fire fuel duration, this ensures fire time is reduced proportionaly/correctly when taking a torch from the fire
                GameManager.GetFireManagerComponent().m_TakeTorchReduceBurnMinutes *= Global.fireFuelDurationMultiplier;
            }
        }
        [HarmonyPatch(typeof(Fire), "AddFuel", null)]
        internal class Fire_AddFuel
        {
            private static void Postfix(Fire __instance)
            {
                if (!MenuManager.modEnabled || !Global.heatRetentionEnabled) return;

                // This is required to avoid a bug I could not get to the bottom of years ago, causing incorrect calculation of fire radius for certain scenes
                IncreaseFireRadiiToHeatWholeScene(__instance);
            }
        }
        [HarmonyPatch(typeof(Fire), "Deserialize", null)]
        internal class Fire_Deserialize
        {
            private static void Postfix(Fire __instance)
            {
                if (!MenuManager.modEnabled || !Global.heatRetentionEnabled) return;

                if (SavedFireHasRetainedHeat(__instance)) AddHeatRemainingToFire(__instance);
            }
        }
        [HarmonyPatch(typeof(Fire), "TurnOn", null)]
        internal class Fire_TurnOn
        {
            private static void Prefix(ref bool maskTempIncrease)
            {
                if (!MenuManager.modEnabled || !Global.heatRetentionEnabled) return;

                maskTempIncrease = false;

                if (!GameManager.GetFireManagerComponent().PointInRadiusOfBurningFire(GameManager.GetPlayerTransform().position))
                {
                    currentRetainedHeatInDegrees = GameManager.GetHeatSourceManagerComponent().GetTemperatureIncrease(GameManager.GetPlayerTransform().position);
                }
            }
            private static void Postfix(Fire __instance)
            {
                if (!MenuManager.modEnabled || !Global.heatRetentionEnabled) return;

                RestartFireAddingToRetainedHeat(__instance);

                if (fireShouldHeatWholeScene) IncreaseFireRadiiToHeatWholeScene(__instance);
            }
        }
        [HarmonyPatch(typeof(Fire), "Update", null)]
        internal class Fire_Update
        {
            private static void Prefix(Fire __instance)
            {
                if (!MenuManager.modEnabled || !Global.heatRetentionEnabled || GameManager.m_IsPaused) return;

                // This is also required to avoid the same bug mentioned above, causing incorrect calculation of fire radii for certain scenes
                MaybeResetFireRadiiToGameDefaults(__instance);

                if (__instance.m_FireState == FireState.Off) ContinueTrackingTimeAfterFireIsOut(__instance);
            }
        }
        // Prevents "turning off" of fire as long as it has residual heat left, even though graphically fire has been turned off
        [HarmonyPatch(typeof(HeatSource), "TurnOffImmediate", null)]
        internal class HeatSource_TurnOffImmediate
        {
            private static bool Prefix(HeatSource __instance)
            {
                if (!MenuManager.modEnabled || !Global.heatRetentionEnabled) return true;

                if (__instance.m_TempIncrease > 0f) return false;
                return true;
            }
        }
        [HarmonyPatch(typeof(Panel_FeedFire), "OnFeedFire", null)]
        internal class Panel_Panel_OnFeedFire
        {
            private static bool Prefix(Panel_FeedFire __instance)
            {
                if (!MenuManager.modEnabled || !Global.heatRetentionEnabled) return true;

                Panel_FeedFire_OnFeedFireRewrite(__instance);
                return false;
            }
        }
        [HarmonyPatch(typeof(HeatSource), "Update", null)]
        internal class HeatSource_Update
        {
            private static bool Prefix(HeatSource __instance)
            {
                if (!MenuManager.modEnabled || !Global.heatRetentionEnabled || GameManager.m_IsPaused) return true;

                HeatSource_UpdateRewrite(__instance);
                return false;
            }
        }

        internal static void ContinueTrackingTimeAfterFireIsOut(Fire __instance)
        {
            __instance.m_ElapsedOnTODSeconds += GameManager.GetTimeOfDayComponent().GetTODSeconds(Time.deltaTime);
        }
        internal static float GetHeatRemaining(Fire __instance)
        {
            float hoursAfterburnOut = (__instance.m_ElapsedOnTODSeconds - __instance.m_MaxOnTODSeconds) / 3600f;

            return __instance.m_HeatSource.m_MaxTempIncrease - (hoursAfterburnOut * currentDegreesHeatLossPerHour);
        }
        internal static bool SavedFireHasRetainedHeat(Fire __instance)
        {
            return __instance.m_FireState == FireState.Off && __instance.m_HeatSource.m_MaxTempIncrease > 0f && __instance.m_MaxOnTODSeconds > 0f && GetHeatRemaining(__instance) > 0;
        }
        internal static void AddHeatRemainingToFire(Fire __instance)
        {
            __instance.m_HeatSource.m_TempIncrease = GetHeatRemaining(__instance);
            GameManager.GetHeatSourceManagerComponent().AddHeatSource(__instance.m_HeatSource);
        }
        internal static void IncreaseFireRadiiToHeatWholeScene(Fire __instance)
        {
            __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = 1000f;
            __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = 1000f;
        }
        internal static void MaybeResetFireRadiiToGameDefaults(Fire __instance)
        {
            if (!GameManager.GetWeatherComponent().IsIndoorScene() || __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius == 1000f || __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius == 1000f)
            {
                __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = 2;
                __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = 30;
            }
        }
        internal static void RestartFireAddingToRetainedHeat(Fire __instance)
        {
            __instance.m_HeatSource.m_MaxTempIncrease += currentRetainedHeatInDegrees;
            __instance.m_FuelHeatIncrease = __instance.m_HeatSource.m_MaxTempIncrease;
        }
        internal static void UpdateHeatRetentionFactors(float temperatureReliance, float degreeHeatLossPerHour, bool shouldHeatWholeScene)
        {
            indoorOutdoorTemperatureReliance = temperatureReliance;            
            currentDegreesHeatLossPerHour = degreeHeatLossPerHour;
            fireShouldHeatWholeScene = shouldHeatWholeScene;
        }
        // This method may need updates as new regions and buildings are added, last updated on game version 1.99
        internal static void SetHeatRetentionFactorsForScene()
        {
            string activeScene = GameManager.m_ActiveScene.ToLower();

            if (GameManager.GetWeatherComponent().IsIndoorScene())
            {
                if (activeScene.Contains("cave") || activeScene.Contains("mine"))
                {
                    UpdateHeatRetentionFactors(0.80f, caveDegreesHeatLossPerHour, false);
                }
                else
                {
                    bool warehouseScene = activeScene.Contains("whalingwarehouse") || activeScene.Contains("dam") || activeScene.Contains("damtransitionzone") || activeScene.Contains("whalingship") || activeScene.Contains("barnhouse") || activeScene.Contains("maintenanceshed") || activeScene.Contains("blackrockinterior");

                    if (warehouseScene)
                    {
                        UpdateHeatRetentionFactors(1f, warehouseDegreesHeatLossPerHour, false);
                    }
                    // Player is in a small, more insulated, indoor area
                    else
                    {
                        UpdateHeatRetentionFactors(1f, cabinDegreesHeatLossPerHour, true);
                    }
                }
            }
            // Player is outdoors
            else
            {
                UpdateHeatRetentionFactors(1f, outdoorDegreesHeatLossPerHour, false);
            }
        }
        // OnFeedFire of the Panel_FeedFire class is a large method, it's been patched and integrated here with RN logic following the last mono assembly.
        internal static void Panel_FeedFire_OnFeedFireRewrite(Panel_FeedFire __instance)
        {
            if (__instance.ProgressBarIsActive())
            {
                GameAudioManager.PlayGUIError();
                return;
            }
            GearItem selectedFuelSource = __instance.GetSelectedFuelSource();
            if (selectedFuelSource == null)
            {
                GameAudioManager.PlayGUIError();
                return;
            }
            FuelSourceItem fuelSourceItem = selectedFuelSource.m_FuelSourceItem;
            if (fuelSourceItem == null)
            {
                GameAudioManager.PlayGUIError();
                return;
            }
            GameObject m_FireContainer = __instance.m_FireContainer;
            if (!m_FireContainer)
            {
                GameAudioManager.PlayGUIError();
                return;
            }
            Fire m_Fire = __instance.m_Fire;
            if (!m_Fire)
            {
                return;
            }
            if (m_Fire.FireShouldBlowOutFromWind())
            {
                Utilities.DisallowActionWithGameMessage("GAMEPLAY_TooWindyToAddFuel");
                return;
            }
            bool fireInForge = __instance.FireInForge();
            if (!fireInForge)
            {
                float currentFireBurnDurationHours = m_Fire.GetRemainingLifeTimeSeconds() / 3600f;
                float fuelSourceItemBurnDurationHours = fuelSourceItem.GetModifiedBurnDurationHours(selectedFuelSource.GetNormalizedCondition());
                float fireNewBurnDurationHours = currentFireBurnDurationHours + fuelSourceItemBurnDurationHours;

                // RN logic is inserted here, if both fire duration and temperature is maxed, do not allow adding further fuel
                if (fireNewBurnDurationHours > GameManager.GetFireManagerComponent().m_MaxDurationHoursOfFire && m_Fire.GetCurrentTempIncrease() == rnMaxNonForgeFireTemperature)
                {
                    Utilities.DisallowActionWithGameMessage("GAMEPLAY_CannotAddMoreFuel");
                    return;
                }
            }
            if (selectedFuelSource.m_ResearchItem && !selectedFuelSource.m_ResearchItem.IsResearchComplete())
            {
                __instance.m_ResearchItemToBurn = selectedFuelSource;
                InterfaceManager.m_Panel_Confirmation.ShowBurnResearchNotification(new Action(() => __instance.ForceBurnResearchItem()));
                return;
            }
            if (selectedFuelSource == GameManager.GetPlayerManagerComponent().m_ItemInHands)
            {
                GameManager.GetPlayerManagerComponent().UnequipItemInHandsSkipAnimation();
            }
            if (selectedFuelSource == GameManager.GetPlayerManagerComponent().m_PickupGearItem)
            {
                GameManager.GetPlayerManagerComponent().ResetPickup();
            }
            GameAudioManager.PlaySound(__instance.m_FeedFireAudio, InterfaceManager.GetSoundEmitter());
            m_Fire.AddFuel(selectedFuelSource, __instance.FireInForge());
            GameManager.GetPlayerManagerComponent().ConsumeUnitFromInventory(fuelSourceItem.gameObject);
        }
        // Update method of the HeatSource class is a large method, it's been patched and integrated here with RN logic following the last mono assembly.
        internal static void HeatSource_UpdateRewrite(HeatSource __instance)
        {
            float m_TempIncrease = __instance.m_TempIncrease;

            if (Mathf.Approximately(__instance.m_TimeToReachMaxTempMinutes, 0f))
            {
                m_TempIncrease = __instance.m_MaxTempIncrease;
            }
            else
            {
                float todMinutes = GameManager.GetTimeOfDayComponent().GetTODMinutes(Time.deltaTime);
                if (__instance.m_TurnedOn)
                {
                    m_TempIncrease += todMinutes / __instance.m_TimeToReachMaxTempMinutes * __instance.m_MaxTempIncrease;
                }
                else
                {
                    // RN logic is here, with sceneDegreeHeatLossPerHour governing the rate at which temperature drops from heat retention
                    m_TempIncrease -= (todMinutes / 60f) * currentDegreesHeatLossPerHour;
                }

                if (__instance.m_MaxTempIncrease > 0f)
                {
                    m_TempIncrease = Mathf.Clamp(m_TempIncrease, 0f, __instance.m_MaxTempIncrease);
                }
                else
                {
                    m_TempIncrease = Mathf.Clamp(m_TempIncrease, __instance.m_MaxTempIncrease, 0f);
                }
            }
            __instance.m_TempIncrease = m_TempIncrease;
        }
        public delegate void ForceBurnResearchItem();
    }
}