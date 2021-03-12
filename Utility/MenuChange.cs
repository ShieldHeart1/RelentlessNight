using System;
using Harmony;
using UnityEngine;
using IL2CPP = Il2CppSystem.Collections.Generic;

namespace RelentlessNight
{
    public class MenuChange
    {
        private const int mainMenuButtonIndex = 3;

        public static void MoveUpMainMenuWordmark(int numOfMainMenuItems)
        {
            GameObject.Find("Panel_MainMenu/MainPanel/Main/TLD_wordmark").transform.localPosition += new Vector3(0, (numOfMainMenuItems - 8) * 30, 0);
        }

        [HarmonyPatch(typeof(BasicMenu), "InternalClickAction", null)]
        internal class BasicMenu_InternalClickAction_Pre
        {
            private static void Prefix(int index, BasicMenu __instance)
            {
                IL2CPP.List<BasicMenu.BasicMenuItemModel> list = __instance.m_ItemModelList;

                if (list[index].m_LabelText == "Relentless Night")
                {
                    RnGlobal.rnActive = true;
                }
            }
        }

        [HarmonyPatch(typeof(BasicMenu), "UpdateTitleHeader", null)]
        internal class BasicMenu_UpdateTitleHeader_Postt
        {
            private static void Postfix(BasicMenu __instance)
            {
                if (!RnGlobal.rnActive || !InterfaceManager.m_Panel_SelectExperience.IsEnabled()) return;

                __instance.m_TitleHeaderLabel.capsLock = false;
                __instance.m_TitleHeaderLabel.fontSize = 16;
            }
        }

        [HarmonyPatch(typeof(GameManager), "GetVersionString", null)]
        internal class GameManager_GetVersionString_Postt
        {
            private static void Postfix(ref string __result)
            {
                __result += "Relentless Night " + RnGlobal.RnVersion;
            }
        }

        [HarmonyPatch(typeof(Panel_Badges), "OnFeats", null)]
        internal class Panel_Badges_OnFeats_Postt
        {
            private static void Postfix()
            {
                if (RnGlobal.rnFeatsActive)
                {
                    RnGlobal.rnActive = true;
                }
            }
        }

        [HarmonyPatch(typeof(Panel_ChooseSandbox), "ConfigureMenu", null)]
        internal class Panel_ChooseSandbox_ConfigureMenu_Postt
        {
            private static void Postfix(Panel_ChooseSandbox __instance)
            {
                if (!RnGlobal.rnActive) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", string.Empty, Vector3.zero);
            }
        }

        [HarmonyPatch(typeof(Panel_ChooseSandbox), "ProcessMenu", null)]
        internal class Panel_ChooseSandbox_ProcessMenu_Pre
        {
            private static void Prefix()
            {
                if (!RnGlobal.rnActive) return;

                SaveGameSlots.SANDBOX_SLOT_PREFIX = RnGlobal.RnSlotPrefix;
            }
        }

        [HarmonyPatch(typeof(Panel_ChooseSandbox), "Update", null)]
        internal class Panel_ChooseSandbox_Update_Pre
        {
            private static void Prefix()
            {
                if (!RnGlobal.rnActive) return;
            }
        }

        [HarmonyPatch(typeof(Panel_Confirmation), "ShowRenamePanel", null)]
        internal class Panel_Confirmation_ShowRenamePanel_Pre
        {
            private static void Prefix(ref string locID, ref string currentName)
            {
                if (!RnGlobal.rnActive) return;

                currentName = "Relentless " + (InterfaceManager.m_Panel_OptionsMenu.m_State.m_NumGamesPlayed + 1).ToString();
                locID = "GAMEPLAY_NameSandbox";
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "Start", null)]
        public class Panel_MainMenu_Start_Postt
        {
            public static void Postfix(Panel_MainMenu __instance)
            {
                if (!InterfaceManager.IsMainMenuActive()) return;

                MoveUpMainMenuWordmark(Convert.ToInt16(__instance.m_BasicMenu.m_MenuItems.Count.ToString()));

                Panel_MainMenu.MainMenuItem mainMenuItem = new Panel_MainMenu.MainMenuItem();
                mainMenuItem.m_LabelLocalizationId = "Relentless Night";
                mainMenuItem.m_Type = Panel_MainMenu.MainMenuItem.MainMenuItemType.Sandbox;
                __instance.m_MenuItems.Insert(mainMenuButtonIndex, mainMenuItem);


                string id = __instance.m_MenuItems[mainMenuButtonIndex].m_Type.ToString();
                int type = (int)__instance.m_MenuItems[mainMenuButtonIndex].m_Type;

                __instance.m_BasicMenu.AddItem(id, type, mainMenuButtonIndex, "Relentless Night", "", "", new Action(__instance.OnSandbox), Color.gray, Color.white);
            }
        }

        [HarmonyPatch(typeof(BasicMenu), "UpdateDescription", null)]
        internal class BasicMenu_UpdateDescription_Post
        {
            private static void Postfix(BasicMenu __instance, int buttonIndex)
            {
                if (InterfaceManager.IsMainMenuActive() && !InterfaceManager.m_Panel_MainMenu.IsSubMenuEnabled() && buttonIndex == mainMenuButtonIndex - 1)
                {
                    __instance.m_DescriptionLabel.text = "The earth seems to be slowing down. Days and nights are getting longer. Each night is colder and harsher than the last. How long will you survive?";
                }
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "ConfigureMenu", null)]
        internal class Panel_Panel_MainMenu_ConfigureMenu_Post
        {
            private static void Postfix(Panel_MainMenu __instance)
            {
                if (!RnGlobal.rnActive || InterfaceManager.IsMainMenuActive()) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", string.Empty, Vector3.zero);
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnSandbox", null)]
        internal class Panel_MainMenu_OnSandbox_Pre
        {
            private static bool Prefix(Panel_MainMenu __instance)
            {
                if (!RnGlobal.rnActive) return true;

                IL2CPP.List<SlotData> m_SaveSlots = SaveGameSlots.m_SaveSlots;
                foreach (SlotData slotData in m_SaveSlots)
                {
                    if (slotData.m_Name.Contains("relentless") && slotData.m_GameMode != RnGlobal.RnSlotType) slotData.m_GameMode = RnGlobal.RnSlotType;
                }

                bool m_Quit = __instance.m_Quit;
                if (m_Quit) return false;

                if (SaveGameSlots.SlotsAreLoading(RnGlobal.RnSlotType))
                {
                    SaveGameSlots.SetLoadingPriority(RnGlobal.RnSlotType);
                    GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                    return false;
                }

                IL2CPP.List<SaveSlotInfo> sortedSaveSlots = SaveGameSystem.GetSortedSaveSlots(Episode.One, RnGlobal.RnSlotType);
                __instance.m_SandboxSlots = sortedSaveSlots;
                GameAudioManager.PlayGUIButtonClick();
                SaveSlotType saveSlotType = RnGlobal.RnSlotType;
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
                if (!RnGlobal.rnActive) return true;

                InterfaceManager.m_Panel_SelectExperience.Enable(true);
                return false;
            }
        }

        [HarmonyPatch(typeof(Panel_MainMenu), "OnSelectSurvivorContinue", null)]
        internal static class Panel_MainMenu_OnSelectSurvivorContinue_Pre
        {
            private static bool Prefix(Panel_MainMenu __instance)
            {
                if (!RnGlobal.rnActive) return true;

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
        internal class Panel_PanelMainMenu_ConfigureMenu_Post
        {
            private static void Prefix(Panel_MainMenu __instance)
            {
                if (InterfaceManager.IsMainMenuActive() && !__instance.IsSubMenuEnabled())
                {
                    RnGlobal.rnActive = false;
                }
            }
        }

        [HarmonyPatch(typeof(Panel_PauseMenu), "ConfigureMenu", null)]
        internal class Panel_PauseMenu_ConfigureMenu_Post
        {
            private static void Postfix(Panel_PauseMenu __instance)
            {
                if (!RnGlobal.rnActive || !InterfaceManager.m_Panel_PauseMenu.IsEnabled()) return;

                string[] array = new string[]
                {
                    (RnGlobal.glEndgameActive) ? ("ENDGAME ACTIVE: YES") : ("ENDGAME ACTIVE: NO"),                   
                    "ENDGAME DAY: " + RnGlobal.glEndgameDay.ToString(),                 
                    (RnGlobal.glEndgameAurora) ? ("ENDGAME AURORA: YES") : ("ENDGAME AURORA: NO"),
                    "DAY LENGTH CHANGE RATE: " + RnGlobal.glRotationDecline.ToString() + "%",
                    "MINIMUM AIR TEMPERATURE: " + RnGlobal.glMinimumTemperature.ToString() + "°C",
                    "INDOOR/OUTDOOR TEMP: " + RnGlobal.glTemperatureEffect.ToString() + "%",                    
                    (RnGlobal.glHeatRetention) ? ("HEAT RETENTION: YES") : ("HEAT RETENTION: NO"),                 
                    (RnGlobal.glRealisticFreezing) ? ("REALISTIC FREEZING: YES") : ("REALISTIC FREEZING: NO"),   
                    (RnGlobal.glWildlifeFreezing) ? ("TEMP AFFECTS WILDLIFE: YES") : ("TEMP AFFECTS WILDLIFE: NO"),
                    "MIN WILDLIFE AMOUNT: " + RnGlobal.glMinWildlifeAmount.ToString() + "%",
                    "MIN WILDLIFE DAY: " + RnGlobal.glMinWildlifeDay.ToString(),
                    "FIRE FUEL BURN TIME: " + RnGlobal.glFireFuelFactor.ToString() + "X",
                    "LANTERN FUEL BURN TIME: " + RnGlobal.glLanternFuelFactor.ToString() + "X",
                    "TORCH BURN TIME: " + RnGlobal.glTorchFuelFactor.ToString() + "X"
                };

                string rnSettings = "";
                for (int i = 0; i < array.Length; i++)
                {
                    rnSettings = rnSettings + array[i] + "\n";
                }

                BasicMenu basicMenu = __instance.m_BasicMenu;
                basicMenu.UpdateTitle("Relentless Night", rnSettings, new Vector3(0f, -145f, 0f));
                basicMenu.m_TitleHeaderLabel.fontSize = 12;
                basicMenu.m_TitleHeaderLabel.capsLock = true;
                basicMenu.m_TitleHeaderLabel.useFloatSpacing = true;
                basicMenu.m_TitleHeaderLabel.floatSpacingY = 1.5f;
            }
        }

        [HarmonyPatch(typeof(Panel_Sandbox), "ConfigureMenu", null)]
        internal class Panel_Sandbox_ConfigureMenu_Postt
        {
            private static void Postfix(Panel_Sandbox __instance)
            {
                if (!RnGlobal.rnActive) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", string.Empty, Vector3.zero);
            }
        }

        [HarmonyPatch(typeof(Panel_Sandbox), "OnClickFeats", null)]
        internal class Panel_Sandbox_OnClickFeats_Pre
        {
            private static void Prefix()
            {
                RnGlobal.rnFeatsActive = RnGlobal.rnActive;
            }
        }

        [HarmonyPatch(typeof(Panel_SelectExperience), "ConfigureMenu", null)]
        internal class Panel_SelectExperience_ConfigureMenu_Postt
        {
            private static void Postfix(Panel_SelectExperience __instance)
            {
                if (!RnGlobal.rnActive) return;

                __instance.m_BasicMenu.UpdateTitle("Relentless Night", "See game Options and Mod Settings to\nconfigure your Relentless Night run.", new Vector3(0f, -75f, 0f));
            }
        }
    }
}