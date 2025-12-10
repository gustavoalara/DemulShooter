using HarmonyLib;
using Virtuallyz.VRShooter;

namespace BepInEx_DemulShooter_Plugin
{
    class mGameManager
    {
        /// <summary>
        /// On LevelStopped (quit or died) => set Ammo and Life to 0
        /// </summary>
        [HarmonyPatch(typeof(GameManager), "OnLevelActStopped")]
        class Update
        {
            static bool Prefix()
            {
                DemulShooter_Plugin.MyLogger.LogWarning("Virtuallyz.VRShooter.GameManager.OnLevelActStopped()");
                for (int i = 0; i < DemulShooter_Plugin.MAX_PLAYERS; i++)
                {
                    DemulShooter_Plugin.OutputData.Life[i] = 0;
                    DemulShooter_Plugin.OutputData.Ammo[i] = 0; 
                }

                return true;
            }

        }
    }
}
