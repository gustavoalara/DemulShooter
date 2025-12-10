using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mPauseGame
    {
        /// <summary>
        /// Replace the ESC key used to go in pause by TEST key
        /// </summary>
        [HarmonyPatch(typeof(PauseGame))]
        [HarmonyPatch("Update")]
        public static class Update
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
            {
                var codes = new List<CodeInstruction>(instructions);
                for (var i = 0; i < codes.Count; i++)
                {
                    if (codes[i].opcode == OpCodes.Call && codes[i].operand.ToString().Contains("GetKeyUp"))
                    {
                        if (codes[i - 1].opcode == OpCodes.Ldc_I4_S)
                        {
                            if (codes[i - 1].operand.ToString().Equals("27"))
                            {
                                codes[i - 1].operand = DemulShooter_Plugin.Test_Key.KeyCode;
                                DemulShooter_Plugin.MyLogger.LogMessage("PauseGame.Update(): Patched ESC key");
                                break;
                            }
                        }
                    }
                }
                return codes;
            }
        }
    }
}
