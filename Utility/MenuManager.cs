using System;
using HarmonyLib;
using Il2Cpp;
using UnityEngine;

namespace RelentlessNight
{
    internal class MenuManager
    {
        internal const int rnButtonIndex = 3;

        internal static bool modEnabled = false;
        internal static bool featsPanelEnabled = false;

        [HarmonyPatch(typeof(Panel_MainMenu), "AddMenuItem", null)]
        internal static class Panel_MainMenu_AddMenuItem
        {
            private static void Prefix(Panel_MainMenu __instance, int itemIndex)
            {
				MelonLoader.MelonLogger.Warning("Panel_MainMenu_AddMenuItem");

                if (__instance.IsSubMenuEnabled() || itemIndex != rnButtonIndex) return;

				MelonLoader.MelonLogger.Warning("AddRnMainMenuButton");
				AddRnMainMenuButton(__instance, itemIndex);
            }
        }
        // Enables Relentless Night when user clicks on main menu "Relentless Night" button
        [HarmonyPatch(typeof(BasicMenu), "InternalClickAction", null)]
        internal class BasicMenu_InternalClickAction
        {
            private static void Prefix(int index, BasicMenu __instance)
            {
                if (__instance.m_ItemModelList[index].m_LabelText == "Relentless Night")
                {
                    modEnabled = true;
                }
            }
        }
        // Disables Relentless Night when user returns to main menu
        [HarmonyPatch(typeof(Panel_MainMenu), "ConfigureMenu", null)]
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

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", string.Empty, Vector3.zero);
            }
        }
        // Prevents a bug where Relentless Night game slots cannot be deleted or renamed
        [HarmonyPatch(typeof(Panel_ChooseSandbox), "ProcessMenu", null)]
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
        [HarmonyPatch(typeof(Panel_Confirmation), "AddConfirmation", new Type[] { typeof(Panel_Confirmation.ConfirmationType), typeof(string), typeof(string), typeof(Panel_Confirmation.ButtonLayout), typeof(string), typeof(string), typeof(Panel_Confirmation.Background), typeof(Panel_Confirmation.CallbackDelegate), typeof(Panel_Confirmation.EnableDelegate) })]
        internal class Panel_Confirmation_AddConfirmation
        {
            private static void Prefix(ref string titleLocID, ref string currentName)
            {
                if (!modEnabled) return;

                currentName = "Relentless " + (Il2CppTLD.SaveState.ProfileState.Instance.m_NumGamesPlayed + 1).ToString();
                titleLocID = "name relentless night game";
            }
        }
        [HarmonyPatch(typeof(SaveGameSlots), "BuildSlotName", null)]
        internal class SaveGameSlots_BuildSlotName
        {
            private static bool Prefix(int n, ref string __result)
            {
                if (!modEnabled) return true;

                __result = SaveManager.rnSavePrefix + n.ToString();

                return false;
            }
        }
        [HarmonyPatch(typeof(Panel_ChooseSandbox), "ConfigureMenu", null)]
        internal class Panel_ChooseSandbox_ConfigureMenu
        {
            private static void Postfix(Panel_ChooseSandbox __instance)
            {
                if (!modEnabled) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", string.Empty, Vector3.zero);
            }
        }
        [HarmonyPatch(typeof(Panel_Sandbox), "ConfigureMenu", null)]
        internal class Panel_Sandbox_ConfigureMenu
        {
            private static void Postfix(Panel_Sandbox __instance)
            {
                if (!modEnabled) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", string.Empty, Vector3.zero);
            }
        }
        [HarmonyPatch(typeof(Panel_SelectExperience), "ConfigureMenu", null)]
        internal class Panel_SelectExperience_ConfigureMenu
        {
            private static void Postfix(Panel_SelectExperience __instance)
            {
                if (!modEnabled) return;

                AddRnTitleHeader(__instance);
            }
        }
        [HarmonyPatch(typeof(Panel_PauseMenu), "ConfigureMenu", null)]
        internal class Panel_PauseMenu_ConfigureMenu
        {
            private static void Postfix(Panel_PauseMenu __instance)
			{
                if (!modEnabled || !InterfaceManager.GetPanel<Panel_PauseMenu>().IsEnabled()) return;

                AddRnTitleHeader(__instance);
            }
        }
        // Below two patches prevent a bug where mod will become enabled when clicking back from the feats menu
        [HarmonyPatch(typeof(Panel_Sandbox), "OnClickFeats", null)]
        internal class Panel_Sandbox_OnClickFeat
        {
            private static void Prefix()
            {
                featsPanelEnabled = modEnabled;
            }
        }
        [HarmonyPatch(typeof(Panel_Badges), "OnFeats", null)]
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
        internal static void AddRnTitleHeader(Panel_SelectExperience __instance)
        {
            __instance.m_BasicMenu.UpdateTitle("Relentless Night", "See game Options and Mod Settings to\nconfigure your Relentless Night game.", new Vector3(0f, -75f, 0f));
            __instance.m_BasicMenu.m_TitleHeaderLabel.capsLock = false;
            __instance.m_BasicMenu.m_TitleHeaderLabel.fontSize = 16;
        }
        internal static void AddRnTitleHeader(Panel_PauseMenu __instance)
        {
            __instance.m_BasicMenu.UpdateTitle("Relentless Night", "See Options then Mod Settings to\nconfigure your Relentless Night game.", new Vector3(0f, -75f, 0f));
            __instance.m_BasicMenu.m_TitleHeaderLabel.capsLock = false;
            __instance.m_BasicMenu.m_TitleHeaderLabel.fontSize = 16;
        }
    }
}