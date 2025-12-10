using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mRS232
    {
        /// <summary>
        /// Prevent the game to open Serial COM
        /// </summary>
        [HarmonyPatch(typeof(RS232), "Connect")]
        class Connect
        {
            static bool Prefix()
            {
                return false;
            }
        }
    }
}
