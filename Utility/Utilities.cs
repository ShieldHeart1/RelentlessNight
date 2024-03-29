﻿using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using IL2CPP = Il2CppSystem.Collections.Generic;

namespace RelentlessNight
{
	internal class Utilities
	{
		// A multiplier to speed up time through the console command "rn_hypertime", helps test time-dependent features much faster
		internal static int devTimeSpeedMultiplier = 1;

		[HarmonyPatch(typeof(ConsoleManager), "RegisterCommands", new Type[] { })]
		internal class ConsoleManager_RegisterCommands
		{
			private static void Postfix()
			{
				uConsole.RegisterCommand("rn_active", new Action(ModLogIsRnActive));
				uConsole.RegisterCommand("rn_globals", new Action(ModLogGlobals));
				uConsole.RegisterCommand("rn_hypertime", new Action(ToggleTimeSpeedUp));
				uConsole.RegisterCommand("rn_scene", new Action(ModLogActiveScene));
				uConsole.RegisterCommand("rn_panels", new Action(ModLogListActivePanels));
				uConsole.RegisterCommand("rn_fires", new Action(ModLogListFires));
				uConsole.RegisterCommand("rn_reset_carcass_moving", new Action(CarcassMoving.ResetCarcassMoving));
			}
		}

		internal static void ModLog(string message)
		{
			MelonLoader.MelonLogger.Msg(ConsoleColor.Cyan, "RN > " + message);
		}
		internal static void ModWarn(string message)
		{
			MelonLoader.MelonLogger.Warning("RN W > " + message);
		}
		internal static void ModError(string message)
		{
			MelonLoader.MelonLogger.Error("RN E > " + message);
		}
		internal static void PlayGameErrorAudio()
		{
			GameAudioManager.PlaySound(GameAudioManager.Instance.m_ErrorAudio, GameAudioManager.Instance.gameObject);
		}
		internal static void DisallowActionWithGameMessage(string message)
		{
			PlayGameErrorAudio();
			HUDMessage.AddMessage(Localization.Get(message), false);
		}
		internal static void DisallowActionWithModMessage(string message)
		{
			PlayGameErrorAudio();
			HUDMessage.AddMessage(message, false);
		}
		internal static List<GameObject> GetRootObjects()
		{
			List<GameObject> rootObj = new List<GameObject>();
			Scene scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene();
			GameObject[] sceneObj = scene.GetRootGameObjects();
			foreach (GameObject obj in sceneObj)
			{
				rootObj.Add(obj);
			}
			return rootObj;
		}

		internal static void GetChildrenWithNameArray(GameObject obj, string[] lookup, Dictionary<int, GameObject> found)
		{
			if (obj.transform.childCount > 0)
			{
				for (int i = 0; i < obj.transform.childCount; i++)
				{
					GameObject child = obj.transform.GetChild(i).gameObject;
					if (lookup.Any(child.name.ToLower().Contains) && !found.ContainsKey(child.GetInstanceID()))
					{
						found.Add(child.GetInstanceID(), child);
					}
					GetChildrenWithNameArray(child, lookup, found);
				}
			}
		}

		internal static void ModLogIsRnActive()
		{
			ModLog(MenuManager.modEnabled.ToString());
		}
		internal static void ModLogGlobals()
		{
			ModLog("worldSpinDeclinePercent: " + Global.worldSpinDeclinePercent.ToString());
			ModLog("endgameEnabled: " + Global.endgameEnabled.ToString());
			ModLog("endgameDay: " + Global.endgameDay.ToString());
			ModLog("endgameAuroraEnabled: " + Global.endgameAuroraEnabled.ToString());
			ModLog("minAirTemperature: " + Global.minAirTemperature.ToString());
			ModLog("indoorOutdoorTemperaturePercent: " + Global.indoorOutdoorTemperaturePercent.ToString());
			ModLog("carcassMovingEnabled: " + Global.carcassMovingEnabled.ToString());
			ModLog("electricTorchLightingEnabled: " + Global.electricTorchLightingEnabled.ToString());
			ModLog("heatRetentionEnabled: " + Global.heatRetentionEnabled.ToString());
			ModLog("realisticFreezingEnabled: " + Global.realisticFreezingEnabled.ToString());
			ModLog("minWildlifePercent: " + Global.minWildlifePercent.ToString());
			ModLog("minWildlifeDay: " + Global.minWildlifeDay.ToString());
			ModLog("minFishPercent: " + Global.minFishPercent.ToString());
			ModLog("minFishDay: " + Global.minFishDay.ToString());
			ModLog("fireFuelDurationMultiplier: " + Global.fireFuelDurationMultiplier.ToString());
			ModLog("lanternFuelDurationMultiplier: " + Global.lanternFuelDurationMultiplier.ToString());
			ModLog("torchBurnDurationMultiplier: " + Global.torchBurnDurationMultiplier.ToString());
		}
		internal static void ModLogActiveScene()
		{
			ModLog(GameManager.m_ActiveScene);
		}
		internal static void ModLogListActivePanels()
		{
			foreach (Il2CppSystem.Type panel in InterfaceManager.m_MainMenuPanels)
			{
				ModLog(panel.ToString());
			}
		}
		internal static void ToggleTimeSpeedUp()
		{
			if (devTimeSpeedMultiplier == 1)
			{
				GameManager.GetPlayerManagerComponent().m_God = true;
				devTimeSpeedMultiplier = 5000;
			}
			else
			{
				GameManager.GetPlayerManagerComponent().m_God = false;
				devTimeSpeedMultiplier = 1;
			}
		}
		internal static void ModLogListFires()
		{
			IL2CPP.List<HeatSource> heatsources = GameManager.GetHeatSourceManagerComponent().m_HeatSources;

			for (int i = 0; i < heatsources.Count; i++)
			{
				ModLog(i.ToString() + ": " + heatsources[i].GetTempIncrease(GameManager.GetPlayerTransform().position).ToString());
			}
			ModLog(HeatRetentionManager.currentDegreesHeatLossPerHour.ToString());
		}
	}
}