using HarmonyLib;
using UnityEngine;
using UnityPlugin_BepInEx_Core;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mUnityEngine
    {
        /// <summary>
        /// Replacing OPluginControllers.getButtonDown as the flag is already used in the main plugin
        /// So just using the Keycode as GetKeyDown is valid untill end of frame, no matter ther number of request
        /// </summary>
        [HarmonyPatch(typeof(UnityEngine.Input), "GetButtonDown", new System.Type[] { typeof(string) })]
        class GetButtonDown
        {
            static bool Prefix(string buttonName, ref bool __result)
            {
                if (buttonName.Equals("PlayerStart1"))
                {
                    __result = Input.GetKeyDown((KeyCode)DemulShooter_Plugin.PluginControllers[0].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode);
                    return false;
                }
                else if (buttonName.Equals("PlayerStart2"))
                {
                    __result = Input.GetKeyDown((KeyCode)DemulShooter_Plugin.PluginControllers[1].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode);
                    return false;
                }
                return true;
            }
        }
    }
}
