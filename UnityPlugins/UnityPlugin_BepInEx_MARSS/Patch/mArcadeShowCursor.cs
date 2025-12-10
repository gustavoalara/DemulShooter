using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
using UnityEngine;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mArcadeShowCursor
    {
        /// <summary>
        /// Using that call to insert Resolution Change
        /// </summary>
        [HarmonyPatch(typeof(ArcadeShowCursor), "OnEnter")]
        class OnEnter
        {
            static bool Prefix(ArcadeShowCursor __instance)
            {
                DemulShooter_Plugin.MyLogger.LogWarning("ArcadeShowCursor.Onenter()");
                __instance.showCursor = false;
                if (DemulShooter_Plugin.ForceResolution)
                    Screen.SetResolution(DemulShooter_Plugin.ScreenWidth, DemulShooter_Plugin.ScreenHeight, DemulShooter_Plugin.Fullscreen);
                return true;
            }
        }
    }
}
