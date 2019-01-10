using System;

namespace RelentlessNight
{    
    internal class RnGl
    {
        public const string RnVersion = "v3.02";
        public const string RnSlotPrefix = "relentless";
        public const Panel_MainMenu.MainMenuItem.MainMenuItemType MY_MENU_ITEM_TYPE = (Panel_MainMenu.MainMenuItem.MainMenuItemType)7;
        public const SaveSlotType RnSlotType = (SaveSlotType)21070;

        public static bool glEndgameActive = true;
        public static int glEndgameDay = 200;
        public static int glRotationDecline = 15;
        public static int glTemperatureEffect = 50;
        public static bool glHeatRetention = true;
        public static bool glRealisticFreezing = true;
        public static bool glWildlifeFreezing = true;
        public static int glMinWildlifeDay = 200;
        public static int glMinWildlifeAmount = 10;
        public static float glFireFuelFactor = 1.5f;
        public static float glLanternFuelFactor = 1.5f;

        public static int glDayTidallyLocked = -1;
        public static int glDayNum = 1;
        public static float glLastOutdoorTempNoBliz = -10f;
        public static bool glIsCarryingCarcass = false;
        public static string glSerializedCarcass; 
        public static float rnCurrentRetainedHeat = 0f;

        public static bool rnFireShouldHeatWholeScene = false;
        public static float rnIndoorTempFactor = 1f;
        public static float rnHeatDissapationFactor = 15f;

        public static bool rnActive = false;
        public static int rnTimeAccel = 1;
        public static bool rnFeatsActive = false;

        public static float rnElapsedHours;
        public static float rnElapsedHoursAccumulator;
        public static float rnNormNum5;

        public static void UpdateRnGlobalsForScene()
        {
            string text = GameManager.m_ActiveScene;
            text = text.ToLower();
            bool flag = GameManager.GetWeatherComponent().IsIndoorScene();
            if (flag)
            {
                bool flag2 = text.Contains("cave") || text.Contains("mine");
                if (flag2)
                {
                    rnIndoorTempFactor = 0.80f;
                    rnFireShouldHeatWholeScene = false;
                    rnHeatDissapationFactor = 3f;
                }
                else
                {
                    bool flag3 = text.Contains("whalingwarehouse") || text.Contains("dam") || text.Contains("damtransitionzone") || 
                        text.Contains("whalingship") || text.Contains("barnhouse") || text.Contains("maintenanceshed");
                    if (flag3)
                    {
                        rnIndoorTempFactor = 1f;
                        rnFireShouldHeatWholeScene = false;
                        rnHeatDissapationFactor = 3f;
                    }
                    else
                    {
                        rnIndoorTempFactor = 1f;
                        rnFireShouldHeatWholeScene = true;
                        rnHeatDissapationFactor = 1f;
                    }
                }
            }
            else
            {
                rnFireShouldHeatWholeScene = false;
                rnHeatDissapationFactor = 15f;
            }
        }        
    }

    internal class RnSd
    {
        public bool sdEndgameActive;
        public int sdEndgameDay;
        public int sdRotationDecline;
        public int sdTemperatureEffect;
        public bool sdHeatRetenion;
        public bool sdRealisticFreezing;
        public bool sdWildlifeFreezing;
        public int sdMinWildlifeDay;
        public int sdMinWildlifeAmount;
        public float sdFireFuelFactor;
        public float sdLanternFuelFactor;
        public int sdDayTidallyLocked;
        public int sdDayNum;
        public float sdLastOutdoorTempNoBliz;
        public bool sdIsCarryingCarcass;
        public string sdSerializedCarcass;
    }
}