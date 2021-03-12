using System;
using UnityEngine;
using Harmony;

namespace RelentlessNight
{
    public class HeatRetention
    {
        public static float currentRetainedHeatInDegrees = 0f;
        public static float currentDegreeHeatLossPerHour = 15f;
        public static bool fireShouldHeatWholeScene = false;
        public static float indoorOutdoorTempReliance = 1f;        

        [HarmonyPatch(typeof(MissionServicesManager), "PostSceneLoadCo", null)]
        public class MissionServicesManager_PostSceneLoadCo_Post
        {
            private static void Postfix()
            {
                if (!RnGlobal.rnActive || !RnGlobal.glHeatRetention) return;

                UpdateHeatRetentionFactors();
            }
        }

        public static void UpdateHeatRetentionFactors()
        {
            string activeScene = GameManager.m_ActiveScene.ToLower();

            if (GameManager.GetWeatherComponent().IsIndoorScene())
            {
                if (activeScene.Contains("cave") || activeScene.Contains("mine"))
                {
                    indoorOutdoorTempReliance = 0.80f;
                    fireShouldHeatWholeScene = false;
                    currentDegreeHeatLossPerHour = 3f;
                }
                else
                {
                    bool largeIndoorScene = activeScene.Contains("whalingwarehouse") || activeScene.Contains("dam") || activeScene.Contains("damtransitionzone") ||
                        activeScene.Contains("whalingship") || activeScene.Contains("barnhouse") || activeScene.Contains("maintenanceshed");

                    if (largeIndoorScene)
                    {
                        indoorOutdoorTempReliance = 1f;
                        fireShouldHeatWholeScene = false;
                        currentDegreeHeatLossPerHour = 3f;
                    }
                    else
                    {
                        indoorOutdoorTempReliance = 1f;
                        fireShouldHeatWholeScene = true;
                        currentDegreeHeatLossPerHour = 1f;
                    }
                }
            }
            else
            {
                fireShouldHeatWholeScene = false;
                currentDegreeHeatLossPerHour = 15f;
            }
        }

        [HarmonyPatch(typeof(GameManager), "CacheComponents", null)]
        internal static class GameManager_CacheComponents_Post
        {
            private static void Postfix(GameManager __instance)
            {
                if (!RnGlobal.rnActive) return;

                GameManager.GetFireManagerComponent().m_MaxHeatIncreaseOfFire = 120f;
                GameManager.GetFireManagerComponent().m_TakeTorchReduceBurnMinutes *= RnGlobal.glFireFuelFactor;
            }
        }

        [HarmonyPatch(typeof(Panel_FeedFire), "OnFeedFire", null)]
        public class Panel_Panel_FeedFire_Pre
        {
            private static bool Prefix(Panel_FeedFire __instance)
            {
                if (!RnGlobal.rnActive || !RnGlobal.glHeatRetention) return true;

                if (__instance.ProgressBarIsActive())
                {
                    GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                    return false;
                }

                GearItem selectedFuelSource = __instance.GetSelectedFuelSource();
                if (selectedFuelSource == null)
                {
                    GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                    return false;
                }

                FuelSourceItem fuelSourceItem = selectedFuelSource.m_FuelSourceItem;
                if (fuelSourceItem == null)
                {
                    GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                    return false;
                }

                GameObject m_FireContainer = __instance.m_FireContainer;
                if (!m_FireContainer)
                {
                    GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                    return false;
                }

                Fire m_Fire = __instance.m_Fire;
                if (!m_Fire)
                {
                    return false;
                }

                if (m_Fire.FireShouldBlowOutFromWind())
                {
                    HUDMessage.AddMessage(Localization.Get("GAMEPLAY_TooWindyToAddFuel"), false);
                    GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                    return false;
                }

                bool FireInForge = __instance.FireInForge();
                if (!FireInForge)
                {
                    float num = fuelSourceItem.GetModifiedBurnDurationHours(selectedFuelSource.GetNormalizedCondition()) * 60f;
                    float num2 = m_Fire.GetRemainingLifeTimeSeconds() / 60f;
                    float num3 = (num + num2) / 60f;
                    if (num3 > GameManager.GetFireManagerComponent().m_MaxDurationHoursOfFire && m_Fire.GetCurrentTempIncrease() == m_Fire.m_HeatSource.m_MaxTempIncrease)
                    {
                        GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                        HUDMessage.AddMessage(Localization.Get("GAMEPLAY_CannotAddMoreFuel"), false);
                        return false;
                    }
                }
                if (selectedFuelSource.m_ResearchItem && !selectedFuelSource.m_ResearchItem.IsResearchComplete())
                {
                    __instance.m_ResearchItemToBurn = selectedFuelSource;

                    InterfaceManager.m_Panel_Confirmation.ShowBurnResearchNotification(new Action(() => __instance.ForceBurnResearchItem()));
                    return false;
                }
                GameAudioManager.PlaySound(__instance.m_FeedFireAudio, InterfaceManager.GetSoundEmitter());
                m_Fire.AddFuel(selectedFuelSource, FireInForge);
                GameManager.GetPlayerManagerComponent().ConsumeUnitFromInventory(fuelSourceItem.gameObject);

                return false;
            }
        }
        public delegate void ForceBurnResearchItem();

        [HarmonyPatch(typeof(Fire), "AddFuel", null)]
        public class Fire_AddFuel_Post
        {
            private static void Postfix(Fire __instance)
            {
                if (!RnGlobal.rnActive || !RnGlobal.glHeatRetention) return;

                __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = 1000f;
                __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = 1000f;
            }
        }

        [HarmonyPatch(typeof(Fire), "Deserialize", null)]
        public class Fire_Deserialize_Post
        {
            private static void Postfix(Fire __instance)
            {
                if (!RnGlobal.rnActive || !RnGlobal.glHeatRetention) return;

                FireState m_FireState = __instance.m_FireState;

                float hourAfterburnOut = (__instance.m_ElapsedOnTODSeconds - __instance.m_MaxOnTODSeconds) / 3600f;
                float heatRemaining = __instance.m_HeatSource.m_MaxTempIncrease - (hourAfterburnOut * currentDegreeHeatLossPerHour);

                if (m_FireState == FireState.Off && __instance.m_HeatSource.m_MaxTempIncrease > 0f && __instance.m_MaxOnTODSeconds > 0f)
                {
                    if (heatRemaining > 0f)
                    {
                        __instance.m_HeatSource.m_TempIncrease = heatRemaining;
                        GameManager.GetHeatSourceManagerComponent().AddHeatSource(__instance.m_HeatSource);
                    }
                }

            }
        }

        [HarmonyPatch(typeof(Fire), "TurnOn", null)]
        public class Fire_TurnOn_Post
        {
            private static void Postfix(Fire __instance)
            {
                if (!RnGlobal.rnActive || !RnGlobal.glHeatRetention) return;

                __instance.m_HeatSource.m_MaxTempIncrease += currentRetainedHeatInDegrees;
                __instance.m_FuelHeatIncrease = __instance.m_HeatSource.m_MaxTempIncrease;

                if (fireShouldHeatWholeScene)
                {
                    __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = 1000f;
                    __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = 1000f;
                }

            }
        }

        [HarmonyPatch(typeof(Fire), "TurnOn", null)]
        public class Fire_TurnOn_Pre
        {
            private static void Prefix(ref bool maskTempIncrease)
            {
                if (!RnGlobal.rnActive || !RnGlobal.glHeatRetention) return;

                maskTempIncrease = false;

                if (!GameManager.GetFireManagerComponent().PointInRadiusOfBurningFire(GameManager.GetPlayerTransform().position))
                {
                    currentRetainedHeatInDegrees = GameManager.GetHeatSourceManagerComponent().GetTemperatureIncrease(GameManager.GetPlayerTransform().position);
                }
            }
        }

        // Tracks how long has passed even after the fire has gone out, for future calculation on how much heat remains
        [HarmonyPatch(typeof(Fire), "Update", null)]
        public class Fire_Update_Pre
        {
            private static void Prefix(Fire __instance)
            {
                if (!RnGlobal.rnActive || !RnGlobal.glHeatRetention || GameManager.m_IsPaused) return;

                if (__instance.m_HeatSource.m_MaxTempIncreaseInnerRadius == 1000f || __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius == 1000f)
                {
                    __instance.m_HeatSource.m_MaxTempIncreaseInnerRadius = 2;
                    __instance.m_HeatSource.m_MaxTempIncreaseOuterRadius = 30;
                }

                if (__instance.m_FireState == FireState.Off)
                {
                    __instance.m_ElapsedOnTODSeconds += GameManager.GetTimeOfDayComponent().GetTODSeconds(Time.deltaTime);
                }
            }
        }

        [HarmonyPatch(typeof(HeatSource), "TurnOffImmediate", null)]
        public class HeatSource_TurnOffImmediate_Pre
        {
            private static bool Prefix(HeatSource __instance)
            {
                if (!RnGlobal.rnActive || !RnGlobal.glHeatRetention) return true;

                if (__instance.m_TempIncrease > 0f) return false;

                return true;
            }
        }

        // Changes the temperature decline rate after a fire has gone out, this simulates the heat retention
        [HarmonyPatch(typeof(HeatSource), "Update", null)]
        public class HeatSource_Update_Pre
        {
            private static bool Prefix(HeatSource __instance)
            {
                if (!RnGlobal.rnActive || !RnGlobal.glHeatRetention) return true;                

                if (GameManager.m_IsPaused) return false;

                float m_TempIncrease = __instance.m_TempIncrease;

                if (Mathf.Approximately(__instance.m_TimeToReachMaxTempMinutes, 0f))
                {
                    m_TempIncrease = __instance.m_MaxTempIncrease;
                }
                else
                {
                    float todminutes = GameManager.GetTimeOfDayComponent().GetTODMinutes(Time.deltaTime);
                    bool m_TurnedOn = __instance.m_TurnedOn;

                    if (m_TurnedOn)
                    {
                        m_TempIncrease += todminutes / __instance.m_TimeToReachMaxTempMinutes * __instance.m_MaxTempIncrease;
                    }
                    else
                    {
                        m_TempIncrease -= todminutes / 24f * currentDegreeHeatLossPerHour;
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

                return false;
            }
        }
    }
}