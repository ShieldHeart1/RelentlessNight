using System;
using System.Collections.Generic;
using Harmony;
using UnityEngine;
using UnityEngine.SceneManagement;
using IL2CPP = Il2CppSystem.Collections.Generic;

namespace RelentlessNight
{
    public class Utilities
    {
        internal static List<GameObject> GetRootObjects()
        {
            List<GameObject> rootObj = new List<GameObject>();

            for (int i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                Scene scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);

                GameObject[] sceneObj = scene.GetRootGameObjects();

                foreach (GameObject obj in sceneObj)
                {
                    rootObj.Add(obj);
                }
            }

            return rootObj;
        }

        internal static void GetChildrenWithName(GameObject obj, string name, List<GameObject> result)
        {
            if (obj.transform.childCount > 0)
            {
                for (int i = 0; i < obj.transform.childCount; i++)
                {
                    GameObject child = obj.transform.GetChild(i).gameObject;

                    if (child.name.ToLower().Contains(name))
                    {
                        result.Add(child);
                    }

                    GetChildrenWithName(child, name, result);
                }
            }
        }

        internal static void CreateTorchLightingSceneItem(string objectName)
        {
            List<GameObject> rObjs = GetRootObjects();
            List<GameObject> result = new List<GameObject>();

            foreach (GameObject rootObj in rObjs)
            {
                GetChildrenWithName(rootObj, objectName, result);

                if (result.Count > 0)
                {
                    foreach (GameObject child in result)
                    {
                        //Debug.Log(child.name);
                        child.layer = 12;
                    }
                }
            }
        }

        public static void HyperTime()
        {
            if (RnGlobal.rnTimeAccel != 1000) RnGlobal.rnTimeAccel = 1000;
            else RnGlobal.rnTimeAccel = 1;
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
        public class ConsoleManager_RegisterCommands_Post
        {
            public static void Postfix()
            {
                uConsole.RegisterCommand("rn_hypertime", new System.Action(HyperTime));
                uConsole.RegisterCommand("rn_fires", new System.Action(Fires));
            }
        }           
    }
}
