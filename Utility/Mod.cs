using MelonLoader;

namespace RelentlessNight
{
    internal class Mod : MelonMod
    {
        public override void OnInitializeMelon()
        {
            Settings.OnLoad();
        }
    }
}