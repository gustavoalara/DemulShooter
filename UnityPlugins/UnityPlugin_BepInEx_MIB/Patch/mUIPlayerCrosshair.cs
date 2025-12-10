using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mUIPlayerCrosshair
    {
        /// <summary>
        /// Removing corsshairs, but not used in-game
        /// </summary>
        [HarmonyPatch(typeof(UIPlayerCrosshairs), "Update")]
        class Update
        {
            static bool Prefix(UIPlayerCrosshairs __instance)
            {
                if (!DemulShooter_Plugin.CrossHairVisibility)
                {
                    __instance.Player1.enabled = false;
                    __instance.Player2.enabled = false;

                    return false;
                }
                return true;
            }
        }
    }
}
