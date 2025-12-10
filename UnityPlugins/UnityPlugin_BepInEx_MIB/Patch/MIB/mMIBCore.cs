using System.Collections.Generic;
using System.Reflection.Emit;
using HarmonyLib;
using MIB;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mMIBCore
    {
        [HarmonyPatch(typeof(MIB.MIBCore), "AuthenticatedData")]
        class AuthenticatedData
        {
            static bool Prefix(ref bool __result)
            {
                DemulShooter_Plugin.MyLogger.LogMessage("MIBCore.AuthenticatedData()");
                __result = true;
                return false;
            }
        }

        /// <summary>
        /// Keep inputs enabled
        /// </summary>
        [HarmonyPatch(typeof(MIBCore), "Update")]
        class Update
        {
            static bool Prefix(ref int ___m_securityCounter)
            {
                ___m_securityCounter = 0;
                return true;
            }
            static void Postfix()
            {
                MIBCore.DisableInputs = false;
            }
        }

        /// <summary>
        /// Replace the path from hardcoded "C:/Sega/ShellData" to game local folder
        /// </summary>
        [HarmonyPatch(typeof(MIBCore), "GetCreditInformaion")]
        class GetCreditInformaion
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var code = new List<CodeInstruction>(instructions);
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Ldstr && (string)code[i].operand == "C:/Sega/ShellData/ShellData.ini")
                    {
                        code[i].operand = BepInEx.Paths.GameRootPath + "/ShellData/ShellData.ini";
                    }
                }
                return code;
            }
        }

        /// <summary>
        /// Replace the path from hardcoded "C:/Sega/ShellData" to game local folder
        /// </summary>
        [HarmonyPatch(typeof(MIBCore), "SetCalibration")]
        class SetCalibration
        {
            static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il)
            {
                var code = new List<CodeInstruction>(instructions);
                for (int i = 0; i < code.Count; i++)
                {
                    if (code[i].opcode == OpCodes.Ldstr && (string)code[i].operand == "C:/Sega/ShellData/ShellData.ini")
                    {
                        code[i].operand = BepInEx.Paths.GameRootPath + "/ShellData/ShellData.ini";
                    }
                }
                return code;
            }
        }
    }
}
