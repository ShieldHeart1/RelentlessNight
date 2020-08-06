using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class TemperatureChange
    {
        [HarmonyPatch(typeof(Weather), "CalculateCurrentTemperature", null)]
        public class Weather_CalculateCurrentTemperature_Pre
        {
            private static bool Prefix(Weather __instance)
            {
                if (!RnGl.rnActive) return true;

                float curDay = GameManager.GetTimeOfDayComponent().GetDayNumber();
                float m_CurrentTemperature = __instance.m_CurrentTemperature;
                int curCelestialHour = GameManager.GetTimeOfDayComponent().GetHour();
                int num2 = curCelestialHour * 60 + GameManager.GetTimeOfDayComponent().GetMinutes();

                float m_TempHigh = __instance.m_TempHigh;
                float m_TempLow = __instance.m_TempLow;

                bool m_GenerateNewTempHigh = __instance.m_GenerateNewTempHigh;
                bool m_GenerateNewTempLow = __instance.m_GenerateNewTempLow;

                float tempIncrease = RnGl.glRotationDecline * (curDay - 1f) * 0.015f;
                float tempDecrease = 1.5f * tempIncrease;

                if (curCelestialHour >= __instance.m_HourWarmingBegins && curCelestialHour < __instance.m_HourCoolingBegins)
                {
                    if (m_GenerateNewTempHigh)
                    {
                        __instance.GenerateTempHigh();
                    }
                    int num8 = num2 - __instance.m_HourWarmingBegins * 60;
                    float num9 = (__instance.m_HourCoolingBegins - __instance.m_HourWarmingBegins) * 60f;

                    if (m_TempLow - tempDecrease < RnGl.glMinimumTemperature)
                    {
                        tempDecrease = 100f + m_TempLow;
                    }

                    if (m_TempHigh + tempIncrease > 2f)
                    {
                        tempIncrease = 2f - m_TempHigh;
                    }
                    __instance.m_CurrentTemperature = m_TempLow - tempDecrease + num8 / num9 * (m_TempHigh + (tempIncrease + tempDecrease) - m_TempLow);
                }
                else
                {
                    if (m_GenerateNewTempLow)
                    {
                        __instance.GenerateTempLow();
                    }
                    int num10 = num2 - __instance.m_HourCoolingBegins * 60;

                    if (num10 < 0)
                    {
                        num10 = num2 + (24 - __instance.m_HourCoolingBegins) * 60;
                    }
                    float num11 = (24 - __instance.m_HourCoolingBegins + __instance.m_HourWarmingBegins) * 60f;

                    if (m_TempHigh + tempIncrease > 2f)
                    {
                        tempIncrease = 2f - m_TempHigh;
                    }

                    if (m_TempLow - tempDecrease < RnGl.glMinimumTemperature)
                    {
                        tempDecrease = Mathf.Abs(RnGl.glMinimumTemperature) + m_TempLow;
                    }
                    __instance.m_CurrentTemperature = m_TempHigh + tempIncrease - (num10 / num11 * (m_TempHigh + (tempIncrease + tempDecrease) - m_TempLow));
                }

                //Debug.Log("TEMP1: " + __instance.m_CurrentTemperature);

                if (RnGl.glDayTidallyLocked != -1)
                {
                    __instance.m_CurrentTemperature -= (curDay - RnGl.glDayTidallyLocked) * 2f;

                    if (__instance.m_CurrentTemperature < RnGl.glMinimumTemperature) __instance.m_CurrentTemperature = RnGl.glMinimumTemperature;
                }

                //Debug.Log("TEMP2: " + __instance.m_CurrentTemperature);

                float m_CurrentBlizzardDegreesDrop = __instance.m_CurrentBlizzardDegreesDrop;

                bool isCountedIndoors = false;
                if (__instance.IsIndoorEnvironment())
                {
                    isCountedIndoors = (!GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger || !GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_UseOutdoorTemperature);
                }
                if (isCountedIndoors)
                {
                    if (RnGl.glTemperatureEffect != 0)
                    {
                        __instance.m_CurrentTemperature = __instance.m_IndoorTemperatureCelsius + (1f + RnGl.glTemperatureEffect / 10f) + __instance.m_CurrentTemperature * (RnGl.glTemperatureEffect / 100f) * RnGl.rnIndoorTempFactor;
                    }
                    else
                    {
                        __instance.m_CurrentTemperature = __instance.m_IndoorTemperatureCelsius;
                    }
                }
                else
                {
                    __instance.m_CurrentTemperature -= m_CurrentBlizzardDegreesDrop;
                }

                //Debug.Log("TEMP3: " + __instance.m_CurrentTemperature);

                if (GameManager.GetSnowShelterManager().PlayerInNonRuinedShelter())
                {
                    __instance.m_CurrentTemperature += GameManager.GetSnowShelterManager().GetTemperatureIncreaseCelsius();
                }
                //Debug.Log("TEMP4: " + __instance.m_CurrentTemperature);

                if (GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger)
                {
                    __instance.m_CurrentTemperature += GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_TemperatureDeltaCelsius;
                }
                //Debug.Log("TEMP5: " + __instance.m_CurrentTemperature);
                if (GameManager.GetPlayerInVehicle().IsInside())
                {
                    __instance.m_CurrentTemperature += GameManager.GetPlayerInVehicle().GetTempIncrease();
                }

                //Debug.Log("TEMP6: " + __instance.m_CurrentTemperature);

                __instance.m_CurrentTemperature += __instance.m_ArtificalTempIncrease;
                __instance.m_CurrentTemperatureWithoutHeatSources = __instance.m_CurrentTemperature;
                __instance.m_CurrentTemperature += GameManager.GetHeatSourceManagerComponent().GetTemperatureIncrease(GameManager.GetPlayerTransform().position);
                __instance.m_CurrentTemperature += GameManager.GetFeatColdFusion().GetTemperatureCelsiusBonus();
                __instance.m_CurrentTemperature = Mathf.Max(__instance.GetMinAirTemp(), __instance.m_CurrentTemperature);
                __instance.m_CurrentTemperature = Mathf.Clamp(__instance.m_CurrentTemperature, float.NegativeInfinity, __instance.m_MaxAirTemperature);
                __instance.m_CurrentTemperature = Mathf.Clamp(__instance.m_CurrentTemperature, __instance.m_MinAirTemperature, __instance.m_MaxAirTemperature);

                //Debug.Log("TEMP7: " + __instance.m_CurrentTemperature);

                if (__instance.m_LockedAirTemperature > -1000f)
                {
                    __instance.m_CurrentTemperature = __instance.m_LockedAirTemperature;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(ExperienceModeManager), "GetOutdoorTempDropCelcius", null)]
        public class ExperienceModeManager_GetOutdoorTempDropCelcius_Pos
        {
            private static void Postfix(ref float __result)
            {
                __result = 0f;
            }
        }
    }
}