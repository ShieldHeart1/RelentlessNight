using HarmonyLib;
using UnityEngine;

namespace RelentlessNight
{
    internal class TemperatureManager
    {
        internal const float temperatureChangeMultiplier = 0.014f;
        internal const int maxOutdoorTemperature = 3;
        internal const int dailyRandomTemperatureRange = 6;

        internal static float currentDayRandomTemperatureOffset = 0;

        [HarmonyPatch(typeof(Weather), "CalculateCurrentTemperature", null)]
        internal class Weather_CalculateCurrentTemperature
        {
            private static bool Prefix(Weather __instance)
            {
                if (!MenuManager.modEnabled) return true;

                Weather_CalculateCurrentTemperature_Rewrite(__instance);
                return false;
            }
        }

        // CalculateCurrentTemperature of the Weather class is a large method in the game code, it's been patched and integrated here with with RN temperature logic.
        internal static void Weather_CalculateCurrentTemperature_Rewrite(Weather __instance)
        {
            int currentDay = GameManager.GetTimeOfDayComponent().GetDayNumber();
            int currentCelestialHour = GameManager.GetTimeOfDayComponent().GetHour();
            int currentCelestialMinutes = currentCelestialHour * 60 + GameManager.GetTimeOfDayComponent().GetMinutes();

            float m_TempHigh = __instance.m_TempHigh;
            float m_TempLow = __instance.m_TempLow;

            bool m_GenerateNewTempHigh = __instance.m_GenerateNewTempHigh;
            bool m_GenerateNewTempLow = __instance.m_GenerateNewTempLow;

            float rnTemperatureIncrease = GetRnTemperatureIncrease(currentDay);
            float rnTemperatureDecrease = GetRnTemperatureDecrease(currentDay);

            if (currentCelestialHour >= __instance.m_HourWarmingBegins && currentCelestialHour < __instance.m_HourCoolingBegins)
            {
                if (m_GenerateNewTempHigh) __instance.GenerateTempHigh();

                if (m_TempLow - rnTemperatureDecrease < Global.minAirTemperature)
                {
                    rnTemperatureDecrease = Mathf.Abs(Global.minAirTemperature + dailyRandomTemperatureRange) + m_TempLow;
                }
                if (m_TempHigh + rnTemperatureIncrease > maxOutdoorTemperature)
                {
                    rnTemperatureIncrease = maxOutdoorTemperature - m_TempHigh;
                }

                float minutesIntoWarmingPeriod = currentCelestialMinutes - __instance.m_HourWarmingBegins * 60;
                float warmingPeriodLengthMinutes = (__instance.m_HourCoolingBegins - __instance.m_HourWarmingBegins) * 60;

                __instance.m_CurrentTemperature = m_TempLow - rnTemperatureDecrease + minutesIntoWarmingPeriod / warmingPeriodLengthMinutes * (m_TempHigh + (rnTemperatureIncrease + rnTemperatureDecrease) - m_TempLow);
            }
            else
            {
                if (m_GenerateNewTempLow)
                {
                    __instance.GenerateTempLow();
                }
                int minutesIntoCoolingPeriod = currentCelestialMinutes - __instance.m_HourCoolingBegins * 60;
                if (minutesIntoCoolingPeriod < 0)
                {
                    minutesIntoCoolingPeriod = currentCelestialMinutes + (24 - __instance.m_HourCoolingBegins) * 60;
                }

                float coolingPeriodLengthMinutes = (24 - __instance.m_HourCoolingBegins + __instance.m_HourWarmingBegins) * 60;
                if (m_TempHigh + rnTemperatureIncrease > maxOutdoorTemperature)
                {
                    rnTemperatureIncrease = maxOutdoorTemperature - m_TempHigh;
                }
                if (m_TempLow - rnTemperatureDecrease < Global.minAirTemperature)
                {
                    rnTemperatureDecrease = Mathf.Abs(Global.minAirTemperature + dailyRandomTemperatureRange) + m_TempLow;
                }
                __instance.m_CurrentTemperature = m_TempHigh + rnTemperatureIncrease - (minutesIntoCoolingPeriod / coolingPeriodLengthMinutes * (m_TempHigh + (rnTemperatureIncrease + rnTemperatureDecrease) - m_TempLow));
            }
            if (TimeManager.GameInEndgame() && Global.dayTidalLocked != -1)
            {
                __instance.m_CurrentTemperature -= (currentDay - Global.dayTidalLocked) * 2;
            }
            float m_CurrentBlizzardDegreesDrop = __instance.m_CurrentBlizzardDegreesDrop;
            MaybeApplyNewDailyTemperatureOffset(currentDay);
            __instance.m_CurrentTemperature += currentDayRandomTemperatureOffset + GetMorningTemperatureBonus(currentDay, currentCelestialMinutes);
            float daysPlayedNotPaused = GameManager.GetTimeOfDayComponent().GetHoursPlayedNotPaused() / 24f;

            __instance.m_CurrentTemperature -= GameManager.GetExperienceModeManagerComponent().GetOutdoorTempDropCelcius(daysPlayedNotPaused);

            if (__instance.m_CurrentTemperature < Global.minAirTemperature + dailyRandomTemperatureRange)
            {
                __instance.m_CurrentTemperature = Global.minAirTemperature;
            }

            bool isCountedIndoors = false;

            if (__instance.IsIndoorEnvironment())
            {
                isCountedIndoors = (!GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger || !GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_UseOutdoorTemperature);
            }

            if (isCountedIndoors)
            {
                if (Global.indoorOutdoorTemperaturePercent != 0)
                {
                    __instance.m_CurrentTemperature = __instance.m_IndoorTemperatureCelsius + (1f + Global.indoorOutdoorTemperaturePercent / 10f) + __instance.m_CurrentTemperature * (Global.indoorOutdoorTemperaturePercent / 100f) * HeatRetentionManager.indoorOutdoorTemperatureReliance;
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
        }
        internal static float GetRnTemperatureIncrease(int currentDay)
        {
            return Global.worldSpinDeclinePercent * (currentDay - 1f) * temperatureChangeMultiplier;
        }
        // During cooling period, RN temperatures are decreased 1.5x faster vs the increase during warming period, this causes an average decline in temperatures
        internal static float GetRnTemperatureDecrease(int currentDay)
        {
            return 1.5f * GetRnTemperatureIncrease(currentDay);
        }
        // Some random variation to daily temperatures is added
        internal static void MaybeApplyNewDailyTemperatureOffset(int currentDay)
        {
            if (Global.lastTemperatureOffsetDay != currentDay)
            {
                Global.lastTemperatureOffsetDay = currentDay;
                currentDayRandomTemperatureOffset = new System.Random().Next(-dailyRandomTemperatureRange, dailyRandomTemperatureRange) / 2;
            }
        }
        // Raises temperature back a bit faster throughout mornings, as requested by many players
        internal static float GetMorningTemperatureBonus(int currentDay, int currentCelestialMinutes)
        {
            if (Global.worldSpinDeclinePercent == 0) return 0f;

            float currentCelestialHour = currentCelestialMinutes / 60f;

            if (currentCelestialHour > 7 && currentCelestialHour < 9) return GetRnTemperatureDecrease(currentDay) * (currentCelestialHour - 7f) * 0.4f;            
            if (currentCelestialHour > 9 && currentCelestialHour < 11) return GetRnTemperatureDecrease(currentDay) * (11f - currentCelestialHour) * 0.4f;

            return 0f;
        }
    }
}