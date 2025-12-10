using HarmonyLib;
using MIB;

namespace BepInEx_DemulShooter_Plugin
{
    internal class mMIBClient
    {
        [HarmonyPatch(typeof(MIBClient), "IsConnected")]
        class IsConnected
        {
            static bool Prefix(ref bool __result)
            {
                __result = true;
                return false;
            }
        }

        [HarmonyPatch(typeof(MIBClient), "TriggerRecoil")]
        class TriggerRecoil
        {
            static bool Prefix(MIBData_Recoil data)
            {
                DemulShooter_Plugin.OutputData.Motor[data.PlayerID - 1] = data.StartRecoil ? (byte)1 : (byte)0;
                   
                /*string s = string.Concat(new string[]
                {
                    "TriggerRecoil(playerid: ",
                    data.PlayerID.ToString(),
                    ", FinishRecoil: ",
                    data.FinishRecoil.ToString(),
                    ", Start Recoil: ",
                    data.StartRecoil.ToString(),
                    ", GunType: ",
                    data.GunType.ToString()
                });*/
                return false;
            }
        }
    }
}
