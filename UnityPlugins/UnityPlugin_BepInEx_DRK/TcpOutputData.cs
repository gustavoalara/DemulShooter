using System;

namespace UnityPlugin_BepInEx_Core
{
    public class TcpOutputData : TcpData
    {
        public byte[] IsPlaying = null;
        public byte[] Start_Led = null;
        public byte[] Recoil = null;
        public byte[] Damaged = null;
        public byte[] Rumble = null;
        public int[] Credits = null;

        public TcpOutputData(int PlayerNumer) : base(PlayerNumer) { }
    }
}
