using MelonLoader;
using UnityEngine;

namespace RelentlessNight
{
    internal class Mod : MelonMod
    {
        public override void OnApplicationStart()
        {
            Settings.OnLoad();

            //Debug.Log("[house-lights] Version " + Assembly.GetExecutingAssembly().GetName().Version);
        }
    }
}
