using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin
{
    class mtest_screen
    {
        /// <summary>
        /// Changing resolution when the game is opening it's window
        /// </summary>
        [HarmonyPatch(typeof(test_screen), "Start")]
        class start
        {
            static bool Prefix()
            {
                if (DemulShooter_Plugin.ForceResolution)
                {
                    DemulShooter_Plugin.MyLogger.LogMessage("test_screen.start() => Changing resolution to " + DemulShooter_Plugin.ScreenWidth + "x" + DemulShooter_Plugin.ScreenHeight + " fullscreen: " + DemulShooter_Plugin.Fullscreen);
                    UnityEngine.Screen.SetResolution(DemulShooter_Plugin.ScreenWidth, (int)DemulShooter_Plugin.ScreenHeight, DemulShooter_Plugin.Fullscreen);
                    return false;
                }
                return true;
            }
        }
    }
}
