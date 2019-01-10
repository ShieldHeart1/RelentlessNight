using Harmony;
using RelentlessNight;
using UnityEngine;
using AK;

internal class RnHarvest
{
    public static Panel_BodyHarvest harvestPanelInstance;
    public static GameObject carcassObj;
    public static GameObject moveCarcassBtnObj;
    public static UIButton moveCarcassBtn;

    public static bool carcassBtnExists = false;
    public static bool isCarryingCarcass = false;
    public static bool rotateCamera = false;
    public static bool playSoundClips = false;

    public static float carcassWeight = 0f;
    public static float totalCameraYaw = 0f;
    public static float startingCameraYaw;
    public static float startingCameraPitch;
    public static float CameraYaw;
    public static float CameraPitch;

    internal static void OnMoveCarcass()
    {
        harvestPanelInstance.OnBack();        

        GameManager.GetPlayerManagerComponent().UnequipItemInHands();
        GameManager.GetPlayerManagerComponent().SetControlMode(PlayerControlMode.Locked);       

        startingCameraYaw = GameManager.GetVpFPSCamera().Angle.x;
        startingCameraPitch = GameManager.GetVpFPSCamera().Angle.y;
        CameraYaw = startingCameraYaw;
        CameraPitch = startingCameraPitch;    
        rotateCamera = true;
        playSoundClips = true;
    }
}

[HarmonyPatch(typeof(Panel_BodyHarvest), "Enable", null)]
public static class Panel_BodyHarvest_Enable_Pos
{
    public static void Postfix(Panel_BodyHarvest __instance, BodyHarvest bh, bool enable)
    {
        if (!RnGl.rnActive || !enable || !__instance.CanEnable(bh)) return;

        if (bh.m_DisplayName.Contains("Rabbit"))
        {
            UnityEngine.Object.Destroy(RnHarvest.moveCarcassBtnObj);
            UnityEngine.Object.Destroy(RnHarvest.moveCarcassBtn);
            return;
        }         
        if (RnHarvest.moveCarcassBtnObj == null)
        {
            RnHarvest.moveCarcassBtnObj = GameObject.Instantiate(__instance.m_Mouse_Button_Harvest, __instance.m_Mouse_Button_Harvest.transform);
            RnHarvest.moveCarcassBtnObj.transform.localPosition = new Vector3(0f, -50f, 0f); 
            RnHarvest.moveCarcassBtnObj.GetComponentInChildren<UILocalize>().key = "MOVE CARCASS";

            RnHarvest.moveCarcassBtn = RnHarvest.moveCarcassBtnObj.GetComponentInChildren<UIButton>();
            RnHarvest.moveCarcassBtn.onClick.Clear();
            RnHarvest.moveCarcassBtn.onClick.Add(new EventDelegate(RnHarvest.OnMoveCarcass));
        }
        RnHarvest.carcassWeight = (bh.m_MeatAvailableKG + bh.GetGutsAvailableWeightKg() + bh.GetHideAvailableWeightKg());
        RnHarvest.harvestPanelInstance = __instance;
        RnHarvest.carcassObj = bh.gameObject;
        UnityEngine.Object.DontDestroyOnLoad(RnHarvest.carcassObj);
    }
}

[HarmonyPatch(typeof(EquipItemPopup), "Update", null)]
internal static class EquipItemPopup_Update_Pos
{
    private static void Postfix()
    {
        if (!RnGl.rnActive) return;

        if (InputManager.GetAltFirePressed() && RnGl.glIsCarryingCarcass)
        {
            GameAudioManager.PlaySound(EVENTS.PLAY_BODYFALLLARGE, InterfaceManager.GetSoundEmitter());
            RnGl.glIsCarryingCarcass = false;

            RnHarvest.carcassObj.transform.position = GameManager.GetPlayerTransform().position;
            RnHarvest.carcassObj.transform.rotation = GameManager.GetPlayerTransform().rotation;
            RnHarvest.carcassObj.SetActive(true);
            BodyHarvestManager.AddBodyHarvest(RnHarvest.carcassObj.GetComponent<BodyHarvest>());
        }       
    }
}

[HarmonyPatch(typeof(vp_FPSCamera), "Update", null)]
internal static class vp_FPSCamera_Update_Pos
{
    private static void Postfix(vp_FPSCamera __instance)
    {
        if (!RnGl.rnActive || !RnHarvest.rotateCamera || GameManager.GetVpFPSCamera().m_PanViewCamera.IsDetached()) return;

        if (RnHarvest.playSoundClips) // Trigger at start of carcass pick-up movement
        {
            vp_Spring m_RotationSpring = (vp_Spring)AccessTools.Field(typeof(vp_FPSCamera), "m_RotationSpring").GetValue(__instance);
            m_RotationSpring.AddForce(new Vector3(0f, 0f, 2f));

            GameAudioManager.PlaySound("Play_RopeGetOn", InterfaceManager.GetSoundEmitter());
            RnHarvest.playSoundClips = false;
        }        
        RnHarvest.CameraYaw += Time.deltaTime * 180f; 
        RnHarvest.CameraPitch -= Time.deltaTime * RnHarvest.startingCameraPitch;
        RnHarvest.totalCameraYaw += Time.deltaTime * 180f;

        __instance.SetAngle(RnHarvest.CameraYaw, -RnHarvest.CameraPitch);

        if (RnHarvest.totalCameraYaw > 180f) // Trigger at end of carcass pick-up movement
        {
            RnHarvest.totalCameraYaw = 0;
            RnHarvest.rotateCamera = false;
            RnGl.glIsCarryingCarcass = true;

            GameAudioManager.PlaySound(EVENTS.PLAY_EXERTIONLOW, InterfaceManager.GetSoundEmitter());
            GameManager.GetPlayerManagerComponent().SetControlMode(PlayerControlMode.Normal);

            // Display instructional popup for dropping carcass
            EquipItemPopup equipItemPopup = InterfaceManager.m_Panel_HUD.m_EquipItemPopup;
            AccessTools.Method(typeof(EquipItemPopup), "ShowItemIcons", null, null).Invoke(equipItemPopup, new object[] { string.Empty, "DROP CARCASS", false, false });
            equipItemPopup.m_ButtonPromptFire.ShowPromptForKey(string.Empty, "Fire");
            equipItemPopup.m_ButtonPromptAltFire.ShowPromptForKey("DROP CARCASS", "AltFire");
            AccessTools.Method(typeof(EquipItemPopup), "MaybeRepositionAltFireButtonPrompt", null, null).Invoke(equipItemPopup, new object[] { string.Empty });
            equipItemPopup.m_ButtonPromptReload.ShowPromptForKey(string.Empty, string.Empty);
            equipItemPopup.m_ButtonPromptHolster.ShowPromptForKey(string.Empty, string.Empty);

            RnHarvest.carcassObj.SetActive(false);
        }
    }
}

[HarmonyPatch(typeof(BodyHarvestManager), "Serialize", null)]
public static class BodyHarvestManager_Serialize_Pre
{
    public static void Prefix()
    {
        if (!RnGl.rnActive || RnHarvest.carcassObj == null) return;
        
        if (RnHarvest.carcassObj.activeSelf == false)
        {
            RnHarvest.carcassObj.SetActive(true);
        }
    }
}

[HarmonyPatch(typeof(SaveGameSystem), "SaveSceneData", null)]
public static class SaveGameSystem_SaveSceneData_Pos
{
    public static void Postfix()
    {
        if (!RnGl.rnActive || RnHarvest.carcassObj == null) return;

        if (RnGl.glIsCarryingCarcass)
        {
            RnHarvest.carcassObj.SetActive(false);
        }
    }
}


[HarmonyPatch(typeof(EquipItemPopup), "ShouldHideEquipAndAmmoPopups", null)]
public static class EquipItemPopUp_ShouldHideEquipAndAmmoPopups_Pos
{
    public static void Postfix(EquipItemPopup __instance, ref bool __result)
    {
        if (!RnGl.rnActive || !RnGl.glIsCarryingCarcass) return;

        float m_TimeToHidePopup = (float)AccessTools.Field(typeof(EquipItemPopup), "m_TimeToHidePopup").GetValue(__instance);
        if (RealTime.time > m_TimeToHidePopup) return;

        __result = false;
    }
}

[HarmonyPatch(typeof(PlayerManager), "PlayerCanSprint", null)]
public static class PlayerManager_PlayerCanSprint_Pos
{
    public static void Postfix(ref bool __result)
    {
        if (!RnGl.rnActive) return;

        if (RnGl.glIsCarryingCarcass) __result = false;
    }
}

[HarmonyPatch(typeof(Encumber), "GetEncumbranceSlowdownMultiplier", null)]
internal static class Encumber_GetEncumbranceSlowdownMultiplier_Pos
{
    private static void Postfix(ref float __result)
    {
        if (!RnGl.rnActive || !RnGl.glIsCarryingCarcass) return;

        float carcassEncumberFactor = RnHarvest.carcassWeight / 50f;

        __result *= Mathf.Clamp((1f - carcassEncumberFactor), 0.1f, 0.8f);
    }
}

[HarmonyPatch(typeof(Fatigue), "CalculateFatigueIncrease", null)]
internal static class Fatigue_CalculateFatigueIncrease_Pos
{
    private static void Postfix(ref float __result)
    {
        if (!RnGl.rnActive || !RnGl.glIsCarryingCarcass) return;

        __result *= Mathf.Clamp((1f + RnHarvest.carcassWeight / 50f), 1.2f, 2f);
    }
}