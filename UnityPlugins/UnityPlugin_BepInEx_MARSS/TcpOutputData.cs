using System;

namespace UnityPlugin_BepInEx_Core
{
    public class TcpOutputData : TcpData
    {
        public byte[] IsPlaying = null;
        public byte[] Recoil = null;
        public byte[] WeaponId = null;
        public int[] Ammo = null;
        public int[] Credits = null;
        public byte Flashlight = 0;

        public TcpOutputData(int PlayerNumer) : base(PlayerNumer) { }
    }
}
