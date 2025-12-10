using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mPlayerWindow
    {
        /// <summary>
        /// Adding the possibility to send a "START button pressed" signal by shooting (like it is written on screen)
        /// </summary>
        [HarmonyPatch(typeof(PlayerWindow), "Update")]
        class Update
        {
            static bool Prefix(PlayerWindow __instance)
            {
                for (int i = 0; i < DemulShooter_Plugin.MAX_PLAYERS; i++)
                {
                    if (DemulShooter_Plugin.PluginControllers[i].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Trigger) && DemulShooter_Plugin.InactiveStates[i] != null)
                    {
                        DemulShooter_Plugin.MyLogger.LogMessage("mPlayerWindow.Update() : P" + (i + 1).ToString() + " START pressed");

                        var methodInfo = typeof(InactivePlayerState).GetMethod("OnPlayerStartButtonPressed");
                        methodInfo.Invoke(DemulShooter_Plugin.InactiveStates[i], new object[] { PlayerUtility.GetPlayer(i) });
                    }
                }
                return true;
            }
        }
    }
}
