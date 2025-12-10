using HarmonyLib;
using UnityEngine;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    class mGameChooseUI2
    {
        /// <summary>
        /// Disabling mouse input on Level Select Screen
        /// </summary>
        [HarmonyPatch(typeof(GameChooseUI2), "Update")]
        class Update
        {
            static bool Prefix()
            { 
                return false;
            }
        }

        /// <summary>
        /// Getting recoil in Level-Select screen
        /// </summary>
        [HarmonyPatch(typeof(GameChooseUI2), "HitAction")]
        class HitAction
        {
            static bool Prefix(Vector3 shootPoint, PlayerType playerType)
            {
                //DemulShooter_Plugin.MyLogger.LogWarning("GameChooseUI2.HitAction(): playerType=" + playerType);
                DemulShooter_Plugin.OutputData.Recoil[(int)playerType] = 1;
                return true;
            }
        }
    }
}
