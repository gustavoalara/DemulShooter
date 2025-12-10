using HarmonyLib;
using MIB;
using UnityEngine;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mInitialisation
    {
        [HarmonyPatch(typeof(Initialisation), "Start")]
        class Start
        {
            static void Postfix()
            {
                if (DemulShooter_Plugin.ForceResolution)
                    Screen.SetResolution(DemulShooter_Plugin.ScreenWidth, DemulShooter_Plugin.ScreenHeight, DemulShooter_Plugin.Fullscreen);

                MIBGameSettings.LoadFile();
                MIBCore.Instance.GetCreditInformaion();
            }
        }
    }
}
