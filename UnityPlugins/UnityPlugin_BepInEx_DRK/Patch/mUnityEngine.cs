using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mUnityEngine
    {
        /// <summary>
        /// Game automatically ask for set_Fullscreen() whatever we choose
        /// Disabling that function will stop the game from changing screen mode after we set our own in ApplicationManager::Awake()
        /// Note that it does not prevent us to choose full screen or ALT+RETURN during game
        /// </summary>
        [HarmonyPatch(typeof(UnityEngine.Screen), "set_fullScreen")]
        class set_fullScreen
        {
            static bool Prefix(ref bool value)
            {
                return false;
            }
        }
    }
}
