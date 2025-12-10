using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mHudWindow
    {
        /// <summary>
        /// Using this class to intercept damage info on screen
        /// </summary>
        [HarmonyPatch(typeof(HudWindow), "OnPlayerHitByEnemy")]
        class OnPlayerHitByEnemy
        {
            static bool Prefix(EnemyManager.IEnemyTypeProvider typeProvider, SkyrideUtils.PlayerID player)
            {
                DemulShooter_Plugin.MyLogger.LogMessage("mHudWindow.OnPlayerHitByEnemy() : PlayerId=" + player.ToString());

                if (player == SkyrideUtils.PlayerID.One)
                {
                    DemulShooter_Plugin.OutputData.Damaged[0] = 1;
                }
                else if (player == SkyrideUtils.PlayerID.Two)
                {
                    DemulShooter_Plugin.OutputData.Damaged[1] = 1;
                }
                return true;
            }
        }
    }
}
