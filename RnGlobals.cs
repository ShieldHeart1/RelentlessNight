using System;
using System.Collections.Generic;

namespace RelentlessNight
{    
    public class RnGlobal
    {
        public const string RnVersion = "v4.30";
        public const string RnSlotPrefix = "relentless";
        public const Panel_MainMenu.MainMenuItem.MainMenuItemType MY_MENU_ITEM_TYPE = (Panel_MainMenu.MainMenuItem.MainMenuItemType)7;
        public const SaveSlotType RnSlotType = (SaveSlotType)21070;

        public static bool glEndgameActive = true;
        public static int glEndgameDay = 100;
        public static bool glEndgameAurora = false;
        public static int glRotationDecline = 15;
        public static int glTemperatureEffect = 50;
        public static int glMinimumTemperature = -80;
        public static bool glHeatRetention = true;
        public static bool glRealisticFreezing = false;
        public static bool glWildlifeFreezing = true;
        public static int glMinWildlifeDay = 120;
        public static int glMinWildlifeAmount = 10;
        public static float glFireFuelFactor = 1.5f;
        public static float glLanternFuelFactor = 1.5f;
        public static float glTorchFuelFactor = 1.5f;

        public static float glOutdoorTempWithoutBlizDrop = -10f;
        public static int glCurrentDay = 0;
        public static int glCurrentDayTempOffset = 0;

        public static int glDayTidallyLocked = -1;
        public static int glDayNum = 1;
        public static bool glIsCarryingCarcass = false;
        public static string glSerializedCarcass;       

        public static bool rnActive = false;
        public static int rnTimeAccel = 1;
        public static bool rnFeatsActive = false;
        public static bool rnWordMarkSet = false;

        public static float rnElapsedHours;
        public static float rnElapsedHoursAccumulator;
        public static float rnHours;                
    }

    internal class RnSd
    {
        public bool sdEndgameActive;
        public int sdEndgameDay;
        public bool sdEndgameAurora;
        public int sdRotationDecline;
        public int sdTemperatureEffect;
        public int sdMinimumTemperature;
        public bool sdHeatRetention;
        public bool sdRealisticFreezing;
        public bool sdWildlifeFreezing;
        public int sdMinWildlifeDay;
        public int sdMinWildlifeAmount;
        public float sdFireFuelFactor;
        public float sdLanternFuelFactor;
        public float sdTorchFuelFactor;
        public int sdDayTidallyLocked;
        public int sdDayNum;
        public bool sdIsCarryingCarcass;
        public string sdSerializedCarcass;
    }
}