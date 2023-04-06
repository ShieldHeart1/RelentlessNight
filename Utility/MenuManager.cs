using System;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;
using static RelentlessNight.MenuManager;

namespace RelentlessNight
{
	internal class MenuManager
	{
		internal const int rnButtonIndex = 3;

		internal static bool modEnabled = false;
		internal static bool featsPanelEnabled = false;

		[HarmonyPatch(typeof(Panel_MainMenu), nameof(Panel_MainMenu.AddMenuItem), new Type[] { typeof(int) })]
		internal static class Panel_MainMenu_AddMenuItem
		{
			private static void Prefix(Panel_MainMenu __instance, int itemIndex)
			{

				if (__instance.IsSubMenuEnabled() || itemIndex != rnButtonIndex) return;

				AddRnMainMenuButton(__instance, itemIndex);
			}
		}
		// Enables Relentless Night when user clicks on main menu "Relentless Night" button
		[HarmonyPatch(typeof(BasicMenu), nameof(BasicMenu.InternalClickAction), new Type[] { typeof(int), typeof(bool) })]
		internal class BasicMenu_InternalClickAction
		{
			private static void Prefix(BasicMenu __instance, int index)
			{
				if (__instance.m_ItemModelList[index].m_LabelText == "Relentless Night")
				{
					modEnabled = true;
				}
			}
		}
		// Disables Relentless Night when user returns to main menu
		[HarmonyPatch(typeof(Panel_MainMenu), nameof(Panel_MainMenu.ConfigureMenu))]
		internal class Panel_MainMenu_ConfigureMenu
		{
			private static void Prefix(Panel_MainMenu __instance)
			{
				if (InterfaceManager.IsMainMenuEnabled() && !__instance.IsSubMenuEnabled())
				{
					modEnabled = false;
				}
			}
			private static void Postfix(Panel_MainMenu __instance)
			{
				if (!modEnabled || InterfaceManager.IsMainMenuEnabled()) return;

				AddRnTitleHeaderToBasicMenu(__instance.m_BasicMenu);
			}
		}
		// Prevents a bug where Relentless Night game slots cannot be deleted or renamed
		[HarmonyPatch(typeof(Panel_ChooseSandbox), nameof(Panel_ChooseSandbox.ProcessMenu))]
		internal class Panel_ChooseSandbox_ProcessMenu
		{
			private static void Postfix(Panel_ChooseSandbox __instance)
			{
				if (!modEnabled) return;
				{
					UtilsPanelChoose.ProcessMenu(__instance.m_BasicMenu, true, true, new Action(__instance.BackWithouSFX), __instance.m_MouseButtonRename, new Action(__instance.OnRename), __instance.m_MouseButtonDelete, new Action(__instance.OnDelete));
					UtilsPanelChoose.UpdateButtonLegend(__instance.m_BasicMenu, true, true, true, __instance.m_ButtonLegendContainer);
				}
			}
		}
		[HarmonyPatch(typeof(Panel_Confirmation), nameof(Panel_Confirmation.AddConfirmation), new Type[] { typeof(Panel_Confirmation.ConfirmationType), typeof(string), typeof(string), typeof(Panel_Confirmation.ButtonLayout), typeof(string), typeof(string), typeof(Panel_Confirmation.Background), typeof(Panel_Confirmation.CallbackDelegate), typeof(Panel_Confirmation.EnableDelegate) })]
		internal class Panel_Confirmation_AddConfirmation
		{
			private static void Prefix(ref string titleLocID, ref string currentName)
			{
				if (!modEnabled) return;

				currentName = "Relentless " + (Il2CppTLD.SaveState.ProfileState.Instance.m_NumGamesPlayed + 1).ToString();
				titleLocID = "name relentless night game";
			}
		}
		[HarmonyPatch(typeof(SaveGameSlots), nameof(SaveGameSlots.BuildSlotName), new Type[] { typeof(Episode), typeof(SaveSlotType), typeof(uint), })]
		internal class SaveGameSlots_BuildSlotName
		{
			private static bool Prefix(uint n, ref string __result)
			{
				if (!modEnabled) return true;

				__result = SaveManager.rnSavePrefix + n.ToString();

				return false;
			}
		}
		[HarmonyPatch(typeof(Panel_ChooseSandbox), nameof(Panel_ChooseSandbox.ConfigureMenu))]
		internal class Panel_ChooseSandbox_ConfigureMenu
		{
			private static void Postfix(Panel_ChooseSandbox __instance)
			{
				if (!modEnabled) return;

				AddRnTitleHeaderToBasicMenu(__instance.m_BasicMenu);
			}
		}
		[HarmonyPatch(typeof(Panel_Sandbox), nameof(Panel_Sandbox.ConfigureMenu))]
		internal class Panel_Sandbox_ConfigureMenu
		{
			private static void Postfix(Panel_Sandbox __instance)
			{
				if (!modEnabled) return;

				AddRnTitleHeaderToBasicMenu(__instance.m_BasicMenu);
			}
		}
		[HarmonyPatch(typeof(Panel_SelectExperience), nameof(Panel_SelectExperience.ConfigureMenu))]
		internal class Panel_SelectExperience_ConfigureMenu
		{
			private static void Postfix(Panel_SelectExperience __instance)
			{
				if (!modEnabled) return;

				AddRnTitleHeaderToBasicMenu(__instance.m_BasicMenu);
			}
		}
		[HarmonyPatch(typeof(Panel_PauseMenu), nameof(Panel_PauseMenu.ConfigureMenu))]
		internal class Panel_PauseMenu_ConfigureMenu
		{
			private static void Postfix(Panel_PauseMenu __instance)
			{
				if (!modEnabled || !InterfaceManager.GetPanel<Panel_PauseMenu>().IsEnabled()) return;

				AddRnTitleHeaderToBasicMenu(__instance.m_BasicMenu);
			}
		}
		// Below two patches prevent a bug where mod will become enabled when clicking back from the feats menu
		[HarmonyPatch(typeof(Panel_Sandbox), nameof(Panel_Sandbox.OnClickFeats))]
		internal class Panel_Sandbox_OnClickFeat
		{
			private static void Prefix()
			{
				featsPanelEnabled = modEnabled;
			}
		}
		[HarmonyPatch(typeof(Panel_Badges), nameof(Panel_Badges.OnFeats))]
		internal class Panel_Badges_OnFeats
		{
			private static void Postfix()
			{
				if (featsPanelEnabled)
				{
					modEnabled = true;
				}
			}
		}

		public delegate void OnSlotClicked();

		internal static void AddRnMainMenuButton(Panel_MainMenu __instance, int itemIndex)
		{
			string buttonDescription = "The earth's spin is slowing down. Days and nights are getting longer. Each night is colder and harsher than the last. How long will you survive?";
			__instance.m_BasicMenu.AddItem("Sandbox", (int)Panel_MainMenu.MainMenuItem.MainMenuItemType.Sandbox, itemIndex, "Relentless Night", buttonDescription, "", new Action(__instance.OnSandbox), Color.gray, Color.white);
		}

		internal static void AddRnTitleHeaderToBasicMenu(BasicMenu basicMenu, bool addHeader = true)
		{
			if (addHeader == true)
			{
				basicMenu.UpdateTitle("Relentless Night " + Global.RnVersion, "See Options then Mod Settings to\nconfigure your Relentless Night game.", new Vector3(0f, 0f, 0f));
			}
			else
			{
				basicMenu.UpdateTitle("Relentless Night " + Global.RnVersion, String.Empty, new Vector3(0f, 0f, 0f));
			}
			basicMenu.m_TitleHeaderLabel.capsLock = false;
			basicMenu.m_TitleHeaderLabel.fontSize = 16;
			basicMenu.m_TitleLabel.gameObject.transform.localPosition = new Vector3(65f, -125f, 0f);
			basicMenu.m_TitleHeaderLabel.gameObject.transform.localPosition = new Vector3(65f, -160f, 0f);
		}
	}

	[HarmonyPatch(typeof(Panel_Sandbox), nameof(Panel_Sandbox.ConfigureMenu))]
	internal class Panel_Sandbox_ConfigureMenu
	{
		private static void Prefix()
		{
			if (!modEnabled)
			{
				return;
			}
			if (SaveGameSlotHelper.GetSaveSlotInfoList(SaveSlotType.SANDBOX).Count == 0)
			{
				SaveGameSlotHelper.m_SandboxSlots.Clear();
			}
		}
	}
	
}