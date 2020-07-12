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
    
        float curDay = GameManager.GetTimeOfDayComponent().GetDayNumber();
        float m_CurrentTemperature = __instance.m_CurrentTemperature;
        int curCelestialHour = GameManager.GetTimeOfDayComponent().GetHour();
        int num2 = curCelestialHour * 60 + GameManager.GetTimeOfDayComponent().GetMinutes();
        float outdoorTempDropCelcius = GameManager.GetExperienceModeManagerComponent().GetOutdoorTempDropCelcius(curDay);


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
            float num9 = (float)(__instance.m_HourCoolingBegins - __instance.m_HourWarmingBegins) * 60f;

            if (m_TempLow - tempDecrease < -100f)
            {
                tempDecrease = 100f + m_TempLow;
            }

            if (m_TempHigh + tempIncrease - outdoorTempDropCelcius > 2f)
            {
                tempIncrease = 2f - m_TempHigh + outdoorTempDropCelcius;
            }
            __instance.m_CurrentTemperature = m_TempLow - tempDecrease + (float)num8 / num9 * (m_TempHigh + (tempIncrease + tempDecrease) - m_TempLow);
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

        m_CurrentTemperature += __instance.m_ArtificalTempIncrease;
        __instance.m_CurrentTemperatureWithoutHeatSources = m_CurrentTemperature;
        m_CurrentTemperature += GameManager.GetHeatSourceManagerComponent().GetTemperatureIncrease(GameManager.GetPlayerTransform().position);
        m_CurrentTemperature += GameManager.GetFeatColdFusion().GetTemperatureCelsiusBonus();        
        m_CurrentTemperature = Mathf.Max(__instance.GetMinAirTemp(), m_CurrentTemperature);
        m_CurrentTemperature = Mathf.Clamp(m_CurrentTemperature, float.NegativeInfinity, __instance.m_MaxAirTemperature);
        m_CurrentTemperature = Mathf.Clamp(m_CurrentTemperature, __instance.m_MinAirTemperature, __instance.m_MaxAirTemperature);

        //Debug.Log("TEMP8: " + m_CurrentTemperature);

        if (__instance.m_LockedAirTemperature > -1000f)
        {
            m_CurrentTemperature = __instance.m_LockedAirTemperature;
        }       
        if (m_CurrentTemperature < RnGl.glLastOutdoorTempNoBliz - m_CurrentBlizzardDegreesDrop)
        {     
            m_CurrentTemperature = RnGl.glLastOutdoorTempNoBliz - m_CurrentBlizzardDegreesDrop;
        }

        //Debug.Log("TEMP9: " + m_CurrentTemperature);

        __instance.m_CurrentTemperature = m_CurrentTemperature;

        return false;
    }
}
