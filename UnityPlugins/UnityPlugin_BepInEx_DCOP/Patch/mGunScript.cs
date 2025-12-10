using HarmonyLib;
using UnityEngine;

namespace BepInEx_DemulShooter_Plugin
{
    class mGunScript
    {
        /// <summary>
        /// Get ammo info
        /// </summary>
        [HarmonyPatch(typeof(GunScript), "Update")]
        class Update
        {
            static bool Prefix(bool ___gunEnabled, int ___ammoCount)
            {
                if (___gunEnabled)
                    DemulShooter_Plugin.OutputData.P1_Ammo = (byte)___ammoCount;
                else
                    DemulShooter_Plugin.OutputData.P1_Ammo = 0;
                return true;
            }
        }
    }
}
