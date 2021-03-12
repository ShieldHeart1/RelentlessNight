using System;
using UnityEngine;
using Harmony;
using AK;

namespace RelentlessNight
{
    public class CarcassMoving : MonoBehaviour
    {
        static CarcassMoving()
        {
            UnhollowerRuntimeLib.ClassInjector.RegisterTypeInIl2Cpp<CarcassMoving>();
        }
        public CarcassMoving(IntPtr ptr) : base(ptr) { }

        //% fatigue rate increase per kilo of carcass being carried
        public const float carryFatigueMultiplier = 0.05f;

        //% Player speed slow down per kilo of carcass being carried
        public const float carrySlowDownMultiplier = 0.05f;

        //Additional calories burned per hour for every kilo of the carcass being carried
        public const float carryCaloryBurnRateMultiplier = 15f;         

        public static GameObject currentCarryObj;
        public static BodyHarvest currentBodyHarvest;
        public static EquipItemPopup equipItemPopup;

        public static GameObject moveCarcassBtnObj;
        public static UIButton moveCarcassUIBtn;
        public static Panel_BodyHarvest currentHarvestPanel;

        public static bool PlayerIsCarryingCarcass = false;
        public static string carcassOriginalScene;
        public static float carcassWeight;

        private void Update()
        {
            if (!PlayerIsCarryingCarcass) return;

            DisplayDropCarcassPopUp();

            if (HasInjuryPreventingCarry() || GameManager.GetPlayerStruggleComponent().InStruggle() || InputManager.GetAltFirePressed(this))
            {
                DropCarcass();
            }
        }

        internal static void MaybeAddCarcassMoveButton(Panel_BodyHarvest panelInstance, BodyHarvest bodyHarvest)
        {
            if (IsMovableCarcass(bodyHarvest) && !PlayerIsCarryingCarcass)
            {
                if (moveCarcassBtnObj == null)
                {
                    moveCarcassBtnObj = Instantiate(panelInstance.m_Mouse_Button_Harvest, panelInstance.m_Mouse_Button_Harvest.transform);
                    moveCarcassBtnObj.GetComponentInChildren<UILocalize>().key = "MOVE CARCASS";

                    panelInstance.m_Mouse_Button_Harvest.transform.localPosition += new Vector3(-100f, 0f, 0f);
                    moveCarcassBtnObj.transform.localPosition = new Vector3(+200f, 0f, 0f);

                    moveCarcassUIBtn = moveCarcassBtnObj.GetComponentInChildren<UIButton>();
                    moveCarcassUIBtn.onClick.Clear();
                    moveCarcassUIBtn.onClick.Add(new EventDelegate(new Action(OnMoveCarcass)));
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
            return (bodyHarvest.name.Contains("Stag") || bodyHarvest.name.Contains("Deer") || bodyHarvest.name.Contains("Wolf")) && !bodyHarvest.name.Contains("Quarter");
        }

        internal static void OnMoveCarcass()
        {
            if (HasInjuryPreventingCarry())
            {
                GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                currentHarvestPanel.DisplayErrorMessage("CANNOT MOVE CARCASS WHILE INJURED");
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
            currentCarryObj = null;
            currentBodyHarvest = null;
            ResetBodyHarvestManager();
        }

        internal static void HideCarcassFromView()
        {
            currentCarryObj.transform.localScale = new Vector3(0f, 0f, 0f);
        }

        internal static void BringCarcassBackIntoView()
        {
            currentCarryObj.transform.localScale = new Vector3(1f, 1f, 1f);
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
            currentCarryObj.transform.rotation = GameManager.GetPlayerTransform().rotation * Quaternion.Euler(0f, 90f, 0f);
        }

        internal static void AddCarcassToSceneSaveData(BodyHarvest bodyHarvest)
        {
            BodyHarvestManager.AddBodyHarvest(currentBodyHarvest);
            UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(currentCarryObj.transform.root.gameObject, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
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
        public static void ResetBodyHarvestManager()
        {
            if (currentCarryObj != null && PlayerIsCarryingCarcass) MoveCarcassToPlayerPosition();
            string data = BodyHarvestManager.Serialize();
            BodyHarvestManager.DeleteAllActive();
            BodyHarvestManager.Deserialize(data);
        }

        [HarmonyPatch(typeof(Panel_BodyHarvest), "Enable", null)]
        internal class Panel_BodyHarvest_Start_Post
        {
            private static void Postfix(Panel_BodyHarvest __instance, BodyHarvest bh, bool enable)
            {
                if (!RnGlobal.rnActive || !enable || !__instance.CanEnable(bh)) return;

                currentCarryObj = bh.gameObject;
                currentBodyHarvest = bh;
                currentHarvestPanel = __instance;
                MaybeAddCarcassMoveButton(__instance, bh);
            }
        }

        [HarmonyPatch(typeof(GameManager), "TriggerSurvivalSaveAndDisplayHUDMessage", null)]
        internal class GameManager_TriggerSurvivalSaveAndDisplayHUDMessage_Pre
        {
            private static void Prefix()
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return;

                if (currentBodyHarvest != null)
                {
                    currentBodyHarvest.enabled = true;
                    MoveCarcassToPlayerPosition();
                    AddCarcassToSceneSaveData(currentBodyHarvest);
                }
            }
        }

        [HarmonyPatch(typeof(GameManager), "SetAudioModeForLoadedScene")]
        internal class GameManager_SetAudioModeForLoadedScene_Post
        {
            private static void Postfix()
            {
                if (!RnGlobal.rnActive) return;

                ResetBodyHarvestManager();

                if (!PlayerIsCarryingCarcass) return;

                if (currentCarryObj == null)
                {
                    PlayerIsCarryingCarcass = false;
                }

                if (currentBodyHarvest != null) currentBodyHarvest.enabled = true;

                SaveTrigger.trigger = true;
            }
        }

        internal class SaveTrigger
        {
            public static bool trigger = false;
        }

        [HarmonyPatch(typeof(PlayerManager), "Update")]
        internal class PlayerManager_Update_Post
        {
            private static void Postfix(PlayerManager __instance)
            {
                if (SaveTrigger.trigger)
                {
                    SaveTrigger.trigger = false;
                    GearManager.UpdateAll();
                    GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
                }
            }
        }

        [HarmonyPatch(typeof(LoadScene), "Activate", null)]
        internal class LoadScene_Activate_Post
        {
            private static void Postfix(LoadScene __instance)
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return;

                if (currentCarryObj != null)
                {
                    //Do not destroy carcass object through scene transition
                    DontDestroyOnLoad(currentCarryObj.transform.root.gameObject);

                    //Disable carcass object to prevent its saving in the scene being left
                    currentBodyHarvest.enabled = false; 
                }                
            }
        }

        [HarmonyPatch(typeof(RopeClimbPoint), "OnRopeTransition", null)]
        public static class RopeClimbPoint_OnRopeTransition_Post
        {
            private static void Postfix()
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return;

                DropCarcass();
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "PlayerCanSprint", null)]
        public static class PlayerManager_PlayerCanSprint_Post
        {
            private static void Postfix(ref bool __result)
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return;

                __result = false;
            }
        }

        [HarmonyPatch(typeof(Fatigue), "CalculateFatigueIncrease", null)]
        internal class Fatigue_CalculateFatigueIncrease_Post
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return;

                __result *= (1f + carcassWeight * carryFatigueMultiplier);
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "CalculateModifiedCalorieBurnRate")]
        internal static class PlayerManager_CalculateModifiedCalorieBurnRate_Post
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return;

                __result += carcassWeight * carryCaloryBurnRateMultiplier;
            }
        }

        [HarmonyPatch(typeof(Encumber), "GetEncumbranceSlowdownMultiplier", null)]
        internal class MaybeAdjustEncumbranceSlowDown_Post
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return;

                __result *= Mathf.Clamp((1f - carcassWeight * carrySlowDownMultiplier), 0.1f, 0.8f);
            }
        }

        [HarmonyPatch(typeof(Inventory), "GetExtraScentIntensity", null)]
        internal class Inventory_GetExtraScentIntensity_Post
        {
            private static void Postfix(ref float __result)
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return;

                __result += Mathf.Clamp(100f - currentBodyHarvest.m_PercentFrozen, 33f, 100f);           
            }
        }

        [HarmonyPatch(typeof(EquipItemPopup), "ShouldHideEquipPopup", null)]
        internal class EquipItemPopup_ShouldHideEquipAndAmmoPopups_Post
        {
            private static void Postfix(ref bool __result)
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return;

                __result = false;
            }
        }

        [HarmonyPatch(typeof(Panel_BodyHarvest), "CanEnable", null)]
        internal class Panel_BodyHarvest_CarcassTooFrozenToHarvestBareHands_Post
        {
            private static void Postfix(BodyHarvest bodyHarvest, ref bool __result)
            {
                if (!RnGlobal.rnActive) return;

                if (IsMovableCarcass(bodyHarvest) && !(bodyHarvest.GetCondition() < 0.5f)) __result = true;
            }
        }

        [HarmonyPatch(typeof(PlayerManager), "EquipItem", null)]
        internal class PlayerManager_EquipItem_Pre
        {
            private static bool Prefix()
            {
                if (!RnGlobal.rnActive || !PlayerIsCarryingCarcass) return true;

                HUDMessage.AddMessage("CANNOT EQUIP ITEM WHILE CARRYING CARCASS", false);
                GameAudioManager.PlaySound(GameManager.GetGameAudioManagerComponent().m_ErrorAudio, GameManager.GetGameAudioManagerComponent().gameObject);
                return false;
            }
        }

        [HarmonyPatch(typeof(Panel_HUD), "Update", null)]
        internal class Panel_HUD_Update_Post
        {
            private static void Postfix(Panel_HUD __instance)
            {
                if (!RnGlobal.rnActive) return;

                MaybeChangeSprintSpriteColors(__instance);
            }
        }

        [HarmonyPatch(typeof(GameAudioManager), "PlayGUIError", null)]
        internal class GameAudioManager_PlayGUIError_Pre
        {
            private static bool Prefix()
            {
                if (!RnGlobal.rnActive) return true;

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

        [HarmonyPatch(typeof(GameManager), "SetAudioModeForLoadedScene")]
        internal class GameManager_SetAudioModeForLoadedScene_Postt
        {
            private static void Postfix()
            {
                if (!RnGlobal.rnActive) return;

                string data = BodyHarvestManager.Serialize();
                BodyHarvestManager.DeleteAllActive();
                BodyHarvestManager.Deserialize(data);
            }
        }
    }
}