using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    /// <summary>
    /// Adding the possibility to send a "START button pressed" signal by shooting (like it is written on screen)
    /// </summary>
    internal class mContinueWindow
    {
        [HarmonyPatch(typeof(ContinueWindow), "Update")]
        class Update
        {
            static bool Prefix(ContinueWindow __instance)
            {
                for (int i = 0; i < DemulShooter_Plugin.MAX_PLAYERS; i++)
                {
                    if (DemulShooter_Plugin.PluginControllers[i].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Trigger) && DemulShooter_Plugin.InactiveStates[i] != null)
                    {
                        DemulShooter_Plugin.MyLogger.LogMessage("mContinueWindow.Update() : P" + (i + 1).ToString() +" START pressed");

                        var methodInfo = typeof(InactivePlayerState).GetMethod("OnPlayerStartButtonPressed");
                        methodInfo.Invoke(DemulShooter_Plugin.InactiveStates[i], new object[] { PlayerUtility.GetPlayer(i) });

                        methodInfo = typeof(ContinueWindow).GetMethod("OnPlayerStartButtonPressed");
                        methodInfo.Invoke(__instance, new object[] { PlayerUtility.GetPlayer(i) });
                    }
                }
                return true;
            }
        }
    }
}
