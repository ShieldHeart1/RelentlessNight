namespace RelentlessNight
{
    public class Global
    {
        internal const string RnVersion = "4.40";

        // Rn globals representing current mod settings
        internal static int worldSpinDeclinePercent;
        internal static bool endgameEnabled;        
        internal static int endgameDay;
        internal static bool endgameAuroraEnabled;
        internal static int minAirTemperature;
        internal static int indoorOutdoorTemperaturePercent;
        internal static bool carcassMovingEnabled;
        internal static bool electricTorchLightingEnabled;
        internal static bool heatRetentionEnabled;
        internal static bool realisticFreezingEnabled;
        internal static int minWildlifeDay;
        internal static int minWildlifePercent;
        internal static float fireFuelDurationMultiplier;
        internal static float lanternFuelDurationMultiplier;
        internal static float torchBurnDurationMultiplier;

        // Additional globals to keep track of in order for proper save/load operation
        internal static int lastTemperatureOffsetDay;
        internal static int dayTidalLocked;

        public static void SetGameGlobalsForNewGame()
        {
            lastTemperatureOffsetDay = 0;
            dayTidalLocked = -1;
        }
        public static void SetGameGlobalsFromSave(SaveManager.ModSaveData data)
        {
            lastTemperatureOffsetDay = data.lastTemperatureOffsetDay;
            dayTidalLocked = data.dayTidalLocked;
        }
        internal static void SetOptionGlobalsFromSave(SaveManager.ModSaveData data)
        {
            worldSpinDeclinePercent = data.worldSpinDeclinePercent;
            endgameEnabled = data.endgameEnabled;
            endgameDay = data.endgameDay;
            endgameAuroraEnabled = data.endgameAuroraEnabled;
            minAirTemperature = data.minAirTemperature;
            indoorOutdoorTemperaturePercent = data.indoorOutdoorTemperaturePercent;
            carcassMovingEnabled = data.carcassMovingEnabled;
            electricTorchLightingEnabled = data.electricTorchLightingEnabled;
            heatRetentionEnabled = data.heatRetentionEnabled;
            realisticFreezingEnabled = data.realisticFreezingEnabled;
            minWildlifeDay = data.minWildlifeDay;
            minWildlifePercent = data.minWildlifePercent;
            fireFuelDurationMultiplier = data.fireFuelDurationMultiplier;
            lanternFuelDurationMultiplier = data.lanternFuelDurationMultiplier;
            torchBurnDurationMultiplier = data.torchBurnDurationMultiplier;
        }
        internal static void SetOptionGlobalsFromModOptions()
        {
            worldSpinDeclinePercent = Settings.options.worldSpinDeclinePercent;
            endgameEnabled = Settings.options.endgameEnabled;
            endgameDay = Settings.options.endgameDay;
            endgameAuroraEnabled = Settings.options.endgameAuroraEnabled;
            minAirTemperature = Settings.options.minAirTemperature;
            indoorOutdoorTemperaturePercent = Settings.options.indoorOutdoorTemperaturePercent;
            carcassMovingEnabled = Settings.options.carcassMovingEnabled;
            electricTorchLightingEnabled = Settings.options.electricTorchLightingEnabled;
            heatRetentionEnabled = Settings.options.heatRetentionEnabled;
            realisticFreezingEnabled = Settings.options.realisticFreezingEnabled;
            minWildlifeDay = Settings.options.minWildlifeDay;
            minWildlifePercent = Settings.options.minWildlifePercent;
            fireFuelDurationMultiplier = Settings.options.fireFuelDurationMultiplier;
            lanternFuelDurationMultiplier = Settings.options.lanternFuelDurationMultiplier;
            torchBurnDurationMultiplier = Settings.options.torchBurnDurationMultiplier;
        }
        internal static void SetOptionGlobalsToRnClassic()
        {
            worldSpinDeclinePercent = 15;
            endgameEnabled = true;
            endgameDay = 100;
            endgameAuroraEnabled = true;
            minAirTemperature = -80;
            indoorOutdoorTemperaturePercent = 60;
            carcassMovingEnabled = true;
            electricTorchLightingEnabled = true;
            heatRetentionEnabled = true;
            realisticFreezingEnabled = true;
            minWildlifeDay = 120;
            minWildlifePercent = 10;
            fireFuelDurationMultiplier = 1.5f;
            lanternFuelDurationMultiplier = 1.5f;
            torchBurnDurationMultiplier = 1.5f;
        }
        internal static void SetOptionGlobalsToMasochist()
        {
            worldSpinDeclinePercent = 0;
            endgameEnabled = true;
            endgameDay = 0;
            endgameAuroraEnabled = true;
            minAirTemperature = -80;
            indoorOutdoorTemperaturePercent = 60;
            carcassMovingEnabled = true;
            electricTorchLightingEnabled = true;
            heatRetentionEnabled = true;
            realisticFreezingEnabled = true;
            minWildlifeDay = 120;
            minWildlifePercent = 10;
            fireFuelDurationMultiplier = 1.5f;
            lanternFuelDurationMultiplier = 1.5f;
            torchBurnDurationMultiplier = 1.5f;
        }
        internal static void SetOptionGlobalsToEndMeNow()
        {
            worldSpinDeclinePercent = 0;
            endgameEnabled = true;
            endgameDay = 0;
            endgameAuroraEnabled = true;
            minAirTemperature = -120;
            indoorOutdoorTemperaturePercent = 100;
            carcassMovingEnabled = true;
            electricTorchLightingEnabled = false;
            heatRetentionEnabled = false;
            realisticFreezingEnabled = true;
            minWildlifeDay = 0;
            minWildlifePercent = 0;
            fireFuelDurationMultiplier = 1f;
            lanternFuelDurationMultiplier = 1f;
            torchBurnDurationMultiplier = 1f;
        }
    }
}