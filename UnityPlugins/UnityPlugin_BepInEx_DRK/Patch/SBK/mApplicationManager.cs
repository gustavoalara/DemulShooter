using HarmonyLib;
using UnityEngine;
using UnityPlugin_BepInEx_Core;

namespace BepInEx_DemulShooter_Plugin.Patch.SBK_
{
    class mApplicationManager
    {
        /// <summary>
        /// Changing Resolution based on the INI file
        /// </summary>
        [HarmonyPatch(typeof(SBK.ApplicationManager), "Start")]
        class start
        {
            static void Postfix()
            {
                if (DemulShooter_Plugin.ForceResolution)
                    Screen.SetResolution(DemulShooter_Plugin.ScreenWidth, DemulShooter_Plugin.ScreenHeight, DemulShooter_Plugin.Fullscreen);
            }
        }

        /// <summary>
        /// Using this Update loop, running during the whole game, to implement our logic
        /// Usually this is in a custom MonoBehaviour element created with the plugin, but I still have to find how to add one under IL2CPP
        /// </summary>
        [HarmonyPatch(typeof(SBK.ApplicationManager), "Update")]
        class Update
        {
            static bool Prefix()
            {
                //Credits are always 0
                /*
                DemulShooter_Plugin.OutputData.P1_Credits = (byte)PlayerUtility.GetPlayerCredit(0);
                DemulShooter_Plugin.OutputData.P2_Credits = (byte)PlayerUtility.GetPlayerCredit(1);
                */

                UnityEngine.Cursor.visible = false;

                //Auto closing console-debug window
                object developerConsoleVisible = null;
                var mInfo = typeof(UnityEngine.Debug).GetMethod("get_developerConsoleVisible", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                if (mInfo != null)
                {
                    developerConsoleVisible = mInfo.Invoke(null, null);
                    bool mVisible = (bool)developerConsoleVisible;
                    if (mVisible)
                    {
                        var setConsoleVisible_MethodInfo = typeof(UnityEngine.Debug).GetMethod("set_developerConsoleVisible", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public);
                        if (setConsoleVisible_MethodInfo != null)
                        {
                            DemulShooter_Plugin.MyLogger.LogMessage("Debug.set_developerConsoleVisible() found, trying to call it...");
                            setConsoleVisible_MethodInfo.Invoke(null, new object[] { false });
                        }
                        else
                        {
                            DemulShooter_Plugin.MyLogger.LogError("Debug.ClearDeveloperConsole() NOT found !");
                        }
                    }
                }
                else
                {
                    DemulShooter_Plugin.MyLogger.LogError("Debug.ClearDeveloperConsole() NOT found !");
                }


                //Custom Button handling
                DemulShooter_Plugin.Exit_Key.SetButton(Input.GetKey((KeyCode)DemulShooter_Plugin.Exit_Key.KeyCode));
                DemulShooter_Plugin.Test_Key.SetButton(Input.GetKey((KeyCode)DemulShooter_Plugin.Test_Key.KeyCode));
                for (int i = 0; i < DemulShooter_Plugin.MAX_PLAYERS; i++)
                {
                    DemulShooter_Plugin.PluginControllers[i].SetButton(PluginController.MyInputButtons.Start, Input.GetKey((KeyCode)DemulShooter_Plugin.PluginControllers[i].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode) ? (byte)1 : (byte)0);
                    DemulShooter_Plugin.PluginControllers[i].SetButton(PluginController.MyInputButtons.Coin, Input.GetKey((KeyCode)DemulShooter_Plugin.PluginControllers[i].InputButtons[(int)PluginController.MyInputButtons.Coin].KeyCode) ? (byte)1 : (byte)0);

                    if (!DemulShooter_Plugin.EnableInputHack)
                    {
                        DemulShooter_Plugin.PluginControllers[i].SetAimingValues(Input.mousePosition);
                        DemulShooter_Plugin.PluginControllers[i].SetButton(PluginController.MyInputButtons.Trigger, Input.GetMouseButton(0) ? (byte)1 : (byte)0);
                        DemulShooter_Plugin.PluginControllers[i].SetButton(PluginController.MyInputButtons.Reload, Input.GetMouseButton(1) ? (byte)1 : (byte)0);
                    }
                }

                //Quit
                if (DemulShooter_Plugin.Exit_Key.GetButtonDown())
                    UnityEngine.Application.Quit();

                //Fetching some outputs
                if (PlayerUtility.GetPlayer(SkyrideUtils.PlayerID.One).IsPlaying())
                    DemulShooter_Plugin.OutputData.Start_Led[0] = 0;
                else
                    DemulShooter_Plugin.OutputData.Start_Led[0] = 1;

                if (PlayerUtility.GetPlayer(SkyrideUtils.PlayerID.Two).IsPlaying())
                    DemulShooter_Plugin.OutputData.Start_Led[1] = 0;
                else
                    DemulShooter_Plugin.OutputData.Start_Led[1] = 1;

                //Checking for a change in output to send or not
                byte[] bOutputData = DemulShooter_Plugin.OutputData.ToByteArray();
                byte[] bOutputDataBefore = DemulShooter_Plugin.OutputDataBefore.ToByteArray();
                for (int i = 0; i < bOutputData.Length; i++)
                {
                    if (bOutputData[i] != bOutputDataBefore[i])
                    {
                        DemulShooter_Plugin.SendOutputs();
                        break;
                    }
                }

                //Save current state
                DemulShooter_Plugin.OutputDataBefore.Update(bOutputData);

                return true;                
            }
        }
    }
}
