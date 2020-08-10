using MelonLoader;

namespace RelentlessNight
{
    internal class Mod : MelonMod
    {
        public override void OnApplicationStart()
        {
            Settings.OnLoad();
        }
    }
}
