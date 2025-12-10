using HarmonyLib;
using Now;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    class mBulletCounter
    {
        [HarmonyPatch(typeof(BulletCounter), "SetBulletCount")]
        class SetBulletCount
        {
            static bool Prefix(PlayerPanel ___playerPanel, int count)
            {
                DemulShooter_Plugin.MyLogger.LogMessage("SetBulletCount: " + ___playerPanel.playerIndex + ", count=" + count);
                return true;
            }
        }
    }
}
