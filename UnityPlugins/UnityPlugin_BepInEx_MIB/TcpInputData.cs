namespace UnityPlugin_BepInEx_Core
{
    public class TcpInputData : TcpData
    {
		//Game Inputs
		public float[] Axis_X = null;
		public float[] Axis_Y = null;
		public byte[] Trigger = null;
        public byte HideGuns = 0;

        //Generic Inputs
        public byte HideCrosshairs = 0;
		public byte EnableInputsHack = 0;

        public TcpInputData(int PlayerNumer) : base(PlayerNumer) { }
    }
}
