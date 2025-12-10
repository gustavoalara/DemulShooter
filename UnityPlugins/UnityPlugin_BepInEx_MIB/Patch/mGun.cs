using HarmonyLib;
using UnityEngine;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mGun
    {
        /// <summary>
        /// Using this to get recoil
        /// </summary>
        [HarmonyPatch(typeof(Gun), "Fire")]
        class Fire
        {
            static void Prefix(Gun __instance)
            {
                int PlayerIndex = __instance.Holster.PlayerIndex;
                if (!Global.IsPlayerAI(PlayerIndex))
                    DemulShooter_Plugin.OutputData.Recoil[PlayerIndex] = 1;
            }
        }


        /// <summary>
        /// Removing gun model from screen
        /// </summary>
        [HarmonyPatch(typeof(Gun), "Update")]
        class Upddate
        {
            static void Prefix(Gun __instance)
            {
                if (!DemulShooter_Plugin.GunVisibility) 
                    __instance.transform.localScale = new Vector3();
            }
        }

        
    }
}
