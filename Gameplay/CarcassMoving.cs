using System;
using UnityEngine;
using HarmonyLib;
using Il2CppAK;
using Il2CppInterop.Runtime.Injection;
using static RelentlessNight.CarcassMoving;

namespace RelentlessNight
{
	internal class CarcassMoving : MonoBehaviour
	{
		static CarcassMoving()
		{
			ClassInjector.RegisterTypeInIl2Cpp<CarcassMoving>();
		}

		public CarcassMoving(IntPtr ptr) : base(ptr) { }

		internal const float carryAddedFatiguePerKilo = 0.05f;
		internal const float carryAddedSlowDownPerKilo = 0.05f;
		internal const float carryAddedCaloryBurnPerKilo = 15f;

		internal static GameObject moveCarcassBtnObj;
		internal static GameObject carcassObj;
		internal static BodyHarvest bodyHarvest;
		internal static string carcassOriginalScene;
		internal static float carcassWeight;
		internal static bool isCarryingCarcass;
		internal static bool saveTrigger;

		[HarmonyPatch(typeof(Panel_BodyHarvest), nameof(Panel_BodyHarvest.CanEnable), new Type[] { typeof(BodyHarvest) })]
		internal class Panel_BodyHarvest_CanEnable
		{
			private static void Postfix(BodyHarvest bodyHarvest, ref bool __result)
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled) return;

				if (!(bodyHarvest.GetCondition() < 0.5f))
				{
					__result = true;
				}
			}
		}
		[HarmonyPatch(typeof(Panel_BodyHarvest), nameof(Panel_BodyHarvest.Enable), new Type[] { typeof(bool), typeof(BodyHarvest), typeof(bool), typeof(ComingFromScreenCategory) })]
		internal class Panel_BodyHarvest_Enable
		{
			private static void Postfix(Panel_BodyHarvest __instance, BodyHarvest bh, bool enable)
			{
				if (!MenuManager.modEnabled || !enable || !__instance.CanEnable(bh)) return;

				if (isCarryingCarcass) DropCarcass();

				if (Global.carcassMovingEnabled)
				{
					if (IsMovableCarcass(bh) && !isCarryingCarcass)
					{
						bodyHarvest = bh;
						carcassObj = bh.gameObject;

						if (moveCarcassBtnObj == null)
						{
							AddCarcassMoveButton(__instance);
						}
					} else
					{
						// extra check to remove carcass move button
						// would appear if you select a valid carcass and then open screen on an invalid one.
						if (moveCarcassBtnObj != null)
						{
							RemoveCarcassMoveButton(__instance);
						}
					}
				}
				else
				{
					if (moveCarcassBtnObj != null)
					{
						RemoveCarcassMoveButton(__instance);
					}
				}
			}
		}
		//Ensures carried carcass is repopulated from save data if the player was carrying a carcass on last save of the game
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.SetAudioModeForLoadedScene))]
		internal class GameManager_SetAudioModeForLoadedScene
		{
			private static void Postfix()
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				if (carcassObj == null) isCarryingCarcass = false;
				if (bodyHarvest != null) bodyHarvest.enabled = true;

				saveTrigger = true;
			}
		}
		[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.Update))]
		internal class PlayerManager_Update_Post
		{
			private static void Postfix(PlayerManager __instance)
			{
				if (saveTrigger == true)
				{
					saveTrigger = false;
					GearManager.UpdateAll();
					GameManager.TriggerSurvivalSaveAndDisplayHUDMessage();
				}
			}
		}
		[HarmonyPatch(typeof(LoadScene), nameof(LoadScene.Activate), new Type[] { typeof(bool)})]
		internal class LoadScene_Activate
		{
			private static void Postfix()
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass || carcassObj == null) return;

				//Do not destroy carcass object through scene transition
				DontDestroyOnLoad(carcassObj.transform.root.gameObject);

				//Disable carcass to prevent it saving in the previous scene
				bodyHarvest.enabled = false;
			}
		}
		[HarmonyPatch(typeof(GameManager), nameof(GameManager.TriggerSurvivalSaveAndDisplayHUDMessage))]
		internal class GameManager_TriggerSurvivalSaveAndDisplayHUDMessage
		{
			private static void Prefix()
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				// Carcass carried through to indoor scene
				if (bodyHarvest != null)
				{
					// Reenable carcass so it will get saved in new scene
					bodyHarvest.enabled = true;
					MoveCarcassToPlayerPosition();
					AddCarcassToSceneSaveData();
				}
			}
		}
		[HarmonyPatch(typeof(GameAudioManager), nameof(GameAudioManager.PlayGUIError))]
		internal class GameAudioManager_PlayGUIError_Pre
		{
			private static bool Prefix()
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled) return true;

				Panel_BodyHarvest panelBodyHarvest = InterfaceManager.GetPanel<Panel_BodyHarvest>();

				return StopErrorDueToCarcassBeingFrozen(panelBodyHarvest);
			}
		}
		[HarmonyPatch(typeof(RopeClimbPoint), nameof(RopeClimbPoint.OnRopeTransition), new Type[] { typeof(bool) })]
		internal static class RopeClimbPoint_OnRopeTransition
		{
			private static void Postfix()
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				DropCarcass();
			}
		}
		[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.EquipItem), new Type[] { typeof(GearItem), typeof(bool) })]
		internal class PlayerManager_EquipItem
		{
			private static bool Prefix()
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return true;

				Utilities.DisallowActionWithModMessage("CANNOT EQUIP ITEM WHILE CARRYING CARCASS");
				return false;
			}
		}
		[HarmonyPatch(typeof(InputManager), nameof(InputManager.ExecuteAltFire))]
		internal class InputManager_ExecuteAltFire
		{
			private static bool Prefix()
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return true;

				return false;
			}
		}
		[HarmonyPatch(typeof(EquipItemPopup), nameof(EquipItemPopup.ShouldHideEquipPopup))]
		internal class EquipItemPopup_ShouldHideEquipPopup
		{
			private static void Postfix(ref bool __result)
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				__result = false;
			}
		}
		[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.PlayerCanSprint))]
		internal static class PlayerManager_PlayerCanSprint
		{
			private static void Postfix(ref bool __result)
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				__result = false;
			}
		}
		[HarmonyPatch(typeof(Panel_HUD), nameof(Panel_HUD.Update))]
		internal class Panel_HUD_Update
		{
			private static void Postfix(Panel_HUD __instance)
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				__instance.m_Sprite_SprintCenter.color = __instance.m_SprintBarNoSprintColor;
				__instance.m_Sprite_SprintBar.color = __instance.m_SprintBarNoSprintColor;
			}
		}
		[HarmonyPatch(typeof(Fatigue), nameof(Fatigue.CalculateFatigueIncrease), new Type[] { typeof(float) })]
		internal class Fatigue_CalculateFatigueIncrease
		{
			private static void Postfix(ref float __result)
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				__result *= (1f + carcassWeight * carryAddedFatiguePerKilo);
			}
		}
		[HarmonyPatch(typeof(PlayerManager), nameof(PlayerManager.CalculateModifiedCalorieBurnRate), new Type[] { typeof(float) })]
		internal static class PlayerManager_CalculateModifiedCalorieBurnRate
		{
			private static void Postfix(ref float __result)
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				__result += carcassWeight * carryAddedCaloryBurnPerKilo;
			}
		}
		[HarmonyPatch(typeof(Encumber), nameof(Encumber.GetEncumbranceSlowdownMultiplier))]
		internal class Encumber_GetEncumbranceSlowdownMultiplier
		{
			private static void Postfix(ref float __result)
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				__result *= Mathf.Clamp((1f - carcassWeight * carryAddedSlowDownPerKilo), 0.1f, 0.8f);
			}
		}
		[HarmonyPatch(typeof(Inventory), nameof(Inventory.GetExtraScentIntensity))]
		internal class Inventory_GetExtraScentIntensity
		{
			private static void Postfix(ref float __result)
			{
				if (!MenuManager.modEnabled || !Global.carcassMovingEnabled || !isCarryingCarcass) return;

				__result += 33f;
			}
		}
		[HarmonyPatch(typeof(BaseAi), nameof(BaseAi.SetDamageImpactParameter), new Type[] { typeof(BaseAi.DamageSide), typeof(int), typeof(BaseAi.SetupDamageParamsOptions) })]
		internal class BaseAi_SetDamageImpactParameter
		{
			private static void Prefix(BaseAi __instance, ref BaseAi.DamageSide side)
			{
				if (!MenuManager.modEnabled) return;

				if (__instance.m_AiWolf != null || __instance.m_AiStag != null)
				{
					//Ensures kills land on left side when dead - prevents camera orientation bug when reviewing dropped carcasses in harvest panel and simplifies positioning
					side = BaseAi.DamageSide.DamageSideLeft;
				}
			}
		}

		internal void Update()
		{
			if (GameManager.m_IsPaused || !isCarryingCarcass) return;

			if (!Global.carcassMovingEnabled)
			{
				DropCarcass();
			}
			else
			{
				DisplayDropCarcassPopUp();
				if (InputManager.GetAltFirePressed(this) || HasInjuryPreventingCarry() || GameManager.GetPlayerStruggleComponent().InStruggle())
				{
					DropCarcass();
				}
			}
		}
		internal static void AddCarcassMoveButton(Panel_BodyHarvest panelBodyHarvest)
		{
			moveCarcassBtnObj = Instantiate(panelBodyHarvest.m_Mouse_Button_Harvest, panelBodyHarvest.m_Mouse_Button_Harvest.transform);
			moveCarcassBtnObj.GetComponentInChildren<UILocalize>().key = "MOVE CARCASS";

			panelBodyHarvest.m_Mouse_Button_Harvest.transform.localPosition += new Vector3(-100f, 0f, 0f);
			moveCarcassBtnObj.transform.localPosition = new Vector3(+200f, 0f, 0f);

			UIButton moveCarcassButton = moveCarcassBtnObj.GetComponentInChildren<UIButton>();
			moveCarcassButton.onClick.Clear();
			moveCarcassButton.onClick.Add(new EventDelegate(new Action(OnMoveCarcass)));
		}
		internal static void RemoveCarcassMoveButton(Panel_BodyHarvest panelBodyHarvest)
		{
			DestroyImmediate(moveCarcassBtnObj);
			panelBodyHarvest.m_Mouse_Button_Harvest.transform.localPosition += new Vector3(+100f, 0f, 0f);
		}
		internal static bool IsMovableCarcass(BodyHarvest bodyHarvest)
		{
			return (bodyHarvest.name.Contains("Doe") || bodyHarvest.name.Contains("Stag") || bodyHarvest.name.Contains("Deer") || bodyHarvest.name.Contains("Wolf")) && !bodyHarvest.name.Contains("Quarter");
		}
		internal static void OnMoveCarcass()
		{
			if (HasInjuryPreventingCarry())
			{
				Utilities.DisallowActionWithModMessage("cannot move carcass while injured");
			}
			else
			{
				PickUpCarcass();
				InterfaceManager.GetPanel<Panel_BodyHarvest>().OnBack();
			}
		}
		internal static void PickUpCarcass()
		{
			isCarryingCarcass = true;
			carcassWeight = bodyHarvest.m_MeatAvailableKG + bodyHarvest.GetGutsAvailableWeightKg() + bodyHarvest.GetHideAvailableWeightKg();
			carcassOriginalScene = GameManager.m_ActiveScene;
			CarcassMoving carcassMoving = carcassObj.GetComponent<CarcassMoving>();
			if (carcassMoving == null)
			{
				carcassMoving = carcassObj.AddComponent<CarcassMoving>();
			}
			GameManager.GetPlayerManagerComponent().UnequipItemInHands();
			HideCarcassFromView();
			PlayCarcassPickUpAudio();
		}
		internal static void DropCarcass()
		{
			isCarryingCarcass = false;
			MoveCarcassToPlayerPosition();
			BringCarcassBackIntoView();
			if (GameManager.m_ActiveScene != carcassOriginalScene)
			{
				AddCarcassToSceneSaveData();
			}
			PlayCarcassDropAudio();
			EnableCarcassMeshes();
			bodyHarvest = null;
			carcassObj = null;
		}

		internal static void EnableCarcassMeshes()
		{
			SkinnedMeshRenderer[] skinnedMeshRenderers = carcassObj.GetComponentsInChildren<SkinnedMeshRenderer>();
			foreach (SkinnedMeshRenderer skinnedMeshRenderer in skinnedMeshRenderers)
			{
				skinnedMeshRenderer.enabled = true;
			}
		}
		internal static void HideCarcassFromView()
		{
			carcassObj.transform.localScale = new Vector3(0f, 0f, 0f);
		}
		internal static void BringCarcassBackIntoView()
		{
			carcassObj.transform.localScale = new Vector3(1f, 1f, 1f);
		}
		internal static void DisplayDropCarcassPopUp()
		{
			InterfaceManager.GetPanel<Panel_HUD>().m_EquipItemPopup.ShowGenericPopupWithDefaultActions(string.Empty, "DROP CARCASS");
		}
		internal static bool HasInjuryPreventingCarry()
		{
			return GameManager.GetSprainedAnkleComponent().HasSprainedAnkle() || GameManager.GetSprainedWristComponent().HasSprainedWrist() ||
				GameManager.GetSprainedWristComponent().HasSprainedWrist() || GameManager.GetBrokenRibComponent().HasBrokenRib();
		}
		internal static void MoveCarcassToPlayerPosition()
		{
			carcassObj.transform.position = GameManager.GetPlayerTransform().position;
			carcassObj.transform.rotation = GameManager.GetPlayerTransform().rotation * Quaternion.Euler(0f, 90f, 0f);
		}
		internal static void AddCarcassToSceneSaveData()
		{
			BodyHarvestManager.AddBodyHarvest(bodyHarvest);
			UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(carcassObj.transform.root.gameObject, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
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
		internal static bool StopErrorDueToCarcassBeingFrozen(Panel_BodyHarvest panelBodyHarvest)
		{
			if (panelBodyHarvest != null)
			{
				if (!HarvestAmmountsAreSelected(panelBodyHarvest))
				{
					return false;
				}
			}
			return true;
		}
		internal static void ResetCarcassMoving()
		{
			Utilities.ModLog("ResetCarcassMoving");

			Panel_BodyHarvest panelBodyHarvest = InterfaceManager.GetPanel<Panel_BodyHarvest>();

			if (moveCarcassBtnObj != null)
			{
				RemoveCarcassMoveButton(panelBodyHarvest);
			}
			isCarryingCarcass = false;
			bodyHarvest = null;
			carcassObj = null;
			Utilities.ModLog("..Done");
		}
	}
}
