using System;
using Harmony;
using UnityEngine;
using IL2CPP = Il2CppSystem.Collections.Generic;

namespace RelentlessNight
{
    public class RnDebug
    {
        public static void Log(string debugMessage)
        {            
            #if DEBUG
            Debug.Log("-RN- " + debugMessage);
            #endif
        }

        public static void HyperTime()
        {
            if (RnGl.rnTimeAccel != 7200) RnGl.rnTimeAccel = 7200;
            else RnGl.rnTimeAccel = 1;
        }

        public static void Fires()
        {
            IL2CPP.List<HeatSource> heatsources = GameManager.GetHeatSourceManagerComponent().m_HeatSources;
            for (int i = 0; i < heatsources.Count; i++)
            {
                Debug.Log("-RN- Fire" + i.ToString() + ": " + heatsources[i].GetTempIncrease(GameManager.GetPlayerTransform().position).ToString());
            }
        }

        [HarmonyPatch(typeof(ConsoleManager), "RegisterCommands", new Type[] { })]
        public class ConsoleManager_RegisterCommands_Pos
        {
            public static void Postfix()
            {
                uConsole.RegisterCommand("rn_hypertime", new System.Action(HyperTime));
                uConsole.RegisterCommand("rn_fires", new System.Action(Fires));
            }
        }           
    }
}
