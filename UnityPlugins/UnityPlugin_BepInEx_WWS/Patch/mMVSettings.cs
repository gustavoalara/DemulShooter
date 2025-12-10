using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mMVSettings
    {
        /// <summary>
        /// Lot of Input.GetKeyDown() calls for Debug ?
        /// </summary>
        [HarmonyPatch(typeof(MVSettings), "Update")]
        class Update
        {
            static bool Prefix()
            {
                return false;
            }
        }
    }
}
