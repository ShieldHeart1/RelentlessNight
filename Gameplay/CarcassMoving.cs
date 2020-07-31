using UnityEngine;
using Harmony;
using AK;

namespace RelentlessNight
{
    public class CarcassMoving : MonoBehaviour
    {
        public const float carryFatigueMultiplier = 0.05f;              //% fatigue rate increase per kilo of carcass being carried
        public const float carrySlowDownMultiplier = 0.05f;             //% Player speed slow down per kilo of carcass being carried
        public const float carryCaloryBurnRateMultiplier = 15f;         //Additional calories burned per hour for every kilo of the carcass being carried

        public static GameObject currentCarryObj;
        public static BodyHarvest currentBodyHarvest;
        public static EquipItemPopup equipItemPopup;

        public static GameObject moveCarcassBtnObj;
        public static UIButton moveCarcassUIBtn;
        public static Panel_BodyHarvest currentHarvestPanel;

        public static bool PlayerIsCarryingCarcass;
        public static string carcassOriginalScene;
        public static float carcassWeight;

        private void Update()
        {
            if (!PlayerIsCarryingCarcass) return;

            if (HasInjuryPreventingCarry() || GameManager.GetPlayerStruggleComponent().InStruggle())
            {
                DropCarcass();
                return;
            }
            if (InputManager.GetAltFirePressed(this))
            {
                DropCarcass();
            }
        }

        internal static void MaybeAddCarcassMoveButton(Panel_BodyHarvest panelInstance, BodyHarvest bodyHarvest)
        {
            if (IsMovableCarcass(bodyHarvest))
            {
                if (moveCarcassBtnObj == null)
                {
                    moveCarcassBtnObj = Instantiate(panelInstance.m_Mouse_Button_Harvest, panelInstance.m_Mouse_Button_Harvest.transform);
                    moveCarcassBtnObj.GetComponentInChildren<UILocalize>().key = "MOVE CARCASS";

                    panelInstance.m_Mouse_Button_Harvest.transform.localPosition += new Vector3(-100f, 0f, 0f);
                    moveCarcassBtnObj.transform.localPosition = new Vector3(+200f, 0f, 0f);

                    moveCarcassUIBtn = moveCarcassBtnObj.GetComponentInChildren<UIButton>();
                    moveCarcassUIBtn.onClick.Clear();
                    moveCarcassUIBtn.onClick.Add(new EventDelegate(new System.Action(OnMoveCarcass)));
                }
            }
            else
            {
                if (moveCarcassBtnObj != null)
                {
                    DestroyImmediate(moveCarcassBtnObj);
                    panelInstance.m_Mouse_Button_Harvest.transform.localPosition += new Vector3(+100f, 0f, 0f);
                }
            }
        }

        internal static bool IsMovableCarcass(BodyHarvest bodyHarvest)
        {
            return (bodyHarvest.name.Contains("Stag") || bodyHarvest.name.Contains("Deer") || bodyHarvest.name.Contains("Wolf"));
        }

        internal static void OnMoveCarcass()
        {
            if (HasInjuryPreventingCarry())
            {
                GameAudioManager.PlayGUIError();
                AccessTools.Method(typeof(Panel_BodyHarvest), "DisplayErrorMessage", null, null).Invoke(currentHarvestPanel, new object[] { "CANNOT MOVE CARCASS WHILE INJURED" });
                return;
            }
            PickUpCarcass();
        }

        internal static void PickUpCarcass()
        {
            PlayerIsCarryingCarcass = true;
            carcassWeight = currentBodyHarvest.m_MeatAvailableKG + currentBodyHarvest.GetGutsAvailableWeightKg() + currentBodyHarvest.GetHideAvailableWeightKg();
            currentHarvestPanel.OnBack();
            carcassOriginalScene = GameManager.m_ActiveScene;

            CarcassMoving carcassMoving = currentCarryObj.GetComponent<CarcassMoving>();
            if (carcassMoving == null)
            {
                carcassMoving = currentCarryObj.AddComponent<CarcassMoving>();
            }

            GameManager.GetPlayerManagerComponent().UnequipItemInHands();

            DisplayDropCarcassPopUp();
            HideCarcassFromView();
            PlayCarcassPickUpAudio();
        }

        internal static void DropCarcass()
        {
            PlayerIsCarryingCarcass = false;
            MoveCarcassToPlayerPosition();
            BringCarcassBackIntoView();
            if (GameManager.m_ActiveScene != carcassOriginalScene)
            {
                AddCarcassToSceneSaveData(currentBodyHarvest);
            }
            PlayCarcassDropAudio();
        }

        internal static void HideCarcassFromView()
        {
            currentCarryObj.transform.localScale = new Vector3(0, 0, 0);
        }

        internal static void BringCarcassBackIntoView()
        {
            currentCarryObj.transform.localScale = new Vector3(1, 1, 1);
        }

        internal static void DisplayDropCarcassPopUp()
        {
            InterfaceManager.m_Panel_HUD.m_EquipItemPopup.ShowGenericPopupWithDefaultActions(string.Empty, Localization.Get("DROP CARCASS"));
        }

        internal static bool HasInjuryPreventingCarry()
        {
            return GameManager.GetSprainedAnkleComponent().HasSprainedAnkle() || GameManager.GetSprainedWristComponent().HasSprainedWrist() ||
                GameManager.GetSprainedWristComponent().HasSprainedWrist() || GameManager.GetBrokenRibComponent().HasBrokenRib();
        }

        internal static void MoveCarcassToPlayerPosition()
        {
            currentCarryObj.transform.position = GameManager.GetPlayerTransform().position;
            currentCarryObj.transform.rotation = GameManager.GetPlayerTransform().rotation * Quaternion.Euler(0, 90, 0);
        }

        internal static void AddCarcassToSceneSaveData(BodyHarvest bodyHarvest)
        {
            BodyHarvestManager.AddBodyHarvest(currentBodyHarvest);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(currentCarryObj, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        }

        internal static void PlayCarcassPickUpAudio()
        {
            GameAudioManager.PlaySound("Play_RopeGetOn", InterfaceManager.GetSoundEmitter());
            GameAudioManager.PlaySound(EVENTS.PLAY_EXERTIONLOW, InterfaceManager.GetSoundEmitter());
        }

        internal static void PlayCarcassDropAudio()
        {
            GameAudioManager.PlaySound(EVENTS.PLAY_BODYFALLLARGE, InterfaceManager.GetSoundEmitter());
        }

        internal static bool HarvestAmmountsAreSelected(Panel_BodyHarvest __instance)
        {
            return (__instance.m_MenuItem_Meat.m_HarvestAmount > 0f || __instance.m_MenuItem_Hide.m_HarvestAmount > 0f || __instance.m_MenuItem_Gut.m_HarvestAmount > 0f);
        }

        internal static void MaybeChangeSprintSpriteColors(Panel_HUD __instance)
        {
            if (PlayerIsCarryingCarcass)
            {
                __instance.m_Sprite_SprintCenter.color = __instance.m_SprintBarNoSprintColor;
                __instance.m_Sprite_SprintBar.color = __instance.m_SprintBarNoSprintColor;
            }
        }

        [HarmonyPatch(typeof(Panel_BodyHarvest), "Enable", null)]
        internal class Panel_BodyHarvest_Start_Pos
        {
            private static void Postfix(Panel_BodyHarvest __instance, BodyHarvest bh, bool enable)
            {
                if (!RnGl.rnActive || !enable || !__instance.CanEnable(bh)) return;

                currentCarryObj = bh.gameObject;
                currentBodyHarvest = bh;
                currentHarvestPanel = __instance;
                MaybeAddCarcassMoveButton(__instance, bh);
            }
        }
        [HarmonyPatch(typeof(LoadScene), "Activate", null)]
        internal class LoadScene_Activate
        {
            private static void Postfix(LoadScene __instance)
            {
                if (!RnGl.rnActive || !PlayerIsCarryingCarcass) return;

                DontDestroyOnLoad(currentCarryObj); //Do not destroy carcass object through scene transition
                currentBodyHarvest.enabled = false; //Disable Carcass Object to prevent its saving in the scene being left
            }
        }
        [HarmonyPatch(typeof(GameManager), "TriggerSurvivalSaveAndDisplayHUDMessage", null)]
        internal class GameManager_TriggerSurvivalSaveAndDisplayHUDMessage
        {
            private static void Prefix()
            {
                if (!RnGl.rnActive || !PlayerIsCarryingCarcass) return;

                currentBodyHarvest.enabled = true;
                MoveCarcassToPlayerPosition();
                AddCarcassToSceneSaveData(currentBodyHarvest);
            }
        }
        [HarmonyPatch(typeof(MissionServicesManager), "SceneLoadCompleted", null)]
        internal class MissionServicesManager_SceneLoadCompleted
        {
            private static void Postfix()
            {
                if (!RnGl.rnActive || !PlayerIsCarryingCarcass) return;

                currentBodyHarvest.enabled = true;
            }
        }
        [HarmonyPatch(typeof(PlayerManager), "ShouldSaveGameAfterTeleport", null)]
        internal class PlayerManager_ShouldSaveGameAfterTeleport
        {
            private static bool Prefix(ref bool __result)
            {
                if (!RnGl.rnActive || !PlayerIsCarryingCarcass) return true;

                // Ensures game will save after stepping outside as well as inside a new scene, prevents carcass loss on game quit
                __result = !GameManager.m_SceneTransitionData.m_TeleportPlayerSaveGamePosition && GameManager.m_SceneTransitionData.m_SpawnPointName != null;

                return false;
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "PlayerCanSprint", null)]
        public static class MaybeChangeWhetherPlayerCanSprint
        {
            private static void Postfix(ref bool __result)
            {
                if (!RnGl.rnActive || !PlayerIsCarryingCarcass) return;

                __result = false;
            }
        }
        [HarmonyPatch(typeof(Fatigue), "CalculateFatigueIncrease", null)]
        internal class Fatigue_CalculateFatigueIncrease_Pos
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGl.rnActive || !PlayerIsCarryingCarcass) return;

                __result *= (1f + carcassWeight * carryFatigueMultiplier);
            }
        }
        [HarmonyPatch(typeof(PlayerManager), "CalculateModifiedCalorieBurnRate")]
        internal static class PlayerManager_CalculateModifiedCalorieBurnRate_Pos
        {
            private static void Postfix(ref float __result)
            {
                __result += carcassWeight * carryCaloryBurnRateMultiplier;
            }
        }

        [HarmonyPatch(typeof(Encumber), "GetEncumbranceSlowdownMultiplier", null)]
        internal class MaybeAdjustEncumbranceSlowDown
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGl.rnActive || !PlayerIsCarryingCarcass) return;

                __result *= Mathf.Clamp((1f - carcassWeight * carrySlowDownMultiplier), 0.1f, 0.8f);
            }
        }
        [HarmonyPatch(typeof(EquipItemPopup), "ShouldHideEquipPopup", null)]
        internal class EquipItemPopup_ShouldHideEquipAndAmmoPopups
        {
            private static void Postfix(ref bool __result)
            {
                if (!RnGl.rnActive || !PlayerIsCarryingCarcass) return;

                __result = false;
            }
        }
        [HarmonyPatch(typeof(Panel_BodyHarvest), "CanEnable", null)]
        internal class Panel_BodyHarvest_CarcassTooFrozenToHarvestBareHands
        {
            private static void Postfix(BodyHarvest bodyHarvest, ref bool __result)
            {
                if (!RnGl.rnActive) return;

                if (IsMovableCarcass(bodyHarvest) && !(bodyHarvest.GetCondition() < 0.5f)) __result = true;
            }
        }
        [HarmonyPatch(typeof(PlayerManager), "EquipItem", null)]
        internal class PlayerManager_EquipItem
        {
            private static bool Prefix()
            {
                if (!RnGl.rnActive || !PlayerIsCarryingCarcass) return true;

                HUDMessage.AddMessage("CANNOT EQUIP ITEM WHILE CARRYING CARCASS", false);
                GameAudioManager.PlayGUIError();
                return false;
            }
        }
        [HarmonyPatch(typeof(Panel_HUD), "Update", null)]
        internal class Panel_HUD_Update
        {
            private static void Postfix(Panel_HUD __instance)
            {
                if (!RnGl.rnActive) return;

                MaybeChangeSprintSpriteColors(__instance);
            }
        }

        // Below two patches prevent game from displaying TooFrozenHarvestError when first enabling harvest panel on a frozen carcass
        [HarmonyPatch(typeof(Panel_BodyHarvest), "DisplayCarcassToFrozenMessage", null)]
        internal class Panel_BodyHarvest_DisplayCarcassToFrozenMessage
        {
            private static bool Prefix(Panel_BodyHarvest __instance)
            {
                if (!RnGl.rnActive || __instance.m_MenuItem_Meat == null || __instance.m_MenuItem_Hide == null || __instance.m_MenuItem_Gut == null) return true;

                if (!HarvestAmmountsAreSelected(__instance))
                {
                    return false;
                }
                return true;
            }
        }
        [HarmonyPatch(typeof(GameAudioManager), "PlayGUIError", null)]
        internal class GameAudioManager_PlayGUIError
        {
            private static bool Prefix()
            {
                if (!RnGl.rnActive) return true;

                Panel_BodyHarvest panel_BodyHarvest = InterfaceManager.m_Panel_BodyHarvest;
                if (panel_BodyHarvest != null)
                {
                    if (!HarvestAmmountsAreSelected(panel_BodyHarvest))
                    {
                        return false;
                    }
                }
                return true;
            }
        }
    }
}
