using System.Reflection;
using HarmonyLib;
using UnityEngine;

namespace BepInEx_DemulShooter_Plugin
{
    class mIOManager
    {
        /// <summary>
        /// Force not using IO board
        /// </summary>
        [HarmonyPatch(typeof(IOManager), "set_m_UseRio")]
        class set_m_UseRio
        {
            static bool Prefix(ref bool value)
            {
                DemulShooter_Plugin.MyLogger.LogMessage("IOManager.set_m_UseRio() : value=" + value);
                value = false;
                return false;
            }
        }

        
        /// <summary>
        /// Update buttons based on Keyboard / Custom input data
        /// That function is called when m_UseRio is set to false
        /// </summary>
        [HarmonyPatch(typeof(IOManager), "UpdateInput_Unity")]
        class UpdateInput_Unity
        {
            static bool Prefix(DigitalInputData[] ___m_DigitalInputData, IOManager __instance)
            {
                //DemulShooter_Plugin.MyLogger.LogMessage("IOManager.ButtonNewlyPressed() : inputIn=" + inputIn.ToString());
                for (int i = 0; i < ___m_DigitalInputData.Length; i++)
                {
                    ___m_DigitalInputData[i].switchCntLast = ___m_DigitalInputData[i].switchCnt;

                    uint iKeyPressed = 0;
                    switch (i)
                    {
                        case (int)DigitalGameInput.Coin1: iKeyPressed = DemulShooter_Plugin.PluginControllers[0].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Coin) ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.Coin2: iKeyPressed = DemulShooter_Plugin.PluginControllers[1].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Coin) ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.DBV: iKeyPressed = DemulShooter_Plugin.DBV_Key.GetButton() ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.GunShoulder1: iKeyPressed = DemulShooter_Plugin.PluginControllers[0].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Action) ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.GunShoulder2: iKeyPressed = DemulShooter_Plugin.PluginControllers[1].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Action) ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.GunTrigger1: iKeyPressed = DemulShooter_Plugin.PluginControllers[0].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Trigger) ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.GunTrigger2: iKeyPressed = DemulShooter_Plugin.PluginControllers[1].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Trigger) ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.Service: iKeyPressed = DemulShooter_Plugin.Service_Key.GetButton() ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.Start1: iKeyPressed = DemulShooter_Plugin.PluginControllers[0].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Start) ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.Start2: iKeyPressed = DemulShooter_Plugin.PluginControllers[1].GetButton(UnityPlugin_BepInEx_Core.PluginController.MyInputButtons.Start) ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.Test: iKeyPressed = DemulShooter_Plugin.Test_Key.GetButton() ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.VolumeDown: iKeyPressed = DemulShooter_Plugin.VolumeDown_Key.GetButton() ? (uint)1 : (uint)0; break;
                        case (int)DigitalGameInput.VolumeUp: iKeyPressed = DemulShooter_Plugin.VolumeUp_Key.GetButton() ? (uint)1 : (uint)0; break;
                        default: break;
                    }
                    ___m_DigitalInputData[i].switchCnt = iKeyPressed;

                    if (___m_DigitalInputData[i].switchCntLast == ___m_DigitalInputData[i].switchCnt)
                    {
                        ___m_DigitalInputData[i].time += Time.deltaTime;
                    }
                    else
                    {
                        ___m_DigitalInputData[i].time = 0f;
                    }
                }

                MethodInfo mi = __instance.GetType().GetMethod("SetAnalogValue", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (mi != null)
                {
                    mi.Invoke(__instance, new object[] { AnalogGameInput.Gun1X, DemulShooter_Plugin.PluginControllers[0].GetAimingPosition().x / (float)Screen.width });
                    mi.Invoke(__instance, new object[] { AnalogGameInput.Gun1Y, DemulShooter_Plugin.PluginControllers[0].GetAimingPosition().y / (float)Screen.height });
                    mi.Invoke(__instance, new object[] { AnalogGameInput.Gun2X, DemulShooter_Plugin.PluginControllers[1].GetAimingPosition().x / (float)Screen.width });
                    mi.Invoke(__instance, new object[] { AnalogGameInput.Gun2Y, DemulShooter_Plugin.PluginControllers[1].GetAimingPosition().y / (float)Screen.height });
                }
               
                return false;                
            }
        }
    }
}
