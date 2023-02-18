using HarmonyLib;
using static RelentlessNight.SaveManager;

namespace RelentlessNight
{
	internal class SaveManager
	{
		internal const string rnSavePrefix = "RNight";

		internal static bool isFirstMenuInitialize = true;

		internal class ModSaveData
		{
			// Mod settings
			public int worldSpinDeclinePercent;
			public bool endgameEnabled;
			public int endgameDay;
			public bool endgameAuroraEnabled;
			public int minAirTemperature;
			public int indoorOutdoorTemperaturePercent;
			public bool carcassMovingEnabled;
			public bool electricTorchLightingEnabled;
			public bool heatRetentionEnabled;
			public bool realisticFreezingEnabled;
			public int minWildlifePercent;
			public int minWildlifeDay;
			public int minFishPercent;
			public int minFishDay;
			public float fireFuelDurationMultiplier;
			public float lanternFuelDurationMultiplier;
			public float torchBurnDurationMultiplier;

			// Other mod data
			public int lastTemperatureOffsetDay;
			public int dayTidalLocked;
		}

		[HarmonyPatch(typeof(Panel_MainMenu), nameof(Panel_MainMenu.OnEnable))]
		internal class Panel_MainMenu_OnEnable
		{
			private static void Postfix()
			{
				if (isFirstMenuInitialize)
				{
					Utilities.ModLog("Relentless Night V" + Global.RnVersion + " Loaded Successfully");
					LoadRnSaveFiles();
					isFirstMenuInitialize = false;
				}
				//string lastRnSaveData = GetLastRnSaveData();
				//if (lastRnSaveData != null)
				//{
				//    ModSaveData data = JsonConvert.DeserializeObject<ModSaveData>(lastRnSaveData);

				//    Global.SetOptionGlobalsFromSave(data);
				//    MaybeUpdateOptionGlobalsForNewModVersion(data);
				//    Settings.SetModSettingsToOptionGlobals();
				//}
			}
		}
		[HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.SaveGlobalData), new Type[] { typeof(SlotData) })]
		internal class SaveGameSystem_SaveGlobalData
		{
			private static void Postfix(SlotData slot)
			{
				//if (!MenuManager.modEnabled) return;
				//SaveGlobalsToSaveFile(gameMode, name);
			}
		}
		[HarmonyPatch(typeof(SaveGameSystem), nameof(SaveGameSystem.RestoreGlobalData), new Type[] { typeof(string) })]
		internal class SaveGameSystem_RestoreGlobalData
		{
			private static void Postfix()
			{
				//if (!MenuManager.modEnabled || GameManager.m_ActiveScene == "MainMenu") return;

				////                string modData = SaveGameSlots.LoadDataFromSlot(SaveGameSystem.GetCurrentSaveName(), savedataKey);
				//string modData = Global.dataManager.Load();
				//if (modData != null)
				//{
				//	//ModSaveData data = JsonConvert.DeserializeObject<ModSaveData>(modData);
				//	ModSaveData data = JSON.Load(modData).Make<ModSaveData>();


				//	Global.SetOptionGlobalsFromSave(data);
				//	Global.SetGameGlobalsFromSave(data);
				//	MaybeUpdateOptionGlobalsForNewModVersion(data);
				//	Settings.SetModSettingsToOptionGlobals();
				//}
				//else
				//{
				//	Global.SetOptionGlobalsToRnClassic();
				//	Global.SetGameGlobalsForNewGame();
				//}
			}
		}
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.LaunchSandbox))]
		internal static class GameManager_LaunchSandbox
		{
			private static void Postfix()
			{
				if (!MenuManager.modEnabled) return;
				Global.SetOptionGlobalsFromModOptions();
			}
		}
		[HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.GetSlotPrefix), new Type[] { typeof(SaveSlotType) })]
		internal class SaveGameSlots_GetSlotPrefix
		{

			private static void Postfix(ref string __result)
			{
				if (!MenuManager.modEnabled) return;
				__result = rnSavePrefix;
			}
		}
		[HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.GetSaveSlotTypeFromName), new Type[] { typeof(string) })]
		internal class SaveGameSlots_GetSaveSlotTypeFromName
		{

			private static void Postfix(ref SaveSlotType __result)
			{
				if (!MenuManager.modEnabled) return;
				__result = SaveSlotType.SANDBOX;
			}
		}

		[HarmonyPatch(typeof(SaveGameSlotHelper), nameof(SaveGameSlotHelper.GetSaveSlotInfoList), new Type[] { typeof(SaveSlotType) })]
		internal class SaveGameSlotHelper_GetSaveSlotInfoList
		{
			private static void Postfix(SaveSlotType saveSlotType, ref Il2CppSystem.Collections.Generic.List<SaveSlotInfo> __result)
			{
				if (saveSlotType != SaveSlotType.SANDBOX)
				{
					return;
				}

				Il2CppSystem.Collections.Generic.List<SaveSlotInfo> newList = new();

				foreach (SaveSlotInfo slotInfo in __result)
				{
					if (MenuManager.modEnabled)
					{
						if (slotInfo.m_SaveSlotName.Contains(rnSavePrefix))
						{
							newList.Add(slotInfo);
						}
					}
					else
					{
						if (!slotInfo.m_SaveSlotName.Contains(rnSavePrefix))
						{
							newList.Add(slotInfo);
						}
					}
				}
				__result = newList;
			}
		}

		internal static void SaveGlobalsToSaveFile(SaveSlotType gameMode, string name)
		{
#warning TODO - rework saving to mod settings and restore from there also

			ModSaveData data = new ModSaveData
			{
				worldSpinDeclinePercent = Global.worldSpinDeclinePercent,
				endgameEnabled = Global.endgameEnabled,
				endgameDay = Global.endgameDay,
				endgameAuroraEnabled = Global.endgameAuroraEnabled,
				minAirTemperature = Global.minAirTemperature,
				indoorOutdoorTemperaturePercent = Global.indoorOutdoorTemperaturePercent,
				carcassMovingEnabled = Global.carcassMovingEnabled,
				electricTorchLightingEnabled = Global.electricTorchLightingEnabled,
				heatRetentionEnabled = Global.heatRetentionEnabled,
				realisticFreezingEnabled = Global.realisticFreezingEnabled,
				minWildlifePercent = Global.minWildlifePercent,
				minWildlifeDay = Global.minWildlifeDay,
				minFishPercent = Global.minFishPercent,
				minFishDay = Global.minFishDay,
				fireFuelDurationMultiplier = Global.fireFuelDurationMultiplier,
				lanternFuelDurationMultiplier = Global.lanternFuelDurationMultiplier,
				torchBurnDurationMultiplier = Global.torchBurnDurationMultiplier,

				lastTemperatureOffsetDay = Global.lastTemperatureOffsetDay,
				dayTidalLocked = Global.dayTidalLocked,
			};
			Global.dataManager.Save(JSON.Dump(data, EncodeOptions.NoTypeHints | EncodeOptions.PrettyPrint));
			//            SaveGameSlots.SaveDataToSlot(gameMode, SaveGameSystem.m_CurrentEpisode, SaveGameSystem.m_CurrentGameId, name, savedataKey, JsonConvert.SerializeObject(data));
		}
		internal static void LoadRnSaveFiles()
		{
			int c = 0;
			foreach (FileInfo saveFile in GetAllGameSaveFiles())
			{
				if (IsCompatibleRnSave(saveFile.Name))
				{
					SaveGameDataPC.LoadData(saveFile.Name);
					c++;
				}
			}
			Utilities.ModLog("Relentless Night | Loaded " + c.ToString() + " saves");
		}
		internal static FileInfo[] GetAllGameSaveFiles()
		{
			DirectoryInfo directoryInfo = new DirectoryInfo(PersistentDataPath.m_Path);
			return directoryInfo.GetFiles("*", SearchOption.AllDirectories);
		}
		internal static bool IsCompatibleRnSave(string fileName)
		{
			return fileName.StartsWith(rnSavePrefix);
		}
		internal static bool IsIncompatibleRnSave(string fileName)
		{
			// Pre-4.40 save file name prefix & pre 4.6.0
			return (fileName.StartsWith("ep1relentless") || fileName.StartsWith("relentless"));
		}
		internal static void MaybeUpdateOptionGlobalsForNewModVersion(ModSaveData data)
		{
			if (data.endgameDay == 0) Global.endgameDay = 1;
			if (data.minWildlifeDay == 0) Global.minWildlifeDay = 1;
			if (data.minFishDay == 0)
			{
				Global.minFishPercent = 20;
				Global.minFishDay = 120;
			}
		}
	}
}