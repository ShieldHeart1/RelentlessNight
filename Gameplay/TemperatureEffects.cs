using Harmony;
using RelentlessNight;
using UnityEngine;

[HarmonyPatch(typeof(Weather), "CalculateCurrentTemperature", null)]
public class Weather_CalculateCurrentTemperature_Pre
{
    private static bool Prefix(Weather __instance)
    {
        //Debug.Log("Weather_CalculateCurrentTemperature_Pre");
        if (!RnGl.rnActive) return true;
    
        float curDay = (float)GameManager.GetTimeOfDayComponent().GetDayNumber();
        int curCelestialHour = GameManager.GetTimeOfDayComponent().GetHour();
        int num2 = curCelestialHour * 60 + GameManager.GetTimeOfDayComponent().GetMinutes();
        float outdoorTempDropCelcius = GameManager.GetExperienceModeManagerComponent().GetOutdoorTempDropCelcius(curDay);
        float m_CurrentTemperature = (float)AccessTools.Field(typeof(Weather), "m_CurrentTemperature").GetValue(__instance);
        float m_TempHigh = (float)AccessTools.Field(typeof(Weather), "m_TempHigh").GetValue(__instance);
        float m_TempLow = (float)AccessTools.Field(typeof(Weather), "m_TempLow").GetValue(__instance);
        bool m_GenerateNewTempHigh = (bool)AccessTools.Field(typeof(Weather), "m_GenerateNewTempHigh").GetValue(__instance);
        bool m_GenerateNewTempLow = (bool)AccessTools.Field(typeof(Weather), "m_GenerateNewTempLow").GetValue(__instance);
        float tempIncrease = (float)RnGl.glRotationDecline * (curDay - 1f) * 0.015f;
        float tempDecrease = 1.5f * tempIncrease;
   
        if (curCelestialHour >= __instance.m_HourWarmingBegins && curCelestialHour < __instance.m_HourCoolingBegins)
        {
            if (m_GenerateNewTempHigh)
            {
                AccessTools.Method(typeof(Weather), "GenerateTempHigh", null, null).Invoke(__instance, null);
            }
            int num8 = num2 - __instance.m_HourWarmingBegins * 60;
            float num9 = (float)(__instance.m_HourCoolingBegins - __instance.m_HourWarmingBegins) * 60f;

            if (m_TempLow - tempDecrease < -100f)
            {
                tempDecrease = 100f + m_TempLow;
            }

            if (m_TempHigh + tempIncrease - outdoorTempDropCelcius > 2f)
            {
                tempIncrease = 2f - m_TempHigh + outdoorTempDropCelcius;
            }
            m_CurrentTemperature = m_TempLow - tempDecrease + (float)num8 / num9 * (m_TempHigh + (tempIncrease + tempDecrease) - m_TempLow);
        }
        else
        {
            if (m_GenerateNewTempLow)
            {
                AccessTools.Method(typeof(Weather), "GenerateTempLow", null, null).Invoke(__instance, null);
            }
            int num10 = num2 - __instance.m_HourCoolingBegins * 60;
      
            if (num10 < 0)
            {
                num10 = num2 + (24 - __instance.m_HourCoolingBegins) * 60;
            }
            float num11 = (float)(24 - __instance.m_HourCoolingBegins + __instance.m_HourWarmingBegins) * 60f;

            if (m_TempHigh + tempIncrease - outdoorTempDropCelcius > 2f)
            {
                tempIncrease = 2f - m_TempHigh + outdoorTempDropCelcius;
            }

            if (m_TempLow - tempDecrease < -100f)
            {
                tempDecrease = 100f + m_TempLow;
            }
            m_CurrentTemperature = m_TempHigh + tempIncrease - (float)num10 / num11 * (m_TempHigh + (tempIncrease + tempDecrease) - m_TempLow);
        }

        //Debug.Log("TEMP1: " + m_CurrentTemperature);

        if (RnGl.glDayTidallyLocked != -1)
        {
            m_CurrentTemperature -= (curDay - (float)RnGl.glDayTidallyLocked) * 2f;

            if (m_CurrentTemperature < -100f) m_CurrentTemperature = -100f;           
        }

        //Debug.Log("TEMP2: " + m_CurrentTemperature);

        RnGl.glLastOutdoorTempNoBliz = m_CurrentTemperature - outdoorTempDropCelcius;
        float m_CurrentBlizzardDegreesDrop = (float)AccessTools.Field(typeof(Weather), "m_CurrentBlizzardDegreesDrop").GetValue(__instance);

        bool isCountedIndoors = false;
        if (__instance.IsIndoorEnvironment())
        {
            isCountedIndoors = (!GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger || !GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_UseOutdoorTemperature);
        }     
        if (isCountedIndoors)
        {
            if (RnGl.glTemperatureEffect != 0)
            {
                m_CurrentTemperature = __instance.m_IndoorTemperatureCelsius + (1f + (float)RnGl.glTemperatureEffect / 10f) + RnGl.glLastOutdoorTempNoBliz * ((float)RnGl.glTemperatureEffect / 100f) * RnGl.rnIndoorTempFactor;
            }
            else
            {
                m_CurrentTemperature = __instance.m_IndoorTemperatureCelsius;
            }
        }
        else
        {            
            m_CurrentTemperature -= m_CurrentBlizzardDegreesDrop;
        }

        //Debug.Log("TEMP3: " + m_CurrentTemperature);

        if (GameManager.GetSnowShelterManager().PlayerInNonRuinedShelter())
        {
            m_CurrentTemperature += GameManager.GetSnowShelterManager().GetTemperatureIncreaseCelsius();
        }
        //Debug.Log("TEMP4: " + m_CurrentTemperature);

        if (GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger)
        {
            m_CurrentTemperature += GameManager.GetPlayerManagerComponent().m_IndoorSpaceTrigger.m_TemperatureDeltaCelsius;
        }
        //Debug.Log("TEMP5: " + m_CurrentTemperature);
        if (GameManager.GetPlayerInVehicle().IsInside())
        {
            m_CurrentTemperature += GameManager.GetPlayerInVehicle().GetTempIncrease();
        }
        //Debug.Log("TEMP6: " + m_CurrentTemperature);
        if (!isCountedIndoors)
        {
            m_CurrentTemperature -= outdoorTempDropCelcius;
        }
        //Debug.Log("TEMP7: " + m_CurrentTemperature);
        float m_ArtificalTempIncrease = (float)AccessTools.Field(typeof(Weather), "m_ArtificalTempIncrease").GetValue(__instance);
        m_CurrentTemperature += m_ArtificalTempIncrease;
        AccessTools.Field(typeof(Weather), "m_CurrentTemperatureWithoutHeatSources").SetValue(__instance, m_CurrentTemperature); // New Test
        m_CurrentTemperature += GameManager.GetHeatSourceManagerComponent().GetTemperatureIncrease();
        m_CurrentTemperature += (float)GameManager.GetFeatColdFusion().GetTemperatureCelsiusBonus();
        float GetMinAirTemp = (float)AccessTools.Method(typeof(Weather), "GetMinAirTemp", null, null).Invoke(__instance, null);
        m_CurrentTemperature = Mathf.Max(GetMinAirTemp, m_CurrentTemperature);
        m_CurrentTemperature = Mathf.Clamp(m_CurrentTemperature, float.NegativeInfinity, (float)__instance.m_MaxAirTemperature);
        m_CurrentTemperature = Mathf.Clamp(m_CurrentTemperature, (float)__instance.m_MinAirTemperature, (float)__instance.m_MaxAirTemperature);
        float m_LockedAirTemperature = (float)AccessTools.Field(typeof(Weather), "m_LockedAirTemperature").GetValue(__instance);

        //Debug.Log("TEMP8: " + m_CurrentTemperature);

        if (m_LockedAirTemperature > -1000f)
        {
            m_CurrentTemperature = m_LockedAirTemperature;
        }       
        if (m_CurrentTemperature < RnGl.glLastOutdoorTempNoBliz - m_CurrentBlizzardDegreesDrop)
        {     
            m_CurrentTemperature = RnGl.glLastOutdoorTempNoBliz - m_CurrentBlizzardDegreesDrop;
        }

        //Debug.Log("TEMP9: " + m_CurrentTemperature);

        AccessTools.Field(typeof(Weather), "m_CurrentTemperature").SetValue(__instance, m_CurrentTemperature);
        return false;
    }
}
