using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using UnityPlugin_BepInEx_Core;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mSBKInputManager
    {
        /// <summary>
        /// change those keys too ?? (right, left, down, select ???)
        /// </summary>
        [HarmonyPatch(typeof(SBKInputManager), "GetRightKeyDown")]
        class GetRightKeyDown
        {
            static void Prefix()
            {
                DemulShooter_Plugin.MyLogger.LogMessage("SBKInputManager.GetRightKeyDown()");
            }
        }
            

        /// <summary>
        /// Changing default Start buttons Keys
        /// </summary>
        [HarmonyPatch(typeof(SBKInputManager), "GetPlayerKeyDown")]
        class Update
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("GetKeyDown"))
                    {
                        if (codes[i - 1].opcode == OpCodes.Ldc_I4)
                        {
                            switch (codes[i - 1].operand.ToString())
                            {
                                case "257":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[0].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode; break;
                                case "259":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[1].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode; break;
                                case "263":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[2].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode; break;
                                case "265":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[3].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode; break;

                                default: break;
                            }
                        }
                    }
                }

                return codes;
            }
        }

        [HarmonyPatch(typeof(SBKInputManager), "GetActivePlayerKeyDown")]
        class GetActivePlayerKeyDown
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (int i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("GetKeyDown"))
                    {
                        if (codes[i - 1].opcode == OpCodes.Ldc_I4)
                        {
                            switch (codes[i - 1].operand.ToString())
                            {
                                case "257":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[0].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode; break;
                                case "259":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[1].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode; break;
                                case "263":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[2].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode; break;
                                case "265":
                                    codes[i - 1].operand = DemulShooter_Plugin.PluginControllers[3].InputButtons[(int)PluginController.MyInputButtons.Start].KeyCode; break;

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
