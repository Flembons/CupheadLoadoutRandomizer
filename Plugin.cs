using BepInEx;

namespace LoadoutRandomizer
{
    [BepInPlugin("CupheadLoadoutRandomizer", "LoadoutRandomizer", "1.0")]
    public class Plugin : BaseUnityPlugin
    {
        private void Awake()
        {
            new LoadoutRandomizer().Init();
            Logger.LogInfo($"Loadout Randomizer is loaded!");
        }
    }
}
