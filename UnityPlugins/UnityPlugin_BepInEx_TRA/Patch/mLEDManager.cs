using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    /// <summary>
    /// Prevent connection search on COM port
    /// </summary>
    internal class mLEDManager
    {
        [HarmonyPatch(typeof(LEDManager), "Awake")]
        class Awake
        {
            static bool Prefix()
            {
                DemulShooter_Plugin.MyLogger.LogMessage("LEDManager.Awake()");
                return false;
            }
        }        
    }
}
