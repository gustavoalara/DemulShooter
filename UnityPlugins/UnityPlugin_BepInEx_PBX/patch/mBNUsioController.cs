using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityPlugin_BepInEx_Core;

namespace BepInEx_DemulShooter_Plugin
{
    class mBNUsioController
    {  
        /// <summary>
        /// Intercepting START Led conmmands to send to DemulShooter
        /// </summary>
        [HarmonyPatch(typeof(BNUsioController), "setStartLED")]
        class setStartLED
        {
            static void Postfix(int idx, bool isOn)
            {
                //DemulShooter_Plugin.MyLogger.LogMessage("mBNUsioController.setStartLED() => idx : " + idx.ToString() + ", isOn: " + isOn.ToString());
                DemulShooter_Plugin.OutputData.StartLED[idx - 1] = isOn == true ? (byte)1 : (byte)0;              
            }
        }

        /// <summary>
        /// Intercepting Cabinet Led conmmands to send to DemulShooter
        /// </summary>
        [HarmonyPatch(typeof(BNUsioController), "setLED")]
        class setLED
        {
            static void Postfix(int idx, bool isOn)
            {
                //DemulShooter_Plugin.MyLogger.LogMessage("mBNUsioController.setLED() => idx : " + idx.ToString() + ", isOn: " + isOn.ToString());
                DemulShooter_Plugin.OutputData.PlayerLED[idx - 1] = isOn == true ? (byte)1 : (byte)0;
            }
        }

        [HarmonyPatch(typeof(BNUsioController), "update")]
        class Update
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {                
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count - 1; i++)
                {
                    if (codes[i + 1].opcode == OpCodes.Call && codes[i + 1].operand.ToString().Contains("GetKey"))
                    {
                        //For menu navigation and start, it's easy to replace the KeyCodes from the instruction with our own
                        if (codes[i].opcode == OpCodes.Ldc_I4_S || codes[i].opcode == OpCodes.Ldc_I4)
                        {
                            if (codes[i].operand.ToString().Equals("57"))
                                codes[i].operand = DemulShooter_Plugin.Service_Key.KeyCode;
                            else if (codes[i].operand.ToString().Equals("273"))
                                codes[i].operand = DemulShooter_Plugin.MenuUp_Key.KeyCode;
                            else if (codes[i].operand.ToString().Equals("274"))
                                codes[i].operand = DemulShooter_Plugin.MenuDown_Key.KeyCode;
                            else if (codes[i].operand.ToString().Equals("13"))
                                codes[i].operand = DemulShooter_Plugin.MenuSelect_Key.KeyCode;
                            else if (codes[i].operand.ToString().Equals("49"))
                                codes[i].operand = DemulShooter_Plugin.PluginControllers[0].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode;
                            else if (codes[i].operand.ToString().Equals("50"))
                                codes[i].operand = DemulShooter_Plugin.PluginControllers[1].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode;
                        }
                        //On the other hand, TEST key (BACKSPACE) has no operand has it's loaded directly with a Ldc_I4_8
                        //So replacing it by "0" will deactivate the key press, and the boolean will be set in the main plugin Upd
                        else if (codes[i].opcode == OpCodes.Ldc_I4_8)
                        {
                            codes[i].opcode = OpCodes.Ldc_I4_0;
                        }
                    }
                }
                return codes;
            }
        }



    }
}
