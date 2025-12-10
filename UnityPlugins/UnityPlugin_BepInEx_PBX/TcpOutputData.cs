using System;

namespace UnityPlugin_BepInEx_Core
{
    public class TcpOutputData : TcpData
    {
        public byte[] Recoil = null;
        public byte[] StartLED = null;
        public byte[] PlayerLED = null;
        public byte[] Life = null;
        public int[] Ammo = null;
        public byte Credits = 0;

        public TcpOutputData(int PlayerNumer) : base(PlayerNumer) { }
    }
}
