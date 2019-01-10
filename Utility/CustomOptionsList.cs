using Harmony;
using UnityEngine;
using ModSettings;
using RelentlessNight;

namespace ModCustomization
{
    public class CustomizationList : ModSettingsBase {

        [Section("Relentless Night Settings")]
        [Name("Perpetual night endgame")]
        [Description("Enables the relentless night endgame where the earth eventually becomes tidally locked with the sun and you're left to survive in perpetual darkness for the rest of the game.\n\nIf disabled, the earth's spin will simply keep slowing down forever and the duration of days and nights will increase indefinitely.")]
        public bool coEndgameActive = RnGl.glEndgameActive;

        [Name("Day after which endgame starts")]
        [Description("The endgame will start during the next nightfall after this many days survived.\n\nSetting this day to 0 will start the endgame right away and the entire run will be in darkness. This setting can be ignored if the endgame is disabled.\n\nNote that your journal will still track days survived in regular 24-hour days no matter how long the earth takes to make one full rotation, just as if the player is keeping track of days with a watch.")]
        [Slider(0f, 500f, 251)]
        public int coEndgameDay = RnGl.glEndgameDay;

        [Name("How fast days and nights get longer")]
        [Description("Controls how fast the earth's spin will slow down, and so how quickly the duration of days and nights will increase.\n\n0% - The earth's spin will not slow down, this feature will essentially be disabled.\n\n50% - After every 24 hours survived, the earth will take 50% of a 24 hour day longer to complete one rotation. In other words 12 hours will be added to the original day length every 24 hours.")]
        [Slider(0f, 100f, 101, NumberFormat = "{0,3:D}%")]
        public int coRotationDecline = RnGl.glRotationDecline;

        [Name("Outdoor effect on indoor temperatures")]
        [Description("Controls how much of an effect outdoor temperature will have on indoor temperatures.\n\n0% - No effect, disables this feature entirely.\n\n100% - 100% of the current outdoor air temperature will be subtracted from the indoor environment's base temperature, making indoor temprerature heavily dependant on temperature outside.")]
        [Slider(0f, 100f, 101, NumberFormat = "{0,3:D}%")]
        public int coTemperatureEffect = RnGl.glTemperatureEffect;

        [Name("Fire Heat Retention")]
        [Description("If enabled, fires indoors will keep the building warm for much longer, even after the fire has burnt out. The hotter the fire, the longer it will keep the place warm. Better insulated environments will keep warm for longer.\n\nEspecially recommend enabling this feature if you've also set indoor temperatures to be affected by outdoor temperatures.")]
        public bool coHeatRetention = RnGl.glHeatRetention;

        [Name("Realistic Freezing Damage")]
        [Description("For temperatures below a feels-like temperature of -40C, the rate at which you suffer freezing damage will be increased. This effect can accumulate up to a maximum of 3 times the original damage rate at a -100C feels-like temperature and below.\n\nNote that you will only start seeing the more extreme end of these temperatures near the end of very long nights or during the perpetual night endgame.")]
        public bool coExtraFreezing = RnGl.glRealisticFreezing;

        [Name("Temperature effects wildlife amount")]
        [Description("If enabled, roaming wildlife will be less abundant in extreme cold and more abundant in relatively warm temperatures.\n\nThis feature never actually eliminates the possibility of finding animals, though it reduces their numbers and the probability of finding them. This feature does not affect fish or fishing.")]
        public bool coWildlifeFreezing = RnGl.glWildlifeFreezing;

        [Name("Wildlife final minimum population")]
        [Description("The wildlife population will decline over time as longer nights make temperatures outside harsher for you and wildlife alike. This setting determines the minimum amount that the wildlife population can decline to.\n\n100% - Wildlife populations will not decline over time, disables this feature.\n\n5% - Only 5% of the normal wildlife population will remain once wildlife decline reaches the minimum population.")]
        [Slider(0f, 100f, 21, NumberFormat = "{0,3:D}%")]
        public int coMinWildlifeAmount = RnGl.glMinWildlifeAmount;

        [Name("Day wildlife decline reaches minimum")]
        [Description("Day which roaming wildlife population will reach the minimum value set above. Setting this day to 0 will start the game at the minimum population.\n\nThis feature also does not effect fish or fishing.")]
        [Slider(0f, 500f, 251)]
        public int coMinWildlifeDay = RnGl.glMinWildlifeDay;

        [Name("Fire fuel burn time")]
        [Description("Determines how long books, wood, and coal fuels for fires will last after being placed in the fire.\n\n1x - No change in fire burn time.\n\n3x - Fire fuels will burn for 3 times longer than normal.")]
        [Slider(1f, 3f, 21, NumberFormat = "{0,2:F1}x")]
        public float coFireFuelFactor = RnGl.glFireFuelFactor;

        [Name("Lantern fuel burn time")]
        [Description("Determines how long lantern fuel will last while in use.\n\n1x - No change in lantern fuel burn time.\n\n3x - Lantern fuel will burn for 3 times longer than normal.")]
        [Slider(1f, 3f, 21, NumberFormat = "{0,2:F1}x")]
        public float coLanternFuelFactor = RnGl.glLanternFuelFactor;
    }

    public class BuildcustomizationList
    {
        private static CustomizationList settings;

        public static void OnLoad()
        {
            settings = new CustomizationList();
            settings.AddToCustomModeMenu(Position.AboveAll);
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnSandboxFinal", null)]
        public static class CustomGameStartedPatch
        {
            public static void Postfix()
            {
                //Debug.Log("--- CustomGameStartedPatch");
                if (!RnGl.rnActive || !GameManager.InCustomMode()) return;

                RnGl.glEndgameActive = settings.coEndgameActive;
                RnGl.glEndgameDay = settings.coEndgameDay;
                RnGl.glRotationDecline = settings.coRotationDecline;
                RnGl.glTemperatureEffect = settings.coTemperatureEffect;
                RnGl.glHeatRetention = settings.coHeatRetention;
                RnGl.glRealisticFreezing = settings.coExtraFreezing;
                RnGl.glWildlifeFreezing = settings.coWildlifeFreezing;
                RnGl.glMinWildlifeDay = settings.coMinWildlifeDay;
                RnGl.glMinWildlifeAmount = settings.coMinWildlifeAmount;
                RnGl.glFireFuelFactor = settings.coFireFuelFactor;
                RnGl.glLanternFuelFactor = settings.coLanternFuelFactor;
            }
        }
    }
}