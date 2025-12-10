using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin
{
    internal class mAmwaysOnTop
    {
        /// <summary>
        /// Deactivating the attempt of pinning the game window, it's messing with other windows and I don't like that
        /// </summary>
        [HarmonyPatch(typeof(AlwaysOnTop), "AssignTopmostWindow")]
        class AssignTopmostWindow
        {
            static bool Prefix(string WindowTitle, bool MakeTopmost)
            {
                DemulShooter_Plugin.MyLogger.LogMessage("AlwaysOnTop.AssignTopmostWindow(): WindowTitle=" + WindowTitle + ", MakeTopMost=" + MakeTopmost);
                return false;
            }
        }
    }
}
