namespace UnityPlugin_BepInEx_Core
{
    public class TcpInputData : TcpData
    {
        public byte HideCrosshairs = 0;
        public byte EnableInputsHack = 0;

        public TcpInputData(int PlayerNumer) : base(PlayerNumer) { }
    }
}
