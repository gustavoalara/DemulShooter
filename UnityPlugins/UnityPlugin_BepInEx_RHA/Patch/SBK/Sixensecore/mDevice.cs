
using System.Reflection;
using HarmonyLib;

namespace BepInEx_DemulShooter_Plugin.Patch
{
    internal class mDevice
    {
        class sxCoreGetNumConnectedTrackedDevices
        {
            static MethodBase TargetMethod()
            {
                MethodInfo[] mis = AccessTools.TypeByName("Sixensecore.Plugin.Device").GetMethods(BindingFlags.Public | BindingFlags.Static);
                foreach (MethodInfo mi in mis)
                {
                    if (mi.Name.Equals("sxCoreGetNumConnectedTrackedDevices"))
                        return mi;
                }
                return null;
            }
            static bool Prefix(ref uint __result)
            {
                DemulShooter_Plugin.MyLogger.LogMessage("Sixensecore.Device.Plugin.sxCoreGetNumConnectedTrackedDevices");
                __result = 4;
                return false;
            }
        }
    }
}
