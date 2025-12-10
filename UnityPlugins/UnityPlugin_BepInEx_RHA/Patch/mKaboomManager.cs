using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using KaboomOutput;
using UnityPlugin_BepInEx_Core;

namespace BepInEx_DemulShooter_Plugin
{
    class mKaboomManager
    {
        /// <summary>
        /// Use this funtion to generate DemulShooter custom recoil output
        /// </summary>
        [HarmonyPatch(typeof(KaboomManager), "StartMotorPwm")]
        class StartMotorPwm
        {
            static bool Prefix(KaboomOutput.MotorPwmId MotorId, byte un8PwmDutyCycle, ushort iMsDuration = 144)
            {
                //UnityEngine.Debug.Log("KaboomManager.StartMotorPwm() : MotorId=" + MotorId.ToString() + ", un8PwmDutyCycle=" + un8PwmDutyCycle + ", iMsDuration=" + iMsDuration);         
                if (MotorId.ToString().Contains("P1"))
                    DemulShooter_Plugin.OutputData.Recoil[0] = 1;
                else if (MotorId.ToString().Contains("P2"))
                    DemulShooter_Plugin.OutputData.Recoil[1] = 1;
                else if (MotorId.ToString().Contains("P3"))
                    DemulShooter_Plugin.OutputData.Recoil[2] = 1;
                else if (MotorId.ToString().Contains("P4"))
                    DemulShooter_Plugin.OutputData.Recoil[3] = 1;

                return true;
            }
        }

        /// <summary>
        /// Use this to intercept Light ON status
        /// </summary>
        [HarmonyPatch(typeof(KaboomManager), "SetPlayerLight")]
        class SetPlayerLight
        {
            static bool Prefix(OutputPlayerLight i_Mask)
            {
                //UnityEngine.Debug.Log("KaboomManager.SetPlayerLight() : i_Mask=" + i_Mask.ToString());
                if (i_Mask == OutputPlayerLight.OUTPUT_P1)
                    DemulShooter_Plugin.OutputData.StartLamp[0] = 1;
                else if (i_Mask == OutputPlayerLight.OUTPUT_P2)
                    DemulShooter_Plugin.OutputData.StartLamp[1] = 1;
                else if (i_Mask == OutputPlayerLight.OUTPUT_P3)
                    DemulShooter_Plugin.OutputData.StartLamp[2] = 1;
                else if (i_Mask == OutputPlayerLight.OUTPUT_P4)
                    DemulShooter_Plugin.OutputData.StartLamp[3] = 1;
                else if (i_Mask == OutputPlayerLight.OUTPUT_ALL)
                {
                    DemulShooter_Plugin.OutputData.StartLamp[0] = 1;
                    DemulShooter_Plugin.OutputData.StartLamp[1] = 1;
                    DemulShooter_Plugin.OutputData.StartLamp[2] = 1;
                    DemulShooter_Plugin.OutputData.StartLamp[3] = 1;
                }

                return true;
            }
        }

        /// <summary>
        /// Use this to intercept Light OFF status
        /// </summary>
        [HarmonyPatch(typeof(KaboomManager), "ClearPlayerLight")]
        class ClearPlayerLight
        {
            static bool Prefix(OutputPlayerLight i_Mask)
            {
                //UnityEngine.Debug.Log("KaboomManager.ClearPlayerLight() : i_Mask=" + i_Mask.ToString());
                if (i_Mask == OutputPlayerLight.OUTPUT_P1)
                    DemulShooter_Plugin.OutputData.StartLamp[0] = 0;
                else if (i_Mask == OutputPlayerLight.OUTPUT_P2)
                    DemulShooter_Plugin.OutputData.StartLamp[1] = 0;
                else if (i_Mask == OutputPlayerLight.OUTPUT_P3)
                    DemulShooter_Plugin.OutputData.StartLamp[2] = 0;
                else if (i_Mask == OutputPlayerLight.OUTPUT_P4)
                    DemulShooter_Plugin.OutputData.StartLamp[3] = 0;
                else if (i_Mask == OutputPlayerLight.OUTPUT_ALL)
                {
                    DemulShooter_Plugin.OutputData.StartLamp[0] = 0;
                    DemulShooter_Plugin.OutputData.StartLamp[1] = 0;
                    DemulShooter_Plugin.OutputData.StartLamp[2] = 0;
                    DemulShooter_Plugin.OutputData.StartLamp[3] = 0;
                }

                return true;
            }
        }

        /// <summary>
        /// Changing some default keyboard controls
        /// </summary>
        [HarmonyPatch(typeof(KaboomManager), "Update")]
        class Update
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("GetKey"))
                    {
                        if (codes[i - 1].opcode == OpCodes.Ldc_I4_S || codes[i - 1].opcode == OpCodes.Ldc_I4)
                        {
                            switch (codes[i - 1].operand.ToString())
                            {
                                case "27":
                                    codes[i - 1].operand = DemulShooter_Plugin.Exit_Key.KeyCode; break;
                                case "99":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[0].InputButtons[(int)PluginController.MyInputButtons.Coin].KeyCode; break;
                                case "118":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[1].InputButtons[(int)PluginController.MyInputButtons.Coin].KeyCode; break;
                                case "98":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[2].InputButtons[(int)PluginController.MyInputButtons.Coin].KeyCode; break;
                                case "110":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[3].InputButtons[(int)PluginController.MyInputButtons.Coin].KeyCode; break;
                                case "111":
                                    codes[i - 1].operand = DemulShooter_Plugin.Test_Key.KeyCode; break;
                                case "257":
                                    codes[i - 1].operand = DemulShooter_Plugin.MenuUp_Key.KeyCode; break;
                                case "258":
                                    codes[i - 1].operand = DemulShooter_Plugin.MenuSelect_Key.KeyCode; break;
                                case "259":
                                    codes[i - 1].operand = DemulShooter_Plugin.MenuDown_Key.KeyCode; break;
                                case "103":
                                    codes[i - 1].operand = 0; break;
                                case "282":
                                    codes[i - 1].operand = 0; break;

                                default: break;
                            }
                        }
                                
                                
                    }
                }

                return codes;
            }
        }
    }
}
