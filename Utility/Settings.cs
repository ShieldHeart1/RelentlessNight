using System.Reflection;
using ModSettings;

namespace RelentlessNight
{
    internal class RnSettings : JsonModSettings
    {
        [Section(" ")]

        [Name("How Fast Days And Nights Get Longer")]
        [Description("\n\n\nControls how fast the earth's spin slows down, and so how quickly the duration of days and nights increase.\n\n0% - The earth's spin will not slow down, this feature will essentially be disabled.\n\n100% - A full 24 hours gets added to day length every 24 hours survived.")]
        [Slider(0f, 100f, 101, NumberFormat = "{0,3:D}%")]
        public int worldSpinDeclinePercent = Global.worldSpinDeclinePercent;

        [Name("Perpetual Night Endgame")]
        [Description("Enables the Relentless Night endgame where the earth eventually becomes tidally locked with the sun and you are left to survive in perpetual darkness for the rest of the game.\n\nIf disabled, the earth's spin will simply keep slowing down forever.")]
        public bool endgameEnabled = Global.endgameEnabled;

        [Name("Endgame Start Day")]
        [Description("The endgame will start during the next nightfall after this many days survived, measured in 24 hour days.\n\nSetting this to day one will begin the endgame at the start of the game.\n\nYour journal will still track days survived in regular 24-hour days no matter how long the earth takes to make one full rotation.")]
        [Slider(1f, 500f, 500)]
        public int endgameDay = Global.endgameDay;

        [Name("Permanent Aurora At Endgame")]
        [Description("The aurora borealis will be present throughout the endgame, ensuring you some light at the new dark side of earth.")]
        public bool endgameAuroraEnabled = Global.endgameAuroraEnabled;

        [Name("Minimum Air Temperature")]
        [Description("The minimum air temperature that will be reached at the endgame or during a sufficiently long night. Temperatures do not drop unless nights are set to get longer. This temperature decline, along with all other Relentless Night settings, will stack together with vanilla settings that adjust the same feature.")]
        [Slider(-20f, -120f, 101, NumberFormat = "{0,3:D}°C")]
        public int minAirTemperature = Global.minAirTemperature;

        [Name("Outdoor Affect On Indoor Temperatures")]
        [Description("Controls how much of an effect outdoor temperature will have on indoor temperatures, with a high enough setting no place will be completely safe from the cold.")]
        [Slider(0f, 100f, 101, NumberFormat = "{0,3:D}%")]
        public int indoorOutdoorTemperaturePercent = Global.indoorOutdoorTemperaturePercent;

        [Name("Carcass Moving")]
        [Description("Add ability to haul medium sized kills such as deer and wolves around the world map, including indoors.")]
        public bool carcassMovingEnabled = Global.carcassMovingEnabled;

        [Name("Electric Torch Lighting")]
        [Description("Add ability to light torches using live wires and household outlets during auroras.")]
        public bool electricTorchLightingEnabled = Global.electricTorchLightingEnabled;

        [Name("Fire Heat Retention")]
        [Description("Fires indoors will keep the building warm for much longer, even after the fire has burnt out. The hotter the fire, the longer it will keep the place warm. Better insulated indoor locations will keep warm for longer.")]
        public bool heatRetentionEnabled = Global.heatRetentionEnabled;

        [Name("Realistic Freezing Damage")]
        [Description("For temperatures below a 'feels like' temperature of -40C, the rate at which you suffer freezing damage will be increased. This effect can accumulate up to a maximum of 3x the original damage rate at extremely low temperatures.")]
        public bool realisticFreezingEnabled = Global.realisticFreezingEnabled;

        [Name("Wildlife Final Minimum Population")]
        [Description("The wildlife population and respawn rates will decline over time as longer nights make temperatures outside harsher for you and wildlife alike. This setting determines the minimum population that the wildlife population can fall to.\n\n0% - Eventual extinction of all roaming animals.\n\n100% - Wildlife populations will not decline over time, disables this feature.")]
        [Slider(0f, 100f, 101, NumberFormat = "{0,3:D}%")]
        public int minWildlifePercent = Global.minWildlifePercent;

        [Name("Day Wildlife Decline Reaches Minimum")]
        [Description("Day at which wildlife populations will reach the minimum value set above. Setting this day one will start the game at the minimum population. This does not affect fish or fishing.")]
        [Slider(1f, 500f, 500)]
        public int minWildlifeDay = Global.minWildlifeDay;

        [Name("Fish Final Minimum Population")]
        [Description("Fish population and chances of catching fish will decline over time. This setting determines the minimum that fish population can fall to.\n\n0% - Catching fish will be impossible.\n\n100% - Fishing chances will not decline over time, disables this feature.")]
        [Slider(0f, 100f, 101, NumberFormat = "{0,3:D}%")]
        public int minFishPercent = Global.minFishPercent;

        [Name("Day Fish Decline Reaches Minimum")]
        [Description("Day at which fishing chances reach the minimum value set above. Setting this to day one will start the game at the minimum population.")]
        [Slider(1f, 500f, 500)]
        public int minFishDay = Global.minFishDay;

        [Name("Fire Fuel Burn Time")]
        [Description("Extends how long wood, coal, and book fuels for fires will last after being placed in the fire.\n\n1x - No change in fuel burn time.")]
        [Slider(1f, 3f, 21, NumberFormat = "{0,2:F1}x")]
        public float fireFuelDurationMultiplier = Global.fireFuelDurationMultiplier;

        [Name("Lantern Fuel Burn Time")]
        [Description("Extends how long lantern fuel will last while in use.\n\n1x - No change in lantern fuel burn time.")]
        [Slider(1f, 5f, 41, NumberFormat = "{0,2:F1}x")]
        public float lanternFuelDurationMultiplier = Global.lanternFuelDurationMultiplier;

        [Name("Torch Burn Time")]
        [Description("Extends how long torches will last while in use.\n\n1x - No change in torch burn time.")]
        [Slider(1f, 5f, 41, NumberFormat = "{0,2:F1}x")]
        public float torchBurnDurationMultiplier = Global.torchBurnDurationMultiplier;

        protected override void OnConfirm()
        {
            base.OnConfirm();
            Global.SetOptionGlobalsFromModOptions();
            WildlifeManager.ResetWildlifePopulations();
        }
        protected override void OnChange(FieldInfo field, object oldValue, object newValue)
        {
            if (field.Name == nameof(endgameEnabled) || field.Name == nameof(worldSpinDeclinePercent) || field.Name == nameof(minWildlifePercent) || field.Name == nameof(minFishPercent))
            {
                RefreshFields();
            }
        }
        public void RefreshFields()
        {
            SetFieldVisible(nameof(endgameDay), endgameEnabled);
            SetFieldVisible(nameof(endgameAuroraEnabled), endgameEnabled);
            SetFieldVisible(nameof(minAirTemperature), worldSpinDeclinePercent != 0);
            SetFieldVisible(nameof(minWildlifeDay), minWildlifePercent != 100);
            SetFieldVisible(nameof(minFishDay), minFishPercent != 100);
        }
    }    

    internal class Settings
    {
        public static RnSettings options;

        public static void OnLoad()
        {
            Global.SetOptionGlobalsToRnClassic();

            options = new RnSettings();            
            options.AddToModSettings("Relentless Night");
        }
        internal static void SetModSettingsToOptionGlobals()
        {
            options.worldSpinDeclinePercent = Global.worldSpinDeclinePercent;
            options.endgameEnabled = Global.endgameEnabled;
            options.endgameDay = Global.endgameDay;
            options.endgameAuroraEnabled = Global.endgameAuroraEnabled;
            options.minAirTemperature = Global.minAirTemperature;
            options.indoorOutdoorTemperaturePercent = Global.indoorOutdoorTemperaturePercent;
            options.carcassMovingEnabled = Global.carcassMovingEnabled;
            options.electricTorchLightingEnabled = Global.electricTorchLightingEnabled;
            options.heatRetentionEnabled = Global.heatRetentionEnabled;
            options.realisticFreezingEnabled = Global.realisticFreezingEnabled;
            options.minWildlifePercent = Global.minWildlifePercent;
            options.minWildlifeDay = Global.minWildlifeDay;
            options.minFishPercent = Global.minFishPercent;
            options.minFishDay = Global.minFishDay;
            options.fireFuelDurationMultiplier = Global.fireFuelDurationMultiplier;
            options.lanternFuelDurationMultiplier = Global.lanternFuelDurationMultiplier;
            options.torchBurnDurationMultiplier = Global.torchBurnDurationMultiplier;
        }
    }
}