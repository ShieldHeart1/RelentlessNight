using MelonLoader;
using UnityEngine;

namespace RelentlessNight.Utility
{
    internal class Mod : MelonMod
    {
        public override void OnApplicationStart()
        {
            Debug.Log($"[{InfoAttribute.Name}] Version {InfoAttribute.Version} loaded!");
        }
    }
}
