using System;
using Harmony;
using UnityEngine;
using IL2CPP = Il2CppSystem.Collections.Generic;

namespace RelentlessNight
{
    public class MenuChange
    {
        [HarmonyPatch(typeof(BasicMenu), "InternalClickAction", null)]
        internal class BasicMenu_InternalClickAction_Pre
        {
            private static void Prefix(int index, BasicMenu __instance)
            {
                IL2CPP.List<BasicMenu.BasicMenuItemModel> list = __instance.m_ItemModelList;

                if (list[index].m_LabelText == "Relentless Night")
                {
                    RnGl.rnActive = true;
                }
            }
        }

        [HarmonyPatch(typeof(BasicMenu), "UpdateTitleHeader", null)]
        internal class BasicMenu_UpdateTitleHeader_Pos
        {
            private static void Postfix(BasicMenu __instance)
            {
                if (!RnGl.rnActive || !InterfaceManager.m_Panel_SelectExperience.IsEnabled()) return;

                __instance.m_TitleHeaderLabel.capsLock = false;
                __instance.m_TitleHeaderLabel.fontSize = 16;
            }
        }

        [HarmonyPatch(typeof(GameManager), "GetVersionString", null)]
        internal class GameManager_GetVersionString_Pos
        {
            private static void Postfix(ref string __result)
            {
                __result += "Relentless Night " + RnGl.RnVersion;
            }
        }

        [HarmonyPatch(typeof(Panel_Badges), "OnFeats", null)]
        internal class Panel_Badges_OnFeats_Pos
        {
            private static void Postfix()
            {
                if (RnGl.rnFeatsActive)
                {
                    RnGl.rnActive = true;
                }
            }
        }

        [HarmonyPatch(typeof(Panel_ChooseSandbox), "ConfigureMenu", null)]
        internal class Panel_ChooseSandbox_ConfigureMenu_Pos
        {
            private static void Postfix(Panel_ChooseSandbox __instance)
            {
                if (!RnGl.rnActive) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", string.Empty, Vector3.zero);
            }
        }

        [HarmonyPatch(typeof(Panel_ChooseSandbox), "ProcessMenu", null)]
        internal class Panel_ChooseSandbox_ProcessMenu_Pre
        {
            private static void Prefix()
            {
                Debug.Log("Panel_ChooseSandbox_ProcessMenu_Pre");
                if (!RnGl.rnActive) return;

                SaveGameSlots.SANDBOX_SLOT_PREFIX = RnGl.RnSlotPrefix;
            }
        }

        [HarmonyPatch(typeof(Panel_ChooseSandbox), "Update", null)]
        internal class Panel_ChooseSandbox_Update_Pre
        {
            private static void Prefix()
            {
                Debug.Log("Panel_ChooseSandbox_Update_Pre");
                if (!RnGl.rnActive) return;
            }
        }

        [HarmonyPatch(typeof(Panel_Confirmation), "ShowRenamePanel", null)]
        internal class Panel_Confirmation_ShowRenamePanel_Pre
        {
            private static void Prefix(ref string locID, ref string currentName)
            {
                if (!RnGl.rnActive) return;

                currentName = "Relentless " + (InterfaceManager.m_Panel_OptionsMenu.m_State.m_NumGamesPlayed + 1).ToString();
                locID = "GAMEPLAY_NameSandbox";
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "Start", null)]
        public class Panel_MainMenu_Start_Pos
        {
            public static void Postfix(Panel_MainMenu __instance)
            {
                if (!InterfaceManager.IsMainMenuActive()) return;

                int numOfMainMenuItems = Convert.ToInt16(__instance.m_BasicMenu.m_MenuItems.Count.ToString());
                GameObject.Find("Panel_MainMenu/MainPanel/Main/TLD_wordmark").transform.localPosition += new Vector3(0, (numOfMainMenuItems - 8) * 30, 0);

                Panel_MainMenu.MainMenuItem mainMenuItem = new Panel_MainMenu.MainMenuItem();
                mainMenuItem.m_LabelLocalizationId = "Relentless Night";
                mainMenuItem.m_Type = Panel_MainMenu.MainMenuItem.MainMenuItemType.Sandbox;
                __instance.m_MenuItems.Insert(3, mainMenuItem);


                string id = __instance.m_MenuItems[3].m_Type.ToString();
                int type = (int)__instance.m_MenuItems[3].m_Type;
                string key2 = "The earth seems to be slowing down. Days and nights are getting longer. Each night is colder and harsher than the last. How long will you survive?";

                __instance.m_BasicMenu.AddItem(id, type, 3, "Relentless Night", key2, "", new Action(__instance.OnSandbox), Color.gray, Color.white);
            }
        }

        [HarmonyPatch(typeof(BasicMenu), "UpdateDescription", null)]
        internal class BasicMenu_UpdateDescription_Pos
        {
            private static void Postfix(BasicMenu __instance, int buttonIndex)
            {
                if (InterfaceManager.IsMainMenuActive() && buttonIndex == 2)
                {
                    __instance.m_DescriptionLabel.text = "The earth seems to be slowing down. Days and nights are getting longer. Each night is colder and harsher than the last. How long will you survive?";
                }
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "ConfigureMenu", null)]
        internal class Panel_Panel_MainMenu_ConfigureMenu_Pos
        {
            private static void Postfix(Panel_MainMenu __instance)
            {
                if (!RnGl.rnActive || InterfaceManager.IsMainMenuActive()) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", string.Empty, Vector3.zero);
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnSandbox", null)]
        internal class Panel_MainMenu_OnSandbox_Pre
        {
            private static bool Prefix(Panel_MainMenu __instance)
            {
                Debug.Log("Panel_MainMenu_OnSandbox_Pre");
                if (!RnGl.rnActive) return true;

                IL2CPP.List<SlotData> m_SaveSlots = SaveGameSlots.m_SaveSlots;
                foreach (SlotData slotData in m_SaveSlots)
                {
                    if (slotData.m_Name.Contains("relentless") && slotData.m_GameMode != RnGl.RnSlotType) slotData.m_GameMode = RnGl.RnSlotType;
                }

                bool m_Quit = __instance.m_Quit;
                if (m_Quit) return false;

                if (SaveGameSlots.SlotsAreLoading(RnGl.RnSlotType))
                {
                    SaveGameSlots.SetLoadingPriority(RnGl.RnSlotType);
                    GameAudioManager.PlayGUIError();
                    return false;
                }

                IL2CPP.List<SaveSlotInfo> sortedSaveSlots = SaveGameSystem.GetSortedSaveSlots(Episode.One, RnGl.RnSlotType);
                __instance.m_SandboxSlots = sortedSaveSlots;
                GameAudioManager.PlayGUIButtonClick();
                SaveSlotType saveSlotType = RnGl.RnSlotType;
                __instance.m_SlotTypeSelected = saveSlotType;
                __instance.m_AllObjects.SetActive(false);
                InterfaceManager.m_Panel_Sandbox.Enable(true);

                return false;
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnSelectSurvivorBack", null)]
        internal static class Panel_MainMenu_OnSelectSurvivorBack_Pre
        {
            private static bool Prefix(Panel_MainMenu __instance)
            {
                if (!RnGl.rnActive) return true;

                InterfaceManager.m_Panel_SelectExperience.Enable(true);
                return false;
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnSelectSurvivorContinue", null)]
        internal static class Panel_MainMenu_OnSelectSurvivorContinue_Pre
        {
            private static bool Prefix(Panel_MainMenu __instance)
            {
                if (!RnGl.rnActive) return true;

                GameAudioManager.PlayGuiConfirm();
                int numUnlockedFeats = __instance.GetNumUnlockedFeats();

                if (numUnlockedFeats > 0)
                {
                    __instance.SelectWindow(__instance.m_SelectFeatWindow);
                }
                else
                {
                    FeatEnabledTracker.m_FeatsEnabledThisSandbox = new IL2CPP.List<FeatType>();
                    __instance.ShowNameSaveSlotPopup();
                }
                return false;
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "ConfigureMenu", null)]
        internal class Panel_PanelMainMenu_ConfigureMenu_Pos
        {
            private static void Prefix(Panel_MainMenu __instance)
            {
                if (InterfaceManager.IsMainMenuActive() && !__instance.IsSubMenuEnabled())
                {
                    RnGl.rnActive = false;
                    Debug.Log("RN DISABLED");
                }
            }
        }

        [HarmonyPatch(typeof(Panel_PauseMenu), "ConfigureMenu", null)]
        internal class Panel_PauseMenu_ConfigureMenu_Pos
        {
            private static void Postfix(Panel_PauseMenu __instance)
            {
                if (!RnGl.rnActive || !InterfaceManager.m_Panel_PauseMenu.IsEnabled()) return;

                string[] array = new string[]
                {
                    (RnGl.glEndgameActive) ? ("ENDGAME ACTIVE: YES") : ("ENDGAME ACTIVE: NO"),                   
                    "ENDGAME DAY: " + RnGl.glEndgameDay.ToString(),                 
                    (RnGl.glEndgameAurora) ? ("ENDGAME AURORA: YES") : ("ENDGAME AURORA: NO"),
                    "DAY LENGTH CHANGE RATE: " + RnGl.glRotationDecline.ToString() + "%",
                    "INDOOR/OUTDOOR TEMP: " + RnGl.glTemperatureEffect.ToString() + "%",
                    "MINIMUM TEMPERATURE: " + RnGl.glMinimumTemperature.ToString(),
                    (RnGl.glHeatRetention) ? ("HEAT RETENTION: YES") : ("HEAT RETENTION: NO"),                 
                    (RnGl.glRealisticFreezing) ? ("REALISTIC FREEZING: YES") : ("REALISTIC FREEZING: NO"),   
                    (RnGl.glWildlifeFreezing) ? ("TEMP AFFECTS WILDLIFE: YES") : ("TEMP AFFECTS WILDLIFE: NO"),
                    "MIN WILDLIFE AMOUNT: " + RnGl.glMinWildlifeAmount.ToString() + "%",
                    "MIN WILDLIFE DAY: " + RnGl.glMinWildlifeDay.ToString(),
                    "FIRE FUEL BURNTIME: " + RnGl.glFireFuelFactor.ToString() + "X",
                    "LANTERN FUEL BURNTIME: " + RnGl.glLanternFuelFactor.ToString() + "X"
                };

                string rnSettings = "";
                for (int i = 0; i < array.Length; i++)
                {
                    rnSettings = rnSettings + array[i] + "\n";
                }

                BasicMenu basicMenu = __instance.m_BasicMenu;
                basicMenu.UpdateTitle("Relentless Night", rnSettings, new Vector3(0f, -145f, 0f));
                basicMenu.m_TitleHeaderLabel.fontSize = 14;
                basicMenu.m_TitleHeaderLabel.capsLock = true;
                basicMenu.m_TitleHeaderLabel.useFloatSpacing = true;
                basicMenu.m_TitleHeaderLabel.floatSpacingY = 1.5f;
            }
        }

        [HarmonyPatch(typeof(Panel_Sandbox), "ConfigureMenu", null)]
        internal class Panel_Sandbox_ConfigureMenu_Pos
        {
            private static void Postfix(Panel_Sandbox __instance)
            {
                if (!RnGl.rnActive) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", string.Empty, Vector3.zero);
            }
        }

        [HarmonyPatch(typeof(Panel_Sandbox), "OnClickFeats", null)] // inlined everywhere
        internal class Panel_Sandbox_OnClickFeats_Pre
        {
            private static void Prefix()
            {
                RnGl.rnFeatsActive = RnGl.rnActive;
            }
        }

        [HarmonyPatch(typeof(Panel_SelectExperience), "ConfigureMenu", null)]
        internal class Panel_SelectExperience_ConfigureMenu_Pos
        {
            private static void Postfix(Panel_SelectExperience __instance)
            {
                if (!RnGl.rnActive) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", "", new Vector3(0f, -150f, 0f));
            }
        }
    }
}
