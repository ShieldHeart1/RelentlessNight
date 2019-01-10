using Harmony;
using RelentlessNight;
using UnityEngine;

[HarmonyPatch(typeof(FirstPersonLightSource), "TurnOnEffects", null)]
public class FirstPersonLightSource_TurnOnEffects_Pos
{
    private static void Postfix(FirstPersonLightSource __instance)
    {
        //Debug.Log("FirstPersonLightSource_TurnOnEffects_Pos");
        if (!RnGl.rnActive || !RnGl.rnFireShouldHeatWholeScene) return;

        HeatSource componentInChildren = __instance.m_FXGameObject.GetComponentInChildren<HeatSource>();
        if (componentInChildren)
        {
            componentInChildren.TurnOff();
        }
    }
}

[HarmonyPatch(typeof(Rest), "ShouldInterruptSleep", null)]
internal static class Rest_ShouldInterruptSleep_Pos
{
    private static void Postfix(Rest __instance, ref bool __result)
    {
        //Debug.Log("Rest_ShouldInterruptSleep_Pos");
        if (!RnGl.rnActive) return;

        if (GameManager.GetFreezingComponent().IsFreezing() && GameManager.GetFreezingComponent().CalculateBodyTemperature() < 0f && !__result)
        {
            AccessTools.Field(typeof(Rest), "m_ShouldInterruptWhenFreezing").SetValue(__instance, true);
    
            if (InterfaceManager.m_Panel_HUD.m_Sprite_SystemFadeOverlay.alpha < 1f)
            {
                CameraFade.StartAlphaFade(Color.black, true, 1f, 0.1f, null);
            }
            __result = true;
        }        
    }
}