using System;
using Harmony;
using UnityEngine;

namespace RelentlessNight
{
    public class TemperatureChange
    {
        public static float GetRnTempIncrease(int curDay)
        {
            return RnGlobal.glRotationDecline * (curDay - 1f) * 0.015f;
        }

        public static float GetRnTempDecrease(int curDay)
        {
            return 1.5f * GetRnTempIncrease(curDay);
        }

        public static void MaybeApplyNewDailyTempOffset(int curDay)
        {
            if (RnGlobal.glCurrentDay != curDay)
            {
                RnGlobal.glCurrentDay = curDay;

                System.Random rnd = new System.Random();
                RnGlobal.glCurrentDayTempOffset = rnd.Next(-5, 5);
            }
        }

        public static float MaybeGetMorningTempBonus(int curDay, int curCelestialMinutes)
        {
            if (RnGlobal.glRotationDecline == 0) return 0f;

            float curCelestialHour = curCelestialMinutes / 60f;

            if (curCelestialHour > 7 && curCelestialHour < 9) return GetRnTempDecrease(curDay) * 0.4f * (curCelestialHour - 7);
            
            if (curCelestialHour > 9 && curCelestialHour < 11) return GetRnTempDecrease(curDay) * 0.4f * (11f - curCelestialHour);

            return 0f;
        }

        [HarmonyPatch(typeof(Weather), "CalculateCurrentTemperature", null)]
        public class Weather_CalculateCurrentTemperature_Pre
        {
            private static bool Prefix(Weather __instance)
            {
                if (!RnGlobal.rnActive) return true;

                int curDay = GameManager.GetTimeOfDayComponent().GetDayNumber();

                int curCelestialHour = GameManager.GetTimeOfDayComponent().GetHour();
                int curCelestialMinutes = curCelestialHour * 60 + GameManager.GetTimeOfDayComponent().GetMinutes();

                float m_TempHigh = __instance.m_TempHigh;
                float m_TempLow = __instance.m_TempLow;

                bool m_GenerateNewTempHigh = __instance.m_GenerateNewTempHigh;
                bool m_GenerateNewTempLow = __instance.m_GenerateNewTempLow;

                float tempIncrease = GetRnTempIncrease(curDay);
                float tempDecrease = GetRnTempDecrease(curDay);

                if (curCelestialHour >= __instance.m_HourWarmingBegins && curCelestialHour < __instance.m_HourCoolingBegins)
                {
                    if (m_GenerateNewTempHigh)
                    {
                        __instance.GenerateTempHigh();
                    }
                    int num8 = curCelestialMinutes - __instance.m_HourWarmingBegins * 60;
                    float num9 = (__instance.m_HourCoolingBegins - __instance.m_HourWarmingBegins) * 60f;

                    if (m_TempLow - tempDecrease < RnGlobal.glMinimumTemperature)
                    {
                        tempDecrease = Mathf.Abs(RnGlobal.glMinimumTemperature + 5f) + m_TempLow;
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
                    int num10 = curCelestialMinutes - __instance.m_HourCoolingBegins * 60;

                    if (num10 < 0)
                    {
                        num10 = curCelestialMinutes + (24 - __instance.m_HourCoolingBegins) * 60;
                    }
                    float num11 = (24 - __instance.m_HourCoolingBegins + __instance.m_HourWarmingBegins) * 60f;

                    if (m_TempHigh + tempIncrease > 2f)
                    {
                        tempIncrease = 2f - m_TempHigh;
                    }

                    if (m_TempLow - tempDecrease < RnGlobal.glMinimumTemperature)
                    {
                        tempDecrease = Mathf.Abs(RnGlobal.glMinimumTemperature + 5f) + m_TempLow;
                    }
                    __instance.m_CurrentTemperature = m_TempHigh + tempIncrease - (num10 / num11 * (m_TempHigh + (tempIncrease + tempDecrease) - m_TempLow));
                }

                if (RnGlobal.glDayTidallyLocked != -1)
                {
                    __instance.m_CurrentTemperature -= (curDay - RnGlobal.glDayTidallyLocked) * 2f;

                    if (__instance.m_CurrentTemperature < RnGlobal.glMinimumTemperature + 5) __instance.m_CurrentTemperature = RnGlobal.glMinimumTemperature + 5;
                }

                RnGlobal.glOutdoorTempWithoutBlizDrop = __instance.m_CurrentBlizzardDegreesDrop;

                float m_CurrentBlizzardDegreesDrop = __instance.m_CurrentBlizzardDegreesDrop;

                MaybeApplyNewDailyTempOffset(curDay);
                __instance.m_CurrentTemperature += RnGlobal.glCurrentDayTempOffset + MaybeGetMorningTempBonus(curDay, curCelestialMinutes);

                bool isCountedIndoors = false;
                if (__instance.IsIndoorEnvironment())
                {
                    isCountedIndoors = (!GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger || !GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_UseOutdoorTemperature);
                }
                if (isCountedIndoors)
                {
                    if (RnGlobal.glTemperatureEffect != 0)
                    {
                        __instance.m_CurrentTemperature = __instance.m_IndoorTemperatureCelsius + (1f + RnGlobal.glTemperatureEffect / 10f) + __instance.m_CurrentTemperature * (RnGlobal.glTemperatureEffect / 100f) * HeatRetention.rnIndoorOutdoorTempFactor;
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

                if (GameManager.GetSnowShelterManager().PlayerInNonRuinedShelter())
                {
                    __instance.m_CurrentTemperature += GameManager.GetSnowShelterManager().GetTemperatureIncreaseCelsius();
                }

                if (GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger)
                {
                    __instance.m_CurrentTemperature += GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_TemperatureDeltaCelsius;
                }

                if (GameManager.GetPlayerInVehicle().IsInside())
                {
                    __instance.m_CurrentTemperature += GameManager.GetPlayerInVehicle().GetTempIncrease();
                }

                __instance.m_CurrentTemperature += __instance.m_ArtificalTempIncrease;
                __instance.m_CurrentTemperatureWithoutHeatSources = __instance.m_CurrentTemperature;
                __instance.m_CurrentTemperature += GameManager.GetHeatSourceManagerComponent().GetTemperatureIncrease(GameManager.GetPlayerTransform().position);
                __instance.m_CurrentTemperature += GameManager.GetFeatColdFusion().GetTemperatureCelsiusBonus();
                __instance.m_CurrentTemperature = Mathf.Max(__instance.GetMinAirTemp(), __instance.m_CurrentTemperature);
                __instance.m_CurrentTemperature = Mathf.Clamp(__instance.m_CurrentTemperature, float.NegativeInfinity, __instance.m_MaxAirTemperature);
                __instance.m_CurrentTemperature = Mathf.Clamp(__instance.m_CurrentTemperature, __instance.m_MinAirTemperature, __instance.m_MaxAirTemperature);

                if (__instance.m_LockedAirTemperature > -1000f)
                {
                    __instance.m_CurrentTemperature = __instance.m_LockedAirTemperature;
                }

                return false;
            }
        }

        [HarmonyPatch(typeof(ExperienceModeManager), "GetOutdoorTempDropCelcius", null)]
        public class ExperienceModeManager_GetOutdoorTempDropCelcius_Post
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGlobal.rnActive) return;

                __result /= 2f;
            }
        }
    }
}