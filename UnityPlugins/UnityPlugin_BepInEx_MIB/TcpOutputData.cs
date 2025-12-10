using System;

namespace UnityPlugin_BepInEx_Core
{
    public class TcpOutputData : TcpData
    {
        public byte[] IsPlaying = null;
        public byte[] Recoil = null;
        public byte[] Motor = null;
        public byte[] Damaged = null;
        public byte NeuralyzerLamp = 0;
        public byte[] GunLight = null;
        public int Credits = 0;

        public TcpOutputData(int PlayerNumer) : base(PlayerNumer) { }
    }
}
